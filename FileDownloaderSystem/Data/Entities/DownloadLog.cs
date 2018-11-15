using System;

namespace FileDownloaderSystem.Data.Entities
{
    public class DownloadLog
    {
        public int Id { get; set; }
        public string FileSource { get; set; }
        public string FileDestination { get; set; }
        public string Protocol { get; set; }
        public DateTime StartDownload { get; set; }
        public DateTime EndDownload { get; set; }
        public BenchmarkSizeEnum BenchmarkSize { get; set; }
        public BenchmarkSpeedEnum BenchmarkSpeed { get; set; }
        public float PercentageOfFailure { get; set; }
        public string ErrorMessage { get; set; }
        public DownloadStatusEnum DownloadStatus { get; set; }
        public double DownloadSpeed { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public enum BenchmarkSizeEnum
    {
        NotLargeData, LargeData
    }

    public enum BenchmarkSpeedEnum
    {
        Slow, Fast
    }

    public enum DownloadStatusEnum
    {
        Failure, Success
    }
}