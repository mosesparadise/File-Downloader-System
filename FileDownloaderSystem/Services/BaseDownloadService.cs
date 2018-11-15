using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security;
using FileDownloaderSystem.Extensions;

namespace FileDownloaderSystem.Services
{
    public abstract class BaseDownloadService
    {
        private readonly ILogger _logger;

        protected BaseDownloadService(ILogger logger)
        {
            _logger = logger;
        }
        protected Exception GetExceptionToPropagate(Exception e) =>
            e is WebException || e is SecurityException ? e : new WebException(e.Message, e);

        protected string GetBytesPerSecondString(double speed)
        {
            //double speed = GetBytesPerSecond();
            var prefix = new[] { "", "K", "M", "G" };

            int index = 0;
            while (speed > 1024 && index < prefix.Length - 1)
            {
                speed /= 1024;
                index++;
            }

            int intLen = ((int)speed).ToString().Length;
            int decimals = 3 - intLen;
            if (decimals < 0)
                decimals = 0;

            string format = $"{{0:F{decimals}}}" + "{1}B/s";

            return String.Format(format, speed, prefix[index]);
        }

        protected void DeletePartialDownload(string filename)
        {
            try
            {
                FileHelper.DeleteIfExists(filename);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Unable to delete partial download for {filename}");
            }
        }

        protected void MoveFileToDestination(string source, string destination)
        {
            DirectoryHelper.CreateIfNotExists(System.IO.Path.GetDirectoryName(destination));
            FileHelper.DeleteIfExists(destination);
            System.IO.File.Move(source, destination);
        }
    }
}