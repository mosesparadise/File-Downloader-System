using FileDownloaderSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDownloaderSystem.Services
{
    public class DownloadProgressTracker
    {
        private long _totalFileSize;
        private readonly int _sampleSize;
        private readonly TimeSpan _valueDelay;

        private DateTime _lastUpdateCalculated;
        private long _previousProgress;

        private double _cachedSpeed;
        private readonly Queue<Tuple<DateTime, long>> _changes = new Queue<Tuple<DateTime, long>>();

        public DownloadProgressTracker(int sampleSize, TimeSpan valueDelay)
        {
            _lastUpdateCalculated = DateTime.Now;
            _sampleSize = sampleSize;
            _valueDelay = valueDelay;
        }

        public void ResetProgress()
        {
            _previousProgress = 0;
            SetProgress(0, 0);
        }

        public void SetProgress(long bytesReceived, long totalBytesToReceive)
        {
            _totalFileSize = totalBytesToReceive;

            long diff = bytesReceived - _previousProgress;
            if (diff <= 0)
                return;

            _previousProgress = bytesReceived;

            _changes.Enqueue(new Tuple<DateTime, long>(DateTime.Now, diff));
            while (_changes.Count > _sampleSize)
                _changes.Dequeue();
        }

        private double GetRateInternal()
        {
            if (_changes.Count == 0)
                return 0;

            TimeSpan timespan = _changes.Last().Item1 - _changes.First().Item1;
            long bytes = _changes.Sum(t => t.Item2);

            double rate = bytes / timespan.TotalSeconds;

            if (double.IsInfinity(rate) || double.IsNaN(rate))
                return 0;

            return rate;
        }

        public double GetBytesPerSecond()
        {
            if (DateTime.Now >= _lastUpdateCalculated + _valueDelay)
            {
                _lastUpdateCalculated = DateTime.Now;
                _cachedSpeed = GetRateInternal();
            }

            return _cachedSpeed;
        }

        public double GetProgress()
        {
            return _previousProgress / (double)_totalFileSize;
        }

        public string GetBytesPerSecondString()
        {
            double speed = GetBytesPerSecond();
            var prefix = new[] { "", "K", "M", "G" };

            int index = 0;
            while (speed > 1024 && index < prefix.Length - 1)
            {
                speed /= 1024;
                index++;
            }

            int intLen = ((int)speed).ToString().Length;
            int decimals = 3 - intLen;
            if (decimals < 0)
                decimals = 0;

            string format = $"{{0:F{decimals}}}" + "{1}B/s";

            return String.Format(format, speed, prefix[index]);
        }

        public double GetBytesPerSecondAsUnit()
        {
            double speed = GetBytesPerSecond();
            int index = 0;
            var prefix = new[] { "", "K", "M", "G" };
            while (speed > 1024 && index < prefix.Length - 1)
            {
                speed /= 1024;
                index++;
            }
            int intLen = ((int)speed).ToString().Length;
            int decimals = 3 - intLen;
            if (decimals < 0)
                decimals = 0;

            return Math.Round(speed, MidpointRounding.AwayFromZero);
        }

        public BenchmarkSpeedEnum GetBenchmarkSpeed(long benchmark)
        {
            return (GetBytesPerSecondAsUnit() >= (benchmark * 1024))
                ? BenchmarkSpeedEnum.Fast
                : BenchmarkSpeedEnum.Slow;
        }

        public BenchmarkSizeEnum GetBenchmarkSize(long benchmark)
        {
            return (_totalFileSize > (benchmark * 1024)) ? BenchmarkSizeEnum.LargeData : BenchmarkSizeEnum.NotLargeData;
        }

        public int GetProgressPercentage
        {
            get
            {
                return _totalFileSize < 0 ? 0 : _totalFileSize == 0 ? 100 : (int)((100 * _previousProgress) / _totalFileSize);
            }
        }
    }
}
