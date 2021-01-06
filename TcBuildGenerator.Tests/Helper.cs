using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace TcBuildGenerator.Tests {
    public class Helper {
        public static (SemanticModel model, SyntaxTree tree) GetCompilation(Assembly parentAssembly, string sourceText)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] {tree},
                references: Helper.GetMetadataReferences(
                    parentAssembly
                ),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            var model = compilation.GetSemanticModel(tree);

            foreach (var diagnostic in model.GetDiagnostics()) {
                Console.WriteLine(diagnostic);
            }

            return (model, tree);
        }

        public static INamedTypeSymbol GetClassSymbol(Assembly parentAssembly, string sourceText)
        {
            var (model, tree) = Helper.GetCompilation(parentAssembly, sourceText);

            var classDeclaration = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();
            var classSymbol = model.GetDeclaredSymbol(classDeclaration)!;

            return classSymbol;
        }

        private static IEnumerable<MetadataReference> GetMetadataReferences(Assembly parentAssembly)
        {
            var implicitAssemblies = new[] {
                parentAssembly,
                Assembly.Load("Microsoft.CSharp")
            };

            var dependencyAssemblies = parentAssembly
                .GetTransitiveAssemblies()
                .Select(n => n.TryLoad())
                .Where(n => n != null);

            return implicitAssemblies.Concat(dependencyAssemblies)
                .Distinct()
                .Select(a => MetadataReference.CreateFromFile(a!.Location));
        }
    }


    internal static class AssemblyExtensions {
        public static Assembly? TryLoad(this AssemblyName assemblyName)
        {
            try {
                return Assembly.Load(assemblyName);
            }
            catch {
                return null;
            }
        }

        public static IEnumerable<AssemblyName> GetTransitiveAssemblies(this Assembly assembly)
        {
            var assemblyNames = new HashSet<AssemblyName>(AssemblyNameEqualityComparer.Instance);
            assembly.PopulateTransitiveAssemblies(assemblyNames);

            return assemblyNames;
        }

        private static void PopulateTransitiveAssemblies(this Assembly assembly, ISet<AssemblyName> assemblyNames)
        {
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies()) {
                // Already exists
                if (!assemblyNames.Add(referencedAssemblyName))
                    continue;

                var referencedAssembly = referencedAssemblyName.TryLoad();
                if (referencedAssembly != null)
                    referencedAssembly.PopulateTransitiveAssemblies(assemblyNames);
            }
        }

        internal class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName> {
            public static AssemblyNameEqualityComparer Instance { get; } = new AssemblyNameEqualityComparer();
            public bool Equals(AssemblyName? x, AssemblyName? y) => StringComparer.OrdinalIgnoreCase.Equals(x?.FullName, y?.FullName);
            public int GetHashCode(AssemblyName obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
        }
    }
}
