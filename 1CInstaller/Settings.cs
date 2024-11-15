using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public partial class Settings : Form
    {
        // Поля класса для управления настройками
        private TextBox textBoxServerAddress;
        private TextBox textBoxLogin;
        private TextBox textBoxPassword;
        private TextBox textBoxDownloadDirectory;
        private bool isDataChanged = false; // Флаг для отслеживания изменений
        private bool isLoadingSettings = false;
        private string settingsFilePath;
        private string originalServerAddress;
        private string originalLogin;
        private string originalPassword;
        private string originalDownloadDirectory;

        public Settings(string settingsFilePath)
        {
            InitializeComponent();
            this.settingsFilePath = settingsFilePath; // Присваиваем переданный путь
            InitializeSettingsForm();
            this.BackColor = Color.WhiteSmoke;
            LoadSettings(); // Загрузка данных из settings.ini
        }
        private void InitializeSettingsForm()
        {
            // Поле для ввода "Адрес сервера обновлений"
            Label labelServerAddress = new Label
            {
                Text = "Адрес сервера обновлений:",
                Location = new Point(10, 10),
                Width = 150
            };
            textBoxServerAddress = new TextBox // Инициализируем поле класса
            {
                Location = new Point(170, 10),
                Width = 200,
                Text = originalServerAddress // Получаем сохранённое значение
            };

            // Поле для ввода "Логин"
            Label labelLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(10, 50),
                Width = 150
            };
            textBoxLogin = new TextBox // Инициализируем поле класса
            {
                Location = new Point(170, 50),
                Width = 200,
                Text = originalLogin // Получаем сохранённое значение
            };

            // Поле для ввода "Пароль"
            Label labelPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(10, 90),
                Width = 150
            };
            textBoxPassword = new TextBox // Инициализируем поле класса
            {
                Location = new Point(170, 90),
                Width = 200,
                UseSystemPasswordChar = true, // Чтобы пароль не отображался
                Text = originalPassword // Получаем сохранённое значение
            };

            // Поле для ввода "Адрес каталога для скачивания файла"
            Label labelDownloadDirectory = new Label
            {
                Text = "Каталог загрузки:",
                Location = new Point(10, 130),
                Width = 150
            };
            textBoxDownloadDirectory = new TextBox
            {
                Location = new Point(170, 130),
                Width = 170, // Полная ширина текстового поля
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            // Кнопка с тремя точками для выбора каталога
            Button buttonBrowse = new Button
            {
                Text = "...",
                Width = 30,
                Height = textBoxDownloadDirectory.Height, // Высота кнопки равна высоте поля
                Location = new Point(textBoxDownloadDirectory.Location.X + textBoxDownloadDirectory.Width, textBoxDownloadDirectory.Location.Y), // Справа от TextBox                
                Anchor = AnchorStyles.Top | AnchorStyles.Right, // Чтобы кнопка оставалась справа при изменении размера
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0)
            };
            // Добавляем кнопку в текстовое поле
            textBoxDownloadDirectory.Controls.Add(buttonBrowse);

            // Обработчик выбора каталога
            buttonBrowse.Click += (sender, e) =>
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Выберите папку для загрузки файлов";
                    folderBrowserDialog.SelectedPath = textBoxDownloadDirectory.Text; // Устанавливаем текущий путь

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        textBoxDownloadDirectory.Text = folderBrowserDialog.SelectedPath; // Устанавливаем выбранный путь
                        isDataChanged = true; // Устанавливаем флаг изменений
                    }
                }
            };
            // Подписка на изменение значений в текстовых полях
            textBoxServerAddress.TextChanged += (sender, e) => { if (!isLoadingSettings) isDataChanged = true; };
            textBoxLogin.TextChanged += (sender, e) => { if (!isLoadingSettings) isDataChanged = true; };
            textBoxPassword.TextChanged += (sender, e) => { if (!isLoadingSettings) isDataChanged = true; };
            textBoxDownloadDirectory.TextChanged += (sender, e) => { if (!isLoadingSettings) isDataChanged = true; };


            // Добавляем контролы на форму
            this.Controls.Add(labelServerAddress);
            this.Controls.Add(textBoxServerAddress);
            this.Controls.Add(labelLogin);
            this.Controls.Add(textBoxLogin);
            this.Controls.Add(labelPassword);
            this.Controls.Add(textBoxPassword);
            this.Controls.Add(labelDownloadDirectory);
            this.Controls.Add(textBoxDownloadDirectory);
            this.Controls.Add(buttonBrowse); // Добавляем кнопку на форму

            // Подписываемся на событие закрытия формы
            this.FormClosing += Settings_FormClosing;
        }

        private void LoadSettings()
        {
            isLoadingSettings = true; // Начинаем загрузку настроек
            // Проверяем, существует ли файл settings.ini
            if (!File.Exists(settingsFilePath))
            {
                // Если файл не существует, создаём его с пустыми начальными значениями
                using (StreamWriter writer = new StreamWriter(settingsFilePath))
                {
                    writer.WriteLine("ServerAddress=");
                    writer.WriteLine("Login=");
                    writer.WriteLine("Password=");
                    writer.WriteLine("DownloadDirectory=");
                }
            }
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
                            textBoxServerAddress.Text = value;
                            break;
                        case "Login":
                            textBoxLogin.Text = value;
                            break;
                        case "Password":
                            try
                            {
                                // Дешифруем пароль перед загрузкой в TextBox
                                textBoxPassword.Text = EncryptionHelper.DecryptString(value);
                            }
                            catch
                            {
                                textBoxPassword.Text = "Не удалось расшифровать";// string.Empty; // Если не удалось расшифровать
                            }
                            break;
                        case "DownloadDirectory":
                            textBoxDownloadDirectory.Text = value;
                            break;
                    }
                }
            }

            isLoadingSettings = false; // Завершаем загрузку настроек
        }
        private void SaveSettings()
        {
            // Сохраняем данные в файл settings.ini
            using (StreamWriter writer = new StreamWriter(settingsFilePath))
            {
                writer.WriteLine($"ServerAddress={textBoxServerAddress.Text}");
                writer.WriteLine($"Login={textBoxLogin.Text}");
                // Шифруем пароль перед записью в файл
                string encryptedPassword = EncryptionHelper.EncryptString(textBoxPassword.Text);
                writer.WriteLine($"Password={encryptedPassword}");

                writer.WriteLine($"DownloadDirectory={textBoxDownloadDirectory.Text}");
            }
        }
        // Обработчик закрытия формы
        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isDataChanged) // Если данные были изменены
            {
                var result = MessageBox.Show("Сохранить изменения?", "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveSettings(); // Сохраняем данные в settings.ini
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // Отменяем закрытие
                }
            }
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int HTCAPTION = 0x2;

            // Блокируем только перемещение через заголовок окна
            if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
            {
                return; // Игнорируем перемещение пользователем
            }

            base.WndProc(ref m);
        }
    }
}
