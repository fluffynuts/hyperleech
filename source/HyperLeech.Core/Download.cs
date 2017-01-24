using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PeanutButter.Utils;

namespace HyperLeech
{
    public enum DownloadRequestStates
    {
        Pending,
        Busy,
        Complete,
        Error
    }

    public interface IDownload
    {
        long TotalSize { get; }
        long Downloaded { get; }
        EventHandler OnActivity { get; set; }
        DownloadRequestStates State { get; }
        Exception LastError { get; }
        Task Start();
        Task Stop();
        byte[] Get();
    }

    public class Download : IDownload
    {
        public DownloadRequestStates State { get; private set; }
        public long TotalSize { get; private set; }
        public long Downloaded { get; private set; }
        public Exception LastError { get; private set; }

        private readonly IRequestConfig _config;
        private bool _started;
        private const long SIXTEEN_MEGABYTES = 16 * 1024 * 1024;

        public Download(IRequestConfig config)
        {
            _config = config;
            State = DownloadRequestStates.Pending;
        }

        public Task Start()
        {
            if (_config.TargetPath == null)
                throw new InvalidOperationException("Request has not been set up for filesystem download");
            return Task.Run(() =>
            {
                using (var targetStream = File.Open(_config.TargetPath, FileMode.Append))
                {
                    targetStream.Seek(targetStream.Length, SeekOrigin.Begin);
                    TryDownloadWith(i => new DownloadStream(0, targetStream));
                }
            });
        }

        public Task Stop()
        {
            return Task.Run(() =>
            {
                _started = false;
                while (State == DownloadRequestStates.Busy)
                    Thread.Sleep(50);
            });
        }

        public byte[] Get()
        {
            DownloadStream wrappedResult = null;
            TryDownloadWith(i =>
            {
                var size = i > 0 ? i : 0;
                var buffer = new byte[size];
                wrappedResult = new DownloadStream(0, new MemoryStream(buffer), SIXTEEN_MEGABYTES);
                return wrappedResult;
            });
            wrappedResult?.Stream.Seek(0, SeekOrigin.Begin);
            return wrappedResult?.Stream.ReadAllBytes();
        }

        private void TryDownloadWith(Func<long, DownloadStream> getTargetStream)
        {
            try
            {
                DownloadWith(getTargetStream);
            }
            catch (Exception e)
            {
                LastError = e;
                State = DownloadRequestStates.Error;
            }
        }

        private void DownloadWith(Func<long, DownloadStream> getTargetStream)
        {
            _started = true;
            State = DownloadRequestStates.Busy;
            LastError = null;
            var request = WebRequest.Create(new Uri(_config.Url));
            if (!_started)
            {
                State = DownloadRequestStates.Pending;
                RaiseEvent();
                return;
            }
            request.Headers.Add(HttpRequestHeader.Authorization, CreateBasicAuthHeaderString());
            // TODO: add resume header / offset
            using (var response = request.GetResponse())
            {
                using (var srcStream = response.GetResponseStream())
                {
                    var targetStream = getTargetStream(response.ContentLength);
                    TotalSize = response.ContentLength > 0 ? response.ContentLength : -1;
                    var remaining = response.ContentLength > 0 ? response.ContentLength - targetStream.Offset : int.MaxValue;
                    var buffer = new byte[_config.ChunkSize];
                    while (remaining > 0)
                    {
                        if (!_started)
                        {
                            State = DownloadRequestStates.Pending;
                            RaiseEvent();
                            return;
                        }
                        var toRead = remaining > _config.ChunkSize ? _config.ChunkSize : (int) remaining;
                        var actuallyRead = srcStream.Read(buffer, 0, toRead);
                        if (actuallyRead == 0)
                            return;
                        remaining -= actuallyRead;

                        targetStream.Expand(actuallyRead);

                        targetStream.Stream.Write(buffer, 0, actuallyRead);
                        targetStream.Stream.Flush();
                        RaiseEvent();
                    }
                }
            }
            State = DownloadRequestStates.Complete;
            RaiseEvent();
        }

        private void RaiseEvent()
        {
            var handlers = OnActivity;
            if (handlers == null)
                return;
            handlers(this, new EventArgs());
        }

        private string CreateBasicAuthHeaderString()
        {
            return string.Join(" ",
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.UserName + ":" + _config.Password))
            );
        }

        public EventHandler OnActivity { get; set; }
    }
}