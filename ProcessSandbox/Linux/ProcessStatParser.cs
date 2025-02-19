using System.Buffers;
using System.Runtime.Versioning;
using System.Text;

namespace ProcessSandbox.Linux;

/// <summary>
/// Предоставляет методы для чтения состояния процесса.
/// </summary>
/// <remarks>
/// Подробности см. в разделе <a href="https://man7.org/linux/man-pages/man5/proc.5.html">/proc/pid/stat</a>.
/// </remarks>
[SupportedOSPlatform("linux")]
internal static class ProcessStatParser
{
    private const int BUFFER_SIZE = 4096;

    /// <summary>
    /// Возвращает состояние указанного процесса.
    /// </summary>
    public static ProcessStat GetProcessStat(int processId)
    {
        return TryReadFile($"/proc/{processId}/stat", out var stat)
            ? ParseProcessStat(stat!)
            : default;
    }

    /// <summary>
    /// Возвращает состояние процесса на содержимого файла статуса.
    /// </summary>
    public static ProcessStat ParseProcessStat(string stat)
    {
        var result = default(ProcessStat);

        var offset = 0;

        _ = TryReadNextInt32(stat, ref offset, out result.Id)
            && TryReadNextTermInParentheses(stat, ref offset, out result.Command)
            && TryReadNextChar(stat, ref offset, out result.State)
            && TryReadNextInt32(stat, ref offset, out result.ParentId)
            && TryReadNextInt32(stat, ref offset, out result.GroupId)
            && TryReadNextInt32(stat, ref offset, out result.SessionId)
            ;

        return result;
    }

    private static bool TryReadFile(string path, out string? contents)
    {
        var bytes = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        var count = 0;

        try
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1, useAsync: false);

            while (true)
            {
                var read = fileStream.Read(bytes, count, bytes.Length - count);

                if (read == 0)
                {
                    contents = Encoding.UTF8.GetString(bytes, 0, count);
                    return true;
                }

                count += read;

                if (count >= bytes.Length)
                {
                    var temp = ArrayPool<byte>.Shared.Rent(bytes.Length * 2);
                    Array.Copy(bytes, temp, bytes.Length);
                    var toReturn = bytes;
                    bytes = temp;
                    ArrayPool<byte>.Shared.Return(toReturn);
                }
            }
        }
        catch (Exception ex) when (ex is IOException || ex.InnerException is IOException)
        {
            contents = null;
            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    private static bool TryReadNextInt32(string content, ref int offset, out int value)
    {
        value = default;

        return TryReadNextTerm(content, ref offset, out var term)
            && int.TryParse(term, out value);
    }

    private static bool TryReadNextInt64(string content, ref int offset, out long value)
    {
        value = default;

        return TryReadNextTerm(content, ref offset, out var term)
            && long.TryParse(term, out value);
    }

    private static bool TryReadNextUInt64(string content, ref int offset, out ulong value)
    {
        value = default;

        return TryReadNextTerm(content, ref offset, out var term)
            && ulong.TryParse(term, out value);
    }

    private static bool TryReadNextChar(string content, ref int offset, out char value)
    {
        value = default;

        return TryReadNextTerm(content, ref offset, out var term)
            && char.TryParse(term, out value);
    }

    private static bool TryReadNextTerm(string content, ref int offset, out string? value)
    {
        var valueStartedAt = offset;
        for (; offset < content.Length && !char.IsWhiteSpace(content[offset]); ++offset) { }
        var valueLength = offset - valueStartedAt;

        MoveToNextTerm(content, ref offset);

        if (valueLength > 0)
        {
            value = content.Substring(valueStartedAt, valueLength);
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryReadNextTermInParentheses(string content, ref int offset, out string? value)
    {
        if ((offset + 1) >= content.Length || content[offset] != '(')
        {
            value = null;
            return false;
        }

        var valueStartedAt = ++offset;
        for (; offset < content.Length && content[offset] != ')'; ++offset) { }
        var valueLength = offset - valueStartedAt;

        MoveToNextTerm(content, ref offset);

        if (valueLength > 0)
        {
            value = content.Substring(valueStartedAt, valueLength);
            return true;
        }

        value = null;
        return false;
    }

    private static void MoveToNextTerm(string content, ref int offset)
    {
        if (offset < content.Length)
        {
            ++offset;

            if (offset < content.Length && char.IsWhiteSpace(content[offset]))
            {
                ++offset;
            }
        }
    }
}
