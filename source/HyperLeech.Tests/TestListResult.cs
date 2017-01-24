using System.Linq;
using NUnit.Framework;

namespace HyperLeech.Tests
{
    [TestFixture]
    public class TestListResult: AssertionHelper
    {
        [Test]
        public void Construct_GivenHtmlWithOneFolderAndOneFile_ShouldAddThem()
        {
            //--------------- Arrange -------------------
            var html = "<html><head><title></title></head><body><h1>Index of /</h1><a href=\"../\">../</a><a href=\"Some Folder/\">Some Folder</a><a href=\"Some File.txt\">Some File.txt</a></body></html>";
            var url = "http://some.site/stuff";

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Create(url, html);

            //--------------- Assert -----------------------
            Expect(result.Folders, Is.Not.Empty);
            var folder = result.Folders.Single();
            Expect(folder.Name, Is.EqualTo("Some Folder"));
            Expect(folder.Url, Is.EqualTo(url + "/Some Folder"));
            var file = result.Files.Single();
            Expect(file.Name, Is.EqualTo("Some File.txt"));
            Expect(file.Url, Is.EqualTo(url + "/Some File.txt"));
        }

        private ListResult Create(string url, string html)
        {
            return new ListResult(url, html);
        }
    }
}