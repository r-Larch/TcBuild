using TcPluginBase.Lister;
using Xunit;


namespace TcPluginBase.Test {
    public class SupportExpressionTest {
        [Fact]
        public void Bytes()
        {
            var result = SupportExpression.Create(_ => _[1] == 2);

            Assert.Equal(
                expected: @"[1] = 2",
                actual: result.Value
            );
        }

        [Fact]
        public void Force()
        {
            var result = SupportExpression.Create(_ => _.Force);

            Assert.Equal(
                expected: @"FORCE",
                actual: result.Value
            );
        }

        [Fact]
        public void Multimedia()
        {
            var result = SupportExpression.Create(_ => _.Multimedia);

            Assert.Equal(
                expected: @"MULTIMEDIA",
                actual: result.Value
            );
        }

        [Fact]
        public void Size()
        {
            var result = SupportExpression.Create(_ => _.Size == 10);

            Assert.Equal(
                expected: @"SIZE = 10",
                actual: result.Value
            );
        }


        [Fact]
        public void ClosureVariables1()
        {
            var maxSize = 10;

            var result = SupportExpression.Create(_ => _.Size == maxSize);

            Assert.Equal(
                expected: @"SIZE = 10",
                actual: result.Value
            );
        }


        [Fact]
        public void ClosureVariables2()
        {
            var obj = new {
                Prop = new[] {
                    1, 2, 3, 4
                }
            };

            var result = SupportExpression.Create(_ => _.Size == obj.Prop[1]);

            Assert.Equal(
                expected: @"SIZE = 2",
                actual: result.Value
            );
        }


        [Fact]
        public void ClosureVariables3()
        {
            var obj = new {
                Prop = new[] {
                    1, 2, 3, 4
                }
            };

            var result = SupportExpression.Create(_ => _.Size == MyFunc(obj.Prop, 1) && _.HasExt(new {Hoi = "test"}.Hoi));

            Assert.Equal(
                expected: @"SIZE = 2 & EXT = ""TEST""",
                actual: result.Value
            );
        }

        private static long MyFunc(int[] prop, int i) => prop[i];


        [Fact]
        public void Ext()
        {
            var result = SupportExpression.Create(_ => _.HasExt("txt"));

            Assert.Equal(
                expected: @"EXT = ""TXT""",
                actual: result.Value
            );
        }

        [Fact]
        public void NotExt()
        {
            var result = SupportExpression.Create(_ => _.NotHasExt("txt"));

            Assert.Equal(
                expected: @"EXT != ""TXT""",
                actual: result.Value
            );
        }


        [Fact]
        public void Test1()
        {
            var result = SupportExpression.Create(_ => _.HasExt("wav") || _.HasExt("avi"));

            Assert.Equal(
                expected: @"(EXT = ""WAV"" | EXT = ""AVI"")",
                actual: result.Value
            );
        }


        [Fact]
        public void Test2()
        {
            var result = SupportExpression.Create(_ => _.HasExt("wav") && _[0] == 'R' && _[1] == 'I' && _[2] == 'F' && _[3] == 'F' && _.Find("WAVEfmt"));

            Assert.Equal(
                expected: @"EXT = ""WAV"" & [0] = 82 & [1] = 73 & [2] = 70 & [3] = 70 & FIND(""WAVEfmt"")",
                actual: result.Value
            );
        }


        [Fact]
        public void Test3()
        {
            var result = SupportExpression.Create(_ => _.HasExt("wav") && (_.Size < 1000000 || _.Force));

            Assert.Equal(
                expected: @"EXT = ""WAV"" & (SIZE < 1000000 | FORCE)",
                actual: result.Value
            );
        }


        [Fact]
        public void Test4()
        {
            var result = SupportExpression.Create(_ =>
                (_[0] == 'P' && _[1] == 'K' && _[2] == 3 && _[3] == 4) ||
                (_[0] == 'P' && _[1] == 'K' && _[2] == 7 && _[3] == 8)
            );

            Assert.Equal(
                expected: @"([0] = 80 & [1] = 75 & [2] = 3 & [3] = 4 | [0] = 80 & [1] = 75 & [2] = 7 & [3] = 8)",
                actual: result.Value
            );
        }


        [Fact]
        public void Test5()
        {
            var result = SupportExpression.Create(_ =>
                (_[0] == 1 || _[1] == 2) &&
                (_[0] == 3 || _[1] == 4)
            );

            Assert.Equal(
                expected: @"([0] = 1 | [1] = 2) & ([0] = 3 | [1] = 4)",
                actual: result.Value
            );
        }

        [Fact]
        public void Test6()
        {
            var result = SupportExpression.Create(_ => _.HasExt("txt") && !(_.FindI("<head>") || _.FindI("<body>")));

            Assert.Equal(
                expected: @"EXT = ""TXT"" & !((FINDI(""<head>"") | FINDI(""<body>"")))",
                actual: result.Value
            );
        }

        [Fact]
        public void Test7()
        {
            var result = SupportExpression.Create(_ => _.Multimedia && (_.HasExt("wav") || _.HasExt("mp3")));

            Assert.Equal(
                expected: @"MULTIMEDIA & (EXT = ""WAV"" | EXT = ""MP3"")",
                actual: result.Value
            );
        }
    }
}
