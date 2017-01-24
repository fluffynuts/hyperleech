using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils;

namespace HyperLeech.Tests
{
    [TestFixture]
    [Explicit]
    public class TestDownloadManager: AssertionHelper
    {
        private const string PATCH_MD5 = "83bfe16985fa78db4e3b173d2c8accf9";
        private const string PATCH_URL = "https://cdn.kernel.org/pub/linux/kernel/v4.x/patch-4.9.5.xz";
        [Test]
        public void DownloadAKernelPatch()
        {
            //--------------- Arrange -------------------
            var outFile = PATCH_URL.Split('/').Last();
            var target = Path.Combine("C:\\tmp", outFile);
            var secrets = SecretFetcher.GetSecrets();
            var factory = new DownloadFactory(secrets.User, secrets.Pass);
            var sut = new DownloadManager(new DownloadManagerConfig());

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Add(factory, PATCH_URL, target);
            var task = sut.Queue.First().Start();
            task.Wait();

            //--------------- Assert -----------------------
            Expect(target, Does.Exist);
            var downloadedBytes = File.ReadAllBytes(target);
            var downloadedMd5 = downloadedBytes.ToMD5String();
            Expect(downloadedMd5.ToLower(CultureInfo.InvariantCulture), Is.EqualTo(PATCH_MD5));
        }
    }

    [TestFixture]
    [Explicit]
    public class TestDownload: AssertionHelper
    {
        private const string PATCH_MD5 = "83bfe16985fa78db4e3b173d2c8accf9";
        private const string PATCH_URL = "https://cdn.kernel.org/pub/linux/kernel/v4.x/patch-4.9.5.xz";
        [Test]
        public void InterruptedDownloadCanResumt()
        {
            //--------------- Arrange -------------------
            var outFile = PATCH_URL.Split('/').Last();
            var target = Path.Combine("C:\\tmp", outFile);
            var secrets = SecretFetcher.GetSecrets();
            var factory = new DownloadFactory(secrets.User, secrets.Pass);
            var sut = factory.CreateRequestFor(PATCH_URL, target);
            var interrupted = false;
            sut.OnActivity += (s, e) =>
            {
                if (interrupted)
                    return;
                interrupted = true;
                var download = s as IDownload;
                download?.Stop();
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var task1 = sut.Start();
            task1.Wait();
            Expect(target, Does.Exist);
            var downloadedBytes = File.ReadAllBytes(target);
            var downloadedMd5 = downloadedBytes.ToMD5String();
            Expect(downloadedMd5, Is.Not.EqualTo(PATCH_MD5));

            var task2 = sut.Start();
            task2.Wait();

            //--------------- Assert -----------------------
            downloadedBytes = File.ReadAllBytes(target);
            downloadedMd5 = downloadedBytes.ToMD5String();
            Expect(downloadedMd5, Is.EqualTo(PATCH_MD5));
        }

    }
}