using NUnit.Framework;

namespace ProcessSandbox.Integration;

public abstract class BaseIntegrationTest
{
    // Нет запущенных процессов - только shell wrappers и ps с заголовком
    protected const int NO_PROCESSES = 5;

    protected const string STAT_EXT = "stat";
    protected const string OUT_EXT = "out";
    protected const string ERR_EXT = "err";
    protected const string PROC_EXT = "proc";

    protected const string SANDBOX_USER = "sandbox";
    protected const int SANDBOX_USER_UID = 1888;
    protected const int SANDBOX_USER_GID = 1888;

    protected abstract string App { get; }
    protected abstract string Lang { get; }

    protected string? OutputPath { get; private set; }

    [SetUp]
    public void SetUp()
    {
        var resultOut = Environment.GetEnvironmentVariable("RESULT_OUT");

        // In case of debugging in VS
        if (string.IsNullOrWhiteSpace(resultOut) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VisualStudioEdition")))
        {
            resultOut = Environment.CurrentDirectory + @"\..\..\..\..\integration-tests-out";
        }

        if (string.IsNullOrWhiteSpace(resultOut))
        {
            throw new AssertionException("The RESULT_OUT variable is not defined.");
        }

        OutputPath = Path.Combine(resultOut!, App, Lang)!;
    }

    protected static ExecutionStatistics ReadExecutionStatisticsFile(string statFile)
    {
        var lines = ReadFileLines(statFile);

        if (lines.Count() != 10)
        {
            throw new AssertionException($"The {statFile} file has incorrect number of lines.");
        }

        try
        {
            var index = 0;

            return new ExecutionStatistics
            {
                ExitCode = int.Parse(lines.ElementAt(index++)),
                ElapsedTime = long.Parse(lines.ElementAt(index++)),
                CpuUsage = long.Parse(lines.ElementAt(index++)),
                MemoryUsage = long.Parse(lines.ElementAt(index++)),
                StandardOutputLength = long.Parse(lines.ElementAt(index++)),
                StandardOutputLimitExceeded = int.Parse(lines.ElementAt(index++)) > 0,
                StandardErrorLength = long.Parse(lines.ElementAt(index++)),
                StandardErrorLimitExceeded = int.Parse(lines.ElementAt(index++)) > 0,
                SelfCompletion = int.Parse(lines.ElementAt(index++)) > 0,
                HadChildren = int.Parse(lines.ElementAt(index++)) > 0
            };
        }
        catch (Exception e)
        {
            throw new AssertionException($"The {statFile} file has incorrect format.", e);
        }
    }

    protected static IEnumerable<string> ReadFileLines(string testResultFile)
    {
        if (!File.Exists(testResultFile))
        {
            throw new AssertionException($"The {testResultFile} not found.");
        }

        return File.ReadLines(testResultFile);
    }

    protected static string ReadFileText(string testResultFile)
    {
        if (!File.Exists(testResultFile))
        {
            throw new AssertionException($"The {testResultFile} not found.");
        }

        return File.ReadAllText(testResultFile);
    }

    protected static bool CannotBeStartedOrTerminatedByTimeLimit(int exitCode)
    {
        return exitCode == (int)SpecialExitCode.CannotStartProcess
            || exitCode == (int)SpecialExitCode.TotalTimeout
            || exitCode == (int)SpecialExitCode.CpuLimit;
    }


    protected record ExecutionStatistics
    {
        public required int ExitCode;
        public required long ElapsedTime;
        public required long CpuUsage;
        public required long MemoryUsage;
        public required long StandardOutputLength;
        public required bool StandardOutputLimitExceeded;
        public required long StandardErrorLength;
        public required bool StandardErrorLimitExceeded;
        public required bool SelfCompletion;
        public required bool HadChildren;
    }
}
