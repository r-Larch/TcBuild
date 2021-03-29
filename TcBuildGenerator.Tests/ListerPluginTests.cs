using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;


namespace TcBuildGenerator.Tests {
    public class ListerPluginTests {
        [Test]
        public void Test_TcPluginSymbolVisitor()
        {
            var parentAssembly = typeof(TcPluginBase.TcPlugin).Assembly;
            var classSymbol = Helper.GetClassSymbol(parentAssembly, @"
                using System;
                using Microsoft.Extensions.Configuration;
                using TcPluginBase.Lister;
                using System.Windows.Controls;

                namespace MyNamespace {
                    public class MyLister : WpfListerPlugin<UserControl> {
                        public MyLister(IConfiguration pluginSettings) : base(pluginSettings)
                        {
                        }

                        public override SupportExpression CanHandle { get; }

                        public override WpfLister<UserControl>? Load(ParentWindow parent, string fileToLoad, ShowFlags showFlags)
                        {
                            return new(parent, new UserControl());
                        }
                    }
                }
            ");

            var visitor = new TcPluginSymbolVisitor(Log);
            visitor.Visit(classSymbol);
            var plugins = visitor.Plugins;

            Assert.AreEqual(1, plugins.Count);
            var plugin = plugins.Single();

            Assert.AreEqual(PluginType.Lister, plugin.Type);

            Assert.AreEqual("MyNamespace", plugin.Namespace);
            Assert.AreEqual("MyNamespace.MyLister", plugin.ClassFullName);

            Assert.AreEqual(1, plugin.ImplementedMethods.Count);
            Assert.AreEqual("Load", plugin.ImplementedMethods[0].MethodName);
            Assert.AreEqual("TcPluginBase.Lister.IListerPlugin<TcPluginBase.Lister.WpfLister<System.Windows.Controls.UserControl>>.Load(TcPluginBase.Lister.ParentWindow, string, TcPluginBase.Lister.ShowFlags)", plugin.ImplementedMethods[0].Signature);
            Assert.AreEqual("MyNamespace.MyLister", plugin.ImplementedMethods[0].ContainingType);

            Assert.AreEqual(PluginType.Lister, plugin.Definition.Type);
            Assert.AreEqual("IListerPlugin", plugin.Definition.Name);
            Assert.AreEqual(9, plugin.Definition.Methods.Count);

            var getFilesMethod = plugin.Definition.Methods[plugin.ImplementedMethods[0].Signature];

            Assert.AreEqual("ListLoad,ListLoadW", string.Join(",", getFilesMethod.WrapperData.MethodNames));


            var source = TcBuildSourceGenerator.GenerateWrapperSource(plugin);
            var final = TcBuildSourceGenerator.ModifySource(plugin, source);

            Console.WriteLine(final);
        }


        private static void Log(Diagnostic diagnostic)
        {
            Console.WriteLine(diagnostic.Descriptor.Title);
            Console.WriteLine(diagnostic.Descriptor.MessageFormat);
        }
    }
}
