namespace ProcessSandbox.Integration;

public abstract class BaseSandboxIntegrationTest : BaseIntegrationTest
{
    protected override string App => "sandbox";

    protected ExecutionStatistics ReadExecutionStatistics(string testName)
    {
        var testStatFile = Path.Combine(OutputPath!, testName, $"{testName}.{STAT_EXT}");
        return ReadExecutionStatisticsFile(testStatFile);
    }

    protected IEnumerable<string> ReadOutFile(string testName)
    {
        return ReadFile(testName, OUT_EXT);
    }

    protected IEnumerable<string> ReadErrFile(string testName)
    {
        return ReadFile(testName, ERR_EXT);
    }

    protected IEnumerable<string> ReadProcFile(string testName)
    {
        return ReadFile(testName, PROC_EXT);
    }

    private IEnumerable<string> ReadFile(string testName, string ext)
    {
        var testResultFile = Path.Combine(OutputPath!, testName, $"{testName}.{ext}");
        return ReadFileLines(testResultFile);
    }
}
