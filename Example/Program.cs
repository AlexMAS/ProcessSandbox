namespace Example;

using ProcessSandbox;

internal class Program
{
    static async Task Main(string[] args)
    {
        var processSandbox = new ProcessSandbox(new()
        {
            Command = args[0],
            Arguments = args[1..],
            CpuLimit = TimeSpan.FromSeconds(5),
            MemoryLimit = 10 * 1024 * 1024,
        });

        Console.WriteLine($"EXECUTE >>> {Environment.CommandLine}");
        Console.WriteLine($"OUTPUT >>>");

        await processSandbox.Start();

        Console.WriteLine("EXECUTION RESULT >>>");
        Console.WriteLine($"Exit Code          : {processSandbox.ExitCode}");
        Console.WriteLine($"Self Completion    : {processSandbox.SelfCompletion}");
        Console.WriteLine($"Elapsed Time, ms   : {processSandbox.ElapsedTime.TotalMilliseconds}");
        Console.WriteLine($"CPU Usage, ms      : {processSandbox.CpuUsage.TotalMilliseconds}");
        Console.WriteLine($"Memory Usage, bytes: {processSandbox.MemoryUsage}");
    }
}
