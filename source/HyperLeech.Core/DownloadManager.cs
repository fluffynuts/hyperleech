using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLeech
{
    public interface IDownloadManager
    {
        IDownload[] Queue { get; }
        void Add(IDownloadFactory factory, string url, string targetPath);
        Task Start();
        Task Stop();
    }

    public interface IDownloadManagerConfig
    {
        int MaxConcurrentDownloads { get; set; }
    }

    public class DownloadManagerConfig : IDownloadManagerConfig
    {
        public int MaxConcurrentDownloads { get; set; }
    }

    public class DownloadManager : IDownloadManager
    {
        public IDownload[] Queue => _downloads.ToArray();
        private readonly List<IDownload> _downloads = new List<IDownload>();
        public IDownloadManagerConfig Config { get; }
        private readonly List<Task> _currentlyRunning = new List<Task>();

        public DownloadManager(IDownloadManagerConfig config)
        {
            Config = config;
        }

        public void Add(IDownloadFactory factory, string url, string targetPath)
        {
            _downloads.Add(factory.CreateRequestFor(url, targetPath));
        }

        private readonly object _lock = new object();

        public async Task Start()
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    while (_currentlyRunning.Count < Config.MaxConcurrentDownloads)
                        StartNextDownload();
                }
            });
        }

        private void StartNextDownload()
        {
            var toAdd = Queue.FirstOrDefault(d => d.State == DownloadRequestStates.Pending);
            if (toAdd == null)
                return;
            _currentlyRunning.Add(toAdd.Start());
        }

        public async Task Stop()
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    var toStop = Queue.Where(d => d.State == DownloadRequestStates.Busy).ToArray();
                    foreach (var download in toStop)
                    {
                        download.Stop();
                    }
                    while (_currentlyRunning.Any(t => !t.IsCompleted))
                        Thread.Sleep(50);
                }
            });
        }
    }
}