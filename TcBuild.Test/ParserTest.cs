using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TcPluginBase;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace TcBuild.Test {
    public class ParserTest {
        private readonly ITestOutputHelper _output;

        public ParserTest(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output, ""));
            Console.SetError(new Converter(output, ""));
        }

        [Fact]
        public void Test()
        {
            //var parser = new AssemblyParser(new FileInfo(GetType().Assembly.FullName!));
            //var (pluginType, pluginClass, pluginAssemblyName) = parser.PluginInformation();

            //_output.WriteLine($"PluginAssembly: {pluginAssemblyName.FullName}");
            //_output.WriteLine($"Type:           {pluginType}");
            //_output.WriteLine($"Class:          {pluginClass}");
        }

        //[Fact]
        //public void Test2()
        //{
        //    var logger = new BuildLogger() {BuildEngine = new FakeBuildEngine(_output)};

        //    var MSBuildFrameworkToolsPath = @"C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\";
        //    var FrameworkSDKRoot = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\";

        //    var (pluginType, excludedMethods, pluginClass, pluginAssemblyName) = new Processor(logger, tools).AnalyzeAssembly(new FileInfo(GetType().Assembly.Location));

        //    _output.WriteLine("-----------------------------");

        //    _output.WriteLine($"PluginAssembly: {pluginAssemblyName.FullName}");
        //    _output.WriteLine($"Type:           {pluginType}");
        //    _output.WriteLine($"Class:          {pluginClass}");
        //    _output.WriteLine($"ExcludedMethods:");
        //    foreach (var method in excludedMethods) {
        //        _output.WriteLine($"    {method}");
        //    }

        //    var contentPluginMethods = new[] {
        //        "FsContentGetDefaultSortOrder",
        //        "FsContentGetDefaultView",
        //        "FsContentGetDefaultViewW",
        //        "FsContentGetSupportedField",
        //        "FsContentGetSupportedFieldFlags",
        //        "FsContentGetValue",
        //        "FsContentGetValueW",
        //        "FsContentPluginUnloading",
        //        "FsContentSetValue",
        //        "FsContentSetValueW",
        //        "FsContentStopGetValue",
        //        "FsContentStopGetValueW"
        //    };

        //    Assert.Equal(PluginType.FileSystem, pluginType);

        //    // ensure all ContentPluginMethods are removed
        //    Assert.True(excludedMethods.ContainsAll(contentPluginMethods));
        //}
    }


    public static class AssertEx {
        public static bool ContainsAll<T>(this IEnumerable<T> haystack, IEnumerable<T> needles, IEqualityComparer<T> comparer = null)
        {
            var haystackArray = haystack.ToArray();
            var needlesArray = needles.ToArray();
            if (needlesArray.Intersect(haystackArray, comparer).Count() == needlesArray.Length) {
                return true;
            }

            throw new ContainsException(haystackArray.OrderBy(_ => _), needlesArray.OrderBy(_ => _));
        }
    }
}
