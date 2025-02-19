namespace ProcessSandbox.Linux;

/// <summary>
/// Состояние процесса.
/// </summary>
/// <remarks>
/// Подробности см. в разделе <a href="https://man7.org/linux/man-pages/man5/proc.5.html">/proc/pid/stat</a>.
/// </remarks>
internal struct ProcessStat
{
    /// <summary>
    /// Идентификатор процесса.
    /// </summary>
    public int Id;

    /// <summary>
    /// Имя исполняемого файла.
    /// </summary>
    public string? Command;

    /// <summary>
    /// Состояние процесса.
    /// </summary>
    public char State;

    /// <summary>
    /// Идентификатор родительского процесса.
    /// </summary>
    public int ParentId;

    /// <summary>
    /// Идентификатор группы процессов.
    /// </summary>
    public int GroupId;

    /// <summary>
    /// Идентификатор сессии.
    /// </summary>
    public int SessionId;
}
