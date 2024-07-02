using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public class PingChecker
    {
        private Timer pingTimer;
        private string ftpServerIP;
        private RichTextBox outputTextBox;

        public PingChecker(string ftpServerIP, RichTextBox outputTextBox)
        {
            this.ftpServerIP = ftpServerIP;
            this.outputTextBox = outputTextBox;
            InitializePingTimer();
        }

        private void InitializePingTimer()
        {
            pingTimer = new Timer();
            pingTimer.Interval = 60000; // 1 минута
            pingTimer.Tick += PingTimer_Tick;
            pingTimer.Start();
        }

        private async void PingTimer_Tick(object sender, EventArgs e)
        {
            bool isPingSuccessful = await PingServer(ftpServerIP);

            if (isPingSuccessful)
            {
                AddMessageToRichTextBox(outputTextBox, $"FTP сервер {ftpServerIP} доступен.");
            }
            else
            {
                AddMessageToRichTextBox(outputTextBox, $"FTP сервер {ftpServerIP} недоступен.");
            }
        }

        private async Task<bool> PingServer(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    for (int i = 0; i < 4; i++) // Пинг 4 раза
                    {
                        PingReply reply = await ping.SendPingAsync(ipAddress);
                        if (reply.Status != IPStatus.Success)
                        {
                            return false; // Если хотя бы один пинг не удался, вернуть false
                        }
                    }
                    return true; // Если все 4 пинга успешны, вернуть true
                }
            }
            catch (Exception ex)
            {
                AddMessageToRichTextBox(outputTextBox, $"Ошибка пинга: {ex.Message}");
                return false;
            }
        }

        private void AddMessageToRichTextBox(RichTextBox richTextBox, string message)
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
    }
}
