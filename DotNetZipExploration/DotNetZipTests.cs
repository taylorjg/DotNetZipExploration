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
            var zipFileStream = ReadZipFile("OneFile.zip");

            // Act
            var zipFile = ZipFile.Read(zipFileStream);

            // Assert
            Assert.That(zipFile.EntryFileNames.Count, Is.EqualTo(1));
            Assert.That(zipFile.EntryFileNames.ElementAt(0), Is.EqualTo("File1.txt"));
            Assert.That(zipFile.Entries.Count, Is.EqualTo(1));
            Assert.That(zipFile[0].FileName, Is.EqualTo("File1.txt"));
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingThreeFiles_WeCanInspectTheContentsOfTheZipFile()
        {
            // Arrange
            var zipFileStream = ReadZipFile("ThreeFiles.zip");

            // Act
            var zipFile = ZipFile.Read(zipFileStream);

            // Assert
            Assert.That(zipFile.EntryFileNames.Count, Is.EqualTo(3));
            Assert.That(zipFile.EntryFileNames.Contains("File1.txt"), Is.True);
            Assert.That(zipFile.EntryFileNames.Contains("File2.txt"), Is.True);
            Assert.That(zipFile.EntryFileNames.Contains("File3.txt"), Is.True);
            Assert.That(zipFile.Entries.Count, Is.EqualTo(3));
        }

        [Test]
        public void GivenAStreamContainingAZipFileContainingASingleFile_WeCanExtractTheFileIntoAMemoryStream()
        {
            // Arrange
            var zipFileStream = ReadZipFile("OneFile.zip");
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

        [Test]
        public void GivenAStreamContainingAZipFileContainingThreeFiles_WeCanExtractEachFileIntoAMemoryStream()
        {
            // Arrange
            var zipFileStream = ReadZipFile("ThreeFiles.zip");
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

        private static Stream ReadZipFile(string fileName)
        {
            var fullPath = Path.Combine("TestFiles", fileName);
            var fileStream = File.OpenRead(fullPath);
            return fileStream;
        }
    }
}
