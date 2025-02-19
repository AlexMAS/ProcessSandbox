using NUnit.Framework;

using System.Text;

using ProcessSandbox.IO;

namespace ProcessSandbox;

[TestFixture]
public class ProcessOutputStreamReaderTest
{
    [Test]
    public void ShouldNotifyEOFWhenDisposeAfterCreate()
    {
        // Given
        var readOutput = new List<string?>();
        var outputStream = () => new FixedStreamReader("Something");
        var outputStreamReady = new ManualResetEventSlim(true);
        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, -1, () => false, (reader, readChars) => readOutput.Add(readChars));

        // When
        target.Dispose();

        // Then
        Assert.That(readOutput, Has.Exactly(1).EqualTo(null));
    }

    [Test]
    public void ShouldNotifyEOFWhenDisposeAfterStartingReading()
    {
        // Given
        var readOutput = new List<string?>();
        var outputStream = () => new FixedStreamReader("Something");
        var outputStreamReady = new ManualResetEventSlim(true);
        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, -1, () => false, (reader, readChars) => readOutput.Add(readChars));

        // When
        TryAwait(target.BeginReading());
        target.Dispose();

        // Then
        Assert.That(readOutput, Has.Count.GreaterThan(0));
        Assert.That(readOutput.Last(), Is.Null);
    }

    [Test]
    public async Task ShouldReadEntireStreamWithoutLimit()
    {
        // Given

        var output = "Process output string";
        var outputLimit = -1;
        var outputStream = () => new FixedStreamReader(output);
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwait(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(output.Length));
        Assert.That(readOutput.ToString(), Is.EqualTo(output));
    }

    [Test]
    public async Task ShouldReadStreamWithLimit()
    {
        // Given

        var output = "Process output string";
        var outputLimit = output.Length - 5;
        var outputStream = () => new FixedStreamReader(output);
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwait(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(outputLimit));
        Assert.That(readOutput.ToString(), Is.EqualTo(output[..outputLimit]));
    }

    [Test]
    public async Task ShouldReadStreamWithZeroLimit()
    {
        // Given

        var output = "Process output string";
        var outputLimit = 0;
        var outputStream = () => new FixedStreamReader(output);
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwait(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.Zero);
        Assert.That(readOutput.ToString(), Is.Empty);
    }

    [Test]
    public async Task ShouldReadStreamWithLargeLimit()
    {
        // Given

        var output = "Process output string";
        var outputLimit = 1024 * 1024; // >1Mb
        var outputStream = () => new FixedStreamReader(output);
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwait(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(output.Length));
        Assert.That(readOutput.ToString(), Is.EqualTo(output));
    }

    [Test]
    public async Task ShouldReadInfiniteStreamWithLimit()
    {
        // Given

        var outputLimit = 1234;
        var outputStream = () => new InfiniteStreamReader("Process output string");
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwait(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(outputLimit));
        Assert.That(target.OutputLimitExceeded, Is.EqualTo(true));
        Assert.That(readOutput.Length, Is.EqualTo(outputLimit));
    }

    [Test]
    public async Task ShouldCancelReadingAfterTermination()
    {
        // Given

        var outputLimit = -1;
        var outputStream = () => new InfiniteStreamReader("Process output string");
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();
        var isTerminated = false;

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => isTerminated, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await Task.Delay(1000);
        isTerminated = true;
        await TryAwait(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.GreaterThan(0));
        Assert.That(readOutput.Length, Is.GreaterThan(0));
    }

    [Test]
    public async Task ShouldCancelReadingAfterDisposing()
    {
        // Given

        var outputLimit = -1;
        var outputStream = () => new InfiniteStreamReader("Process output string");
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await Task.Delay(1000);
        target.Dispose();
        await TryAwait(readCompleted);

        // Then
        Assert.That(target.OutputLength, Is.GreaterThan(0));
        Assert.That(readOutput.Length, Is.GreaterThan(0));
    }

    [Test]
    public async Task ShouldCancelStuckReadingAfterDisposing()
    {
        // Given

        var outputLimit = -1;
        var outputStream = () => new StuckStreamReader();
        var outputStreamReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await Task.Delay(1000);
        target.Dispose();
        await TryAwait(readCompleted);

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(0));
        Assert.That(readOutput.Length, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldNotStartReadingIfStreamIsNeverReady()
    {
        // Given

        var output = "Process output string";
        var outputLimit = -1;
        var outputStream = () => new FixedStreamReader(output);
        var outputStreamNeverReady = new ManualResetEventSlim(false);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(outputStream, outputStreamNeverReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwaitTimeout(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(0));
        Assert.That(target.OutputLimitExceeded, Is.EqualTo(false));
        Assert.That(readOutput.ToString(), Is.EqualTo(""));
    }

    [Test]
    public async Task ShouldNotStartReadingIfStreamIsNotReady()
    {
        // Given

        var outputLimit = -1;
        var outputStreamIsNotReady = new ManualResetEventSlim(true);

        var readOutput = new StringBuilder();
        var readCompleted = new TaskCompletionSource<bool>();

        var target = new ProcessOutputStreamReader(() => null, outputStreamIsNotReady, outputLimit, () => false, (reader, readChars) =>
        {
            if (readChars != null)
            {
                readOutput.Append(readChars);
            }
            else
            {
                readCompleted.SetResult(true);
            }
        });

        // When
        TryAwait(target.BeginReading());
        await TryAwaitTimeout(readCompleted);
        target.Dispose();

        // Then
        Assert.That(target.OutputLength, Is.EqualTo(0));
        Assert.That(target.OutputLimitExceeded, Is.EqualTo(false));
        Assert.That(readOutput.ToString(), Is.EqualTo(""));
    }

    private static void TryAwait(ManualResetEventSlim @event)
    {
        var eventSet = @event.Wait(5000);
        Assert.That(eventSet, Is.EqualTo(true), "The event has not been set.");
    }

    private static async Task TryAwait<T>(TaskCompletionSource<T> taskSource)
    {
        var firstCompleted = await Task.WhenAny(taskSource.Task, Task.Delay(5000));
        Assert.That(firstCompleted, Is.EqualTo(taskSource.Task), "The task has not been completed.");
    }

    private static async Task TryAwaitTimeout<T>(TaskCompletionSource<T> taskSource)
    {
        var firstCompleted = await Task.WhenAny(taskSource.Task, Task.Delay(2000));
        Assert.That(firstCompleted, Is.Not.EqualTo(taskSource.Task), "The task must not completed.");
    }
}
