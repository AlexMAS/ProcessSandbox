using System.Diagnostics;

namespace ProcessSandbox;

internal class DefaultProcessRunner : IProcessRunner
{
    public static readonly DefaultProcessRunner INSTANCE = new();

    public Process Start(ProcessSandboxStartInfo startInfo)
    {
        var process = new Process();
        process.StartInfo.UserName = startInfo.UserName;
        process.StartInfo.WorkingDirectory = startInfo.WorkingDirectory;
        process.StartInfo.FileName = startInfo.Command;

        if (startInfo.Arguments?.Count() > 0)
        {
            process.StartInfo.Arguments = string.Join(' ', startInfo.Arguments);
        }

        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();

        return process;
    }
}
