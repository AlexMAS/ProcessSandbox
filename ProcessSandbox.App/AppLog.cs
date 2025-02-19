using ProcessSandbox.Properties;

namespace ProcessSandbox;

internal static class AppLog
{
    private static readonly bool IsVerboseOutputNeeded = AppEnv.IsVerboseOutputNeeded;
    private static readonly bool IsSyslogOutputNeeded = AppEnv.IsSyslogOutputNeeded;


    public static void Info(string message)
    {
        if (IsVerboseOutputNeeded)
        {
            Console.Error.WriteLine(message);

            if (IsSyslogOutputNeeded && OperatingSystem.IsLinux())
            {
                Linux.Syslog.Info(Resources.AppName, message);
            }
        }
    }

    public static void Error(string message, Exception? error)
    {
        if (IsVerboseOutputNeeded)
        {
            var logMessage = (error != null)
                ? message + Environment.NewLine + error
                : message;

            Console.Error.WriteLine(logMessage);

            if (IsSyslogOutputNeeded && OperatingSystem.IsLinux())
            {
                Linux.Syslog.Error(Resources.AppName, message);
            }
        }
    }

    public static void Fatal(string message, Exception? error)
    {
        var logMessage = (error != null)
            ? message + Environment.NewLine + error
            : message;

        Console.Error.WriteLine(logMessage);

        if (IsSyslogOutputNeeded && OperatingSystem.IsLinux())
        {
            Linux.Syslog.Fatal(Resources.AppName, message);
        }
    }
}
