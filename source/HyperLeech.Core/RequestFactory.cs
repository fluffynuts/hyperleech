using System.IO;

namespace HyperLeech
{

    public interface IDownloadFactory
    {
        IDownload CreateRequestFor(string url);
        IDownload CreateRequestFor(string url, string targetPath);
    }

    public class DownloadFactory: IDownloadFactory
    {
        private readonly string _username;
        private readonly string _password;

        public DownloadFactory(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public IDownload CreateRequestFor(string url, string targetPath)
        {
            return new Download(CreateConfig(url, targetPath));
        }

        private IRequestConfig CreateConfig(string url, string targetPath)
        {
            return new RequestConfig(_username, _password, url, targetPath);
        }

        public IDownload CreateRequestFor(string url)
        {
            return CreateRequestFor(url, null);
        }
    }
}