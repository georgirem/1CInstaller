using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1CInstaller
{
    public interface IFtpClient
    {
        Task<List<string>> GetFileList(string directoryPath, string version, Label progressLabel, RichTextBox output);
        Task<bool> DownloadFileAsync(string ftpFilePath, string localFilePath, Label progressLabel, RichTextBox output, CancellationToken cancellationToken);
        Task<long> GetFileSizeAsync(string ftpFilePath, RichTextBox output);
    }
}
