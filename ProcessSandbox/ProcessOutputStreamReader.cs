using System.Text;

namespace ProcessSandbox;

/// <summary>
/// Предоставляет возможность асинхронного чтения выходного потока процесса (stdout/stderr).
/// </summary>
/// <remarks>
/// Логика работы стандартного класса <a href="https://github.com/dotnet/runtime/blob/release/7.0/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/AsyncStreamReader.cs">AsyncStreamReader</a>
/// такова, что пользователь не уведомляется до тех пор, пока в исходном потоке не встретится символ окончания строки (<c>\n</c> - Linux, <c>\r</c> - Mac, <c>\r\n</c> - Windows).
/// До этих пор производится буферизация считанных данных. В случае, если процесс выводит большой объем данных без использования переноса строк, размер буфера становится настолько
/// большим, что приложение может быть прервано с ошибкой Out Of Memory (OOM). Данный класс производит чтение входного потока с буферизацией, но без поиска символа окончания строки.
/// Таким образом, пользователь будет получать данные порциями и вероятность OOM сводится к минимуму.
/// </remarks>
internal class ProcessOutputStreamReader : IDisposable
{
    private const int DEFAULT_OUTPUT_BUFFER_SIZE = 1024; // bytes

    private readonly Func<StreamReader?> _outputStream;
    private readonly ManualResetEventSlim _outputStreamReady;
    private readonly long _outputLimit;
    private readonly Func<bool> _isTerminated;
    private readonly Action<ProcessOutputStreamReader, string?> _readCallback;
    private readonly byte[] _outputBuffer;

    private volatile bool _disposed;

    /// <summary>
    /// Создает экземпляр класса.
    /// </summary>
    /// <param name="outputStream">Функция для получения выходного поток процесса.</param>
    /// <param name="outputStreamReady">Сигнал готовности выходного потока процесса.</param>
    /// <param name="outputLimit">Лимит на количество символов для чтения.</param>
    /// <param name="isTerminated">Функция для возможности досрочного завершения чтения.</param>
    /// <param name="readCallback">Функция для обработки очередной порции прочитанных данных.</param>
    /// <remarks>
    /// Для начала чтения необходимо вызвать метод <see cref="BeginReading()"/>.
    /// Отрицательное значение <paramref name="outputLimit"/> означает отсутствие лимита.
    /// При достижении лимита чтения или конца потока в функцию <paramref name="readCallback"/> передается <c>null</c>, как признак окончания чтения.
    /// </remarks>
    public ProcessOutputStreamReader(
        Func<StreamReader?> outputStream,
        ManualResetEventSlim outputStreamReady,
        long outputLimit,
        Func<bool> isTerminated,
        Action<ProcessOutputStreamReader, string?> readCallback)
    {
        _outputStream = outputStream;
        _outputStreamReady = outputStreamReady;

        // Лимит отсутствует
        if (outputLimit < 0)
        {
            _outputBuffer = new byte[DEFAULT_OUTPUT_BUFFER_SIZE];
        }
        // Лимит есть, настройка размера буфера
        else if (outputLimit > 0)
        {
            // Приблизительная оценка размера выходных данных
            var outputLimitInBytes = (outputLimit < DEFAULT_OUTPUT_BUFFER_SIZE)
                ? Encoding.UTF8.GetMaxByteCount((int)outputLimit)
                : DEFAULT_OUTPUT_BUFFER_SIZE;

            _outputBuffer = new byte[Math.Min(DEFAULT_OUTPUT_BUFFER_SIZE, outputLimitInBytes)];
        }
        // Лимит нулевой, чтение вывода отключено
        else
        {
            _outputBuffer = Array.Empty<byte>();
        }

        _outputLimit = outputLimit;
        _isTerminated = isTerminated;
        _readCallback = readCallback;
    }

    /// <summary>
    /// Количество прочитанных символов.
    /// </summary>
    public long OutputLength { get; private set; }

    /// <summary>
    /// Превышен ли установленный лимит чтения.
    /// </summary>
    public bool OutputLimitExceeded { get; private set; }

    /// <summary>
    /// Начинает асинхронное чтение.
    /// </summary>
    public ManualResetEventSlim BeginReading()
    {
        var readingThreadStarted = new ManualResetEventSlim();

        if (_outputLimit == 0 || _disposed || _isTerminated())
        {
            PostEOF(true);
            readingThreadStarted.Set();
            return readingThreadStarted;
        }

        // Создаем поток явно, чтобы гарантировать начало процесса чтения сразу при вызове метода
        var thread = new Thread(() =>
        {
            readingThreadStarted.Set();

            _outputStreamReady.Wait();

            var outputStreamRef = _outputStream();

            if (outputStreamRef == null)
            {
                return;
            }

            var stream = outputStreamRef.BaseStream;
            var encoding = outputStreamRef.CurrentEncoding;

            while (!_disposed && !_isTerminated())
            {
                try
                {
                    var bytesRead = stream.Read(_outputBuffer, 0, _outputBuffer.Length);

                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    // Продолжаем читать данные, чтобы не вызывать остановку пишущего потока
                    if (OutputLimitExceeded)
                    {
                        continue;
                    }

                    var outputChars = encoding.GetString(_outputBuffer, 0, bytesRead);

                    // Лимит отсутствует
                    if (_outputLimit < 0)
                    {
                        OutputLength += outputChars.Length;
                        PostOutputChars(outputChars);
                    }
                    else
                    {
                        var remainingLimit = _outputLimit - OutputLength;

                        // Лимит позволяет обработать все считанные данные
                        if (remainingLimit > outputChars.Length)
                        {
                            OutputLength += outputChars.Length;
                            PostOutputChars(outputChars);
                        }
                        // Лимит позволяет обработать часть считанных данных
                        else if (remainingLimit > 0)
                        {
                            OutputLength = _outputLimit;
                            OutputLimitExceeded = true;
                            PostOutputChars(outputChars[..(int)remainingLimit]);
                            PostEOF(false);
                        }
                        // Лимит превышен
                        else
                        {
                            OutputLimitExceeded = true;
                            PostEOF(false);
                        }
                    }
                }
                catch
                {
                    break;
                }
            }

            PostEOF(true);
        });

        thread.IsBackground = true;
        thread.Start();

        return readingThreadStarted;
    }

    private void PostOutputChars(string outputChars)
    {
        if (!_disposed)
        {
            _readCallback(this, outputChars);
        }
    }

    private void PostEOF(bool dispose)
    {
        if (!_disposed)
        {
            _disposed = dispose;

            try
            {
                _readCallback(this, null);
            }
            catch
            {
                // Ignore
            }
        }
    }

    public void Dispose()
    {
        PostEOF(true);
    }
}
