using System;
using NUnit.Framework;
using PeanutButter.Utils;

namespace HyperLeech.Tests
{
    [Explicit]
    [TestFixture]
    public class TestLister
    {
        [Test]
        public void VisualTestOfListing()
        {
            //--------------- Arrange -------------------
            var secrets = SecretFetcher.GetSecrets();
            var requestFactory = new DownloadFactory(secrets.User, secrets.Pass);
            var sut = new Lister(requestFactory);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.List(secrets.Site);

            //--------------- Assert -----------------------
            result.Folders.ForEach(i =>
            {
                Console.WriteLine("+ " + i.Url);
            });
            result.Files.ForEach(i =>
            {
                Console.WriteLine("  " + i.Url);
            });
        }

    }
}