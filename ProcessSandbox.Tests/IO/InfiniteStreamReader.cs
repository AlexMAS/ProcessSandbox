using System.Text;

namespace ProcessSandbox.IO;

public class InfiniteStreamReader : StreamReader
{
    private static readonly Encoding UTF8 = new UTF8Encoding(false);


    public InfiniteStreamReader(string repeatedData)
        : this(UTF8.GetBytes(repeatedData))
    {
    }

    public InfiniteStreamReader(byte[] repeatedData)
        : base(new InfiniteStream(repeatedData), UTF8)
    {
    }


    private class InfiniteStream : MemoryStream
    {
        private readonly byte[] _repeatedData;

        public InfiniteStream(byte[] repeatedData)
        {
            _repeatedData = repeatedData;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = Math.Min(Math.Min(buffer.Length - offset, count), _repeatedData.Length);
            Array.Copy(_repeatedData, 0, buffer, offset, readBytes);
            return readBytes;
        }
    }
}
