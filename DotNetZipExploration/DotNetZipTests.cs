using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using NUnit.Framework;

namespace DotNetZipExploration
{
    // ReSharper disable InconsistentNaming

    [TestFixture]
    internal class DotNetZipTests
    {
        [Test]
        public void GivenAStreamContainingAZipFileContainingASingleFile_WeCanInspectTheContentsOfTheZipFile()
        {
            // Arrange
            using (var zipFileStream = ReadZipFile("OneFile.zip"))
            {
                // Act
                var zipFile = ZipFile.Read(zipFileStream);

                // Assert
                Assert.That(zipFile.EntryFileNames.Count, Is.EqualTo(1));
                Assert.That(zipFile.EntryFileNames.ElementAt(0), Is.EqualTo("File1.txt"));
                Assert.That(zipFile.Entries.Count, Is.EqualTo(1));
                Assert.That(zipFile[0].FileName, Is.EqualTo("File1.txt"));
            }
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingThreeFiles_WeCanInspectTheContentsOfTheZipFile()
        {
            // Arrange
            using (var zipFileStream = ReadZipFile("ThreeFiles.zip"))
            {
                // Act
                var zipFile = ZipFile.Read(zipFileStream);

                // Assert
                Assert.That(zipFile.EntryFileNames.Count, Is.EqualTo(3));
                Assert.That(zipFile.EntryFileNames.Contains("File1.txt"), Is.True);
                Assert.That(zipFile.EntryFileNames.Contains("File2.txt"), Is.True);
                Assert.That(zipFile.EntryFileNames.Contains("File3.txt"), Is.True);
                Assert.That(zipFile.Entries.Count, Is.EqualTo(3));
            }
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingASingleFile_WeCanExtractTheFileIntoAMemoryStream()
        {
            // Arrange
            using (var zipFileStream = ReadZipFile("OneFile.zip"))
            {
                var zipFile = ZipFile.Read(zipFileStream);

                // Act
                var zipEntry = zipFile[0];
                string fileContents;
                using (var memoryStream = new MemoryStream())
                {
                    zipEntry.Extract(memoryStream);
                    var bytes = memoryStream.ToArray();
                    fileContents = Encoding.UTF8.GetString(bytes);
                }

                // Assert
                Assert.That(fileContents, Is.EqualTo("File1.txt"));
            }
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingThreeFiles_WeCanExtractEachFileIntoAMemoryStream()
        {
            // Arrange
            using (var zipFileStream = ReadZipFile("ThreeFiles.zip"))
            {
                var zipFile = ZipFile.Read(zipFileStream);

                // Act
                var fileNamesToFileContents = new Dictionary<string, string>();
                foreach (var zipEntry in zipFile)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        zipEntry.Extract(memoryStream);
                        var bytes = memoryStream.ToArray();
                        var fileContents = Encoding.UTF8.GetString(bytes);
                        fileNamesToFileContents[zipEntry.FileName] = fileContents;
                    }
                }

                // Assert
                Assert.That(fileNamesToFileContents.Count(), Is.EqualTo(3));
                Assert.That(fileNamesToFileContents["File1.txt"], Is.EqualTo("File1.txt"));
                Assert.That(fileNamesToFileContents["File2.txt"], Is.EqualTo("File2.txt"));
                Assert.That(fileNamesToFileContents["File3.txt"], Is.EqualTo("File3.txt"));
            }
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingAHierarchyOfFilesAndSubDirectories_WeCanInspectTheContentsOfTheZipFile()
        {
            // Arrange
            using (var zipFileStream = ReadZipFile("Hierarchy.zip"))
            {
                // Act
                var zipFile = ZipFile.Read(zipFileStream);

                // Assert
                Assert.That(zipFile.EntriesSorted.Count(e => e.IsDirectory), Is.GreaterThan(0));
                Assert.That(zipFile.EntriesSorted.Count(e => !e.IsDirectory), Is.GreaterThan(0));
            }
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingAHierarchyOfFilesAndSubDirectories_WeCanCreateADictionaryOfSubDirectoriesToFiles()
        {
            // Arrange
            using (var zipFileStream = ReadZipFile("Hierarchy.zip"))
            {
                // Act
                var zipFile = ZipFile.Read(zipFileStream);
                var subDirectoriesToFiles = new Dictionary<string, IList<string>>();
                var fileEntries = zipFile.EntriesSorted.Where(e => !e.IsDirectory);
                foreach (var fileEntry in fileEntries)
                {
                    var lastSlash = fileEntry.FileName.LastIndexOf('/');
                    var subDirectory = lastSlash >= 0 ? fileEntry.FileName.Substring(0, lastSlash) : string.Empty;
                    var fileName = lastSlash >= 0 ? fileEntry.FileName.Substring(lastSlash + 1) : fileEntry.FileName;
                    if (subDirectoriesToFiles.ContainsKey(subDirectory))
                    {
                        subDirectoriesToFiles[subDirectory].Add(fileName);
                    }
                    else
                    {
                        subDirectoriesToFiles.Add(subDirectory, new List<string> {fileName});
                    }
                }

                // Assert
                Assert.That(subDirectoriesToFiles.ContainsKey(""), Is.True);
                Assert.That(subDirectoriesToFiles[""].Contains("File1.txt"), Is.True);
                Assert.That(subDirectoriesToFiles[""].Contains("File2.txt"), Is.True);
                Assert.That(subDirectoriesToFiles[""].Contains("File3.txt"), Is.True);

                Assert.That(subDirectoriesToFiles.ContainsKey("SubDirectoryA"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryA"].Contains("SubDirectoryA.txt"), Is.True);

                Assert.That(subDirectoriesToFiles.ContainsKey("SubDirectoryA/SubDirectoryA-1"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryA/SubDirectoryA-1"].Contains("SubDirectoryA-1_File1.txt"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryA/SubDirectoryA-1"].Contains("SubDirectoryA-1_File2.txt"), Is.True);

                Assert.That(subDirectoriesToFiles.ContainsKey("SubDirectoryA/SubDirectoryA-2"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryA/SubDirectoryA-2"].Contains("SubDirectoryA-2_File1.txt"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryA/SubDirectoryA-2"].Contains("SubDirectoryA-2_File2.txt"), Is.True);

                Assert.That(subDirectoriesToFiles.ContainsKey("SubDirectoryB"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryB"].Contains("SubDirectoryB.txt"), Is.True);

                Assert.That(subDirectoriesToFiles.ContainsKey("SubDirectoryB/SubDirectoryB-1"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryB/SubDirectoryB-1"].Contains("SubDirectoryB-1_File1.txt"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryB/SubDirectoryB-1"].Contains("SubDirectoryB-1_File2.txt"), Is.True);

                Assert.That(subDirectoriesToFiles.ContainsKey("SubDirectoryB/SubDirectoryB-2"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryB/SubDirectoryB-2"].Contains("SubDirectoryB-2_File1.txt"), Is.True);
                Assert.That(subDirectoriesToFiles["SubDirectoryB/SubDirectoryB-2"].Contains("SubDirectoryB-2_File2.txt"), Is.True);
            }
        }

        private static Stream ReadZipFile(string fileName)
        {
            var fullPath = Path.Combine("TestFiles", fileName);
            var fileStream = File.OpenRead(fullPath);
            return fileStream;
        }
    }
}
