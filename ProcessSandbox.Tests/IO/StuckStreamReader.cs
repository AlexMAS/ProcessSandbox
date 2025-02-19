using System.Text;

namespace ProcessSandbox.IO;

public class StuckStreamReader : StreamReader
{
    private static readonly Encoding UTF8 = new UTF8Encoding(false);

    private readonly TaskCompletionSource _anyRead;


    public StuckStreamReader()
        : base(new StuckStream(), UTF8)
    {
        _anyRead = new TaskCompletionSource();
    }


    public override int Read(char[] buffer, int index, int count)
    {
        _anyRead.SetResult();
        return base.Read(buffer, index, count);
    }

    public Task AnyRead()
    {
        return _anyRead.Task;
    }


    private class StuckStream : MemoryStream
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(TimeSpan.FromHours(1));
            return count;
        }
    }
}
