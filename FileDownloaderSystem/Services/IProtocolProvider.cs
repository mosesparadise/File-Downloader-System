using FileDownloaderSystem.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public interface IProtocolProvider
    {
        List<string> ProtocolNames { get; }
        void Initialise();
        Task DownloadFileAsync(InputFIleInfo input);
    }

    //public abstract class BaseProtocolProvider : IProtocolProvider
    //{
    //    public abstract string ProtocolName { get; set; }

    //    public virtual void Initialise()
    //    {

    //    }
    //}

    public class DefaultProtocolProvider : BaseProtocolProvider, IProtocolProvider
    {
        private readonly ILogger<HttpProtocolProvider> _logger;
        public List<string> ProtocolNames { get; private set; }

        public DefaultProtocolProvider(ILogger<HttpProtocolProvider> logger)
        {
            _logger = logger;
            ProtocolNames = new List<string> { "default" };
        }

        public void Initialise()
        {
        }

        public Task DownloadFileAsync(InputFIleInfo input)
        {
            throw new System.NotImplementedException();
        }
    }

    public class HttpProtocolProvider : IProtocolProvider
    {
        private readonly ILogger<HttpProtocolProvider> _logger;
        public List<string> ProtocolNames { get; private set; }

        private WebClient client;

        public HttpProtocolProvider(ILogger<HttpProtocolProvider> logger)
        {
            _logger = logger;
            ProtocolNames = new List<string> { "http", "https" };
        }

        public void Initialise()
        {
        }

        public Task DownloadFileAsync(InputFIleInfo input)
        {
            InitialiseWebClient();
            throw new System.NotImplementedException();
        }

        private void InitialiseWebClient()
        {
            client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //if (DownloadProgressChanged == null) return;
            //MyDownloadProgressChangedEventArgs args = new MyDownloadProgressChangedEventArgs(e.ProgressPercentage, e.UserState, e.BytesReceived, e.TotalBytesToReceive);
            //DownloadProgressChanged(this, args);
        }

        
        //public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        //protected virtual void OnDownloadProgressChanged(MyDownloadProgressChangedEventArgs e) => DownloadProgressChanged?.Invoke(this, e);

    }

    //public delegate void DownloadProgressChangedEventHandler(object sender, MyDownloadProgressChangedEventArgs e);
    //public class MyDownloadProgressChangedEventArgs : System.ComponentModel.ProgressChangedEventArgs
    //{
    //    internal MyDownloadProgressChangedEventArgs(int progressPercentage, object userToken, long bytesReceived,
    //        long totalBytesToReceive) :
    //        base(progressPercentage, userToken)
    //    {
    //        BytesReceived = bytesReceived;
    //        TotalBytesToReceive = totalBytesToReceive;
    //    }

    //    public long BytesReceived { get; }
    //    public long TotalBytesToReceive { get; }
    //}

    public abstract class BaseProtocolProvider
    {
        static BaseProtocolProvider()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        }

        protected virtual WebRequest GetRequest(Uri address)
        {
            WebRequest request = WebRequest.Create(address);
            request.Timeout = 30000;
            return request;
        }

        protected Uri GetUri(string address)
        {
            Uri uri;
            if (!Uri.TryCreate(address, UriKind.Absolute, out uri))
                return new Uri(Path.GetFullPath(address));
            return uri;
        }
    }



}
