﻿using System;
using System.IO;
using System.Linq;

namespace CopyDiff
{
    public class Program
    {
        static int Main(string[] args)
        {
            string sourceDir = "";
            string targetDir = "";
            Options _options = new Options();
            if (args is null || args.Count() < 2)
            {
                Console.WriteLine("Syntax: <from_path> <to_path> [options]");
                Console.WriteLine("-v, --verbose - full file path");
                Console.WriteLine("-x, --exclude <dir[,dir...]> - exclude directories");
                return 1;
            }
            int index = 0;
            while (index < args.Count())
            {
                if (args[index].Equals("-v", StringComparison.OrdinalIgnoreCase) ||
                    args[index].Equals("--verbose", StringComparison.OrdinalIgnoreCase))
                {
                    _options.LongFilenames = true;
                }
                else if (args[index].Equals("-x", StringComparison.OrdinalIgnoreCase) ||
                    args[index].Equals("--exclude", StringComparison.OrdinalIgnoreCase))
                {
                    index++;
                    _options.ExcludeDirs = args[index].Split(',').ToList();
                }
                else if (string.IsNullOrEmpty(sourceDir))
                {
                    sourceDir = args[index];
                }
                else if (string.IsNullOrEmpty(targetDir))
                {
                    targetDir = args[index];
                }
                else
                {
                    Console.WriteLine($"Argument not recognized: {args[index]}");
                    return 1;
                }
                index++;
            }
            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine($"Path not found: {sourceDir}");
                return 1;
            }
            if (!Directory.Exists(targetDir))
            {
                try
                {
                    Directory.CreateDirectory(targetDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {targetDir}\r\n{ex.Message}");
                    return 1;
                }
            }
            try
            {
                Console.WriteLine($"Copying from \"{sourceDir}\" to \"{targetDir}\"");
                CopyAll(sourceDir, targetDir, _options);
                Console.WriteLine($"Files copied: {_options.CopyCount}");
            }
            catch (Exception)
            {
                return 2;
            }
            return 0;
        }

        private static void CopyAll(string sourceDir, string targetDir, Options _options)
        {
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                foreach (string filename in Directory.GetFiles(sourceDir))
                {
                    string fileBaseName = filename.Substring(filename.LastIndexOf('\\') + 1);
                    if (InvalidFilename(fileBaseName))
                        continue;
                    CopySingle(sourceDir, targetDir, fileBaseName, _options);
                }
                foreach (string dirName in Directory.GetDirectories(sourceDir))
                {
                    string dirBaseName = dirName.Substring(dirName.LastIndexOf('\\') + 1);
                    if (InvalidFolder(dirBaseName, _options))
                        continue;
                    CopyAll(Path.Combine(sourceDir, dirBaseName), Path.Combine(targetDir, dirBaseName), _options);
                }
                return;
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

        private static bool InvalidFolder(string dirBaseName, Options _options)
        {
            if (dirBaseName.StartsWith(".")) return true;
            foreach (string s in _options.ExcludeDirs)
            {
                if (s.Equals(dirBaseName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void CopySingle(string sourceDir, string targetDir, string filename, Options _options)
        {
            string fromFullPath = Path.Combine(sourceDir, filename);
            string toFullPath = Path.Combine(targetDir, filename);
            try
            {
                FileInfo fromFI = new FileInfo(fromFullPath);
                FileInfo toFI = new FileInfo(toFullPath);
                if ((fromFI.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return;
                }
                if ((fromFI.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    return;
                }
                _options.FoundCount++;
                Console.Write($"{_options.FoundCount}\r");
                if (!File.Exists(toFullPath))
                {
                    if (_options.LongFilenames)
                        Console.WriteLine(fromFullPath);
                    else
                        Console.WriteLine(filename);
                    File.Copy(fromFullPath, toFullPath);
                    _options.CopyCount++;
                    return;
                }
                if (fromFI.Length != toFI.Length)
                {
                    if (_options.LongFilenames)
                        Console.WriteLine(fromFullPath);
                    else
                        Console.WriteLine(filename);
                    File.Copy(fromFullPath, toFullPath, true);
                    _options.CopyCount++;
                    return;
                }
                string md5From = MD5Utilities.CalcMD5FromFile(fromFullPath);
                string md5To = MD5Utilities.CalcMD5FromFile(toFullPath);
                if (md5From != md5To)
                {
                    if (_options.LongFilenames)
                        Console.WriteLine(fromFullPath);
                    else
                        Console.WriteLine(filename);
                    File.Copy(fromFullPath, toFullPath, true);
                    _options.CopyCount++;
                    return;
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {fromFullPath}\r\n{ex.Message}");
                throw;
            }
        }
    }
}