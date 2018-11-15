using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Models
{
    public class AppSettings
    {
        public string DefaultDownloadPath { get; set; }
        public long DefaultBenchmarkSize { get; set; }
        public int DefaultBenchmarkSpeed { get; set; }
    }
}
