using System;
using FileDownloaderSystem.Data;
using FileDownloaderSystem.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileDownloaderSystem.Services
{
    public interface ILogService
    {
        Task LogDownload(DownloadLog log);
        Task LogDownload(List<DownloadLog> logs);
    }

    public class LogService : ILogService
    {
        private readonly DatabaseContext _context;

        public LogService(DatabaseContext context)
        {
            _context = context;
            //_context = new DatabaseContext();
        }

        public async Task LogDownload(DownloadLog log)
        {
            await _context.DownloadLog.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogDownload(List<DownloadLog> logs)
        {
            await _context.DownloadLog.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }
    }
}
