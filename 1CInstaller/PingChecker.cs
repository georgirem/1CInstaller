using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public class PingChecker
    {
        private readonly string url;
        private readonly RichTextBox output;
        private readonly System.Windows.Forms.Timer timer;

        public PingChecker(string url, RichTextBox output)
        {
            this.url = url;
            this.output = output;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 60000; // 1 минута
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await CheckPageAvailabilityAsync();
        }

        public async Task CheckPageAvailabilityAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        Form1.AddMessageToRichTextBox(output, $"Страница {url} доступна.");
                    }
                    else
                    {
                        Form1.AddMessageToRichTextBox(output, $"Страница {url} недоступна. Статус: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Form1.AddMessageToRichTextBox(output, $"Ошибка при проверке доступности страницы {url}: {ex.Message}");
            }
        }
    }
}
