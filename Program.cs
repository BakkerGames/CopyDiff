using System;
using System.IO;
using System.Linq;

namespace CopyDiff
{
    class Program
    {
        static int foundCount = 0;

        static int Main(string[] args)
        {
            string fromPath = "";
            string toPath = "";
            if (args is null || args.Count() < 2)
            {
                Console.WriteLine("Syntax: <from_path> <to_path>");
                return 1;
            }
            for (int i = 0; i < args.Count(); i++)
            {
                if (args[i].StartsWith("/") || args[i].StartsWith("--"))
                {
                    Console.WriteLine($"Argument not recognized: {args[i]}");
                    return 1;
                }
                if (fromPath.Length == 0)
                {
                    fromPath = args[i];
                }
                else if (toPath.Length == 0)
                {
                    toPath = args[i];
                }
            }
            if (!Directory.Exists(fromPath))
            {
                Console.WriteLine($"Path not found: {fromPath}");
                return 1;
            }
            if (!Directory.Exists(toPath))
            {
                try
                {
                    Directory.CreateDirectory(toPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {toPath}\r\n{ex.Message}");
                    return 1;
                }
            }
            try
            {
                Console.WriteLine($"Copying from \"{fromPath}\" to \"{toPath}\"");
                int copyCount = CopyAll(fromPath, toPath);
                Console.WriteLine($"Files copied: {copyCount}");
            }
            catch (Exception)
            {
                return 2;
            }
            return 0;
        }

        private static int CopyAll(string fromPath, string toPath)
        {
            int copyCount = 0;
            try
            {
                if (!Directory.Exists(toPath))
                {
                    Directory.CreateDirectory(toPath);
                }
                foreach (string filename in Directory.GetFiles(fromPath))
                {
                    string fileBaseName = filename.Substring(filename.LastIndexOf('\\') + 1);
                    if (InvalidFilename(fileBaseName)) continue;
                    foundCount++;
                    Console.Write($"{foundCount}\r");
                    if (CopySingle(fileBaseName, fromPath, toPath))
                    {
                        copyCount++;
                    }
                }
                foreach (string dirName in Directory.GetDirectories(fromPath))
                {
                    string dirBaseName = dirName.Substring(dirName.LastIndexOf('\\') + 1);
                    if (InvalidFolder(dirBaseName)) continue;
                    copyCount += CopyAll(Path.Combine(fromPath, dirBaseName), Path.Combine(toPath, dirBaseName));
                }
                return copyCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool InvalidFilename(string fileBaseName)
        {
            if (fileBaseName.StartsWith(".")) return true;
            return false;
        }

        private static bool InvalidFolder(string dirBaseName)
        {
            if (dirBaseName.StartsWith(".")) return true;
            return false;
        }

        private static bool CopySingle(string filename, string fromPath, string toPath)
        {
            try
            {
                string fromFullPath = Path.Combine(fromPath, filename);
                string toFullPath = Path.Combine(toPath, filename);
                FileInfo fromFI = new FileInfo(fromFullPath);
                FileInfo toFI = new FileInfo(toFullPath);
                if ((fromFI.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return false;
                }
                if ((fromFI.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    return false;
                }
                if (!File.Exists(toFullPath))
                {
                    Console.WriteLine(filename);
                    File.Copy(fromFullPath, toFullPath);
                    return true;
                }
                if (fromFI.Length != toFI.Length)
                {
                    Console.WriteLine(filename);
                    File.Copy(fromFullPath, toFullPath, true);
                    return true;
                }
                string md5From = MD5Utilities.CalcMD5FromFile(fromFullPath);
                string md5To = MD5Utilities.CalcMD5FromFile(toFullPath);
                if (md5From != md5To)
                {
                    Console.WriteLine(filename);
                    File.Copy(fromFullPath, toFullPath, true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {filename}\r\n{ex.Message}");
                throw;
            }
        }
    }
}
