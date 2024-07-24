using System;
using System.IO;
using System.Linq;

namespace CmdTools
{
    internal static class LineOfCodeCounter
    {
        public static string CountLineOfCodes(string sourceFolder)
        {
            var folders = Directory
                .GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(".cs") || x.EndsWith(".xaml"))
                .Where(x => x.Contains("AssemblyInfo") == false && x.Contains("\\obj\\") == false)
                .Select(x => CreateRelativePath(x, sourceFolder))
                .Select(x => new FileEntry(x))
                .GroupBy(x => x.FolderName)
                .Select(x => new FolderEntry(x.Key, sourceFolder, x.ToArray()))
                .ToArray();

            int maxLength = folders.Select(x => x.FolderName.Length).Max() + 1;
            var lines = folders.Select(x => x.FolderName.PadRight(maxLength) + x.LineOfCodes).ToList();

            string result = string.Join("\n", lines);
            result += "\n" + new string('-', lines.Max(x => x.Length));
            result += "\n" + new string(' ', maxLength) + folders.Sum(x => x.LineOfCodes);
            return result;
        }

        class FolderEntry
        {
            public string FolderName { get; }
            public int LineOfCodes { get; }
            public FolderEntry(string folderName, string sourceFolder, FileEntry[] files)
            {
                FolderName = folderName;
                LineOfCodes = files.Select(x => x.Path).Sum(y => File.ReadAllLines(sourceFolder + y).Length);
            }
        }

        class FileEntry
        {
            public string FolderName { get; }
            public string Path { get; }

            public FileEntry(string path)
            {
                int i = path.IndexOf('/');
                this.FolderName = path.Substring(0, i);
                this.Path = path;
            }

            public override string ToString()
            {
                return Path;
            }
        }


        //fullName wird als relativer Pfad von der baseFolder-Position aus angegeben
        private static string CreateRelativePath(string fullName, string baseFolder)
        {
            Uri uri1 = new Uri(new FileInfo(fullName).FullName);
            Uri uri2 = new Uri(new FileInfo(baseFolder).FullName);
            string relativeFolder = uri2.MakeRelativeUri(uri1).ToString();
            return relativeFolder;
        }
    }
}
