using System.Text;

namespace ProcessSandbox.IO;

public class FixedStreamReader : StreamReader
{
    private static readonly Encoding UTF8 = new UTF8Encoding(false);


    public FixedStreamReader(string data)
        : this(UTF8.GetBytes(data))
    {
    }

    public FixedStreamReader(byte[] data)
        : base(new MemoryStream(data), UTF8)
    {
    }
}
