using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace ProcessSandbox.Linux;

[SupportedOSPlatform("linux")]
internal class SandboxProcessRunner : IProcessRunner
{
    public static readonly SandboxProcessRunner INSTANCE = new();

    private const string SANDBOX = "sandbox-exec";

    public Process Start(ProcessSandboxStartInfo startInfo)
    {
        var process = new Process();
        process.StartInfo.UserName = startInfo.UserName;
        process.StartInfo.WorkingDirectory = startInfo.WorkingDirectory;
        process.StartInfo.FileName = SANDBOX;
        process.StartInfo.Arguments = BuildSandboxArguments(startInfo);
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();

        return process;
    }

    private static string BuildSandboxArguments(ProcessSandboxStartInfo startInfo)
    {
        var arguments = new StringBuilder();
        AddCpuLimit(arguments, startInfo.CpuLimit, startInfo.CpuLimitAddition);
        AddThreadCountLimit(arguments, startInfo.ThreadCountLimit);
        AddFileSizeLimit(arguments, startInfo.FileSizeLimit);
        AddOpenFileLimit(arguments, startInfo.OpenFileLimit);
        AddCommand(arguments, startInfo.Command);
        AddArguments(arguments, startInfo.Arguments);
        return arguments.ToString();
    }

    private static void AddCpuLimit(StringBuilder arguments, TimeSpan cpuLimit, TimeSpan cpuLimitAddition)
    {
        if (cpuLimit <= TimeSpan.Zero || cpuLimit >= TimeSpan.MaxValue)
        {
            arguments.Append(-1);
        }
        else if (cpuLimitAddition <= TimeSpan.Zero || cpuLimitAddition >= TimeSpan.MaxValue)
        {
            arguments.Append((ulong)Math.Ceiling(cpuLimit.TotalSeconds));
        }
        else
        {
            arguments.Append((ulong)Math.Ceiling(cpuLimit.TotalSeconds + cpuLimitAddition.TotalSeconds));
        }
    }

    private static void AddThreadCountLimit(StringBuilder arguments, long threadCountLimit)
    {
        arguments.Append(' ');

        if (threadCountLimit <= 0 || threadCountLimit >= long.MaxValue)
        {
            arguments.Append(-1);
        }
        else
        {
            arguments.Append(threadCountLimit);
        }
    }

    private static void AddFileSizeLimit(StringBuilder arguments, long fileSizeLimit)
    {
        arguments.Append(' ');

        if (fileSizeLimit <= 0 || fileSizeLimit >= long.MaxValue)
        {
            arguments.Append(-1);
        }
        else
        {
            arguments.Append(fileSizeLimit);
        }
    }

    private static void AddOpenFileLimit(StringBuilder arguments, long openFileLimit)
    {
        arguments.Append(' ');

        if (openFileLimit <= 0 || openFileLimit >= long.MaxValue)
        {
            arguments.Append(-1);
        }
        else
        {
            arguments.Append(openFileLimit);
        }
    }

    private static void AddCommand(StringBuilder arguments, string command)
    {
        arguments.Append(' ').Append(command);
    }

    private static void AddArguments(StringBuilder arguments, IEnumerable<string> args)
    {
        if (args != null)
        {
            foreach (var arg in args)
            {
                arguments.Append(' ').Append(arg);
            }
        }
    }
}
