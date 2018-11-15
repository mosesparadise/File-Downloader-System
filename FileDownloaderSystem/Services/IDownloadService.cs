using FileDownloaderSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public interface IDownloadService : IDownloadServiceEvents
    {
        List<string> ProtocolNames { get; }
        Task<bool> DownloadFileTaskAsync(DownloadFileInfo downloadFileInfo);
        void CancelDownloadTaskAsync();

        void OnDownloadProgressChanged(ProgressData progress);
    }


    //public static class EventExtensions
    //{
    //    public static void Raise(this EventHandler handler, object sender, EventArgs args)
    //    {
    //        handler?.Invoke(sender, args);
    //    }
    //}


}
