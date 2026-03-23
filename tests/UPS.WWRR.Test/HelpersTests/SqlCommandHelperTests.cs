#nullable enable
using UPS.WWRR.Business.Common.Helper;

namespace UPS.WWRR.UnitTests.HelpersTests;

public class SqlCommandHelperTests
{
    #region BuildCopyCommand Tests

    [Fact]
    public void BuildCopyCommand_WithHeaderTrue_IncludesHeaderOption()
    {
        var result = SqlCommandHelper.BuildCopyCommand("my_table", new[] { "col1", "col2" }, ",", hasHeader: true);

        Assert.Contains("COPY my_table (col1,col2) FROM STDIN WITH (", result);
        Assert.Contains("FORMAT CSV", result);
        Assert.Contains("DELIMITER ','", result);
        Assert.Contains("HEADER", result);
        Assert.Contains("NULL 'NULL'", result);
    }

    [Fact]
    public void BuildCopyCommand_WithHeaderFalse_OmitsHeaderOption()
    {
        var result = SqlCommandHelper.BuildCopyCommand("my_table", new[] { "col1" }, ",", hasHeader: false);

        Assert.DoesNotContain("HEADER", result);
        Assert.Contains("FORMAT CSV", result);
    }

    [Fact]
    public void BuildCopyCommand_TableNameIsLowercased()
    {
        var result = SqlCommandHelper.BuildCopyCommand("MY_TABLE", new[] { "col1" }, ",", hasHeader: false);

        Assert.StartsWith("COPY my_table", result);
    }

    [Fact]
    public void BuildCopyCommand_MultipleColumns_JoinedWithComma()
    {
        var result = SqlCommandHelper.BuildCopyCommand("tbl", new[] { "a", "b", "c" }, ",", hasHeader: false);

        Assert.Contains("(a,b,c)", result);
    }

    [Fact]
    public void BuildCopyCommand_CustomDelimiter_UsesIt()
    {
        var result = SqlCommandHelper.BuildCopyCommand("tbl", new[] { "a" }, "|", hasHeader: false);

        Assert.Contains("DELIMITER '|'", result);
    }

    [Fact]
    public void BuildCopyCommand_SingleColumn_FormatsCorrectly()
    {
        var result = SqlCommandHelper.BuildCopyCommand("tbl", new[] { "only_col" }, ",", hasHeader: true);

        Assert.Contains("(only_col)", result);
    }

    [Fact]
    public void BuildCopyCommand_IncludesQuoteAndEscapeOptions()
    {
        var result = SqlCommandHelper.BuildCopyCommand("tbl", new[] { "c" }, ",", hasHeader: false);

        Assert.Contains("QUOTE '\"'", result);
        Assert.Contains("ESCAPE '\"'", result);
    }

    #endregion

    #region BuildTruncateCommand Tests

    [Fact]
    public void BuildTruncateCommand_ValidTableName_ReturnsTruncateStatement()
    {
        var result = SqlCommandHelper.BuildTruncateCommand("my_table");

        Assert.Equal("TRUNCATE TABLE my_table", result);
    }

    [Fact]
    public void BuildTruncateCommand_TableNameIsLowercased()
    {
        var result = SqlCommandHelper.BuildTruncateCommand("MY_TABLE");

        Assert.Equal("TRUNCATE TABLE my_table", result);
    }

    [Fact]
    public void BuildTruncateCommand_MixedCaseTableName_IsLowercased()
    {
        var result = SqlCommandHelper.BuildTruncateCommand("Taltccy_Stg");

        Assert.Equal("TRUNCATE TABLE taltccy_stg", result);
    }

    [Fact]
    public void BuildTruncateCommand_NullTableName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SqlCommandHelper.BuildTruncateCommand(null!));
    }

    [Fact]
    public void BuildTruncateCommand_EmptyTableName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SqlCommandHelper.BuildTruncateCommand(string.Empty));
    }

    [Fact]
    public void BuildTruncateCommand_WhitespaceTableName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SqlCommandHelper.BuildTruncateCommand("   "));
    }

    #endregion
}
