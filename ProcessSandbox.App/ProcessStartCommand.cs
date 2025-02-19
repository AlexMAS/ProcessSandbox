namespace ProcessSandbox;

/// <summary>
/// Команда запуска процесса.
/// </summary>
internal record ProcessStartCommand
{
    /// <summary>
    /// Файл для сохранения статистики работы процесса.
    /// </summary>
    public required string ResultFile;

    /// <summary>
    /// Параметры запуска и контроля процесса.
    /// </summary>
    public required ProcessSandboxStartInfo StartInfo;


    public static ProcessStartCommand? Parse(string[] args)
    {
        if (args.Length >= 3)
        {
            try
            {
                var index = 0;
                var resultFile = args[index++];
                var workingDirectory = args[index++];
                var command = args[index++];
                var arguments = args[index..];

                return new()
                {
                    ResultFile = resultFile,

                    StartInfo = new()
                    {
                        // Запуск процесса
                        Command = command,
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory,
                        UserName = AppEnv.UserName,

                        // Настройки лимитов
                        TotalTimeout = AppEnv.TotalTimeout,
                        CpuLimit = AppEnv.CpuLimit,
                        CpuLimitAddition = AppEnv.CpuLimitAddition,
                        MemoryLimit = AppEnv.MemoryLimit,
                        StandardOutputLimit = AppEnv.StandardOutputLimit,
                        StandardErrorLimit = AppEnv.StandardErrorLimit,
                        ThreadCountLimit = AppEnv.ThreadCountLimit,
                        FileSizeLimit = AppEnv.FileSizeLimit,
                        OpenFileLimit = AppEnv.OpenFileLimit,
                        IsChildrenForbidden = AppEnv.IsChildrenForbidden
                    }
                };
            }
            catch { }
        }

        return null;
    }
}
