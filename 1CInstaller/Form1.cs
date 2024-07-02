using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public partial class Form1 : Form
    {
        
        private IFtpClient ftpClient;
        private string installerPath;
        private string latestFtpVersion;
        private List<string> ftpFileList;
        private CancellationTokenSource cancellationTokenSource;
        private PingChecker pingChecker;

        public Form1()
        {
            InitializeComponent();
            InitializeInstallerPath();
            
            ftpClient = new FtpClient("ftp://ftp.in-grp.net", "anonymous", "1@1");
            login1C.Visible = false;
            pingChecker = new PingChecker("ftp.in-grp.net", richTextBoxVersions);
        }

        private void InitializeInstallerPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            installerPath = Path.Combine(appDataPath, "InstanceInstaller");
            if (!Directory.Exists(installerPath))
            {
                Directory.CreateDirectory(installerPath);
            }
        }
        private void ClearDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
        }
        private async void Update_Click(object sender, EventArgs e)
        {
            if (Update.Text == "Остановить")
            {
                cancellationTokenSource.Cancel();
                Update.Text = "Обновить";
                downloadStatusLabel.Text = string.Empty;
                return;
            }

            richTextBoxVersions.Clear();
            Update.Text = "Остановить";
            cancellationTokenSource = new CancellationTokenSource();

            if (!SevenZipChecker.Is7ZipInstalled(out string debugInfo))
            {
                AddMessageToRichTextBox(richTextBoxVersions, "7-Zip не установлен. Пожалуйста, установите 7-Zip.");
                AddMessageToRichTextBox(richTextBoxVersions, debugInfo);
                Update.Text = "Обновить";
                return;
            }

            if (!await FetchFtpDataAsync() || ftpFileList.Count == 0)
            {
                AddMessageToRichTextBox(richTextBoxVersions, "Ошибка: файлы не найдены на сервере.");
                Update.Text = "Обновить";
                return;
            }

            var installedVersions = GetAllInstalledVersions();
            if (!installedVersions.Contains(latestFtpVersion))
            {
                AddMessageToRichTextBox(richTextBoxVersions, $"Необходимо установить версию с FTP: {latestFtpVersion}");

                // Очистка каталога перед скачиванием
                ClearDirectory(installerPath);

                string fileToDownload = ftpFileList.FirstOrDefault();
                if (string.IsNullOrEmpty(fileToDownload))
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Ошибка: подходящий файл не найден.");
                    Update.Text = "Обновить";
                    return;
                }

                string ftpFilePath = "ftp://ftp.in-grp.net/installers/" + fileToDownload;
                string localFilePath = Path.Combine(installerPath, fileToDownload);

                bool downloadSuccess = await ftpClient.DownloadFileAsync(ftpFilePath, localFilePath, downloadStatusLabel, richTextBoxVersions, cancellationTokenSource.Token);

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Скачивание отменено пользователем.");
                }
                else if (downloadSuccess)
                {
                    try
                    {
                        string destinationFolder = Path.Combine(installerPath, Path.GetFileNameWithoutExtension(fileToDownload));
                        SevenZipExtractor.ExtractArchive(localFilePath, destinationFolder);
                        AddMessageToRichTextBox(richTextBoxVersions, $"Файл {fileToDownload} успешно разархивирован в {destinationFolder}.");
                        SevenZipExtractor.RunSetup(destinationFolder, richTextBoxVersions);
                    }
                    catch (Exception ex)
                    {
                        AddMessageToRichTextBox(richTextBoxVersions, "Ошибка разархивации или запуска setup.exe: " + ex.Message);
                    }
                }
                else
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Скачивание не удалось. Разархивация и установка отменены.");
                }
            }
            else
            {
                AddMessageToRichTextBox(richTextBoxVersions, "Все последние версии установлены.");
            }

            Update.Text = "Обновить";
        }





        private async Task<bool> FetchFtpDataAsync()
        {
            if (!string.IsNullOrEmpty(latestFtpVersion) && ftpFileList != null && ftpFileList.Count > 0)
            {
                return true;
            }

            string ftpServer = "ftp://ftp.in-grp.net/installers/";
            string username = "anonymous";
            string password = "1@1";
            ftpFileList = new List<string>();

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServer + "versions.csv");
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    if (!reader.EndOfStream)
                    {
                        string versionInfo = reader.ReadLine();
                        latestFtpVersion = ExtractVersionInfo(versionInfo);
                    }
                }

                ftpFileList = await ftpClient.GetFileList("/installers/", latestFtpVersion.Replace('.', '_'), downloadStatusLabel, richTextBoxVersions);
                return true;
            }
            catch (Exception ex)
            {
                AddMessageToRichTextBox(richTextBoxVersions, "Ошибка при получении данных с FTP: " + ex.Message);
                return false;
            }
        }

        private string[] GetAllInstalledVersions()
        {
            string directoryPath = @"C:\Program Files\1cv8";
            List<string> filteredVersions = new List<string>();
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var directories = Directory.GetDirectories(directoryPath);
                    foreach (var directory in directories)
                    {
                        var dirName = Path.GetFileName(directory);
                        if (dirName.StartsWith("8"))
                            filteredVersions.Add(dirName);
                    }
                }
                else
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Каталог с версиями 1С не найден.");
                }
            }
            catch (Exception ex)
            {
                AddMessageToRichTextBox(richTextBoxVersions, $"Ошибка при чтении каталога: {ex.Message}");
            }
            return filteredVersions.ToArray();
        }

        public static string ExtractVersionInfo(string line)
        {
            string keyword = "version:";
            int index = line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                string version = line.Substring(index + keyword.Length).Trim();
                return version.Replace('_', '.');
            }
            return string.Empty;
        }

        public static void AddMessageToRichTextBox(RichTextBox richTextBox, string message)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.AppendText(message + Environment.NewLine)));
            }
            else
            {
                richTextBox.AppendText(message + Environment.NewLine);
            }
        }

        public static void AddMessageToLabel(Label label, string message)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text = message));
            }
            else
            {
                label.Text = message;
            }
        }

        private async void login1C_Click(object sender, EventArgs e)
        {
            try
            {
                var client = await Client.NewClientAsync(Client.loginURL, Client.releasesURL, "your_login", "your_password", richTextBoxVersions);
                AddMessageToRichTextBox(richTextBoxVersions, "Успешная авторизация");

                string exampleUrl = $"{Client.releasesURL}/required_endpoint";
                var response = await client.GetAsync(exampleUrl, richTextBoxVersions);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    AddMessageToRichTextBox(richTextBoxVersions, "Запрос выполнен успешно: " + responseBody);
                }
                else
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Ошибка при выполнении запроса: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                AddMessageToRichTextBox(richTextBoxVersions, "Ошибка авторизации: " + ex.Message);
            }
        }
    }
}
