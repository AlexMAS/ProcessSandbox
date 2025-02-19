using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ProcessSandbox.Linux;

/// <summary>
/// Набор системных функций Linux для работы с процессами.
/// </summary>
[SupportedOSPlatform("linux")]
internal static class ProcessInterop
{
    private const string Libc = "libc";
    private const int SIGKILL = 9;
    private const int SIGSTOP = 19;


    [DllImport(Libc, EntryPoint = "getpid", SetLastError = true)]
    public static extern int GetCurrentProcessId();

    [DllImport(Libc, EntryPoint = "getppid", SetLastError = true)]
    public static extern int GetParentProcessId();

    [DllImport(Libc, EntryPoint = "getpgid", SetLastError = true)]
    public static extern int GetProcessGroupId(int pid);

    [DllImport(Libc, EntryPoint = "setpgid", SetLastError = true)]
    private static extern int SetProcessGroupId(int pid, int pgid);

    [DllImport(Libc, EntryPoint = "getsid", SetLastError = true)]
    public static extern int GetProcessSessionId(int pid);

    [DllImport(Libc, EntryPoint = "setsid", SetLastError = true)]
    private static extern int SetProcessSessionId();

    [DllImport(Libc, EntryPoint = "kill", SetLastError = true)]
    private static extern int KillProcess(int pid, int sig);


    /// <summary>
    /// Преобразует код завершения процесса в известный статус, если это возможно.
    /// </summary>
    /// <remarks>
    /// Код <c>152</c> или <c>158</c> означают, что процесс был завершен ОС принудительно, так как достиг верхней
    /// границы системного лимита на использование CPU (<c>RLIMIT_CPU</c>). Данный код можно интерпретировать как
    /// "CPU Limit" и это вполне корректно.
    /// <br/>
    /// 
    /// Код <c>137</c> означает, что процесс завершен ОС принудительно из-за нехватки оперативной памяти.
    /// Данный код интерпретируется как "Memory Limit", так как ситуация говорит именно об этом. Причины, по которым
    /// может возникнуть "дефицит" оперативной памяти, могут быть разными. Самый простой случай, когда система
    /// действительно имеет небольшой объем памяти. Более сложный вариант, когда ситуация стала следствием
    /// загруженности системы. Так или иначе оперативная память является разделяемым ресурсом, поэтому параллельно
    /// исполняемые процессы косвенным образом могут влиять друг на друга. Если один процесс использует слишком
    /// много памяти, второму её может не хватить и он будет завершен с кодом <c>137</c>.
    /// </remarks>
    public static int TranslateExitCode(int exitCode)
    {
        return exitCode switch
        {
            152 or 158 => (int)SpecialExitCode.CpuLimit,
            137 => (int)SpecialExitCode.MemoryLimit,
            _ => exitCode
        };
    }


    /// <summary>
    /// Возвращает список родительских процессов.
    /// </summary>
    public static void GetParentProcesses(int processId, ISet<int> parentIds)
    {
        while (processId > 0)
        {
            var processStat = ProcessStatParser.GetProcessStat(processId);

            processId = processStat.ParentId;

            if (processId > 0)
            {
                parentIds.Add(processId);
            }
        }
    }

    /// <summary>
    /// Проверяет, имеет ли наблюдаемый процесс дочерние.
    /// </summary>
    /// <param name="observableProcessId">Идентификатор наблюдаемого процесса.</param>
    /// <returns><c>true</c>, если наблюдаемый процесс имеет дочерние.</returns>
    /// <remarks>
    /// При запуске наблюдаемого процесса он становится лидером новой группы процессов,
    /// таким образом его группа и группа всех дочерних процессов по умолчанию совпадает
    /// с идентификатором наблюдаемого процесса. Успешность работы данного метода основана
    /// на предположении, что ни наблюдаемый процесс, ни его возможные потомки не сменят
    /// идентификатор группы.
    /// </remarks>
    public static bool CheckObservableProcessHasChildren(int observableProcessId)
    {
        try
        {
            // Получаем список родительских процессов
            var currentProcessId = GetCurrentProcessId();
            var parentProcessId = GetParentProcessId();
            var parentProcesses = new HashSet<int>(2 + 1) { currentProcessId, parentProcessId };
            GetParentProcesses(parentProcessId, parentProcesses);

            foreach (var processId in EnumerateProcessIds())
            {
                // Все процессы из той же группы, что и наблюдаемый процесс
                if (processId != observableProcessId
                    && !parentProcesses.Contains(processId)
                    && GetProcessGroupId(processId) == observableProcessId)
                {
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// Завершает дерево наблюдаемых процессов.
    /// </summary>
    /// <param name="observableProcessId">Идентификатор наблюдаемого процесса.</param>
    /// <returns><c>true</c>, если наблюдаемый процесс имел дочерние.</returns>
    /// <remarks>
    /// При запуске наблюдаемого процесса он становится лидером новой группы процессов,
    /// таким образом его группа и группа всех дочерних процессов по умолчанию совпадает
    /// с идентификатором наблюдаемого процесса. Успешность работы данного метода основана
    /// на предположении, что ни наблюдаемый процесс, ни его возможные потомки не сменят
    /// идентификатор группы.
    /// </remarks>
    public static bool TerminateObservableProcessTree(int observableProcessId)
    {
        var hadChildren = false;

        try
        {
            // Сразу же останавливаем группу наблюдаемого процесса
            KillProcess(-observableProcessId, SIGSTOP);
            KillProcess(observableProcessId, SIGSTOP);

            hadChildren = CheckObservableProcessHasChildren(observableProcessId);

            KillProcess(-observableProcessId, SIGKILL);
            KillProcess(observableProcessId, SIGKILL);
        }
        catch { }

        return hadChildren;
    }

    private static IEnumerable<int> EnumerateProcessIds()
    {
        foreach (var procDir in Directory.EnumerateDirectories("/proc/"))
        {
            var dirName = Path.GetFileName(procDir);

            if (int.TryParse(dirName, NumberStyles.Integer, CultureInfo.InvariantCulture, out int processId))
            {
                yield return processId;
            }
        }
    }
}
