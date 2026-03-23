#nullable enable
using UPS.WWRR.Business.Common.Helper;

namespace UPS.WWRR.UnitTests.HelpersTests;

public class ParseCsvWithValidationHelperTests
{
    #region Empty / Null Content

    [Fact]
    public void ParseCsvWithValidation_NullContent_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(null!));
    }

    [Fact]
    public void ParseCsvWithValidation_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(string.Empty));
    }

    [Fact]
    public void ParseCsvWithValidation_WhitespaceOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation("   "));
    }

    #endregion

    #region Header Validation

    [Fact]
    public void ParseCsvWithValidation_UnexpectedHeader_ThrowsFormatException()
    {
        var csv = "Wrong,Header\nval1,val2";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    [Fact]
    public void ParseCsvWithValidation_SingleColumnHeader_ThrowsFormatException()
    {
        var csv = "TableName\nTALTCCY";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    [Fact]
    public void ParseCsvWithValidation_ExtraUnexpectedColumn_ThrowsFormatException()
    {
        var csv = "TableName,FileExtractName,Destination,Extra\nTALTCCY,TALTCCY_2026_03_11_1.csv,SRC,bonus";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    #endregion

    #region Valid Two-Column Header (TableName, FileExtractName)

    [Fact]
    public void ParseCsvWithValidation_ValidTwoColumnHeader_ReturnsHeaderAndRows()
    {
        var csv = "TableName,FileExtractName\nTALTCCY,TALTCCY_2026_03_11_1.csv";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Equal(2, rows.Count);
        Assert.Equal("TableName", rows[0][0]);
        Assert.Equal("FileExtractName", rows[0][1]);
        Assert.Equal("TALTCCY", rows[1][0]);
        Assert.Equal("TALTCCY_2026_03_11_1.csv", rows[1][1]);
    }

    [Fact]
    public void ParseCsvWithValidation_TwoColumnHeaderCaseInsensitive_Succeeds()
    {
        var csv = "tablename,fileextractname\nTALTCCY,TALTCCY_2026_03_11_1.csv";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void ParseCsvWithValidation_TwoColumnMultipleRows_ReturnsAll()
    {
        var csv = "TableName,FileExtractName\nTALTCCY,TALTCCY_2026_03_11_1.csv\nTASYBRL,TASYBRL_2026_03_11_2.csv";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Equal(3, rows.Count); // 1 header + 2 data
        Assert.Equal("TALTCCY", rows[1][0]);
        Assert.Equal("TASYBRL", rows[2][0]);
    }

    #endregion

    #region Valid Three-Column Header (TableName, FileExtractName, Destination)

    [Fact]
    public void ParseCsvWithValidation_ValidThreeColumnHeader_ReturnsHeaderAndRows()
    {
        var csv = "TableName,FileExtractName,Destination\nTALTCCY,TALTCCY_2026_03_11_1.csv,SRC";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Equal(2, rows.Count);
        Assert.Equal(3, rows[0].Length);
        Assert.Equal("Destination", rows[0][2]);
        Assert.Equal("SRC", rows[1][2]);
    }

    [Fact]
    public void ParseCsvWithValidation_ThreeColumnCaseInsensitive_Succeeds()
    {
        var csv = "TABLENAME,FILEEXTRACTNAME,DESTINATION\nTALTCCY,TALTCCY_2026_03_11_1.csv,SRC";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Equal(2, rows.Count);
    }

    #endregion

    #region Row-Level Validation

    [Fact]
    public void ParseCsvWithValidation_EmptyTableName_ThrowsFormatException()
    {
        var csv = "TableName,FileExtractName\n,TALTCCY_2026_03_11_1.csv";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    [Fact]
    public void ParseCsvWithValidation_EmptyFileExtractName_ThrowsFormatException()
    {
        var csv = "TableName,FileExtractName\nTALTCCY,";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    [Fact]
    public void ParseCsvWithValidation_FileExtractNameNotCsv_ThrowsFormatException()
    {
        var csv = "TableName,FileExtractName\nTALTCCY,TALTCCY_2026_03_11_1.txt";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    #endregion

    #region Header Only (No Data Rows)

    [Fact]
    public void ParseCsvWithValidation_HeaderOnly_ReturnsHeaderRow()
    {
        var csv = "TableName,FileExtractName";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Single(rows); // Just the header
        Assert.Equal("TableName", rows[0][0]);
    }

    #endregion

    #region Trimming

    [Fact]
    public void ParseCsvWithValidation_ValuesWithWhitespace_AresTrimmed()
    {
        var csv = "TableName,FileExtractName\n  TALTCCY  ,  TALTCCY_2026_03_11_1.csv  ";
        var rows = ParseCsvWithValidationHelper.ParseCsvWithValidation(csv);

        Assert.Equal("TALTCCY", rows[1][0]);
        Assert.Equal("TALTCCY_2026_03_11_1.csv", rows[1][1]);
    }

    #endregion

    #region Bad Data / Malformed CSV

    [Fact]
    public void ParseCsvWithValidation_MalformedQuotedField_ThrowsFormatException()
    {
        // A field with an unclosed quote triggers the BadDataFound callback
        var csv = "TableName,FileExtractName\n\"TALTCCY,TALTCCY_2026_03_11_1.csv";
        Assert.Throws<FormatException>(() => ParseCsvWithValidationHelper.ParseCsvWithValidation(csv));
    }

    #endregion
}
