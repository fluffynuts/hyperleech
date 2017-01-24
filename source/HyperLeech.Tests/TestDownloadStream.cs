using System.IO;
using System.Text;
using NUnit.Framework;
using PeanutButter.Utils;

namespace HyperLeech.Tests
{
    [TestFixture]
    public class TestDownloadStream: AssertionHelper
    {
        [Test]
        public void Resize_ShouldWorkOnMemoryStream()
        {
            //--------------- Arrange -------------------
            var original = Encoding.UTF8.GetBytes("Hello");
            var next = Encoding.UTF8.GetBytes(" World");
            var sut = new DownloadStream(0, new MemoryStream(original), 4096);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.Expand(next.Length);
            sut.Stream.Write(next, 0, next.Length);
            var result = sut.Stream.ReadAllBytes();
            var textResult = Encoding.UTF8.GetString(result);

            //--------------- Assert -----------------------
            Expect(textResult, Is.EqualTo("Hello World"));
        }

    }
}