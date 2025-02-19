using NUnit.Framework;

using static ProcessSandbox.AppEnv;

namespace ProcessSandbox;

[TestFixture]
public class ProcessStartCommandTest
{
    [Test]
    public void ShouldParseCommand()
    {
        // Given

        EnvironmentVariable(SANDBOX_USER, "some-user");
        EnvironmentVariable(SANDBOX_TOTAL_TIMEOUT, 5000);
        EnvironmentVariable(SANDBOX_CPU_LIMIT, 1000);
        EnvironmentVariable(SANDBOX_CPU_LIMIT_ADDITION, 1000);
        EnvironmentVariable(SANDBOX_MEMORY_LIMIT, 256000000);
        EnvironmentVariable(SANDBOX_STDOUT_LIMIT, 3000);
        EnvironmentVariable(SANDBOX_STDERR_LIMIT, 500);
        EnvironmentVariable(SANDBOX_THREAD_COUNT_LIMIT, 100);
        EnvironmentVariable(SANDBOX_FILE_SIZE_LIMIT, 1024);
        EnvironmentVariable(SANDBOX_OPEN_FILE_LIMIT, 200);
        EnvironmentVariable(SANDBOX_CHILDREN_FORBIDDEN, 1);

        var arguments = new[]
        {
            "/solution/123/app.stat",
            "/solution/123",
            "/solution/123/app.exe",
            "arg1",
            "arg2",
            "arg3"
        };

        // When
        var command = ProcessStartCommand.Parse(arguments);

        // Then

        Assert.That(command, Is.Not.Null);
        Assert.That(command!.StartInfo, Is.Not.Null);

        Assert.That(command!.ResultFile, Is.EqualTo("/solution/123/app.stat"));
        Assert.That(command!.StartInfo.WorkingDirectory, Is.EqualTo("/solution/123"));
        Assert.That(command!.StartInfo.Command, Is.EqualTo("/solution/123/app.exe"));
        Assert.That(command!.StartInfo.Arguments, Is.EqualTo(new[] { "arg1", "arg2", "arg3" }));

        Assert.That(command!.StartInfo.UserName, Is.EqualTo("some-user"));
        Assert.That(command!.StartInfo.TotalTimeout, Is.EqualTo(TimeSpan.FromMilliseconds(5000)));
        Assert.That(command!.StartInfo.CpuLimit, Is.EqualTo(TimeSpan.FromMilliseconds(1000)));
        Assert.That(command!.StartInfo.CpuLimitAddition, Is.EqualTo(TimeSpan.FromSeconds(1)));
        Assert.That(command!.StartInfo.MemoryLimit, Is.EqualTo(256000000));
        Assert.That(command!.StartInfo.StandardOutputLimit, Is.EqualTo(3000));
        Assert.That(command!.StartInfo.StandardErrorLimit, Is.EqualTo(500));
        Assert.That(command!.StartInfo.ThreadCountLimit, Is.EqualTo(100));
        Assert.That(command!.StartInfo.FileSizeLimit, Is.EqualTo(1024));
        Assert.That(command!.StartInfo.OpenFileLimit, Is.EqualTo(200));
        Assert.That(command!.StartInfo.IsChildrenForbidden, Is.True);
    }

    private static void EnvironmentVariable(string name, object value)
    {
        Environment.SetEnvironmentVariable(name, value.ToString(), EnvironmentVariableTarget.Process);
    }
}
