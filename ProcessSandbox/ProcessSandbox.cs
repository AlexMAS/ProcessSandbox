using System.Diagnostics;

namespace ProcessSandbox;

/// <summary>
/// Реализует <see cref="IProcessSandbox"/> и предоставляет возможность запуска процесса с возможностью контроля системных ресурсов.
/// </summary>
public class ProcessSandbox : IProcessSandbox
{
    private readonly ProcessSandboxStartInfo _startInfo;
    private readonly TaskCompletionSource<int> _processResult;
    private readonly Stopwatch _elapsedTime;
    private Process? _process;
    private ProcessInputStreamWriter? _standardInput;
    private ProcessOutputStreamReader? _standardOutput;
    private ProcessOutputStreamReader? _standardError;
    private TimeSpan _cpuUsage;
    private long _memoryUsage;
    private bool _selfCompletion;
    private bool _hadChildren;
    private volatile bool _terminated;

    /// <summary>
    /// Общее время работы процесса.
    /// </summary>
    public TimeSpan ElapsedTime => _elapsedTime.Elapsed;

    /// <summary>
    /// Время использования CPU.
    /// </summary>
    public TimeSpan CpuUsage => _cpuUsage;

    /// <summary>
    /// Размер используемой памяти.
    /// </summary>
    public long MemoryUsage => _memoryUsage;

    /// <summary>
    /// Количество символов в стандартном выводе (stdout).
    /// </summary>
    public long StandardOutputLength { get; private set; }

    /// <summary>
    /// Превышен ли лимит записи в стандартный вывод (stdout).
    /// </summary>
    public bool StandardOutputLimitExceeded { get; private set; }

    /// <summary>
    /// Количество символов в стандартном выводе ошибок (stderr).
    /// </summary>
    public long StandardErrorLength { get; private set; }

    /// <summary>
    /// Превышен ли лимит записи в стандартный вывод ошибок (stderr).
    /// </summary>
    public bool StandardErrorLimitExceeded { get; private set; }

    /// <summary>
    /// Код завершения процесса или <c>-1</c>, если процесс не завершен.
    /// </summary>
    public int ExitCode => _processResult.Task.IsCompletedSuccessfully ? _processResult.Task.Result : -1;

    /// <summary>
    /// Процесс завершился самостоятельно.
    /// </summary>
    public bool SelfCompletion => _selfCompletion;

    /// <summary>
    /// Были ли созданы дочерние процессы.
    /// </summary>
    public bool HadChildren => _hadChildren;

    /// <summary>
    /// Причина невозможности запуска процесса или прерывания его работы.
    /// </summary>
    public Exception? TerminationReason { get; private set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="startInfo">Параметры запуска и контроля процесса.</param>
    public ProcessSandbox(ProcessSandboxStartInfo startInfo)
    {
        _startInfo = startInfo;
        _processResult = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        _elapsedTime = new Stopwatch();
        _cpuUsage = TimeSpan.Zero;
        _memoryUsage = 0;
        _terminated = false;
    }

    /// <summary>
    /// Запускает процесс, используя параметры, указанные при создании класса.
    /// </summary>
    /// <returns>Задача для ожидания кода завершения процесса.</returns>
    /// <seealso cref="SpecialExitCode"/>
    public Task<int> Start()
    {
        // Сначала запускаются все контролирующие потоки (порядок действий не случаен)

        var processStarted = new ManualResetEventSlim();
        var standardOutputClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var standardErrorClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var standardInputThreadStarted = RedirectStandardInput(processStarted);
        var standardOutputThreadStarted = RedirectStandardOutput(processStarted, standardOutputClosed);
        var standardErrorThreadStarted = RedirectStandardError(processStarted, standardErrorClosed);
        var processCompletionWaitStarted = WaitForProcessCompletion(processStarted, standardOutputClosed.Task, standardErrorClosed.Task);
        var processResourceConsumptionWatchStarted = WatchProcessResourceConsumption(processStarted);
        standardInputThreadStarted.Wait();
        standardInputThreadStarted.Dispose();
        standardOutputThreadStarted.Wait();
        standardOutputThreadStarted.Dispose();
        standardErrorThreadStarted.Wait();
        standardErrorThreadStarted.Dispose();
        processCompletionWaitStarted.Wait();
        processCompletionWaitStarted.Dispose();
        processResourceConsumptionWatchStarted.Wait();
        processResourceConsumptionWatchStarted.Dispose();

        Process? process = null;

        try
        {
            process = ProcessUtils.Start(_startInfo);
            _process = process;

            processStarted.Set();
            _elapsedTime.Start();
        }
        catch (Exception e)
        {
            processStarted.Set();

            // Нет исполняемого файла или превышен лимит на потоки
            return TryTerminateProcess((int)SpecialExitCode.CannotStartProcess, killProcess: false, selfCompletion: false, reason: e);
        }

        // Если дочерние процессы запрещены
        if (_startInfo.IsChildrenForbidden)
        {
            // Даем возможность запущенному процессу начать выполнение
            Thread.Sleep(0);

            var processId = -1;

            if (!_terminated)
            {
                lock (this)
                {
                    if (!_terminated)
                    {
                        try
                        {
                            processId = process.Id;
                        }
                        catch
                        {
                            // Наблюдаемый процесс завершился
                        }
                    }
                }
            }

            if (processId >= 0 && ProcessUtils.CheckObservableProcessHasChildren(processId))
            {
                _hadChildren = true;
                return TryTerminateProcess((int)SpecialExitCode.HadChildren, killProcess: true, selfCompletion: false, reason: null);
            }
        }

        return _processResult.Task;
    }

    private ManualResetEventSlim RedirectStandardInput(ManualResetEventSlim processStarted)
    {
        _standardInput = new ProcessInputStreamWriter(() => _process?.StandardInput, processStarted, _startInfo.StandardInput, () => _terminated);
        return _standardInput.BeginWriting();
    }

    private ManualResetEventSlim RedirectStandardOutput(ManualResetEventSlim processStarted, TaskCompletionSource<bool> outputClosed)
    {
        _standardOutput = new ProcessOutputStreamReader(() => _process?.StandardOutput, processStarted, _startInfo.StandardOutputLimit, () => _terminated, (reader, outputChars) =>
        {
            StandardOutputLength = reader.OutputLength;
            StandardOutputLimitExceeded = reader.OutputLimitExceeded;

            if (string.IsNullOrEmpty(outputChars))
            {
                outputClosed.TrySetResult(true);
            }
            else
            {
                try
                {
                    _startInfo.StandardOutput.Write(outputChars);
                }
                catch (Exception e)
                {
                    outputClosed.TrySetResult(false);
                    TryTerminateProcess((int)SpecialExitCode.CannotRedirectStandardOutput, killProcess: true, selfCompletion: false, reason: e);
                }
            }
        });

        return _standardOutput.BeginReading();
    }

    private ManualResetEventSlim RedirectStandardError(ManualResetEventSlim processStarted, TaskCompletionSource<bool> outputClosed)
    {
        _standardError = new ProcessOutputStreamReader(() => _process?.StandardError, processStarted, _startInfo.StandardErrorLimit, () => _terminated, (reader, outputChars) =>
        {
            StandardErrorLength = reader.OutputLength;
            StandardErrorLimitExceeded = reader.OutputLimitExceeded;

            if (string.IsNullOrEmpty(outputChars))
            {
                outputClosed.TrySetResult(true);
            }
            else
            {
                try
                {
                    _startInfo.StandardError.Write(outputChars);
                }
                catch (Exception e)
                {
                    outputClosed.TrySetResult(false);
                    TryTerminateProcess((int)SpecialExitCode.CannotRedirectStandardError, killProcess: true, selfCompletion: false, reason: e);
                }
            }
        });

        return _standardError.BeginReading();
    }

    private ManualResetEventSlim WaitForProcessCompletion(ManualResetEventSlim processStarted, Task<bool> standardOutputClosed, Task<bool> standardErrorClosed)
    {
        var threadStarted = new ManualResetEventSlim();

        var thread = new Thread(async () =>
        {
            threadStarted.Set();
            processStarted.Wait();

            var process = _process;

            if (process == null)
            {
                return;
            }

            var processCompleted = Task.WhenAll(
                    process.WaitForExitAsync(),
                    standardOutputClosed,
                    standardErrorClosed);

            if (_startInfo.TotalTimeout > TimeSpan.Zero && _startInfo.TotalTimeout < TimeSpan.MaxValue)
            {
                var processTimeout = Task.Delay(_startInfo.TotalTimeout);

                if (await Task.WhenAny(processCompleted, processTimeout) == processCompleted)
                {
                    var exitCode = ProcessUtils.TranslateExitCode(process.ExitCode);
                    _ = TryTerminateProcess(exitCode, killProcess: false, selfCompletion: true, reason: null);
                }
                else
                {
                    _ = TryTerminateProcess((int)SpecialExitCode.TotalTimeout, killProcess: true, selfCompletion: false, reason: null);
                }
            }
            else
            {
                await processCompleted;
                var exitCode = ProcessUtils.TranslateExitCode(process.ExitCode);
                _ = TryTerminateProcess(exitCode, killProcess: false, selfCompletion: true, reason: null);
            }
        });

        thread.IsBackground = true;
        thread.Start();

        return threadStarted;
    }

    private ManualResetEventSlim WatchProcessResourceConsumption(ManualResetEventSlim processStarted)
    {
        var threadStarted = new ManualResetEventSlim();

        if (_startInfo.CpuLimit < TimeSpan.Zero && _startInfo.MemoryLimit < 0)
        {
            threadStarted.Set();
            return threadStarted;
        }

        var pollPeriod = (_startInfo.PollPeriod <= TimeSpan.Zero)
            ? ProcessSandboxStartInfo.DEFAULT_POLL_PERIOD
            : _startInfo.PollPeriod;

        var thread = new Thread(() =>
        {
            threadStarted.Set();
            processStarted.Wait();

            var process = _process;

            if (process == null)
            {
                return;
            }

            while (CheckProcessResourcesWithinLimits(process))
            {
                Thread.Sleep(pollPeriod);
            }
        });

        thread.Priority = ThreadPriority.Highest;
        thread.IsBackground = true;
        thread.Start();

        return threadStarted;
    }

    private bool CheckProcessResourcesWithinLimits(Process process)
    {
        if (_terminated)
        {
            return false;
        }

        TimeSpan currentCpuUsage;
        long currentMemoryUsage;

        try
        {
            currentCpuUsage = process.TotalProcessorTime;
            currentMemoryUsage = Math.Max(_memoryUsage, Math.Max(process.PrivateMemorySize64, process.PeakWorkingSet64));
        }
        catch
        {
            return false;
        }

        _cpuUsage = currentCpuUsage;
        _memoryUsage = currentMemoryUsage;

        if (_startInfo.CpuLimit >= TimeSpan.Zero && currentCpuUsage >= _startInfo.CpuLimit)
        {
            TryTerminateProcess((int)SpecialExitCode.CpuLimit, killProcess: true, selfCompletion: false, reason: null);
            return false;
        }

        if (_startInfo.MemoryLimit >= 0 && currentMemoryUsage >= _startInfo.MemoryLimit)
        {
            TryTerminateProcess((int)SpecialExitCode.MemoryLimit, killProcess: true, selfCompletion: false, reason: null);
            return false;
        }

        return true;
    }

    private Task<int> TryTerminateProcess(int exitCode, bool killProcess, bool selfCompletion, Exception? reason)
    {
        if (!_terminated)
        {
            lock (this)
            {
                if (!_terminated)
                {
                    _terminated = true;
                    _elapsedTime.Stop();
                    TerminationReason = reason;

                    try
                    {
                        if (_process != null)
                        {
                            _standardInput?.Dispose();

                            if (_standardOutput != null)
                            {
                                StandardOutputLength = _standardOutput.OutputLength;
                                StandardOutputLimitExceeded = _standardOutput.OutputLimitExceeded;
                                _standardOutput.Dispose();
                            }

                            if (_standardError != null)
                            {
                                StandardErrorLength = _standardError.OutputLength;
                                StandardErrorLimitExceeded = _standardError.OutputLimitExceeded;
                                _standardError.Dispose();
                            }

                            _selfCompletion = selfCompletion;
                            _hadChildren |= ProcessUtils.TerminateObservableProcessTree(_process, killProcess);
                        }
                    }
                    catch { }
                    finally
                    {
                        _standardInput = null;
                        _standardOutput = null;
                        _standardError = null;
                        _process = null;
                        _processResult.TrySetResult(exitCode);
                    }
                }
            }
        }

        return _processResult.Task;
    }

    /// <summary>
    /// Прерывает работу запущенного процесса.
    /// </summary>
    /// <param name="exitCode">Код завершения процесса.</param>
    /// <seealso cref="SpecialExitCode"/>
    public void Terminate(int exitCode)
    {
        TryTerminateProcess(exitCode, killProcess: true, selfCompletion: false, reason: null);
    }
}
