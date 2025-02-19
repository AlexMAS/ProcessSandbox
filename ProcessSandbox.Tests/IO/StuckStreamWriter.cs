using System.Text;

namespace ProcessSandbox.IO;
internal class StuckStreamWriter : StreamWriter
{
    private static readonly Encoding UTF8 = new UTF8Encoding(false);

    private readonly TaskCompletionSource _anyWrite;
    private readonly TaskCompletionSource _disposed;


    public StuckStreamWriter()
        : base(new MemoryStream(), UTF8)
    {
        _anyWrite = new TaskCompletionSource();
        _disposed = new TaskCompletionSource();
    }


    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        _anyWrite.SetResult();

        return Task.Delay(TimeSpan.FromHours(1))
            .ContinueWith(i => WriteAsync(buffer, cancellationToken));
    }

    public Task AnyWrite()
    {
        return _anyWrite.Task;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _disposed.SetResult();
    }

    public Task Disposed()
    {
        return _disposed.Task;
    }
}
