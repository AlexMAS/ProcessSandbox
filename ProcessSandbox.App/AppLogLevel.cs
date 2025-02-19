namespace ProcessSandbox;

internal record AppLogLevel(string Name, int Index)
{
    public static readonly AppLogLevel Fatal = new("fatal", 0);
    public static readonly AppLogLevel Error = new("error", 1);
    public static readonly AppLogLevel Warn = new("warn", 2);
    public static readonly AppLogLevel Info = new("info", 3);
    public static readonly AppLogLevel Debug = new("debug", 4);

    private static readonly Dictionary<string, AppLogLevel> Levels = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { Fatal.Name, Fatal },
        { Error.Name, Error },
        { Warn.Name, Warn },
        { Info.Name, Info },
        { Debug.Name, Debug }
    };

    public static AppLogLevel Parse(string? value) =>
        !string.IsNullOrWhiteSpace(value)
            ? Levels.TryGetValue(value.Trim(), out var level)
                ? level
                : Fatal
            : Fatal;

    public static bool operator >(AppLogLevel a, AppLogLevel b) => a.Index > b.Index;
    public static bool operator <(AppLogLevel a, AppLogLevel b) => a.Index < b.Index;
    public static bool operator >=(AppLogLevel a, AppLogLevel b) => a.Index >= b.Index;
    public static bool operator <=(AppLogLevel a, AppLogLevel b) => a.Index <= b.Index;

}
