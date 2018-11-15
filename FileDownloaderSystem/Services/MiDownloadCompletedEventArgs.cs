using System;
using System.ComponentModel;

namespace FileDownloaderSystem.Services
{
    public class MiDownloadCompletedEventArgs : AsyncCompletedEventArgs
    {
        public MiDownloadCompletedEventArgs(Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
        {
        }

        public static MiDownloadCompletedEventArgs Create(AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            return new MiDownloadCompletedEventArgs(asyncCompletedEventArgs.Error, asyncCompletedEventArgs.Cancelled,
                asyncCompletedEventArgs.UserState);
        }
    }
}