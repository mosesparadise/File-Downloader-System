using System.ComponentModel.DataAnnotations;

namespace FileDownloaderSystem.Models
{
    public class DownloadAuthViewModel
    {
        [Display(Name = "Authentication Required?")]
        public bool IsAuthenticationRequired { get; set; }
        //[Required]
        public string Username { get; set; }
        //[Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}