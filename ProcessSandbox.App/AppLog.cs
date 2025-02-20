using ProcessSandbox.Properties;

namespace ProcessSandbox;

internal static class AppLog
{
    private static readonly AppLogLevel LogLevel = AppEnv.LogLevel;
    private static readonly bool IsSyslogOutputNeeded = AppEnv.IsSyslogOutputNeeded;

#pragma warning disable CA1416 // Validate platform compatibility

    public static void Debug(string message) => Log(AppLogLevel.Fatal, message, null, Linux.Syslog.Fatal);

    public static void Info(string message) => Log(AppLogLevel.Fatal, message, null, Linux.Syslog.Info);

    public static void Error(string message, Exception? error = null) => Log(AppLogLevel.Fatal, message, error, Linux.Syslog.Error);

    public static void Fatal(string message, Exception? error = null) => Log(AppLogLevel.Fatal, message, error, Linux.Syslog.Fatal);

#pragma warning restore CA1416 // Validate platform compatibility

    private static void Log(AppLogLevel logLevel, string message, Exception? error, Action<string, string> syslog)
    {
        if (LogLevel >= logLevel)
        {
            var logMessage = (error != null)
            ? message + Environment.NewLine + error
            : message;

            Console.Error.WriteLine(logMessage);

            if (IsSyslogOutputNeeded && OperatingSystem.IsLinux())
            {
                syslog(Resources.AppName, message);
            }
        }
    }
}
