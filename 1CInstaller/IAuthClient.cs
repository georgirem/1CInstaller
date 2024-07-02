using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public interface IAuthClient
    {
        Task<HttpResponseMessage> GetAsync(string getUrl, RichTextBox richTextBox);
        Task<string> GetAuthTicketURL(string url);
    }
}
