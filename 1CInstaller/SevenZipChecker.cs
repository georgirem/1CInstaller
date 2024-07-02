using System;
using System.IO;

namespace _1CInstaller
{
    public static class SevenZipChecker
    {
        public static string SevenZipPath { get; private set; }

        public static bool Is7ZipInstalled(out string debugInfo)
        {
            string[] possiblePaths = new[]
            {
                Path.Combine("C:\\Program Files\\7-Zip", "7z.exe"),
                Path.Combine("C:\\Program Files (x86)\\7-Zip", "7z.exe")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    SevenZipPath = path;
                    debugInfo = $"7-Zip найден по пути: {path}";
                    return true;
                }
            }

            debugInfo = "7-Zip не найден. Проверяем пути: " + string.Join(", ", possiblePaths);
            return false;
        }
    }
}
