using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public class Client
    {
        private readonly string login;
        private readonly string password;
        private readonly string loginUrl;
        private readonly string baseUrl;
        private readonly HttpClient httpClient;

        public static string releasesURL = "https://releases.1c.ru";
        public static string loginURL = "https://login.1c.ru";
        public const string projectHrefPrefix = "/project/";
        public const string tempFileSuffix = ".d1c";

        private Client(string loginUrl, string baseUrl, string login, string password)
        {
            this.loginUrl = loginUrl;
            this.baseUrl = baseUrl;
            this.login = login;
            this.password = password;

            var handler = new HttpClientHandler { CookieContainer = new System.Net.CookieContainer() };
            httpClient = new HttpClient(handler);
        }

        public static async Task<Client> NewClientAsync(string loginUrl, string baseUrl, string login, string password, RichTextBox richTextBox)
        {
            var client = new Client(loginUrl, baseUrl, login, password);
            var authUrl = await client.GetAuthTicketURL(baseUrl);
            var response = await client.httpClient.GetAsync(authUrl);

            if (response.IsSuccessStatusCode)
            {
                Form1.AddMessageToRichTextBox(richTextBox, "Успешная авторизация");
                return client;
            }
            else
            {
                Form1.AddMessageToRichTextBox(richTextBox, "Ошибка авторизации: " + response.ReasonPhrase);
                throw new Exception("Ошибка авторизации");
            }
        }

        private async Task<string> GetAuthTicketURL(string url)
        {
            var loginParams = new
            {
                Login = login,
                Password = password,
                ServiceNick = url
            };

            var ticketUrl = $"{loginUrl}/rest/public/ticket/get";
            var postBody = JsonConvert.SerializeObject(loginParams);
            var content = new StringContent(postBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync(ticketUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var ticket = JsonConvert.DeserializeObject<TicketResponse>(responseContent);
                return $"{loginURL}/ticket/auth?token={ticket.Ticket}";
            }
            else
            {
                throw new Exception("Ошибка получения URL аутентификации: " + response.ReasonPhrase);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string getUrl, RichTextBox richTextBox)
        {
            if (getUrl.StartsWith("/"))
            {
                getUrl = baseUrl + getUrl;
            }

            var response = await httpClient.GetAsync(getUrl);
            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var authUrl = await GetAuthTicketURL(getUrl);
                response = await httpClient.GetAsync(authUrl);
            }

            if (!response.IsSuccessStatusCode)
            {
                Form1.AddMessageToRichTextBox(richTextBox, "Ошибка при выполнении запроса: " + response.ReasonPhrase);
            }

            return response;
        }

        private class TicketResponse
        {
            public string Ticket { get; set; }
        }
    }
}
