using System.Text;

namespace HyperLeech
{
    public class Lister
    {
        private readonly IDownloadFactory _downloadFactory;

        public Lister(IDownloadFactory downloadFactory)
        {
            _downloadFactory = downloadFactory;
        }

        public IListResult List(string url)
        {
            var request = _downloadFactory.CreateRequestFor(url);
            var bytes = request.Get();
            var html = Encoding.UTF8.GetString(bytes);
            return new ListResult(url, html);
        }
    }
}