using System;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;

namespace DotNetZipExploration
{
    public class MyZipTreeBuilder
    {
        public static MyZipDirectory BuildTree(ZipFile zipFile)
        {
            var subDirectoriesToFiles = new Dictionary<Tuple<string, ZipEntry>, IList<ZipEntry>>();

            var fileEntries = zipFile.EntriesSorted.Where(e => !e.IsDirectory);
            foreach (var fileEntry in fileEntries)
            {
                var lastSlash = fileEntry.FileName.LastIndexOf('/');
                var subDirectory = lastSlash >= 0 ? fileEntry.FileName.Substring(0, lastSlash) : string.Empty;

                var subDirectoryWithTrailingSlash = subDirectory + "/";
                var existingDirectoryZipEntry = subDirectoriesToFiles.Keys.FirstOrDefault(k => k.Item1 == subDirectory);
                if (existingDirectoryZipEntry != null)
                {
                    subDirectoriesToFiles[existingDirectoryZipEntry].Add(fileEntry);
                }
                else
                {
                    var directoryZipEntry = FindDirectoryZipEntry(zipFile, subDirectoryWithTrailingSlash);
                    subDirectoriesToFiles.Add(Tuple.Create(subDirectory, directoryZipEntry), new List<ZipEntry> { fileEntry });
                }
            }

            var root = new MyZipDirectory(string.Empty, null);

            foreach (var kvp in subDirectoriesToFiles)
            {
                var treeNode = FindTreeNodeForKey(root, kvp.Key);
                foreach (var fileEntry in kvp.Value)
                {
                    treeNode.Files.Add(fileEntry);
                }
            }

            return root;
        }

        private static ZipEntry FindDirectoryZipEntry(ZipFile zipFile, string subDirectoryWithTrailingSlash)
        {
            return zipFile.EntriesSorted.FirstOrDefault(ze => ze.IsDirectory && ze.FileName == subDirectoryWithTrailingSlash);
        }

        private static MyZipDirectory FindTreeNodeForKey(MyZipDirectory root, Tuple<string, ZipEntry> key)
        {
            var fullDirectoryPath = key.Item1;
            var directoryZipEntry = key.Item2;
            var directoryNames = fullDirectoryPath.Split('/');

            var treeNodeForPreviousLevel = root;
            var fullDirectoryPathSoFar = string.Empty;

            foreach (var directoryName in directoryNames)
            {
                if (fullDirectoryPathSoFar.Length > 0)
                {
                    fullDirectoryPathSoFar += "/";
                }
                fullDirectoryPathSoFar += directoryName;

                var treeNodeForThisLevel = FindTreeNodeForDirectory(root, directoryName);
                if (treeNodeForThisLevel != null)
                {
                    treeNodeForPreviousLevel = treeNodeForThisLevel;
                }
                else
                {
                    var newSubDirectory = new MyZipDirectory(fullDirectoryPathSoFar, directoryZipEntry);
                    if (treeNodeForPreviousLevel != null)
                    {
                        treeNodeForPreviousLevel.SubDirectories.Add(newSubDirectory);
                        treeNodeForPreviousLevel = newSubDirectory;
                    }
                }
            }

            return treeNodeForPreviousLevel;
        }

        private static MyZipDirectory FindTreeNodeForDirectory(MyZipDirectory directory, string directoryName)
        {
            var lastDirectoryNameComponent = directory.DirectoryName.Split('/').LastOrDefault();
            if (lastDirectoryNameComponent != null && lastDirectoryNameComponent == directoryName)
            {
                return directory;
            }

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var subDirectory in directory.SubDirectories)
            {
                var result = FindTreeNodeForDirectory(subDirectory, directoryName);

                if (result != null)
                {
                    return result;
                }
            }
            // ReSharper restore LoopCanBeConvertedToQuery

            return null;
        }

        public static IList<string> FindLeafDirectories(MyZipDirectory directory)
        {
            var leafDirectories = new List<string>();
            FindLeafDirectories(leafDirectories, directory);
            return leafDirectories;
        }

        private static void FindLeafDirectories(ICollection<string> leafDirectories, MyZipDirectory directory)
        {
            if (directory.SubDirectories.Count == 0)
            {
                leafDirectories.Add(directory.DirectoryName);
                return;
            }

            foreach (var subDirectory in directory.SubDirectories)
            {
                FindLeafDirectories(leafDirectories, subDirectory);
            }
        }
    }
}
