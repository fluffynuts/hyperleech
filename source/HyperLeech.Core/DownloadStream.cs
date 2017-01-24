using System;
using System.IO;

namespace HyperLeech
{
    public interface IDownloadStream
    {
        long Offset { get; set; }
        Stream Stream { get; set ; }
        void Resize(long newSize);
    }

    public class DownloadStream: IDownloadStream
    {
        public long Offset { get; set; }
        public Stream Stream { get; set; }
        public long MaxResize { get; }

        public DownloadStream(long offset, Stream stream): this(offset, stream, 0)
        {
        }
        public DownloadStream(long offset, Stream stream, long maxResize)
        {
            Offset = offset;
            Stream = stream;
            MaxResize = maxResize;
        }

        public void Resize(long newSize)
        {
            var memStream = Stream as MemoryStream;
            if (memStream == null)
                return;
            if (newSize > MaxResize)
                throw new InvalidOperationException($"Cannot resize beyond {MaxResize} bytes");
            var newData = new byte[newSize];
            memStream.Seek(0, SeekOrigin.Begin);
            var toWrite = Math.Min(newSize, memStream.Length);
            memStream.Read(newData, 0, (int)toWrite);  // poo
            var oldStream = Stream;
            Stream = new MemoryStream(newData);
            Stream.Seek(toWrite, SeekOrigin.Begin);
            oldStream.Dispose();
        }

        public void Expand(int by)
        {
            Resize(Stream.Length + by);
        }
    }
}