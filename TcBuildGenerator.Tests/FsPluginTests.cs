using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;


namespace TcBuildGenerator.Tests {
    public class FsPluginTests {
        [Test]
        public void Test_TcPluginSymbolVisitor()
        {
            var parentAssembly = typeof(TcPluginBase.TcPlugin).Assembly;
            var classSymbol = Helper.GetClassSymbol(parentAssembly, @"
                using System.IO;
                using System.Collections.Generic;
                using Microsoft.Extensions.Configuration;
                using TcPluginBase.FileSystem;

                namespace MyNamespace {
                    public class MyFsPlugin : FsPlugin {
                        public MyFsPlugin(IConfiguration pluginSettings) : base(pluginSettings)
                        {
                        }

                        public override IEnumerable<FindData> GetFiles(RemotePath path)
                        {
                            return new [] {
                                new FindData(""folder"", FileAttributes.Directory),
                                new FindData(""file1.txt""),
                                new FindData(""file2.txt""),
                                new FindData(""file3.txt""),
                            };
                        }
                    }
                }
            ");

            var visitor = new TcPluginSymbolVisitor(Log);
            visitor.Visit(classSymbol);
            var plugins = visitor.Plugins;

            Assert.AreEqual(1, plugins.Count);
            var plugin = plugins.Single();

            Assert.AreEqual(PluginType.FileSystem, plugin.Type);

            Assert.AreEqual("MyNamespace", plugin.Namespace);
            Assert.AreEqual("MyNamespace.MyFsPlugin", plugin.ClassFullName);

            Assert.AreEqual(1, plugin.ImplementedMethods.Count);
            Assert.AreEqual("GetFiles", plugin.ImplementedMethods[0].MethodName);
            Assert.AreEqual("TcPluginBase.FileSystem.IFsPlugin.GetFiles(TcPluginBase.FileSystem.RemotePath)", plugin.ImplementedMethods[0].Signature);
            Assert.AreEqual("MyNamespace.MyFsPlugin", plugin.ImplementedMethods[0].ContainingType);

            Assert.AreEqual(PluginType.FileSystem, plugin.Definition.Type);
            Assert.AreEqual("IFsPlugin", plugin.Definition.Name);
            Assert.AreEqual(23, plugin.Definition.Methods.Count);

            var getFilesMethod = plugin.Definition.Methods[plugin.ImplementedMethods[0].Signature];

            Assert.AreEqual("FsFindFirst,FsFindFirstW,FsFindNext,FsFindNext,FsFindClose", string.Join(",", getFilesMethod.WrapperData.MethodNames));


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
