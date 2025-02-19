using NUnit.Framework;

using static ProcessSandbox.AppEnv;

namespace ProcessSandbox;

[TestFixture]
public class AppEnvTest
{
    [Test]
    public void ShouldReturnUserName() => AssertStringVariable(SANDBOX_USER, () => UserName);

    [Test]
    public void ShouldReturnTotalTimeout() => AssertDurationVariable(SANDBOX_TOTAL_TIMEOUT, () => TotalTimeout);

    [Test]
    public void ShouldReturnCpuLimit() => AssertDurationVariable(SANDBOX_CPU_LIMIT, () => CpuLimit);

    [Test]
    public void ShouldReturnCpuLimitAddition() => AssertDurationVariable(SANDBOX_CPU_LIMIT_ADDITION, () => CpuLimitAddition);

    [Test]
    public void ShouldReturnMemoryLimit() => AssertLongVariable(SANDBOX_MEMORY_LIMIT, () => MemoryLimit);

    [Test]
    public void ShouldReturnStandardOutputLimit() => AssertLongVariable(SANDBOX_STDOUT_LIMIT, () => StandardOutputLimit);

    [Test]
    public void ShouldReturnStandardErrorLimit() => AssertLongVariable(SANDBOX_STDERR_LIMIT, () => StandardErrorLimit);

    [Test]
    public void ShouldReturnThreadCountLimit() => AssertLongVariable(SANDBOX_THREAD_COUNT_LIMIT, () => ThreadCountLimit);

    [Test]
    public void ShouldReturnFileSizeLimit() => AssertLongVariable(SANDBOX_FILE_SIZE_LIMIT, () => FileSizeLimit);

    [Test]
    public void ShouldReturnOpenFileLimit() => AssertLongVariable(SANDBOX_OPEN_FILE_LIMIT, () => OpenFileLimit);

    [Test]
    public void ShouldReturnFalseByDefaultForChildrenForbidden() => AssertBoolVariable(SANDBOX_CHILDREN_FORBIDDEN, () => IsChildrenForbidden, null, false);

    [Test]
    public void ShouldReturnFalseWhenChildrenAllowed() => AssertBoolVariable(SANDBOX_CHILDREN_FORBIDDEN, () => IsChildrenForbidden, false, false);

    [Test]
    public void ShouldReturnTrueWhenChildrenForbidden() => AssertBoolVariable(SANDBOX_CHILDREN_FORBIDDEN, () => IsChildrenForbidden, true, true);

    [Test]
    public void ShouldReturnFatalByDefaultForLogLevel() => AssertVariable(SANDBOX_LOG_LEVEL, () => LogLevel, null as object, _ => AppLogLevel.Fatal);

    [Test]
    public void ShouldReturnFatalLogLevel() => AssertVariable(SANDBOX_LOG_LEVEL, () => LogLevel, "FATAL", _ => AppLogLevel.Fatal);

    [Test]
    public void ShouldReturnErrorLogLevel() => AssertVariable(SANDBOX_LOG_LEVEL, () => LogLevel, "ERROR", _ => AppLogLevel.Error);

    [Test]
    public void ShouldReturnWarnLogLevel() => AssertVariable(SANDBOX_LOG_LEVEL, () => LogLevel, "WARN", _ => AppLogLevel.Warn);

    [Test]
    public void ShouldReturnInfoLogLevel() => AssertVariable(SANDBOX_LOG_LEVEL, () => LogLevel, "INFO", _ => AppLogLevel.Info);

    [Test]
    public void ShouldReturnDebugLogLevel() => AssertVariable(SANDBOX_LOG_LEVEL, () => LogLevel, "DEBUG", _ => AppLogLevel.Debug);

    [Test]
    public void ShouldReturnFalseByDefaultForSyslogOutputNeeded() => AssertBoolVariable(SANDBOX_SYSLOG, () => IsSyslogOutputNeeded, null, false);

    [Test]
    public void ShouldReturnFalseWhenSyslogOutputNotNeeded() => AssertBoolVariable(SANDBOX_SYSLOG, () => IsSyslogOutputNeeded, false, false);

    [Test]
    public void ShouldReturnTrueWhenSyslogOutputNeeded() => AssertBoolVariable(SANDBOX_SYSLOG, () => IsSyslogOutputNeeded, true, true);


    public static void AssertStringVariable(string variable, Func<string?> accessor)
    {
        var variableValue = Guid.NewGuid().ToString();
        AssertVariable(variable, accessor, variableValue);
    }

    public static void AssertBoolVariable(string variable, Func<bool> accessor, bool? value, bool expected)
    {
        int? variableValue = value != null ? (value.Value ? 1 : 0) : null;
        AssertVariable(variable, accessor, variableValue, i => expected);
    }

    public static void AssertLongVariable(string variable, Func<long> accessor)
    {
        var variableValue = Random.Shared.NextInt64(1, 1_000_000);
        AssertVariable(variable, accessor, variableValue);
    }

    public static void AssertDurationVariable(string variable, Func<TimeSpan> accessor)
    {
        var variableValue = Random.Shared.NextInt64(1, 1_000_000);
        AssertVariable(variable, accessor, variableValue, i => TimeSpan.FromMilliseconds(i));
    }

    public static void AssertVariable<V, R>(string variable, Func<R> accessor, V value, Func<V, R>? converter = null)
    {
        object? expected = converter != null ? converter(value) : value;

        Environment.SetEnvironmentVariable(variable, value?.ToString(), EnvironmentVariableTarget.Process);
        object? actual = accessor();

        Assert.That(actual, Is.EqualTo(expected));
    }
}
