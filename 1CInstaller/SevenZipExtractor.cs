using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace _1CInstaller
{
    public static class SevenZipExtractor
    {
        public static void ExtractArchive(string archivePath, string destinationFolder)
        {
            string sevenZipPath = SevenZipChecker.SevenZipPath;

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"x \"{archivePath}\" -o\"{destinationFolder}\" -y",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
            }
        }

        public static void RunSetup(string destinationFolder, RichTextBox richTextBox)
        {
            string setupPath = Path.Combine(destinationFolder, "setup.exe");

            if (File.Exists(setupPath))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = setupPath,
                    UseShellExecute = true
                };

                using (Process process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Form1.AddMessageToRichTextBox(richTextBox, "Установка успешно завершена.");
                    }
                    else
                    {
                        Form1.AddMessageToRichTextBox(richTextBox, "Установка отменена пользователем.");
                    }
                }
            }
            else
            {
                Form1.AddMessageToRichTextBox(richTextBox, "setup.exe не найден в каталоге разархивации.");
            }
        }
    }
}
