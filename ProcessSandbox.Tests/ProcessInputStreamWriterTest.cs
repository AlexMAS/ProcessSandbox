using NUnit.Framework;

using ProcessSandbox.IO;

namespace ProcessSandbox;

[TestFixture]
public class ProcessInputStreamWriterTest
{
    [Test]
    public async Task ShouldTransferAllData()
    {
        // Given
        var inputStream = new MemoryStreamWriter();
        var inputStreamReady = new ManualResetEventSlim(true);
        var inputData = "Some input data";
        var inputStreamData = new FixedStreamReader(inputData);
        var inputStreamInvoked = new TaskCompletionSource();
        var target = new ProcessInputStreamWriter(() => inputStream, inputStreamReady, inputStreamData, () => false);

        // When
        TryAwait(target.BeginWriting());
        await inputStream.Disposed();

        // Then
        Assert.That(inputStream.GetAllText(), Is.EqualTo(inputData));
    }

    [Test]
    public async Task ShouldCancelWritingAfterDisposing()
    {
        // Given
        var inputStream = new MemoryStreamWriter();
        var inputStreamReady = new ManualResetEventSlim(true);
        var inputStreamData = new FixedStreamReader("Some input data");
        var inputStreamInvoked = new TaskCompletionSource();
        var target = new ProcessInputStreamWriter(() => inputStream, inputStreamReady, inputStreamData, () => false);

        // When
        TryAwait(target.BeginWriting());
        await inputStream.AnyWrite();
        target.Dispose();

        // Then
        await TryAwait(inputStream.Disposed());
    }

    [Test]
    public async Task ShouldCancelWritingAfterTermination()
    {
        // Given
        var inputStream = new MemoryStreamWriter();
        var inputStreamReady = new ManualResetEventSlim(true);
        var inputStreamData = new InfiniteStreamReader("Some input data");
        var inputStreamInvoked = new TaskCompletionSource();
        var isTerminated = false;
        var target = new ProcessInputStreamWriter(() => inputStream, inputStreamReady, inputStreamData, () => isTerminated);

        // When
        TryAwait(target.BeginWriting());
        await inputStream.AnyWrite();
        isTerminated = true;

        // Then
        await TryAwait(inputStream.Disposed());
    }

    [Test]
    public async Task ShouldCancelStuckReadingAfterDisposing()
    {
        // Given
        var inputStream = new MemoryStreamWriter();
        var inputStreamReady = new ManualResetEventSlim(true);
        var inputStreamData = new StuckStreamReader();
        var inputStreamInvoked = new TaskCompletionSource();
        var target = new ProcessInputStreamWriter(() => inputStream, inputStreamReady, inputStreamData, () => false);

        // When
        TryAwait(target.BeginWriting());
        await inputStreamData.AnyRead();
        target.Dispose();

        // Then
        await TryAwait(inputStream.Disposed());
    }

    [Test]
    public async Task ShouldNotStartWritingIfStreamIsNeverReady()
    {
        // Given
        var inputStream = new MemoryStreamWriter();
        var inputStreamNeverReady = new ManualResetEventSlim(false);
        var inputStreamData = new FixedStreamReader("Some input data");
        var target = new ProcessInputStreamWriter(() => inputStream, inputStreamNeverReady, inputStreamData, () => false);

        // When
        TryAwait(target.BeginWriting());

        // Then
        await TryAwaitTimeout(inputStream.AnyWrite());
    }

    [Test]
    public async Task ShouldNotStartWritingIfStreamIsNotReady()
    {
        // Given
        var inputStream = new MemoryStreamWriter();
        var outputStreamIsNotReady = new ManualResetEventSlim(false);
        var inputStreamData = new FixedStreamReader("Some input data");
        var target = new ProcessInputStreamWriter(() => inputStream, outputStreamIsNotReady, inputStreamData, () => false);

        // When
        TryAwait(target.BeginWriting());

        // Then
        await TryAwaitTimeout(inputStream.AnyWrite());
    }

    [Test]
    public async Task ShouldCancelStuckWritingAfterDisposing()
    {
        // Given
        var inputStream = new StuckStreamWriter();
        var inputStreamReady = new ManualResetEventSlim(true);
        var inputStreamData = new FixedStreamReader("Some input data");
        var inputStreamInvoked = new TaskCompletionSource();
        var target = new ProcessInputStreamWriter(() => inputStream, inputStreamReady, inputStreamData, () => false);

        // When
        TryAwait(target.BeginWriting());
        await inputStream.AnyWrite();
        target.Dispose();

        // Then
        await TryAwait(inputStream.Disposed());
    }


    private static void TryAwait(ManualResetEventSlim @event)
    {
        var eventSet = @event.Wait(5000);
        Assert.That(eventSet, Is.EqualTo(true), "The event has not been set.");
    }

    private static async Task TryAwait(Task task)
    {
        var firstCompleted = await Task.WhenAny(task, Task.Delay(5000));
        Assert.That(firstCompleted, Is.EqualTo(task), "The task has not been completed.");
    }

    private static async Task TryAwaitTimeout(Task task)
    {
        var firstCompleted = await Task.WhenAny(task, Task.Delay(2000));
        Assert.That(firstCompleted, Is.Not.EqualTo(task), "The task must not completed.");
    }
}
