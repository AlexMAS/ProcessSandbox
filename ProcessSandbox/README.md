# ProcessSandbox

Provides a simple and lightweight way to run processes with given limits (CPU, RAM, I/O etc).

The library is designed for Linux, nonetheless it can be used on Windows but with some limitations in functionality.
For more details see the project repository.

## Example

The next example runs a process with CPU and memory limits, and prints the execution statistics afterwards.

```cs
var processSandbox = new ProcessSandbox(new ProcessSandboxStartInfo
{
    Command = "my-calc",
    Arguments = [ "1", "+", "2" ],
    CpuLimit = TimeSpan.FromSeconds(1),
    MemoryLimit = 1_000_000
});

var exitCode = await processSandbox.Start();

if (exitCode != 0 && !processSandbox.SelfCompletion)
{
    switch (exitCode)
    {
        case (int)SpecialExitCode.CpuLimit:
            Console.WriteLine("CPU limit exceeded!");
            break;
        case (int)SpecialExitCode.MemoryLimit:
            Console.WriteLine("Memory limit exceeded!");
            break;
        // ...
    }
}

Console.WriteLine("Exit Code          : " + exitCode);
Console.WriteLine("Elapsed Time, ms   : " + processSandbox.ElapsedTime.TotalMilliseconds);
Console.WriteLine("CPU Usage, ms      : " + processSandbox.CpuUsage.TotalMilliseconds);
Console.WriteLine("Memory Usage, bytes: " + processSandbox.MemoryUsage);
```
