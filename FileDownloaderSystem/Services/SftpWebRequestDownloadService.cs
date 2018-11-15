using FileDownloaderSystem.Models;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public class SftpWebRequestDownloadService : BaseDownloadService, IDownloadService
    {
        private readonly ILogger<SftpWebRequestDownloadService> _logger;
        private DownloadFileInfo _downloadFileInfo;
        private uint bufferSize = 4 * 1024;

        //private ProgressData progressData;
        //private long _fileSize;
        //double oldPercentDone = 0.0;
        public SftpWebRequestDownloadService(ILogger<SftpWebRequestDownloadService> logger) : base(logger)
        {
            _logger = logger;
            ProtocolNames = new List<string> { "sftp" };
        }

        public event EventHandler<MiDownloadCompletedEventArgs> DownloadComplete;
        public event EventHandler<MiDownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<MiDownloadCompletedEventArgs> DownloadCancel;
        public List<string> ProtocolNames { get; }
        public async Task<bool> DownloadFileTaskAsync(DownloadFileInfo downloadFileInfo)
        {
            bool downloadStatus = false;
            _downloadFileInfo = downloadFileInfo;
            try
            {

                using (var client = new SftpClient(downloadFileInfo.InputFIleInfo.Uri.Host, downloadFileInfo.AuthModel.Username, downloadFileInfo.AuthModel.Password))
                {
                    client.Connect();
                    var remoteFilename = downloadFileInfo.InputFIleInfo.Uri.PathAndQuery;
                    SftpFileAttributes attributes = client.GetAttributes(remoteFilename);
                    var fileSize = attributes.Size;
                    ProgressData progressData = null;
                    using (FileStream fileStream = new FileStream(downloadFileInfo.TempPath, FileMode.Create))
                    {
                        client.BufferSize = bufferSize;
                        var oldPercentDone = 0.0;
                        var onProgressChanged = new Progress<ProgressData>(OnDownloadProgressChanged);
                        // Download with progress callback
                        client.DownloadFile(remoteFilename, fileStream, uploaded =>
                        {
                            //var inPercentDone = oldPercentDone;
                            progressData = new ProgressData
                            {
                                BytesReceived = (long)uploaded,
                                TotalBytesToReceive = fileSize,
                                ProgressPercentage = (int)((100 * (long)uploaded) / fileSize)
                            };
                            oldPercentDone = SendDownloadProgress(onProgressChanged, progressData, oldPercentDone);
                            //oldPercentDone = inPercentDone;
                        });
                        oldPercentDone = SendDownloadProgress(onProgressChanged, progressData, oldPercentDone, true);
                    }
                    client.Disconnect();
                    if (progressData != null && progressData.TotalBytesToReceive == progressData.BytesReceived)
                    {
                        CompleteDownload();
                        downloadStatus = true;
                    }
                }
            }
            catch (Exception exception)
            {
                CancelDownload(exception);
            }
            return downloadStatus;
        }

        public void CancelDownloadTaskAsync()
        {
            CancelDownload(new NotImplementedException("Not Yet Implemented"));
            //throw new NotImplementedException();
        }

        public void OnDownloadProgressChanged(ProgressData progress)
        {
            var args = new MiDownloadProgressChangedEventArgs(progress.ProgressPercentage, progress.UserToken,
                progress.BytesReceived, progress.TotalBytesToReceive);
            DownloadProgressChanged?.Invoke(this, args);
        }

        private void CancelDownload(Exception exception, bool isCancelled = false)
        {
            _logger.LogError(exception, exception.Message);
            DeletePartialDownload(_downloadFileInfo.TempPath);
            DownloadCancel?.Invoke(this, new MiDownloadCompletedEventArgs(GetExceptionToPropagate(exception), isCancelled, null));
        }

        private void CompleteDownload()
        {
            MoveFileToDestination(_downloadFileInfo.TempPath, _downloadFileInfo.DestinationPath);
            DownloadComplete?.Invoke(this, new MiDownloadCompletedEventArgs(null, false, null));
        }

        private double SendDownloadProgress(IProgress<ProgressData> onProgressChanged, ProgressData progressData, double oldPercentDone, bool isCompletion = false)
        {
            var percentDone = (double)progressData.BytesReceived / progressData.TotalBytesToReceive;

            if (!isCompletion && !(percentDone - oldPercentDone > 0.05))
                return oldPercentDone;

            onProgressChanged?.Report(progressData);

            return percentDone;
        }
    }
}