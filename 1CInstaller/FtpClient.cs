using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public class FtpClient : IFtpClient
    {
        private readonly string ftpServer;
        private readonly string username;
        private readonly string password;

        public FtpClient(string ftpServer, string username, string password)
        {
            this.ftpServer = ftpServer;
            this.username = username;
            this.password = password;
        }

        public async Task<long> GetFileSizeAsync(string ftpFilePath, RichTextBox output)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(username, password);

            try
            {
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    long fileSize = response.ContentLength;
                    Form1.AddMessageToRichTextBox(output, $"Размер файла: {fileSize} байт");
                    return fileSize;
                }
            }
            catch (Exception ex)
            {
                Form1.AddMessageToRichTextBox(output, "Ошибка при получении размера файла: " + ex.Message);
                return -1;
            }
        }

        public async Task<List<string>> GetFileList(string directoryPath, string version, Label progressLabel, RichTextBox output)
        {
            List<string> files = new List<string>();
            string ftpDirectoryPath = ftpServer + directoryPath;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpDirectoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        string filename = reader.ReadLine();
                        if (filename.Contains(version))
                        {
                            files.Add(filename);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Form1.AddMessageToRichTextBox(output, "Ошибка при получении списка файлов: " + ex.Message);
            }

            return files;
        }

        public async Task<bool> DownloadFileAsync(string ftpFilePath, string localFilePath, Label progressLabel, RichTextBox output, CancellationToken cancellationToken)
        {
            long fileSize = await GetFileSizeAsync(ftpFilePath, output);

            if (fileSize <= 0)
            {
                return false;
            }

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    byte[] buffer = new byte[2048];
                    int bytesRead;
                    long totalRead = 0;

                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Form1.AddMessageToLabel(progressLabel, string.Empty);
                            return false;
                        }

                        fileStream.Write(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        int progressPercentage = (int)((double)totalRead * 100 / fileSize);
                        Form1.AddMessageToLabel(progressLabel, $"Скачивание... {progressPercentage}% завершено");
                    }
                }

                Form1.AddMessageToLabel(progressLabel, "Скачивание завершено успешно.");
                return true;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // Убедимся, что сообщение об отмене выводится только один раз.
                    Form1.AddMessageToLabel(progressLabel, string.Empty);
                }
                else
                {
                    Form1.AddMessageToRichTextBox(output, "Ошибка скачивания: " + ex.Message);
                    Form1.AddMessageToLabel(progressLabel, string.Empty);
                }
                return false;
            }
        }




    }
}
