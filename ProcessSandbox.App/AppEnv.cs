namespace ProcessSandbox;

/// <summary>
/// Переменные окружения приложения.
/// </summary>
internal static class AppEnv
{
    public const string SANDBOX_USER = nameof(SANDBOX_USER);
    public const string SANDBOX_TOTAL_TIMEOUT = nameof(SANDBOX_TOTAL_TIMEOUT);
    public const string SANDBOX_CPU_LIMIT = nameof(SANDBOX_CPU_LIMIT);
    public const string SANDBOX_CPU_LIMIT_ADDITION = nameof(SANDBOX_CPU_LIMIT_ADDITION);
    public const string SANDBOX_MEMORY_LIMIT = nameof(SANDBOX_MEMORY_LIMIT);
    public const string SANDBOX_STDOUT_LIMIT = nameof(SANDBOX_STDOUT_LIMIT);
    public const string SANDBOX_STDERR_LIMIT = nameof(SANDBOX_STDERR_LIMIT);
    public const string SANDBOX_THREAD_COUNT_LIMIT = nameof(SANDBOX_THREAD_COUNT_LIMIT);
    public const string SANDBOX_FILE_SIZE_LIMIT = nameof(SANDBOX_FILE_SIZE_LIMIT);
    public const string SANDBOX_OPEN_FILE_LIMIT = nameof(SANDBOX_OPEN_FILE_LIMIT);
    public const string SANDBOX_CHILDREN_FORBIDDEN = nameof(SANDBOX_CHILDREN_FORBIDDEN);
    public const string SANDBOX_VERBOSE = nameof(SANDBOX_VERBOSE);
    public const string SANDBOX_SYSLOG = nameof(SANDBOX_SYSLOG);


    /// <summary>
    /// Возвращает нулевой файл, если путь не указан.
    /// </summary>
    public static string OrNullFile(this string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return OperatingSystem.IsWindows()
                ? "NUL"
                : "/dev/null";
        }

        return path;
    }


    /// <summary>
    /// Пользователь, от имени которого будет запущен процесс.
    /// </summary>
    /// </remarks>
    public static string? UserName => GetStringVariable(SANDBOX_USER);

    /// <summary>
    /// Максимальное общее время работы процесса.
    /// </summary>
    public static TimeSpan TotalTimeout => GetDurationVariable(SANDBOX_TOTAL_TIMEOUT);

    /// <summary>
    /// Возвращает лимит использования CPU.
    /// </summary>
    public static TimeSpan CpuLimit => GetDurationVariable(SANDBOX_CPU_LIMIT);

    /// <summary>
    /// Прибавка к лимиту использования CPU для установки системного лимита (RLIMIT_CPU).
    /// </summary>
    public static TimeSpan CpuLimitAddition => GetDurationVariable(SANDBOX_CPU_LIMIT_ADDITION);

    /// <summary>
    /// Лимит использования памяти.
    /// </summary>
    public static long MemoryLimit => GetLongVariable(SANDBOX_MEMORY_LIMIT);

    /// <summary>
    /// Лимит на количество символов в стандартном выводе (stdout).
    /// </summary>
    public static long StandardOutputLimit => GetLongVariable(SANDBOX_STDOUT_LIMIT);

    /// <summary>
    /// Лимит на количество символов в стандартном выводе ошибок (stderr).
    /// </summary>
    public static long StandardErrorLimit => GetLongVariable(SANDBOX_STDERR_LIMIT);

    /// <summary>
    /// Лимит на количество одновременно запущенных потоков (RLIMIT_NPROC).
    /// </summary>
    public static long ThreadCountLimit => GetLongVariable(SANDBOX_THREAD_COUNT_LIMIT);

    /// <summary>
    /// Лимит в байтах на максимальный размер создаваемых файлов (RLIMIT_FSIZE).
    /// </summary>
    public static long FileSizeLimit => GetLongVariable(SANDBOX_FILE_SIZE_LIMIT);

    /// <summary>
    /// Лимит на количество одновременно открытых файлов (RLIMIT_NOFILE).
    /// </summary>
    public static long OpenFileLimit => GetLongVariable(SANDBOX_OPEN_FILE_LIMIT);

    /// <summary>
    /// Возвращает <c>true</c> в случае, если наблюдаемый процесс не может порождать дочерние.
    /// </summary>
    public static bool IsChildrenForbidden => GetBoolVariable(SANDBOX_CHILDREN_FORBIDDEN);

    /// <summary>
    /// Возвращает <c>true</c> в случае, если необходимо включить подробный вывод о ходе работы.
    /// </summary>
    public static bool IsVerboseOutputNeeded => GetBoolVariable(SANDBOX_VERBOSE);

    /// <summary>
    /// Возвращает <c>true</c> в случае, если необходимо включить вывод о ходе работы в системный лог.
    /// </summary>
    public static bool IsSyslogOutputNeeded => GetBoolVariable(SANDBOX_SYSLOG);


    private static string? GetStringVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return value?.Trim();
    }

    private static TimeSpan GetDurationVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return long.TryParse(value, out var result) ? TimeSpan.FromMilliseconds(result) : TimeSpan.MinValue;
    }

    private static long GetLongVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return long.TryParse(value, out var result) ? result : long.MinValue;
    }

    private static bool GetBoolVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return (value != null) && value.Equals("1");
    }
}
