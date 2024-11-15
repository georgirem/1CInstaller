using System;
using System.IO;

namespace _1CInstaller
{
    public class SettingsManager
    {
        private string settingsFilePath;
        private IFtpClient ftpClient;
        private string platformsPath;

        public SettingsManager(string settingsFilePath, IFtpClient ftpClient)
        {
            this.settingsFilePath = settingsFilePath;
            this.ftpClient = ftpClient;
            LoadSettings();
        }

        public string PlatformsPath => platformsPath;
        public IFtpClient FtpClient => ftpClient;

        public void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                var settings = File.ReadAllLines(settingsFilePath);
                foreach (var line in settings)
                {
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
                                    ftpClient.Password = EncryptionHelper.DecryptString(value);
                                }
                                catch
                                {
                                    ftpClient.Password = "Ошибка расшифровки";
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
                InitializeDefaultSettings();
            }
        }

        public void InitializeDefaultSettings()
        {
            string defaultServerAddress = "ftp://ftp.in-grp.net";
            string defaultLogin = "anonymous";
            string defaultPassword = EncryptionHelper.EncryptString("1@1");
            string defaultDownloadDirectory = Path.Combine(Path.GetDirectoryName(settingsFilePath), "platforms");

            using (StreamWriter writer = new StreamWriter(settingsFilePath))
            {
                writer.WriteLine($"ServerAddress={defaultServerAddress}");
                writer.WriteLine($"Login={defaultLogin}");
                writer.WriteLine($"Password={defaultPassword}");
                writer.WriteLine($"DownloadDirectory={defaultDownloadDirectory}");
            }

            if (!Directory.Exists(defaultDownloadDirectory))
            {
                Directory.CreateDirectory(defaultDownloadDirectory);
            }

            ftpClient.ServerAddress = defaultServerAddress;
            ftpClient.Username = defaultLogin;
            ftpClient.Password = EncryptionHelper.DecryptString(defaultPassword);
            platformsPath = defaultDownloadDirectory;
        }

        public void SaveSettings(string serverAddress, string login, string password, string downloadDirectory)
        {
            using (StreamWriter writer = new StreamWriter(settingsFilePath))
            {
                writer.WriteLine($"ServerAddress={serverAddress}");
                writer.WriteLine($"Login={login}");
                string encryptedPassword = EncryptionHelper.EncryptString(password);
                writer.WriteLine($"Password={encryptedPassword}");
                writer.WriteLine($"DownloadDirectory={downloadDirectory}");
            }
        }
    }
}
