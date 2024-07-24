using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMachine.Model
{
    static class FileNameHelper
    {
        public static string DataDirectory = @"\..\..\..\..\Data\";

        public static string GetPathRelativeToCurrentDirectory(string absolutFileName)
        {
            var relativePathFromCurrentDirectoryToFileName = new Uri(Directory.GetCurrentDirectory() + "\\").MakeRelativeUri(new Uri(absolutFileName)).ToString();
            return Path.GetDirectoryName(relativePathFromCurrentDirectoryToFileName) + "\\" + Path.GetFileName(absolutFileName);
        }

        public static string GetFileName(string absolutFileName)
        {
            return Path.GetFileName(absolutFileName);
        }
    }
}
