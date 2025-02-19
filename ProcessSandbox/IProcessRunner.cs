using System.Diagnostics;

namespace ProcessSandbox;

/// <summary>
/// Предоставляет метод для запуска процесса с указанным набором ограничений.
/// </summary>
internal interface IProcessRunner
{
    Process Start(ProcessSandboxStartInfo startInfo);
}
