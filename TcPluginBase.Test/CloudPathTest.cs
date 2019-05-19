using System;
using TcPluginBase.FileSystem;
using Xunit;


namespace TcPluginBase.Test {
    public class RemotePathTest {
        [Fact]
        public void Converts_Backslash()
        {
            var actual = (RemotePath) "/Test/Folder/Path/File.txt";
            var expected = @"\Test\Folder\Path\File.txt";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Keep_TrailingSlash()
        {
            RemotePath path = "/some/folder/";
            Assert.Equal(@"\some\folder\", (string) path);

            RemotePath path2 = @"\some\folder\";
            Assert.Equal(@"\some\folder\", (string) path2);
        }

        [Fact]
        public void FileName()
        {
            RemotePath path = "/MyAccount/MyContainer/Folder1/Folder2/File.txt";
            var actual = path.FileName;
            var expected = "File.txt";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FileNameWithoutExtension()
        {
            RemotePath path = "/MyAccount/MyContainer/Folder1/Folder2/File.txt";
            var actual = path.FileNameWithoutExtension;
            var expected = "File";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Extension()
        {
            RemotePath path = "/MyAccount/MyContainer/Folder1/Folder2/File.txt";
            var actual = path.Extension;
            var expected = ".txt";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Directory()
        {
            RemotePath path = @"\MyAccount\MyContainer\Folder1\Folder2\File.txt";

            path = path.Directory;
            Assert.Equal(@"\MyAccount\MyContainer\Folder1\Folder2", (string) path);

            path = path.Directory;
            Assert.Equal(@"\MyAccount\MyContainer\Folder1", (string) path);

            path = path.Directory;
            Assert.Equal(@"\MyAccount\MyContainer", (string) path);

            path = path.Directory;
            Assert.Equal(@"\MyAccount", (string) path);

            path = path.Directory;
            Assert.Equal(@"\", (string) path);
        }

        [Fact]
        public void Level()
        {
            var level0 = (RemotePath) @"\";
            var level1 = (RemotePath) @"\segment";
            var level2 = (RemotePath) @"\segment\segment";

            Assert.Equal(0, level0.Level);
            Assert.Equal(1, level1.Level);
            Assert.Equal(2, level2.Level);
        }

        [Fact]
        public void GetSegment()
        {
            RemotePath path = "/segment1/segment2/segment3/segment4";

            Assert.Null(path.GetSegment(0));
            Assert.Equal("segment1", path.GetSegment(1));
            Assert.Equal("segment2", path.GetSegment(2));
            Assert.Equal("segment3", path.GetSegment(3));
            Assert.Equal("segment4", path.GetSegment(4));
        }

        [Fact]
        public void Implicit_Add_SubPath()
        {
            RemotePath path = @"\MyAccount\MyContainer\Folder1\";
            var actual = path + ((RemotePath) @"\subPath\file.exe");
            var expected = @"\MyAccount\MyContainer\Folder1\subPath\file.exe";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Implicit_Prefix_String()
        {
            RemotePath path = @"\MyAccount\MyContainer\Folder1";
            var actual = "/hoi" + path;
            var expected = @"\hoi\MyAccount\MyContainer\Folder1";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Implicit_Postfix_String()
        {
            RemotePath path = @"\MyAccount\MyContainer\Folder1";
            var actual = path + @"-file.exe";
            var expected = @"\MyAccount\MyContainer\Folder1-file.exe";
            Assert.Equal(expected, actual);
        }
    }
}
