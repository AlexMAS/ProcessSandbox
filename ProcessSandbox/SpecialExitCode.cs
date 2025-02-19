namespace ProcessSandbox;

/// <summary>
/// Предопределенные коды возврата.
/// </summary>
public enum SpecialExitCode : int
{
    /// <summary>
    /// Неправильное количество аргументов запуска или их формат.
    /// </summary>
    WrongArgs = 200,

    /// <summary>
    /// При запуске произошла непредвиденная ошибка.
    /// </summary>
    CannotStartProcess = 201,

    /// <summary>
    /// Ошибка перенаправления потока стандартного вывода (stdout).
    /// </summary>
    CannotRedirectStandardOutput = 202,

    /// <summary>
    /// Ошибка перенаправления потока стандартного вывода ошибок (stderr).
    /// </summary>
    CannotRedirectStandardError = 203,

    /// <summary>
    /// Ошибка перенаправления потока стандартного ввода (stdin).
    /// </summary>
    CannotRedirectStandardInput = 204,

    /// <summary>
    /// Превышено общее время работы процесса.
    /// </summary>
    TotalTimeout = 205,

    /// <summary>
    /// Превышено время использования CPU.
    /// </summary>
    CpuLimit = 206,

    /// <summary>
    /// Превышен объем используемой памяти.
    /// </summary>
    MemoryLimit = 207,

    /// <summary>
    /// Обнаружены дочерние процессы.
    /// </summary>
    HadChildren = 208,

    /// <summary>
    /// Непредвиденная ошибка.
    /// </summary>
    UnexpectedError = 209
}
