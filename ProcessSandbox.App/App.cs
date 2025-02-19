using ProcessSandbox.Properties;

namespace ProcessSandbox;

internal class App
{
    public static async Task<int> Main(string[] args)
    {
        var startCommand = ProcessStartCommand.Parse(args);

        if (startCommand == null)
        {
            Console.WriteLine(Resources.Help);
            return (int)SpecialExitCode.WrongArgs;
        }

        var process = new ProcessSandbox(startCommand.StartInfo);
        int exitCode;

        try
        {
            exitCode = await process.Start();

            if (exitCode == (int)SpecialExitCode.CannotStartProcess)
            {
                AppLog.Fatal("Cannot start the process.", process.TerminationReason);
            }
        }
        catch (Exception e)
        {
            AppLog.Fatal("An unexpected exception on the process start.", e);
            process.Terminate(exitCode = (int)SpecialExitCode.UnexpectedError);
        }

        ProcessStatisticsFormatter.Format(startCommand.ResultFile, exitCode, process);

        return exitCode;
    }
}
