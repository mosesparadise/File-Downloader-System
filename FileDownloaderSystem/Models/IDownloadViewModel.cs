using System.Threading.Tasks;
using FileDownloaderSystem.Services;

namespace FileDownloaderSystem.Models
{
    public interface IDownloadViewModel : IDownloadServiceEvents
    {
        Task<bool> DownloadFileTaskAsync();
        void CancelDownloadTaskAsync();
    }
}