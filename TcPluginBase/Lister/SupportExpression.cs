using System;
using System.Linq.Expressions;


namespace TcPluginBase.Lister {
    public class SupportExpression {
        public string Value { get; }

        private SupportExpression(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Examples:
        /// <list type="bullet">
        ///   <item><description> EXT="WAV" | EXT="AVI" The file may be a Wave or AVI file. </description></item>
        ///   <item><description> EXT="WAV" &amp; [0]="R" &amp; [1]="I" &amp; [2]="F" &amp; [3]="F" &amp; FIND("WAVEfmt") </description></item>
        ///   <item><description> Also checks for Wave header "RIFF" and string "WAVEfmt" </description></item>
        ///   <item><description> EXT="WAV" &amp; (SIZE&lt;1000000 | FORCE) Load wave files smaller than 1000000 bytes at startup/file change, and all wave files if the user explictly chooses Image/Multimedia from the menu. </description></item>
        ///   <item><description> ([0]="P" &amp; [1]="K" &amp; [2]=3 &amp; [3]=4) | ([0]="P" &amp; [1]="K" &amp; [2]=7 &amp; [3]=8) Checks for the ZIP header PK#3#4 or PK#7#8 (the latter is used for multi-volume zip files). </description></item>
        ///   <item><description> EXT="TXT" &amp; !(FINDI("&lt;HEAD>") | FINDI("&lt;BODY>")) This plugin handles text files which aren't HTML files. A first detection is done with the &lt;HEAD> and &lt;BODY> tags. If these are not found, a more thorough check may be done in the plugin itself. </description></item>
        ///   <item><description> MULTIMEDIA &amp; (EXT="WAV" | EXT="MP3") Replace the internal player for WAV and MP3 files (which normally uses Windows Media Player as a plugin). Requires TC 6.0 or later! </description></item>
        /// </list>
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static SupportExpression Create(Expression<Func<x, bool>> expression)
        {
            var visitor = new MyExpressionParser();
            var value = visitor.Parse(expression);
            return new SupportExpression(value);
        }
    }


    internal class MyExpressionParser : ExpressionVisitor {
        public string Parse(Expression expression)
        {
            var reduced = Visit(expression);

            if (reduced is ResultExpression result) {
                return result.Value;
            }

            throw new NotSupportedException($"Invalid Expression: {expression}");
        }

        protected virtual Expression VisitObject(ObjectExpression objectExpression)
        {
            var value = objectExpression.GetValue();
            var constant = Expression.Constant(value);
            return Visit(constant);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var expression = (Expression<T>) base.VisitLambda(node);

            if (expression.Body is ResultExpression result) {
                return result;
            }

            return new ObjectExpression(node, node.Type);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value switch {
                string v => $@"""{v}""",
                char v => $@"""{v}""",
                int v => v.ToString(),
                long v => v.ToString(),
                _ => null,
            };

            if (value != null) {
                return new ResultExpression(value, node.Type);
            }

            return new ObjectExpression(node, node.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var expression = (MethodCallExpression) base.VisitMethodCall(node);

            if (expression is {Arguments: {Count: 1} arguments} && arguments[0] is var parameter) {
                var arg = Parse(parameter);

                var result = node.Method.Name switch {
                    nameof(x.HasExt) => $@"EXT = {arg.ToUpper()}",
                    nameof(x.NotHasExt) => $@"EXT != {arg.ToUpper()}",
                    nameof(x.Find) => $@"FIND({arg})",
                    nameof(x.FindI) => $@"FINDI({arg})",
                    "get_Item" => $@"[{arg}]",
                    _ => null,
                };

                if (result != null) {
                    return new ResultExpression(result, expression.Type);
                }
            }

            return new ObjectExpression(node, node.Type);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var expression = (BinaryExpression) base.VisitBinary(node);

            var op = node.NodeType switch {
                ExpressionType.OrElse => "|",
                ExpressionType.AndAlso => "&",
                ExpressionType.GreaterThan => ">",
                ExpressionType.LessThan => "<",
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                _ => null,
            };

            if (op != null) {
                var left = Parse(expression.Left);
                var right = Parse(expression.Right);

                var result = $"{left} {op} {right}";

                if (op == "|") {
                    result = $"({result})";
                }

                return new ResultExpression(result, expression.Type);
            }

            return new ObjectExpression(node, node.Type);
        }


        protected override Expression VisitUnary(UnaryExpression node)
        {
            var expression = (UnaryExpression) base.VisitUnary(node);

            if (expression.Operand is ResultExpression result) {
                return expression.NodeType switch {
                    ExpressionType.Not => new ResultExpression($"!({result.Value})", expression.Type),
                    _ => new ResultExpression(result.Value, expression.Type)
                };
            }

            return new ObjectExpression(node, node.Type);
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = (MemberExpression) base.VisitMember(node);

            if (expression.NodeType == ExpressionType.MemberAccess && expression.Member.DeclaringType == typeof(x)) {
                return new ResultExpression(node.Member.Name.ToUpper(), node.Type);
            }

            return new ObjectExpression(node, node.Type);
        }


        protected class ResultExpression : Expression {
            public sealed override ExpressionType NodeType => ExpressionType.Constant;
            protected override Expression Accept(ExpressionVisitor visitor) => this;

            internal ResultExpression(string value, Type type)
            {
                Value = value;
                Type = type;
            }

            public string Value { get; }
            public override Type Type { get; }
        }


        protected class ObjectExpression : Expression {
            public sealed override ExpressionType NodeType => ExpressionType.Constant;
            protected override Expression Accept(ExpressionVisitor visitor) => ((MyExpressionParser) visitor).VisitObject(this);

            internal ObjectExpression(Expression expression, Type type)
            {
                Expression = expression;
                Type = type;
            }

            public Expression Expression { get; }
            public override Type Type { get; }

            public object? GetValue()
            {
                var lambda = Expression.Lambda(Expression);
                var func = lambda.Compile();
                var result = func.DynamicInvoke();
                return result;
            }
        }
    }


    public class x {
        /// <summary>
        /// The size of the file to be loaded.
        /// </summary>
        public long Size => 0;

        /// <summary>
        /// if the user chose 'Image/Multimedia' from the menu, 0 otherwise.
        /// </summary>
        public bool Force => false;

        /// <summary>
        /// This is always TRUE (also in older TC versions).
        /// If it is present in the string, this plugin overrides internal multimedia viewers in TC.
        /// If not, the internal viewers are used.
        /// <para> MULTIMEDIA &amp; (EXT="WAV" | EXT="MP3") </para>
        /// </summary>
        public bool Multimedia => false;

        /// <summary>
        /// The extension of the file to be loaded (case insensitive).
        /// </summary>
        public bool HasExt(string ext) => false;

        public bool NotHasExt(string ext) => false;

        /// <summary>
        /// Access a specific byte from the file to be loaded. The first 8192 bytes can be checked for a match.
        /// </summary>
        public byte this[int ordinal] => 0;

        /// <summary>
        /// The text inside the braces is searched in the first 8192 bytes of the file. Returns <c>true</c> for success and <c>false</c> for failure.
        /// </summary>
        public bool Find(string text) => false;

        /// <summary>
        /// The text inside the braces is searched in the first 8192 bytes of the file. Upper/lowercase is ignored.
        /// </summary>
        public bool FindI(string text) => false;
    }
}
