using System.Collections.Generic;
using Ionic.Zip;

namespace DotNetZipExploration
{
    public class MyZipDirectory
    {
        public MyZipDirectory(string directoryName, ZipEntry zipEntry)
        {
            DirectoryName = directoryName;
            ZipEntry = zipEntry;
            SubDirectories = new List<MyZipDirectory>();
            Files = new List<ZipEntry>();
        }

        public string DirectoryName { get; private set; }
        public ZipEntry ZipEntry { get; private set; }
        public IList<MyZipDirectory> SubDirectories { get; private set; }
        public IList<ZipEntry> Files { get; private set; }
    }
}
