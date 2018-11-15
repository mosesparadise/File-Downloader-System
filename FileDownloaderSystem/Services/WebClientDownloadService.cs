using FileDownloaderSystem.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public class WebClientDownloadService : BaseDownloadService, IDownloadService
    {
        private readonly ILogger<WebClientDownloadService> _logger;
        public List<string> ProtocolNames { get; private set; }
        private WebClient _webClient;
        private DownloadFileInfo _downloadFileInfo;

        public async Task<bool> DownloadFileTaskAsync(DownloadFileInfo downloadFileInfo)
        {
            _downloadFileInfo = downloadFileInfo;
            try
            {
                using (_webClient = new WebClient())
                {
                    if (downloadFileInfo.AuthModel != null && downloadFileInfo.AuthModel.IsAuthenticationRequired)
                    {
                        _webClient.Credentials = new NetworkCredential(downloadFileInfo.AuthModel.Username, downloadFileInfo.AuthModel.Password);
                    }
                    _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
                    _webClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
                    await _webClient.DownloadFileTaskAsync(downloadFileInfo.InputFIleInfo.Uri, downloadFileInfo.TempPath);
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                _logger.LogError(exception, exception.Message);
                DeletePartialDownload(downloadFileInfo.TempPath);
                //MiDownloadCompletedEventArgs eventArgs =
                //    new MiDownloadCompletedEventArgs(GetExceptionToPropagate(exception), false, null);
                //DownloadCancel?.Invoke(this, eventArgs);
                return false;
            }
            return true;
        }

        public void CancelDownloadTaskAsync()
        {
            if (_webClient.IsBusy)
                _webClient.CancelAsync();
        }

        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs asyncCompletedEventArgs)
        {
            var progress = new ProgressData
            {
                TotalBytesToReceive = asyncCompletedEventArgs.TotalBytesToReceive,
                BytesReceived = asyncCompletedEventArgs.BytesReceived,
                UserToken = asyncCompletedEventArgs.UserState,
                ProgressPercentage = asyncCompletedEventArgs.ProgressPercentage
            };
            OnDownloadProgressChanged(progress);
        }

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            if (asyncCompletedEventArgs.Cancelled || asyncCompletedEventArgs.Error != null)
            {
                DownloadCancel?.Invoke(sender, MiDownloadCompletedEventArgs.Create(asyncCompletedEventArgs));
            }
            else
            {
                MoveFileToDestination(_downloadFileInfo.TempPath, _downloadFileInfo.DestinationPath);
                DownloadComplete?.Invoke(sender, MiDownloadCompletedEventArgs.Create(asyncCompletedEventArgs));
            }
        }

        public void OnDownloadProgressChanged(ProgressData progress)
        {
            //int progressPercentage = progress.TotalBytesToReceive < 0 ? 0 : progress.TotalBytesToReceive == 0 ? 100 : (int)((100 * progress.BytesReceived) / progress.TotalBytesToReceive);
            var args = new MiDownloadProgressChangedEventArgs(progress.ProgressPercentage, progress.UserToken,
                progress.BytesReceived, progress.TotalBytesToReceive);
            DownloadProgressChanged?.Invoke(this, args);
        }

        public event EventHandler<MiDownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<MiDownloadCompletedEventArgs> DownloadComplete;
        public event EventHandler<MiDownloadCompletedEventArgs> DownloadCancel;

        public WebClientDownloadService(ILogger<WebClientDownloadService> logger) : base(logger)
        {
            _logger = logger;
            ProtocolNames = new List<string> { "http", "https", "default" };
        }

    }
}