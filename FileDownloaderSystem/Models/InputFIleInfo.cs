using System;

namespace FileDownloaderSystem.Models
{
    public class InputFIleInfo
    {
        public string Protocol { get; set; }
        public Uri Uri { get; set; }
        public string StrUri { get; set; }

        public InputFIleInfo(string uri)
        {
            ThrowIfNull(uri, nameof(uri));
            StrUri = uri;
            Uri = new Uri(uri);
            Protocol = Uri.Scheme;
        }

        public string GetFilename() => GetFilename(StrUri);
        private string GetFilename(string hrefLink)
        {
            Uri uri = new Uri(hrefLink);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            return filename;
        }

        private static void ThrowIfNull(object argument, string parameterName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}