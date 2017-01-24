using System.Text;
using NUnit.Framework;

namespace HyperLeech.Tests
{
    [TestFixture]
    [Explicit]
    public class DiscoveryTests: AssertionHelper
    {
        [Test]
        public void GetIndexFile()
        {
            //--------------- Arrange -------------------
            var secrets = SecretFetcher.GetSecrets();
            var factory = new DownloadFactory(secrets.User, secrets.Pass);
            var request = factory.CreateRequestFor(secrets.Site);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = request.Get();
            var textResult = Encoding.UTF8.GetString(result);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Null);
            Expect(textResult, Is.Not.Null);
            Expect(textResult, Does.StartWith("<html>"));
        }

    }
}
