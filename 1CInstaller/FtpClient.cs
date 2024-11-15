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
        /*private readonly string ftpServer;
        private readonly string username;
        private readonly string password;*/
        public string ServerAddress { get; set; } // Адрес сервера
        public string Username { get; set; } // Имя пользователя
        public string Password { get; set; } // Пароль

        public FtpClient(string serverAddress, string username, string password)
        {
            ServerAddress = serverAddress;
            Username = username;
            Password = password;
        }
        private FtpWebRequest CreateFtpWebRequest(string url, string method)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Credentials = new NetworkCredential(Username, Password);
            return request;
        }

        /*public async Task<long> GetFileSizeAsync(string ftpFilePath, RichTextBox output)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(Username, Password);

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
        }*/
        public async Task<long> GetFileSizeAsync(string ftpFilePath, RichTextBox output)
        {
            FtpWebRequest request = CreateFtpWebRequest(ftpFilePath, WebRequestMethods.Ftp.GetFileSize);

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
        /*public async Task<List<string>> GetFileList(string directoryPath, string version, Label progressLabel, RichTextBox output)
        {
            List<string> files = new List<string>();
            string ftpDirectoryPath = ServerAddress;// + directoryPath;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpDirectoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(Username, Password);
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
        }*/
        public async Task<List<string>> GetFileList(string directoryPath, string version, Label progressLabel, RichTextBox output)
        {
            List<string> files = new List<string>();
            string ftpDirectoryPath = ServerAddress;

            try
            {
                FtpWebRequest request = CreateFtpWebRequest(ftpDirectoryPath, WebRequestMethods.Ftp.ListDirectory);

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
                request.Credentials = new NetworkCredential(Username, Password);

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
        public async Task<bool> CheckConnectionAsync(RichTextBox output)
        {
            try
            {
                string ftpServer = ServerAddress;
                if (!ftpServer.EndsWith("/"))
                {
                    ftpServer += "/";
                }

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServer);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(Username, Password);

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == FtpStatusCode.OpeningData || response.StatusCode == FtpStatusCode.DataAlreadyOpen || response.StatusCode == FtpStatusCode.ClosingData)
                    {
                        Form1.AddMessageToRichTextBox(output, "Соединение с FTP-сервером успешно установлено.");
                        return true;
                    }
                    else
                    {
                        Form1.AddMessageToRichTextBox(output, $"Не удалось установить соединение с FTP-сервером: {response.StatusDescription}");
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse response)
                {
                    Form1.AddMessageToRichTextBox(output, $"Ошибка при проверке соединения с FTP: {response.StatusDescription}");
                }
                else
                {
                    Form1.AddMessageToRichTextBox(output, $"Ошибка при проверке соединения с FTP: {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Form1.AddMessageToRichTextBox(output, $"Неизвестная ошибка при проверке соединения с FTP: {ex.Message}");
                return false;
            }
        }
    }
}
