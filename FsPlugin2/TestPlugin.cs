using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using TcPluginBase.FileSystem;


namespace FsPlugin2 {
    public class TestPlugin : FsPlugin {
        public TestPlugin(IConfiguration pluginSettings) : base(pluginSettings)
        {
        }

        public override IEnumerable<FindData> GetFiles(RemotePath path)
        {
            return new[] {
                new FindData("folder", FileAttributes.Directory),
                new FindData("file1.txt"),
                new FindData("file2.txt"),
                new FindData("file3.txt"),
            };
        }

        public override bool DeleteFile(RemotePath fileName)
        {
            return base.DeleteFile(fileName);
        }
    }
}
