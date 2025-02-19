namespace ProcessSandbox;

/// <summary>
/// Системный процесс.
/// </summary>
public interface IProcessSandbox
{
    /// <summary>
    /// Общее время работы процесса.
    /// </summary>
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Время использования CPU.
    /// </summary>
    public TimeSpan CpuUsage { get; }

    /// <summary>
    /// Размер используемой памяти.
    /// </summary>
    public long MemoryUsage { get; }

    /// <summary>
    /// Количество символов в стандартном выводе (stdout).
    /// </summary>
    public long StandardOutputLength { get; }

    /// <summary>
    /// Превышен ли лимит записи в стандартный вывод (stdout).
    /// </summary>
    public bool StandardOutputLimitExceeded { get; }

    /// <summary>
    /// Количество символов в стандартном выводе ошибок (stderr).
    /// </summary>
    public long StandardErrorLength { get; }

    /// <summary>
    /// Превышен ли лимит записи в стандартный вывод ошибок (stderr).
    /// </summary>
    public bool StandardErrorLimitExceeded { get; }

    /// <summary>
    /// Код завершения процесса или <c>-1</c>, если процесс не завершен.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Процесс завершился самостоятельно.
    /// </summary>
    public bool SelfCompletion { get; }

    /// <summary>
    /// Были ли созданы дочерние процессы.
    /// </summary>
    public bool HadChildren { get; }

    /// <summary>
    /// Запускает процесс.
    /// </summary>
    /// <returns>Задача для ожидания кода завершения процесса.</returns>
    /// <seealso cref="SpecialExitCode"/>
    Task<int> Start();

    /// <summary>
    /// Прерывает работу запущенного процесса.
    /// </summary>
    /// <param name="exitCode">Код завершения процесса.</param>
    /// <seealso cref="SpecialExitCode"/>
    void Terminate(int exitCode);
}
