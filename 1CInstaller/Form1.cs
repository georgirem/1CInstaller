﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Settings settingsForm;
        private IFtpClient ftpClient;
        private string installerPath;//Домашняя директория программы
        private string platformsPath;//Путь к каталогу куда скачивается дистрибутив
        private string latestFtpVersion;
        private List<string> ftpFileList;
        private CancellationTokenSource cancellationTokenSource;
        private PingChecker pingChecker;
        private HashSet<string> oneCPrograms;
        private string settingsFilePath;

        private string ftpServerAddress;
        private string ftpLogin;
        private string ftpPassword;
        private string downloadDirectory;

        private bool isPanelVisible = false; // Индикатор состояния панели
        private int panelWidth = 200; // Ширина панели
        /*private int panelSpeed = 10; // Скорость анимации
        private Timer timer; // Таймер для анимации*/

        public Form1()
        {
            InitializeComponent();
            InitializeInstallerPath();
            
            // Подписка на событие изменения позиции формы
            this.LocationChanged += Form1_LocationChanged;
            // Изменение цвета фона формы            
            this.BackColor = Color.WhiteSmoke; // Светло-серый фон
            // Настройка кнопки "Обновить"
            Update.BackColor = Color.Gold; // Мягкий светло-зелёный цвет
            Update.ForeColor = Color.DarkSlateGray; // Цвет текста кнопки (белый)
            Update.FlatStyle = FlatStyle.Flat; // Убираем 3D-эффект
            Update.Font = new Font("Segoe UI", 12, FontStyle.Bold); // Шрифт кнопки
            // Добавляем обработчики событий для изменения цвета при наведении и уходе курсора
            Update.MouseEnter += (s, e) => { Update.BackColor = Color.DarkGoldenrod; }; // Изменение цвета при наведении
            Update.MouseLeave += (s, e) => { Update.BackColor = Color.Gold; };  // Возвращение цвета при уходе курсора

            // Настройка кнопки "Settings"            
            Settings.FlatStyle = FlatStyle.Flat; // Убираем 3D-эффект
            Settings.BackColor = Color.Transparent;
            Settings.FlatAppearance.BorderSize = 0;            

            // Настройка RichTextBox
            richTextBoxVersions.BackColor = Color.WhiteSmoke; // Цвет фона RichTextBox (светло-серый)
            richTextBoxVersions.ForeColor = Color.Black; // Цвет текста
            richTextBoxVersions.BorderStyle = BorderStyle.FixedSingle; // Сделать рамку тонкой и аккуратной
            richTextBoxVersions.Font = new Font("Consolas", 10); // Более читаемый шрифт с моношириной

            ftpClient = new FtpClient("ftp://ftp.in-grp.net", "anonymous", "1@1");
            login1C.Visible = false;
            //pingChecker = new PingChecker("http://ig.badcore.net:14178/AccountingIG", richTextBoxVersions);
            oneCPrograms = new HashSet<string>();
            LoadInstalledOneCVersions();
            LoadSettings();

        }
        // Обработчик события изменения позиции основной формы
        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            if (settingsForm != null && !settingsForm.IsDisposed)
            {
                // Обновляем позицию формы настроек при перемещении основной формы
                settingsForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
            }
        }
        private void LoadSettings()
        {
            // Проверяем, существует ли файл settings.ini
            if (File.Exists(settingsFilePath))
            {
                var settings = File.ReadAllLines(settingsFilePath);

                foreach (var line in settings)
                {
                    // Ищем первое вхождение символа '=' и разбиваем строку только на две части
                    int indexOfEqualSign = line.IndexOf('=');

                    if (indexOfEqualSign > 0)
                    {
                        string key = line.Substring(0, indexOfEqualSign);
                        string value = line.Substring(indexOfEqualSign + 1);

                        switch (key)
                        {
                            case "ServerAddress":
                                ftpClient.ServerAddress = value;
                                break;
                            case "Login":
                                ftpClient.Username = value;
                                break;
                            case "Password":
                                try
                                {
                                    ftpClient.Password = EncryptionHelper.DecryptString(value); // Расшифровываем пароль
                                }
                                catch
                                {
                                    ftpClient.Password = "Ошибка расшифровки"; // Если не удается расшифровать
                                }
                                break;
                            case "DownloadDirectory":
                                platformsPath = value;
                                break;
                        }
                    }
                }
            }
            else
            {
                // Если файла настроек нет, вызываем InitializeInstallerPath() для его создания
                InitializeInstallerPath();
            }
        }

        private void InitializeInstallerPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            installerPath = Path.Combine(appDataPath, "1CInstaller");

            // Создание каталога 1CInstaller, если он не существует
            if (!Directory.Exists(installerPath))
            {
                Directory.CreateDirectory(installerPath);
            }

            // Создание файла settings.ini в кодировке по умолчанию, если файл не существует
            settingsFilePath = Path.Combine(installerPath, "settings.ini");

            // Проверяем, существует ли файл settings.ini, и если нет - заполняем его значениями по умолчанию
            if (!File.Exists(settingsFilePath) || new FileInfo(settingsFilePath).Length == 0)
            {
                // Значения по умолчанию
                string defaultServerAddress = "ftp://ftp.in-grp.net/installers/";
                string defaultLogin = "anonymous";
                string defaultPassword = EncryptionHelper.EncryptString("1@1");
                string defaultDownloadDirectory = Path.Combine(installerPath, "platforms");
                // Сохранение настроек по умолчанию в settings.ini
                using (StreamWriter writer = new StreamWriter(settingsFilePath))
                {
                    writer.WriteLine($"ServerAddress={defaultServerAddress}");
                    writer.WriteLine($"Login={defaultLogin}");
                    writer.WriteLine($"Password={defaultPassword}");
                    writer.WriteLine($"DownloadDirectory={defaultDownloadDirectory}");
                }
                // Создание каталога для скачивания, если он не существует
                if (!Directory.Exists(defaultDownloadDirectory))
                {
                    Directory.CreateDirectory(defaultDownloadDirectory);
                }
            }

            /*// Создание подкаталога platforms, если он не существует
            platformsPath = Path.Combine(installerPath, "platforms");
            if (!Directory.Exists(platformsPath))
            {
                Directory.CreateDirectory(platformsPath);
            }*/
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
        // Метод для получения и отображения установленных версий 1С
        private void LoadInstalledOneCVersions()
        {
            //HashSet<string> oneCPrograms = new HashSet<string>();

            // Получаем программы для 64-битной системы
            GetInstalledPrograms(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", oneCPrograms);

            // Получаем программы для 32-битной системы
            GetInstalledPrograms(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32),
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", oneCPrograms);

            // Если программ не найдено, выводим сообщение
            if (oneCPrograms.Count == 0)
            {
                // Сообщение о том, что ни одной версии 1С не установлено
                AddMessageToRichTextBox(richTextBoxVersions, "Не установлено ни одной версии 1С:Предприятие.");
            }
            else
            {
                // Сортируем список по алфавиту и выводим в richTextBoxVersions
                richTextBoxVersions.SelectionStart = richTextBoxVersions.TextLength;
                richTextBoxVersions.SelectionFont = new Font(richTextBoxVersions.Font.FontFamily, 13, FontStyle.Bold); // Полужирный шрифт размером 16                
                richTextBoxVersions.AppendText("Установленные версии 1С:Предприятие:\n");
                richTextBoxVersions.SelectionFont = new Font(richTextBoxVersions.Font, FontStyle.Regular); // Вернуть обычный шрифт

                var sortedOneCPrograms = oneCPrograms.OrderBy(name => name).ToList();
                foreach (var program in sortedOneCPrograms)
                {
                    richTextBoxVersions.AppendText(program + Environment.NewLine);
                }
            }
        }

        // Метод для получения установленных программ из реестра
        private void GetInstalledPrograms(RegistryKey root, string key, HashSet<string> oneCPrograms)
        {
            using (RegistryKey registryKey = root.OpenSubKey(key))
            {
                if (registryKey != null)
                {
                    foreach (string subKeyName in registryKey.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = registryKey.OpenSubKey(subKeyName))
                        {
                            var displayName = subKey?.GetValue("DisplayName") as string;
                            if (!string.IsNullOrEmpty(displayName) && displayName.Contains("1С:Предприятие"))                            
                            {
                                oneCPrograms.Add(displayName); // Добавляем только уникальные программы
                            }
                        }
                    }
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
                // Дожидаемся завершения всех асинхронных операций
                await Task.Delay(500); // Небольшая задержка для завершения операций с файлом
                // Очистка каталога platforms при остановке
                ClearDirectory(platformsPath);
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
            
            if (!oneCPrograms.Any(program => program.Contains(latestFtpVersion)))
            {
                AddMessageToRichTextBox(richTextBoxVersions, $"Необходимо установить версию с FTP: {latestFtpVersion}");
                // Путь к каталогу platforms
                //string platformsPath = Path.Combine(installerPath, "platforms");
                // Очистка каталога перед скачиванием
                ClearDirectory(platformsPath);

                string fileToDownload = ftpFileList.FirstOrDefault();
                if (string.IsNullOrEmpty(fileToDownload))
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Ошибка: подходящий файл не найден.");
                    Update.Text = "Обновить";
                    return;
                }

                // string ftpFilePath = "ftp://ftp.in-grp.net/installers/" + fileToDownload;
                //string ftpFilePath = ftpServerAddress + fileToDownload;ftpClient.ServerAddress
                string ftpFilePath = ftpClient.ServerAddress + fileToDownload;
                string localFilePath = Path.Combine(platformsPath, fileToDownload);

                bool downloadSuccess = await ftpClient.DownloadFileAsync(ftpFilePath, localFilePath, downloadStatusLabel, richTextBoxVersions, cancellationTokenSource.Token);

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    AddMessageToRichTextBox(richTextBoxVersions, "Скачивание отменено пользователем.");
                }
                else if (downloadSuccess)
                {
                    try
                    {
                        string destinationFolder = Path.Combine(platformsPath, Path.GetFileNameWithoutExtension(fileToDownload));
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
            if (!await ftpClient.CheckConnectionAsync(richTextBoxVersions))
            {
                AddMessageToRichTextBox(richTextBoxVersions, "Ошибка: не удалось подключиться к FTP-серверу.");
                return false;
            }

            if (!string.IsNullOrEmpty(latestFtpVersion) && ftpFileList != null && ftpFileList.Count > 0)
            {
                return true;
            }

            /*string ftpServer = "ftp://ftp.in-grp.net/installers/";
            string username = "anonymous";
            string password = "1@1";*/
            string ftpServer = ftpClient.ServerAddress;
            string username = ftpClient.Username;
            string password = ftpClient.Password;
            ftpFileList = new List<string>();
            
            if (!ftpServer.EndsWith("/"))
            {
                ftpServer += "/";
            }

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
                //ftpFileList = await ftpClient.GetFileList(ftpServer, latestFtpVersion.Replace('.', '_'), downloadStatusLabel, richTextBoxVersions);
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

        private void Settings_Click(object sender, EventArgs e)
        {
            if (settingsForm == null || settingsForm.IsDisposed)
            {
                // Создаём форму настроек, если она не существует или была закрыта

                string settingsFilePath = Path.Combine(installerPath, "settings.ini");
                settingsForm = new Settings(settingsFilePath);
                settingsForm.StartPosition = FormStartPosition.Manual;
                settingsForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
                settingsForm.Show();
            }
            else
            {
                // Если форма открыта, то закрываем её
                settingsForm.Close();
            }
        }
    }
}
