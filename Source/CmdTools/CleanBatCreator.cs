using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CmdTools
{
    internal static class CleanBatCreator
    {
        public static void CreateCleanFile(string projectFolder, string outputFolder)
        {
            if (projectFolder.Length > 3 && projectFolder.EndsWith("\\") == false) projectFolder += "\\";
            if (outputFolder.Length > 3 && outputFolder.EndsWith("\\") == false) outputFolder += "\\";

            List<string> commands = new List<string>();

            string sourceFolder = CreateRelativePath(projectFolder, outputFolder);
            if (sourceFolder != "")
            {
                commands.Add($"cd {sourceFolder}");
            }

            string outputFolderFullPath = Path.GetFullPath(sourceFolder, new FileInfo(outputFolder).FullName);

            System.IO.DirectoryInfo di = new DirectoryInfo(projectFolder);
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                if (dir.FullName.EndsWith(".vs"))
                {
                    string path = CreateRelativePath(dir.FullName, outputFolderFullPath);
                    commands.Add($"cd {path} && rmdir /S /Q . 2>nul & cd ..");
                }
                else
                {
                    var folder = Directory
                .GetDirectories(dir.FullName)
                .Where(x => x.EndsWith("bin") || x.EndsWith("obj"))
                .ToList();

                    if (folder.Count == 2)
                    {
                        var removeCmds = folder
                            .Select(x => CreateRelativePath(x, outputFolderFullPath))
                            .Select(x => $"cd {x} && rmdir /S /Q . 2>nul & cd ../..")
                            .ToList();
                        commands.AddRange(removeCmds);
                    }
                }

            }

            string result = string.Join("\n", commands);
            File.WriteAllText(outputFolder + "Clean.bat", result);
        }

        //folderFullName wird als relativer Pfad von der outputFolder-Position aus angegeben
        private static string CreateRelativePath(string folderFullName, string outputFolder)
        {
            Uri uri1 = new Uri(new FileInfo(folderFullName).FullName);
            Uri uri2 = new Uri(new FileInfo(outputFolder).FullName);
            string relativeFolder = uri2.MakeRelativeUri(uri1).ToString();
            return relativeFolder;
        }
    }
}
