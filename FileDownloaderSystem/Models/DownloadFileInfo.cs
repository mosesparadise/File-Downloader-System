namespace FileDownloaderSystem.Models
{
    public class DownloadFileInfo
    {
        public string DestinationPath { get; set; }
        public InputFIleInfo InputFIleInfo { get; set; }
        public DownloadAuthViewModel AuthModel { get; set; }
        public string TempPath { get; set; }
    }
}