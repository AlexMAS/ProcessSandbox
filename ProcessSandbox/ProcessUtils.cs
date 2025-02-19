using System.Diagnostics;

namespace ProcessSandbox;

/// <summary>
/// Вспомогательные методы управления процессом.
/// </summary>
internal static class ProcessUtils
{
    /// <summary>
    /// Запускает процесс с указанным набором ограничений.
    /// </summary>
    public static Process Start(ProcessSandboxStartInfo startInfo)
    {
        IProcessRunner processRunner = OperatingSystem.IsLinux()
            ? Linux.SandboxProcessRunner.INSTANCE
            : DefaultProcessRunner.INSTANCE;

        return processRunner.Start(startInfo);
    }


    /// <summary>
    /// Преобразует код завершения процесса в известный статус, если это возможно.
    /// </summary>
    /// <remarks>
    /// Код завершения процесса может быть в диапазоне от <c>0</c> до <c>255</c>, то есть <c>256</c> возможных значений.
    /// Значения больше <c>255</c> преобразуются системой в число, полученное путем деления по модулю <c>256</c>.
    /// Принято, что код <c>0</c> означает успешное завершение процесса, без ошибок; иные значения обычно рассматриваются
    /// как ошибка. Обычно значения <c>1</c>, <c>2</c>, <c>126-165</c> и <c>255</c> имеют специальное назначение и не должны
    /// переопределяться на уровне прикладного кода. Интерпретация значений зависит от особенностей среды исполнения.
    /// </remarks>
    public static int TranslateExitCode(int exitCode)
    {
        return (exitCode > 0 && OperatingSystem.IsLinux())
            ? Linux.ProcessInterop.TranslateExitCode(exitCode)
            : exitCode;
    }


    /// <summary>
    /// Проверяет, имеет ли наблюдаемый процесс дочерние.
    /// </summary>
    /// <param name="observableProcessId">Идентификатор наблюдаемого процесса.</param>
    /// <returns><c>true</c>, если наблюдаемый процесс имеет дочерние.</returns>
    public static bool CheckObservableProcessHasChildren(int observableProcessId)
    {
        return OperatingSystem.IsLinux()
            && Linux.ProcessInterop.CheckObservableProcessHasChildren(observableProcessId);
    }

    /// <summary>
    /// Завершает дерево наблюдаемых процессов.
    /// </summary>
    /// <param name="observableProcess">Наблюдаемый процесс.</param>
    /// <param name="killProcess">Принудительно завершить работу наблюдаемого процесса.</param>
    /// <returns><c>true</c>, если наблюдаемый процесс имел дочерние.</returns>
    public static bool TerminateObservableProcessTree(Process observableProcess, bool killProcess)
    {
        if (!OperatingSystem.IsLinux())
        {
            try
            {
                if (killProcess)
                {
                    try
                    {
                        observableProcess.Kill(true);
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
            finally
            {
                observableProcess.Dispose();
            }

            return false;
        }

        var observableProcessId = observableProcess.Id;
        observableProcess.Dispose();

        return Linux.ProcessInterop.TerminateObservableProcessTree(observableProcessId);
    }
}
