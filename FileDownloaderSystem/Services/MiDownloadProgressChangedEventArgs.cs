using System.ComponentModel;

namespace FileDownloaderSystem.Services
{
    public class MiDownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        internal MiDownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived, long totalBytesToReceive) :
            base(progressPercentage, userToken)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }

        public long BytesReceived { get; }
        public long TotalBytesToReceive { get; }
        //public string JobId { get; set; }

        public double DownloadSpeed { get; set; }

        public ProgressData GetProgressData => new ProgressData
        {
            BytesReceived = BytesReceived,
            TotalBytesToReceive = TotalBytesToReceive,
            ProgressPercentage = ProgressPercentage,
            UserToken = UserState
        };
    }
}