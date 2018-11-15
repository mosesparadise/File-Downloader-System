using FileDownloaderSystem.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public class FtpWebRequestDownloadService : BaseDownloadService, IDownloadService
    {
        private readonly ILogger<FtpWebRequestDownloadService> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public FtpWebRequestDownloadService(ILogger<FtpWebRequestDownloadService> logger) : base(logger)
        {
            _logger = logger;
            ProtocolNames = new List<string> { "ftp" };
        }

        public event EventHandler<MiDownloadCompletedEventArgs> DownloadComplete;
        public event EventHandler<MiDownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<MiDownloadCompletedEventArgs> DownloadCancel;
        public List<string> ProtocolNames { get; }


        private int bufferSize = 2048;

        private DownloadFileInfo _downloadFileInfo;


        public async Task<bool> DownloadFileTaskAsync(DownloadFileInfo downloadFileInfo)
        {
            _downloadFileInfo = downloadFileInfo;
            bool downloadStatus = false;
            try
            {
                var fileSize = GetFileSize(downloadFileInfo.InputFIleInfo.Uri);
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _cancellationTokenSource.Token;
                /* Create an FTP Request */
                var request = CreateRequestStream(downloadFileInfo.InputFIleInfo.Uri, WebRequestMethods.Ftp.DownloadFile);
                /* Establish Return Communication with the FTP Server */
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    ProgressData progressData = null;
                    /* Get the FTP Server's Response Stream */
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) throw new Exception("Can't Read Response Stream");
                        /* Open a File Stream to Write the Downloaded File */
                        using (FileStream fileStream = new FileStream(downloadFileInfo.TempPath, FileMode.Create))
                        {
                            /* Buffer for the Downloaded Data */
                            byte[] byteBuffer = new byte[bufferSize];

                            int bytesRead = 0;

                            var oldPercentDone = 0.0;
                            var onProgressChanged = new Progress<ProgressData>(OnDownloadProgressChanged);
                            do
                            {
                                bytesRead = await responseStream.ReadAsync(byteBuffer, 0, bufferSize, cancellationToken);
                                if (bytesRead <= 0)
                                    continue;
                                await fileStream.WriteAsync(byteBuffer, 0, bytesRead, cancellationToken);
                                progressData = new ProgressData
                                {
                                    BytesReceived = fileStream.Length,
                                    TotalBytesToReceive = fileSize,
                                    ProgressPercentage = (int)((100 * fileStream.Length) / fileSize)
                                };
                                oldPercentDone = SendDownloadProgress(onProgressChanged, progressData, oldPercentDone);
                            } while (bytesRead > 0);
                            oldPercentDone = SendDownloadProgress(onProgressChanged, progressData, oldPercentDone, true);
                        }
                    }
                    if (progressData != null && progressData.TotalBytesToReceive == progressData.BytesReceived)
                    {
                        CompleteDownload();
                        downloadStatus = true;
                    }
                }
            }
            catch (IOException exception)
            {
                CancelDownload(exception);
            }
            catch (OperationCanceledException exception)
            {
                CancelDownload(exception, true);
            }
            catch (WebException exception)
            {
                CancelDownload(exception);
            }
            catch (Exception exception)
            {
                CancelDownload(exception);
            }
            return downloadStatus;
        }

        public void CancelDownloadTaskAsync()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void OnDownloadProgressChanged(ProgressData progress)
        {
            var args = new MiDownloadProgressChangedEventArgs(progress.ProgressPercentage, progress.UserToken,
                progress.BytesReceived, progress.TotalBytesToReceive);
            DownloadProgressChanged?.Invoke(this, args);
        }

        private long GetFileSize(Uri address)
        {
            long fileSize;
            /* Create an FTP Request */
            //FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
            ///* Log in to the FTP Server with the User Name and Password Provided */
            //InitialiseCredential(request);
            ///* When in doubt, use these options */
            //request.UseBinary = true;

            ///* Specify the Type of FTP Request */
            //request.Method = WebRequestMethods.Ftp.GetFileSize;
            var request = CreateRequestStream(address, WebRequestMethods.Ftp.GetFileSize);
            /* Establish Return Communication with the FTP Server */
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                fileSize = response.ContentLength;
            }
            return fileSize;
        }

        private void InitialiseCredential(FtpWebRequest request)
        {
            if (_downloadFileInfo?.AuthModel != null && _downloadFileInfo.AuthModel.IsAuthenticationRequired)
            {
                request.Credentials = new NetworkCredential(_downloadFileInfo.AuthModel.Username, _downloadFileInfo.AuthModel.Password);
            }
        }

        private FtpWebRequest CreateRequestStream(Uri address, string method)
        {
            /* Create an FTP Request */
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
            /* Log in to the FTP Server with the User Name and Password Provided */
            InitialiseCredential(request);
            /* When in doubt, use these options */
            //request.UseBinary = true;

            /* Specify the Type of FTP Request */
            request.Method = method;
            return request;
        }

        private Stream CreateResponseStream(Uri address, string method)
        {
            var request = CreateRequestStream(address, method);
            var response = (FtpWebResponse)request.GetResponse();
            return response.GetResponseStream();
        }

        private double SendDownloadProgress(IProgress<ProgressData> onProgressChanged, ProgressData progressData, double oldPercentDone, bool isCompletion = false)
        {
            var percentDone = (double)progressData.BytesReceived / progressData.TotalBytesToReceive;

            if (!isCompletion && !(percentDone - oldPercentDone > 0.05))
                return oldPercentDone;

            onProgressChanged?.Report(progressData);

            return percentDone;
        }

        private void CompleteDownload()
        {
            MoveFileToDestination(_downloadFileInfo.TempPath, _downloadFileInfo.DestinationPath);
            DownloadComplete?.Invoke(this, new MiDownloadCompletedEventArgs(null, false, null));
        }

        private void CancelDownload(Exception exception, bool isCancelled = false)
        {
            _logger.LogError(exception, exception.Message);
            DeletePartialDownload(_downloadFileInfo.TempPath);
            DownloadCancel?.Invoke(this, new MiDownloadCompletedEventArgs(GetExceptionToPropagate(exception), isCancelled, null));
        }
    }
}
