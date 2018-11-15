using System;

namespace FileDownloaderSystem.Services
{
    public interface IDownloadServiceEvents
    {
        event EventHandler<MiDownloadCompletedEventArgs> DownloadComplete;
        event EventHandler<MiDownloadProgressChangedEventArgs> DownloadProgressChanged;
        event EventHandler<MiDownloadCompletedEventArgs> DownloadCancel;
        //event EventHandler<MiDownloadProgressChangedEventArgs> DownloadPause;
    }
}