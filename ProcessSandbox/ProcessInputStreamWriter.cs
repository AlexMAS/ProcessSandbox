namespace ProcessSandbox;

/// <summary>
/// Предоставляет возможность асинхронной записи во входной поток процесса (stdin).
/// </summary>
internal class ProcessInputStreamWriter : IDisposable
{
    private const int INPUT_BUFFER_SIZE = 4096;

    private readonly Func<StreamWriter?> _inputStream;
    private readonly ManualResetEventSlim _inputStreamReady;
    private readonly TextReader _inputStreamData;
    private readonly Func<bool> _isTerminated;
    private readonly CancellationTokenSource _inputStreamCancellation;

    private StreamWriter? _inputStreamRef;
    private volatile bool _disposed;

    /// <summary>
    /// Создает экземпляр класса.
    /// </summary>
    /// <param name="inputStream">Функция для получения входного поток процесса.</param>
    /// <param name="inputStreamReady">Сигнал готовности входного потока процесса.</param>
    /// <param name="inputStreamData">Данные для входного потока процесса.</param>
    /// <param name="isTerminated">Функция для возможности досрочного завершения записи.</param>
    /// <remarks>
    /// Для начала чтения необходимо вызвать метод <see cref="BeginWriting()"/>.
    /// Данные порциями считываются потока данных и передаются входному потоку процесса.
    /// </remarks>
    public ProcessInputStreamWriter(
        Func<StreamWriter?> inputStream,
        ManualResetEventSlim inputStreamReady,
        TextReader inputStreamData,
        Func<bool> isTerminated)
    {
        _inputStream = inputStream;
        _inputStreamData = inputStreamData;
        _inputStreamReady = inputStreamReady;
        _isTerminated = isTerminated;
        _inputStreamCancellation = new CancellationTokenSource();
    }

    /// <summary>
    /// Начинает асинхронную запись.
    /// </summary>
    public ManualResetEventSlim BeginWriting()
    {
        var writingThreadStarted = new ManualResetEventSlim();

        // Создаем поток явно, чтобы гарантировать начало процесса записи сразу при вызове метода
        var thread = new Thread(async () =>
        {
            writingThreadStarted.Set();

            _inputStreamReady.Wait();

            if (_disposed || _isTerminated())
            {
                return;
            }

            var inputStreamRef = _inputStream();
            _inputStreamRef = inputStreamRef;

            if (inputStreamRef == null)
            {
                return;
            }

            int readChars;
            var buffer = new char[INPUT_BUFFER_SIZE];

            try
            {
                while (!_disposed && !_isTerminated())
                {
                    readChars = _inputStreamData.Read(buffer, 0, buffer.Length);

                    if (readChars > 0)
                    {
                        await inputStreamRef.WriteAsync(new ReadOnlyMemory<char>(buffer, 0, readChars), _inputStreamCancellation.Token);
                    }

                    if (readChars < buffer.Length)
                    {
                        break;
                    }
                }
            }
            catch
            {
                // Наблюдаемый процесс завершился
            }

            PostEOF();
        });

        thread.IsBackground = true;
        thread.Start();

        return writingThreadStarted;
    }

    private void PostEOF()
    {
        if (!_disposed)
        {
            _disposed = true;

            if (_inputStreamRef != null)
            {
                try
                {
                    using (_inputStreamCancellation)
                    {
                        _inputStreamCancellation.Cancel();
                    }
                }
                catch
                {
                    // Ignore
                }

                try
                {
                    _inputStreamRef.Close(); // EOF
                }
                catch
                {
                    // Ignore
                }
                finally
                {
                    _inputStreamRef = null;
                }
            }
        }
    }

    public void Dispose()
    {
        PostEOF();
    }
}
