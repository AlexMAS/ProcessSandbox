using NUnit.Framework;

using System.Diagnostics;
using System.Runtime.Versioning;

using static ProcessSandbox.Linux.ProcessInterop;

namespace ProcessSandbox.Linux;

[TestFixture]
[Platform("linux")]
[SupportedOSPlatform("linux")]
public class InteropTest
{
    [Test]
    public void ShouldGetCurrentProcessId()
    {
        // When
        var currentProcessId = GetCurrentProcessId();

        // Then
        Assert.That(currentProcessId, Is.EqualTo(Process.GetCurrentProcess().Id));
    }

    [Test]
    public void ShouldGetProcessSessionId()
    {
        // Given
        var process = Process.GetCurrentProcess();

        // When
        var sessionId = GetProcessSessionId(process.Id);

        // Then
        Assert.That(sessionId, Is.EqualTo(process.SessionId));
    }

    [Test]
    public void ShouldGetParentProcesses()
    {
        // Given
        var process = Process.GetCurrentProcess();

        // When
        var parentProcesses = new HashSet<int>();
        GetParentProcesses(process.Id, parentProcesses);

        // Then
        Assert.That(parentProcesses, Is.Not.Empty);
    }
}
