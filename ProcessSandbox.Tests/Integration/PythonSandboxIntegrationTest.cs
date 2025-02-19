using NUnit.Framework;

namespace ProcessSandbox.Integration;

[TestFixture]
public class PythonSandboxIntegrationTest : BaseSandboxIntegrationTest
{
    protected override string Lang => "python";

    [Test]
    public void CpuLimit()
    {
        // Given
        var testName = "cpu-limit";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.CpuLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThan(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThan(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void CpuBomb()
    {
        // Given
        var testName = "cpu-bomb";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.CpuLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThan(0), "Negative CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThan(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void CreateSubprocess()
    {
        // Given
        var testName = "create-subprocess";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.TotalTimeout), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void InfiniteLoop()
    {
        // Given
        var testName = "infinite-loop";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.CpuLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThan(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void InfiniteRecursion()
    {
        // Given
        var testName = "infinite-recursion";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(1), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Not.Empty, "The process error must not be empty.");
        Assert.That(error.Any(i => i.Contains("recursion", StringComparison.OrdinalIgnoreCase) || i.Contains("overflow", StringComparison.OrdinalIgnoreCase)), Is.True, "The process error does not contain the stack overflow error.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void MemoryLimit()
    {
        // Given
        var testName = "memory-limit";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.MemoryLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThan(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void BigArray()
    {
        // Given
        var testName = "big-array";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.MemoryLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThan(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void TotalTimeout()
    {
        // Given
        var testName = "total-timeout";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.TotalTimeout), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void UnhandledException()
    {
        // Given
        var testName = "unhandled-exception";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(1), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Not.Empty, "The process error must not be empty.");
        Assert.That(error.Any(i => i.Contains("exception", StringComparison.OrdinalIgnoreCase)), Is.True, "The process error does not contain the exception.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void WithInAndOut()
    {
        // Given
        var testName = "with-in-and-out";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(0), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void WithoutInAndOut()
    {
        // Given
        var testName = "without-in-and-out";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(0), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.StandardOutputLength, Is.EqualTo(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardOutputLimitExceeded, Is.False, "Standard Output must not be exceeded.");
        Assert.That(statistics.StandardErrorLength, Is.EqualTo(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardErrorLimitExceeded, Is.False, "Standard Error must not be exceeded.");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void WithoutInButWithErr()
    {
        // Given
        var testName = "without-in-but-with-err";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(1), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.StandardOutputLength, Is.EqualTo(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardOutputLimitExceeded, Is.False, "Standard Output must not be exceeded.");
        Assert.That(statistics.StandardErrorLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardErrorLimitExceeded, Is.False, "Standard Error must not be exceeded.");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Not.Empty, "The process error must not be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void WithoutInButWithOutAndErr()
    {
        // Given
        var testName = "without-in-but-with-out-and-err";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(1), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.StandardOutputLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardOutputLimitExceeded, Is.False, "Standard Output must not be exceeded.");
        Assert.That(statistics.StandardErrorLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardErrorLimitExceeded, Is.False, "Standard Error must not be exceeded.");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(error, Is.Not.Empty, "The process error must not be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void WithoutInButWithOut()
    {
        // Given
        var testName = "without-in-but-with-out";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(0), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.StandardOutputLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardOutputLimitExceeded, Is.False, "Standard Output must not be exceeded.");
        Assert.That(statistics.StandardErrorLength, Is.EqualTo(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardErrorLimitExceeded, Is.False, "Standard Error must not be exceeded.");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void ForkBomb()
    {
        // Given
        var testName = "fork-bomb";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.HadChildren), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void SubprocessBomb()
    {
        // Given
        var testName = "subprocess-bomb";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        // TotalTimeout, возможно, из-за медлительности Python или subprocess.Popen() не сразу порождает процесс
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.TotalTimeout), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error.Count() == 0 || error.Any(i => i.Contains("Resource temporarily unavailable")), Is.True, "The process error contains the reason.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void InfiniteStdout()
    {
        // Given
        var testName = "infinite-stdout";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.CpuLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.StandardOutputLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardOutputLimitExceeded, Is.True, "Standard Output must be exceeded.");
        Assert.That(statistics.StandardErrorLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardErrorLimitExceeded, Is.False, "Standard Error must not be exceeded.");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(string.Join("\n", output), Is.EqualTo("STDOUT: Lorem ipsum dolor sit amet"));
        Assert.That(error, Is.Not.Empty, "The process error must not be empty.");
        Assert.That(string.Join("\n", error), Is.EqualTo("STDERR: Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void NoOutputTimeout()
    {
        // Given
        var testName = "no-output-timeout";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(0), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void BigInput()
    {
        // Given
        var testName = "big-input";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo(0), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(output.First(), Is.EqualTo("10485760"), "The process output must not be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void SolutionTimeout()
    {
        // Given
        var testName = "solution-timeout";
        var statistics = ReadExecutionStatistics(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.CpuLimit), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThan(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThan(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }
}
