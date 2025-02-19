using NUnit.Framework;

namespace ProcessSandbox.Linux;

[TestFixture]
public class ProcessStatParserTest
{
    [Test]
    public void ShouldParseEmptyStat()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("");

        // Then
        Assert.That(stat.Id, Is.EqualTo(0));
        Assert.That(stat.Command, Is.Null);
        Assert.That(stat.State, Is.EqualTo(default(char)));
        Assert.That(stat.ParentId, Is.EqualTo(0));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseNormalStat()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (app) R 8 14 8 34816 182 4194560 677 54681 24 704 0 4 301 53 20 0 1 0 974235 4284416 879 18446744073709551615 94485065175040 94485065964445 140732383275712 0 0 0 65536 3686404 1266761467 1 0 0 17 7 0 0 1 0 0 94485066197744 94485066245988 94485073690624 140732383279795 140732383279800 140732383279800 140732383281130 0
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("app"));
        Assert.That(stat.State, Is.EqualTo('R'));
        Assert.That(stat.ParentId, Is.EqualTo(8));
        Assert.That(stat.GroupId, Is.EqualTo(14));
        Assert.That(stat.SessionId, Is.EqualTo(8));
    }

    [Test]
    public void ShouldParseExecutableWithWhiteSpaces()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (executable file name) R 8 14 8 34816 182 4194560 677 54681 24 704 0 4 301 53 20 0 1 0 974235 4284416 879 18446744073709551615 94485065175040 94485065964445 140732383275712 0 0 0 65536 3686404 1266761467 1 0 0 17 7 0 0 1 0 0 94485066197744 94485066245988 94485073690624 140732383279795 140732383279800 140732383279800 140732383281130 0
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("executable file name"));
        Assert.That(stat.State, Is.EqualTo('R'));
        Assert.That(stat.ParentId, Is.EqualTo(8));
        Assert.That(stat.GroupId, Is.EqualTo(14));
        Assert.That(stat.SessionId, Is.EqualTo(8));
    }

    [Test]
    public void ShouldParseStatTruncatedAtSessionId()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (app) R 8 14
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("app"));
        Assert.That(stat.State, Is.EqualTo('R'));
        Assert.That(stat.ParentId, Is.EqualTo(8));
        Assert.That(stat.GroupId, Is.EqualTo(14));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseStatTruncatedAtGroupId()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (app) R 8
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("app"));
        Assert.That(stat.State, Is.EqualTo('R'));
        Assert.That(stat.ParentId, Is.EqualTo(8));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseStatTruncatedAtParentId()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (app) R
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("app"));
        Assert.That(stat.State, Is.EqualTo('R'));
        Assert.That(stat.ParentId, Is.EqualTo(0));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseStatTruncatedAtState()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (app)
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("app"));
        Assert.That(stat.State, Is.EqualTo(default(char)));
        Assert.That(stat.ParentId, Is.EqualTo(0));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseStatTruncatedAtCommand()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.Null);
        Assert.That(stat.State, Is.EqualTo(default(char)));
        Assert.That(stat.ParentId, Is.EqualTo(0));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseStatWithBrokenCommand()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 (app
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.EqualTo("app"));
        Assert.That(stat.State, Is.EqualTo(default(char)));
        Assert.That(stat.ParentId, Is.EqualTo(0));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }

    [Test]
    public void ShouldParseUnexpectedCommand()
    {
        // When
        var stat = ProcessStatParser.ParseProcessStat("""
            14 without-parentheses
            """);

        // Then
        Assert.That(stat.Id, Is.EqualTo(14));
        Assert.That(stat.Command, Is.Null);
        Assert.That(stat.State, Is.EqualTo(default(char)));
        Assert.That(stat.ParentId, Is.EqualTo(0));
        Assert.That(stat.GroupId, Is.EqualTo(0));
        Assert.That(stat.SessionId, Is.EqualTo(0));
    }
}
