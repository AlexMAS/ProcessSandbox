using System.Text;

namespace ProcessSandbox.IO;

public class MemoryStreamWriter : StreamWriter
{
    private static readonly Encoding UTF8 = new UTF8Encoding(false);

    private readonly MemoryStream _stream;
    private readonly TaskCompletionSource _anyWrite;
    private readonly TaskCompletionSource _disposed;


    public MemoryStreamWriter()
        : this(new MemoryStream())
    {
    }

    private MemoryStreamWriter(MemoryStream stream)
        : base(stream, UTF8)
    {
        _stream = stream;
        _anyWrite = new TaskCompletionSource();
        _disposed = new TaskCompletionSource();
    }


    public string GetAllText()
    {
        return base.Encoding.GetString(_stream.ToArray());
    }


    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        return base.WriteAsync(buffer, cancellationToken)
            .ContinueWith(i => _anyWrite.TrySetResult());
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
