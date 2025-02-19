using NUnit.Framework;

namespace ProcessSandbox.Integration;

[TestFixture]
public class CppSandboxIntegrationTest : BaseSandboxIntegrationTest
{
    protected override string Lang => "cpp";

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
    public void ForkBomb()
    {
        // Given
        var testName = "fork-bomb";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ExitCode, Is.EqualTo((int)SpecialExitCode.HadChildren), "Unexpected Exit Code.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
        Assert.That(proc.Count() == NO_PROCESSES, Is.True, "The tested process has not been killed.");
    }

    [Test]
    public void ExecuteInSandbox()
    {
        // Given
        var testName = "execute-in-sandbox";
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
        Assert.That(output.Count(), Is.EqualTo(4), "Unexpected output.");
        Assert.That(output.ElementAt(0), Is.EqualTo($"Real UID = {SANDBOX_USER_UID}"), "Real UID must be the sandbox user UID.");
        Assert.That(output.ElementAt(1), Is.EqualTo($"Real GID = {SANDBOX_USER_GID}"), "Real GID must be the sandbox user GID.");
        Assert.That(output.ElementAt(2), Is.EqualTo($"Effective UID = {SANDBOX_USER_UID}"), "Effective UID must be the sandbox user UID.");
        Assert.That(output.ElementAt(3), Is.EqualTo($"Effective GID = {SANDBOX_USER_GID}"), "Effective GID must be the sandbox user GID.");
    }

    [Test]
    public void DetectFork()
    {
        // Given
        var testName = "detect-fork";
        var statistics = ReadExecutionStatistics(testName);
        var output = ReadOutFile(testName);
        var error = ReadErrFile(testName);
        var proc = ReadProcFile(testName);

        // Then
        Assert.That(statistics.ElapsedTime, Is.GreaterThan(0), "Negative Elapsed Time.");
        Assert.That(statistics.CpuUsage, Is.GreaterThanOrEqualTo(0), "Negative CPU Usage.");
        Assert.That(statistics.ElapsedTime, Is.GreaterThanOrEqualTo(statistics.CpuUsage), "Elapsed Time must be greater than CPU Usage.");
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.True, "The tested process must have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
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
        Assert.That(statistics.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Negative Memory Usage");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Empty, "The process output must be empty.");
        Assert.That(error, Is.Empty, "The process error must be empty.");
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
    public void InfiniteStderr()
    {
        // Given
        var testName = "infinite-stderr";
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
        Assert.That(statistics.StandardOutputLimitExceeded, Is.False, "Standard Output must not be exceeded.");
        Assert.That(statistics.StandardErrorLength, Is.GreaterThan(0), "Negative Standard Output Length.");
        Assert.That(statistics.StandardErrorLimitExceeded, Is.True, "Standard Error must be exceeded.");
        Assert.That(statistics.HadChildren, Is.False, "The tested process must not have children.");
        Assert.That(output, Is.Not.Empty, "The process output must not be empty.");
        Assert.That(string.Join("\n", output), Is.EqualTo("STDOUT: Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
        Assert.That(error, Is.Not.Empty, "The process error must not be empty.");
        Assert.That(string.Join("\n", error), Is.EqualTo("STDERR: Lorem ipsum dolor sit amet"));
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
}
