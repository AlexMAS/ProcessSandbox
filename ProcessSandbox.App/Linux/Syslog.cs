using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ProcessSandbox.Linux;

/// <summary>
/// Системный лог Linux.
/// </summary>
/// <remarks>
/// Документация <a href="https://linux.die.net/man/3/syslog">syslog</a>.
/// </remarks>
[SupportedOSPlatform("linux")]
internal class Syslog
{
    [DllImport("libc")]
    private static extern void openlog(nint ident, Option option, Facility facility);

    [DllImport("libc")]
    private static extern void syslog(int priority, string message);

    [DllImport("libc")]
    private static extern void closelog();


    public static void Info(string appName, string message)
    {
        Write(Level.Info, appName, message);
    }

    public static void Error(string appName, string message)
    {
        Write(Level.Err, appName, message);
    }

    public static void Fatal(string appName, string message)
    {
        Write(Level.Crit, appName, message);
    }

    public static void Write(Level level, string appName, string message)
    {
        if (string.IsNullOrWhiteSpace(appName) && string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        nint ident = Marshal.StringToHGlobalAnsi(appName);
        openlog(ident, Option.Console | Option.Pid | Option.PrintError, Facility.User);

        try
        {
            var priority = (int)Facility.User | (int)level;

            // Split multi-line messages, otherwise we end up with "line1 #012 line2 #012 line3" etc
            foreach (var line in message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                syslog(priority, line.Trim());
            }
        }
        finally
        {
            closelog();
            Marshal.FreeHGlobal(ident);
        }
    }


    [Flags]
    private enum Option
    {
        Pid = 0x01,
        Console = 0x02,
        Delay = 0x04,
        NoDelay = 0x08,
        NoWait = 0x10,
        PrintError = 0x20
    }


    [Flags]
    private enum Facility
    {
        User = 1 << 3, // Removed other unused enum values for brevity
    }


    [Flags]
    public enum Level
    {
        Emerg = 0,
        Alert = 1,
        Crit = 2,
        Err = 3,
        Warning = 4,
        Notice = 5,
        Info = 6,
        Debug = 7
    }
}
