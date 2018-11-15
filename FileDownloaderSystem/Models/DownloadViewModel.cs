using System;
using System.Threading.Tasks;
using FileDownloaderSystem.Services;

namespace FileDownloaderSystem.Models
{
    public class DownloadViewModel : IDownloadViewModel, IDisposable
    {
        private readonly IDownloadService _downloadService;
        private readonly DownloadFileInfo _downloadFileInfo;
        //private readonly DownloadSetting _downloadSetting;
        //private BenchmarkSizeEnum? _benchmarkSize;
        //private BenchmarkSpeedEnum? _benchmarkSpeed;
        private DownloadProgressTracker _progressTracker;
        //private bool _isFirstCall = true;

        public DownloadViewModel(IDownloadService downloadService, DownloadFileInfo downloadFileInfo/*, DownloadSetting downloadSetting*/)
        {
            _downloadService = downloadService;
            _downloadFileInfo = downloadFileInfo;
            //_downloadSetting = downloadSetting;

            _downloadService.DownloadCancel += _downloadService_DownloadCancel;
            _downloadService.DownloadProgressChanged += _downloadService_DownloadProgressChanged;
            _downloadService.DownloadComplete += _downloadService_DownloadComplete;

            _progressTracker = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(1000));
        }

        private void _downloadService_DownloadComplete(object sender, MiDownloadCompletedEventArgs e)
        {
            DownloadComplete?.Invoke(sender, e);
        }

        private void _downloadService_DownloadProgressChanged(object sender, MiDownloadProgressChangedEventArgs e)
        {
            //HandleProgress(e.GetProgressData);
            _progressTracker.SetProgress(e.BytesReceived, e.TotalBytesToReceive);
            e.DownloadSpeed = _progressTracker.GetBytesPerSecond();
            //if (e.DownloadSpeed > 0)
            //    Console.WriteLine(e.DownloadSpeed);
            DownloadProgressChanged?.Invoke(sender, e);
        }

        private void _downloadService_DownloadCancel(object sender, MiDownloadCompletedEventArgs e)
        {
            DownloadCancel?.Invoke(sender, e);
        }

        public event EventHandler<MiDownloadCompletedEventArgs> DownloadComplete;
        public event EventHandler<MiDownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<MiDownloadCompletedEventArgs> DownloadCancel;
        public async Task<bool> DownloadFileTaskAsync()
        {
            _progressTracker.ResetProgress();
            return await _downloadService.DownloadFileTaskAsync(_downloadFileInfo);
        }

        public void CancelDownloadTaskAsync()
        {
            _downloadService.CancelDownloadTaskAsync();
        }

        private void HandleProgress(ProgressData progressData)
        {
            if (progressData == null) return;

            _progressTracker.SetProgress(progressData.BytesReceived, progressData.TotalBytesToReceive);
        }

        public DownloadProgressTracker ProgressTracker
        {
            get { return _progressTracker; }
        }

        public void Dispose()
        {
            _downloadService.DownloadCancel -= _downloadService_DownloadCancel;
            _downloadService.DownloadProgressChanged -= _downloadService_DownloadProgressChanged;
            _downloadService.DownloadComplete -= _downloadService_DownloadComplete;
        }
    }
}