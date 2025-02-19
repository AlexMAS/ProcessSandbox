namespace ProcessSandbox;

/// <summary>
/// Параметры запуска и контроля процесса.
/// </summary>
public record ProcessSandboxStartInfo
{
    /// <summary>
    /// Период опроса состояния процесса по умолчанию.
    /// </summary>
    public static readonly TimeSpan DEFAULT_POLL_PERIOD = TimeSpan.FromMilliseconds(100);


    /// <summary>
    /// Период опроса состояния процесса.
    /// </summary>
    /// <remarks>
    /// Возможны только положительные значения, иначе используется значение по умолчанию (100 мс).
    /// </remarks>
    public TimeSpan PollPeriod = DEFAULT_POLL_PERIOD;


    /// <summary>
    /// Исполняемый файл.
    /// </summary>
    public required string Command;

    /// <summary>
    /// Аргументы командной строки.
    /// </summary>
    public IEnumerable<string> Arguments = [];

    /// <summary>
    /// Рабочий каталог.
    /// </summary>
    /// <remarks>
    /// По умолчанию - рабочий каталог текущего процесса.
    /// </remarks>
    public string WorkingDirectory = Environment.CurrentDirectory;

    /// <summary>
    /// Поток для перенаправления стандартного ввода (stdin).
    /// </summary>
    /// <remarks>
    /// По умолчанию - стандартный ввод текущего процесса.
    /// </remarks>
    public TextReader StandardInput = Console.In;

    /// <summary>
    /// Поток для перенаправления стандартного вывода (stdout).
    /// </summary>
    /// <remarks>
    /// По умолчанию - стандартный вывод ошибок текущего процесса.
    /// </remarks>
    public TextWriter StandardOutput = Console.Out;

    /// <summary>
    /// Поток для перенаправления стандартного вывода ошибок (stderr).
    /// </summary>
    /// <remarks>
    /// По умолчанию - стандартный вывод ошибок текущего процесса.
    /// </remarks>
    public TextWriter StandardError = Console.Error;


    /// <summary>
    /// Пользователь, от имени которого будет запущен процесс.
    /// </summary>
    /// <remarks>
    /// По умолчанию - текущий пользователь.
    /// Предполагается, что процесс может быть запущен от имени указанного
    /// непривилегированного пользователя, чтобы снизить возможные риски.
    /// </remarks>
    public string? UserName = null;

    /// <summary>
    /// Максимальное общее время работы процесса.
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// </remarks>
    public TimeSpan TotalTimeout = TimeSpan.MinValue;

    /// <summary>
    /// Лимит использования CPU.
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// Использование CPU наблюдаемым процессом контролируется программно, но есть особые случаи,
    /// когда процесс ведет себя крайне агрессивно, расходуя все доступные процессорные ресурсы.
    /// В этом случае контролирующий поток может вызываться значительно реже обычного, что делает
    /// невозможным сделать программное прерывание наблюдаемого процесса. Для контроля данной
    /// ситуации устанавливается системный лимит на использование CPU, но он делается немного
    /// выше программного, чтобы системный срабатывал только в крайних случаях.
    /// </remarks>
    public TimeSpan CpuLimit = TimeSpan.MinValue;

    /// <summary>
    /// Прибавка к лимиту использования CPU для установки системного лимита (RLIMIT_CPU).
    /// </summary>
    /// <remarks>
    /// По умолчанию <c>0</c>; отрицательные значения игнорируются.
    /// </remarks>
    public TimeSpan CpuLimitAddition = TimeSpan.Zero;

    /// <summary>
    /// Лимит использования памяти.
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// </remarks>
    public long MemoryLimit = long.MinValue;

    /// <summary>
    /// Лимит на количество символов в стандартном выводе (stdout).
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// </remarks>
    public long StandardOutputLimit = long.MinValue;

    /// <summary>
    /// Лимит на количество символов в стандартном выводе ошибок (stderr).
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// </remarks>
    public long StandardErrorLimit = long.MinValue;

    /// <summary>
    /// Лимит на количество одновременно запущенных потоков (RLIMIT_NPROC).
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// Один из способов дестабилизировать работу системы - перегрузить диспетчер задач,
    /// создав очень много процессов или потоков. Установка данного лимита позволяет
    /// устранить данную возможность.
    /// </remarks>
    public long ThreadCountLimit = long.MinValue;

    /// <summary>
    /// Лимит в байтах на максимальный размер создаваемых файлов (RLIMIT_FSIZE).
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// Установка данного лимита позволит устранить возможность атаки на файловую систему.
    /// </remarks>
    public long FileSizeLimit = long.MinValue;

    /// <summary>
    /// Лимит на количество одновременно открытых файлов (RLIMIT_NOFILE).
    /// </summary>
    /// <remarks>
    /// Лимит не установлен по умолчанию и в случае отрицательных значений.
    /// Установка данного лимита позволит устранить возможность атаки на файловую систему.
    /// </remarks>
    public long OpenFileLimit = long.MinValue;

    /// <summary>
    /// Запрещено ли порождение дочерних процессов.
    /// </summary>
    /// <remarks>
    /// По умолчанию порождение дочерних процессов разрешено.
    /// </remarks>
    public bool IsChildrenForbidden = false;
}
