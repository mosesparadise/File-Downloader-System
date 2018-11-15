using FileDownloaderSystem.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace FileDownloaderSystem.Models
{
    public class DownloadSettingViewModel
    {
        public DownloadSettingViewModel()
        {
            AuthModel = new DownloadAuthViewModel();
            SourceUris = new List<string>();
        }

        [Required]
        [Display(Name = "Destination Path")]
        public string DestinationPath { get; set; }

        [Required]
        [Display(Name = "Download Sources")]
        public string DownloadSources { get; set; }

        public List<string> SourceUris { get; set; }

        public DownloadAuthViewModel AuthModel { get; set; }

        [Display(Name = "Benchmark Download Size")]
        [Range(0, Double.MaxValue, ErrorMessage = "Benchmark Value must be greater than zero (0)")]
        public long BenchmarkSize { get; set; }

        [Display(Name = "Benchmark Download Speed")]
        [Range(0, Double.MaxValue, ErrorMessage = "Benchmark Value must be greater than zero (0)")]
        public long BenchmarkSpeed { get; set; }

        public bool StopOnError { get; set; }

        public List<InputFIleInfo> GetUris
        {
            get
            {
                if (SourceUris == null || !SourceUris.Any())
                    return new List<InputFIleInfo>();
                return SourceUris.Select(x => new InputFIleInfo(x)).ToList();
            }
        }

        public IEnumerable<DownloadFileInfo> GetDownloadFileInfos
        {
            get
            {
                foreach (var fIleInfo in GetUris)
                {
                    var filename = fIleInfo.GetFilename();
                    var destination = $"{DestinationPath.EnsureEndsWith('\\')}{filename}";
                    if (System.IO.File.Exists(destination))
                        destination = GetNewDestinationFilename(destination);
                    yield return new DownloadFileInfo
                    {
                        InputFIleInfo = fIleInfo,
                        AuthModel = AuthModel,
                        DestinationPath = destination,
                        TempPath = GetTempFileWithGuid(filename)
                    };
                }
            }
        }

        public string GetTempFileWithGuid(string filePrefix)
        {
            filePrefix = System.IO.Path.GetFileNameWithoutExtension(filePrefix);
            string retFileName = string.Format("{0}{1}_{2}.itmp",
                System.IO.Path.GetTempPath(), filePrefix, Guid.NewGuid());
            return retFileName;
        }

        protected string GetNewDestinationFilename(string filename)
        {
            string newFilename;
            var folderPath = Path.GetDirectoryName(filename);
            var file = Path.GetFileNameWithoutExtension(filename);
            var fileExtension = Path.GetExtension(filename);
            int index = 1;
            do
            {
                newFilename = $"{folderPath.EnsureEndsWith('\\')}{file}_{index}{fileExtension}";
                ++index;
            } while (File.Exists(newFilename));

            return newFilename;
        }
    }

    public class DownloadSetting
    {
        public long BenchmarkSpeed { get; set; }
        public long BenchmarkSize { get; set; }
    }
}
