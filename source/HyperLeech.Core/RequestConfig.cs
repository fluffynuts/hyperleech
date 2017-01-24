namespace HyperLeech
{
    public interface IRequestConfig
    {
        string UserName { get; set; }
        string Password { get; set; }
        string Url { get; set; }
        string TargetPath { get; set; }
        int ChunkSize { get; set; }
    }

    public class RequestConfig : IRequestConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string TargetPath { get; set; }
        public int ChunkSize { get; set; }

        public RequestConfig(string username, string password, string url, string targetPath)
        {
            UserName = username;
            Password = password;
            Url = url;
            TargetPath = targetPath;

            ChunkSize = 32768;
        }
    }
}