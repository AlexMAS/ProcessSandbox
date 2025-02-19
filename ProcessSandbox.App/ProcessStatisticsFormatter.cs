namespace ProcessSandbox;

/// <summary>
/// Предоставляет методы для форматирования результатов запуска.
/// </summary>
internal static class ProcessStatisticsFormatter
{
    public static bool Format(string resultFile, int exitCode, ProcessSandbox process)
    {
        using var resultWriter = File.CreateText(resultFile);
        return Format(resultWriter, exitCode, process);
    }

    public static bool Format(TextWriter resultFile, int exitCode, ProcessSandbox process)
    {
        try
        {
            resultFile.WriteLine(exitCode);
            resultFile.WriteLine((long)process.ElapsedTime.TotalMilliseconds);
            resultFile.WriteLine((long)process.CpuUsage.TotalMilliseconds);
            resultFile.WriteLine(process.MemoryUsage);
            resultFile.WriteLine(process.StandardOutputLength);
            resultFile.WriteLine(process.StandardOutputLimitExceeded ? 1 : 0);
            resultFile.WriteLine(process.StandardErrorLength);
            resultFile.WriteLine(process.StandardErrorLimitExceeded ? 1 : 0);
            resultFile.WriteLine(process.SelfCompletion ? 1 : 0);
            resultFile.WriteLine(process.HadChildren ? 1 : 0);
            resultFile.Flush();
            return true;
        }
        // Ошибки сохранения статистики не являются ошибкой исполнения
        catch
        {
            return false;
        }
    }
}
