namespace FileDownloaderSystem.Services
{
    public class ProgressData
    {
        //internal long BytesSent = 0;
        //internal long TotalBytesToSend = -1;
        internal long BytesReceived = 0;
        internal long TotalBytesToReceive = -1;
        //internal bool HasUploadPhase = false;
        internal object UserToken = null;
        internal int ProgressPercentage = 0;
        internal void Reset()
        {
            //BytesSent = 0;
            //TotalBytesToSend = -1;
            BytesReceived = 0;
            TotalBytesToReceive = -1;
            //HasUploadPhase = false;
            UserToken = null;
            ProgressPercentage = 0;
        }
    }
}