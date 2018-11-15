using System;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public interface IFileDownloadService
    {
        Task DownloadFileAsync(string url);
    }

    public class FileDownloadService : IFileDownloadService
    {
        public FileDownloadService()
        {

        }

        public async Task DownloadFileAsync(string url)
        {

        }

        
    }
}
