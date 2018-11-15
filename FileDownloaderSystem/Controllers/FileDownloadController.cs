using FileDownloaderSystem.Data.Entities;
using FileDownloaderSystem.Models;
using FileDownloaderSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Controllers
{
    public class FileDownloadController : Controller
    {
        private readonly ILogger<FileDownloadController> _logger;
        private ILogService _logService;
        private readonly IEnumerable<IDownloadService> _downloadServices;
        private readonly IHubContext<JobProgressHub> _hubContext;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly AppSettings _appSettings;

        private string _jobId;
        private DownloadFileInfo _downloadFileInfo;
        private int _lastPercentage = -1;
        string _errorMessage;

        public FileDownloadController(IOptions<AppSettings> options, ILogger<FileDownloadController> logger,
            ILogService logService, IEnumerable<IDownloadService> downloadServices,
            IHubContext<JobProgressHub> hubContext, IBackgroundTaskQueue backgroundTaskQueue,
            IHostingEnvironment hostingEnvironment, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _downloadServices = downloadServices;
            _hubContext = hubContext;
            _backgroundTaskQueue = backgroundTaskQueue;
            _hostingEnvironment = hostingEnvironment;
            _serviceScopeFactory = serviceScopeFactory;

            _appSettings = options.Value;
        }

        public IActionResult Index()
        {
            var downloadSetting = new DownloadSettingViewModel();
            if (!string.IsNullOrWhiteSpace(_appSettings.DefaultDownloadPath))
            {
                downloadSetting.DestinationPath = _appSettings.DefaultDownloadPath;
            }
            if (_appSettings.DefaultBenchmarkSize > 0)
                downloadSetting.BenchmarkSize = _appSettings.DefaultBenchmarkSize;
            if (_appSettings.DefaultBenchmarkSpeed > 0)
                downloadSetting.BenchmarkSpeed = _appSettings.DefaultBenchmarkSpeed;
            return View(downloadSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProgress(DownloadSettingViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            model.SourceUris = model.DownloadSources?
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var destinationPath = GetDestinationPath(model.DestinationPath);
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                ModelState.AddModelError("DestinationPath", "Invalid Destination Path");
                return View("Index", model);
            }

            model.DestinationPath = destinationPath;
            //_logger.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(model));

            string jobId = Guid.NewGuid().ToString("N");
            
            _backgroundTaskQueue.QueueBackgroundWorkItem(async => PerformBackgroundJob(jobId, model));

            return RedirectToAction("Progress", new { jobId });
        }

        private async Task PerformBackgroundJob(string jobId, DownloadSettingViewModel model)
        {
            _jobId = jobId;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _logService = scope.ServiceProvider.GetRequiredService<ILogService>();

                _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                await Task.Delay(1000);
                foreach (var downloadFileInfo in model.GetDownloadFileInfos)
                {
                    _downloadFileInfo = downloadFileInfo;
                    var downloadLog = new DownloadLog { StartDownload = DateTime.Now };
                    _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(downloadFileInfo));
                    _logger.LogInformation($"Processing URI of {downloadFileInfo.InputFIleInfo.Uri}");

                    //bool downloadStatus = false;

                    var downloadService = GetDownloadServiceProvider(downloadFileInfo.InputFIleInfo.Protocol);
                    //var downloadSetting = new DownloadSetting
                    //{
                    //    BenchmarkSize = model.BenchmarkSize,
                    //    BenchmarkSpeed = model.BenchmarkSpeed
                    //};
                    using (var downloadViewModel = new DownloadViewModel(downloadService, downloadFileInfo))
                    {
                        downloadViewModel.DownloadProgressChanged += DownloadViewModel_DownloadProgressChanged;
                        downloadViewModel.DownloadCancel += DownloadViewModel_DownloadCancel;
                        downloadViewModel.DownloadComplete += DownloadViewModel_DownloadComplete;
                        var downloadStatus = await downloadViewModel.DownloadFileTaskAsync();
                        downloadLog.BenchmarkSize =
                            downloadViewModel.ProgressTracker.GetBenchmarkSize(model.BenchmarkSize);
                        downloadLog.BenchmarkSpeed =
                            downloadViewModel.ProgressTracker.GetBenchmarkSpeed(model.BenchmarkSpeed);
                        downloadLog.DownloadStatus =
                            downloadStatus ? DownloadStatusEnum.Success : DownloadStatusEnum.Failure;
                        downloadLog.DownloadSpeed = downloadViewModel.ProgressTracker.GetBytesPerSecond();
                        downloadLog.PercentageOfFailure =
                            _lastPercentage == -1
                                ? 0
                                : _lastPercentage; // downloadViewModel.ProgressTracker.GetProgressPercentage;
                        downloadLog.ErrorMessage = !downloadStatus ? _errorMessage : null;
                        downloadLog.Protocol = downloadFileInfo.InputFIleInfo.Protocol;
                        _logger.LogWarning(
                            $"Protocol = {downloadLog.Protocol}. DowloadSpeed = {downloadViewModel.ProgressTracker.GetBytesPerSecondString()}");
                    }

                    downloadLog.EndDownload = DateTime.Now;
                    downloadLog.FileSource = downloadFileInfo.InputFIleInfo.StrUri;
                    downloadLog.FileDestination = downloadFileInfo.DestinationPath;

                    _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(downloadLog));
                    await _logService.LogDownload(downloadLog);
                }
            }
        }

        private async void DownloadViewModel_DownloadComplete(object sender, MiDownloadCompletedEventArgs e)
        {
            _logger.LogDebug("Fired Completed");
            _logger.LogInformation("File succesfully downloaded");
            await _hubContext.Clients.Group(_jobId).SendAsync("complete", $"{_downloadFileInfo.InputFIleInfo.GetFilename()}");
        }

        private async void DownloadViewModel_DownloadCancel(object sender, MiDownloadCompletedEventArgs e)
        {
            _logger.LogDebug("Fired Cancelled");

            if (e.Cancelled)
            {
                _errorMessage = "The download has been cancelled!";
                await _hubContext.Clients.Group(_jobId).SendAsync("error", $"{_downloadFileInfo.InputFIleInfo.GetFilename()}", _errorMessage);
                _logger.LogInformation("The download has been cancelled");
                return;
            }

            if (e.Error != null)
            {
                _errorMessage = $"An error occurred while trying to download file ::: {e.Error.Message}";
                await _hubContext.Clients.Group(_jobId).SendAsync("error", $"{_downloadFileInfo.InputFIleInfo.GetFilename()}", _errorMessage);
                _logger.LogCritical(e.Error, "An error occurred while trying to download file");
                return;
            }
        }

        private async void DownloadViewModel_DownloadProgressChanged(object sender, MiDownloadProgressChangedEventArgs e)
        {
            //_logger.LogDebug("Fired Progress");
            _logger.LogInformation($"Progress = {e.ProgressPercentage}%");
            _logger.LogDebug($"Download Speed :: {e.DownloadSpeed}");
            if (_lastPercentage != e.ProgressPercentage)
            {
                _lastPercentage = e.ProgressPercentage;
                await _hubContext.Clients.Group(_jobId).SendAsync("progress", e.ProgressPercentage, $"{_downloadFileInfo.InputFIleInfo.GetFilename()}");
                //await Task.Delay(1000);
            }
        }

        private void DeletePartialDownload(string filename)
        {
            try
            {
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Unable to delete partial download for {filename}");
            }
        }

        public IActionResult Progress(string jobId)
        {
            ViewBag.JobId = jobId;

            return View();
        }

        private IDownloadService GetDownloadServiceProvider(string protocolName)
        {
            return _downloadServices.FirstOrDefault(x => x.ProtocolNames.Any(y => y == protocolName)) ??
                   _downloadServices.First(x => x.ProtocolNames.Any(y => y == "default"));
        }

        private string GetDestinationPath(string path)
        {
            string filePath;
            if (IsPathValidRootedLocal(path))
            {
                filePath = path;
            }
            else
            {
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                filePath = Path.Combine(contentRootPath, path);
            }
            try
            {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting destination path");
                return null;
            }
            _logger.LogDebug(filePath);
            return filePath;
        }

        public bool IsPathValidRootedLocal(string pathString)
        {
            Uri pathUri;
            Boolean isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out pathUri);
            return isValidUri && pathUri != null && pathUri.IsLoopback;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}