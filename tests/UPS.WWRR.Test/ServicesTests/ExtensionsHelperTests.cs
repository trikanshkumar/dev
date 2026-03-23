#nullable enable
using System.ComponentModel;
using UPS.WWRR.Business.Extensions;

namespace UPS.WWRR.UnitTests.ServicesTests;

public class ExtensionsHelperTests
{
    #region DateTimeExtensions Tests

    [Fact]
    public void ToUnixTime_ReturnsCorrectUnixTime_ForKnownDate()
    {
        // Arrange - January 1, 2020 00:00:00 UTC = 1577836800
        var dateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = dateTime.ToUnixTime();

        // Assert
        Assert.Equal("1577836800", result);
    }

    [Fact]
    public void ToUnixTime_ReturnsCorrectUnixTime_ForCurrentDate()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var result = dateTime.ToUnixTime();

        // Assert
        Assert.True(long.TryParse(result, out var unixTime));
        Assert.True(unixTime > 0);
    }

    [Fact]
    public void ToUnixTimeMilliSeconds_ReturnsCorrectUnixTime_ForKnownDate()
    {
        // Arrange - January 1, 2020 00:00:00.500 UTC = 1577836800500
        var dateTime = new DateTime(2020, 1, 1, 0, 0, 0, 500, DateTimeKind.Utc);

        // Act
        var result = dateTime.ToUnixTimeMilliSeconds();

        // Assert
        Assert.Equal("1577836800500", result);
    }

    [Fact]
    public void ToUnixTimeMilliSeconds_ReturnsLargerValueThanSeconds()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var seconds = dateTime.ToUnixTime();
        var milliseconds = dateTime.ToUnixTimeMilliSeconds();

        // Assert
        Assert.True(long.Parse(milliseconds) > long.Parse(seconds));
    }

    #endregion

    #region Enumerations Tests

    private enum TestEnum
    {
        [Description("First Value Description")]
        FirstValue,
        
        [Description("Second Value Description")]
        SecondValue,
        
        NoDescription
    }

    [Fact]
    public void GetDescription_ReturnsDescription_WhenAttributeExists()
    {
        // Act
        var result = Enumerations.GetDescription(TestEnum.FirstValue);

        // Assert
        Assert.Equal("First Value Description", result);
    }

    [Fact]
    public void GetDescription_ReturnsEnumName_WhenNoDescriptionAttribute()
    {
        // Act
        var result = Enumerations.GetDescription(TestEnum.NoDescription);

        // Assert
        Assert.Equal("NoDescription", result);
    }

    [Fact]
    public void GetDescription_ReturnsDifferentDescriptions_ForDifferentValues()
    {
        // Act
        var first = Enumerations.GetDescription(TestEnum.FirstValue);
        var second = Enumerations.GetDescription(TestEnum.SecondValue);

        // Assert
        Assert.NotEqual(first, second);
        Assert.Equal("First Value Description", first);
        Assert.Equal("Second Value Description", second);
    }

    #endregion

    #region StringExtension Tests

    [Fact]
    public void UnixTimeSecondsToDateTime_ReturnsCorrectDateTime_ForKnownTimestamp()
    {
        // Arrange - 1577836800 = January 1, 2020 00:00:00 UTC
        var unixTime = "1577836800";

        // Act
        var result = unixTime.UnixTimeSecondsToDateTime();

        // Assert
        Assert.Equal(2020, result.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(1, result.Day);
    }

    [Fact]
    public void UnixTimeMillisecondsToDateTime_ReturnsCorrectDateTime_ForKnownTimestamp()
    {
        // Arrange - 1577836800500 = January 1, 2020 00:00:00.500 UTC
        var unixTime = "1577836800500";

        // Act
        var result = unixTime.UnixTimeMillisecondsToDateTime();

        // Assert
        Assert.Equal(2020, result.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(1, result.Day);
        Assert.Equal(500, result.Millisecond);
    }

    [Fact]
    public void UnixTimeSecondsToDateTime_RoundTrips_WithToUnixTime()
    {
        // Arrange
        var originalDate = new DateTime(2023, 6, 15, 12, 30, 45, DateTimeKind.Utc);
        var unixTime = originalDate.ToUnixTime();

        // Act
        var result = unixTime.UnixTimeSecondsToDateTime();

        // Assert - should match to the second (milliseconds lost in seconds conversion)
        Assert.Equal(originalDate.Year, result.Year);
        Assert.Equal(originalDate.Month, result.Month);
        Assert.Equal(originalDate.Day, result.Day);
        Assert.Equal(originalDate.Hour, result.Hour);
        Assert.Equal(originalDate.Minute, result.Minute);
        Assert.Equal(originalDate.Second, result.Second);
    }

    [Fact]
    public void DecompressBytes_DecompressesGzipData_Correctly()
    {
        // Arrange - Create compressed data
        var originalText = "Hello, World! This is a test message for compression.";
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(originalText);
        byte[] compressedBytes;
        
        using (var output = new MemoryStream())
        {
            using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
            {
                gzip.Write(originalBytes, 0, originalBytes.Length);
            }
            compressedBytes = output.ToArray();
        }

        // Act
        var result = StringExtension.DecompressBytes(compressedBytes);

        // Assert
        Assert.Equal(originalBytes, result);
    }

    [Fact]
    public void DecompressMessage_DecompressesBase64GzipString_Correctly()
    {
        // Arrange - Create base64 encoded gzip compressed string
        var originalText = "Hello, World!";
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(originalText);
        string compressedBase64;
        
        using (var output = new MemoryStream())
        {
            using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
            {
                gzip.Write(originalBytes, 0, originalBytes.Length);
            }
            compressedBase64 = Convert.ToBase64String(output.ToArray());
        }

        // Act
        var result = StringExtension.DecompressMessage(compressedBase64);

        // Assert
        Assert.Equal(originalText, result);
    }

    [Fact]
    public void DecompressMessage_HandlesEmptyContent()
    {
        // Arrange - Create base64 encoded gzip compressed empty string
        var originalText = "";
        var originalBytes = System.Text.Encoding.UTF8.GetBytes(originalText);
        string compressedBase64;
        
        using (var output = new MemoryStream())
        {
            using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
            {
                gzip.Write(originalBytes, 0, originalBytes.Length);
            }
            compressedBase64 = Convert.ToBase64String(output.ToArray());
        }

        // Act
        var result = StringExtension.DecompressMessage(compressedBase64);

        // Assert
        Assert.Equal(originalText, result);
    }

    [Fact]
    public void UnixTimeSecondsToDateTime_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => "not_a_number".UnixTimeSecondsToDateTime());
    }

    [Fact]
    public void UnixTimeMillisecondsToDateTime_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => "not_a_number".UnixTimeMillisecondsToDateTime());
    }

    [Fact]
    public void UnixTimeSecondsToDateTime_ZeroValue_ReturnsEpoch()
    {
        var result = "0".UnixTimeSecondsToDateTime();

        Assert.Equal(1970, result.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(1, result.Day);
    }

    [Fact]
    public void UnixTimeMillisecondsToDateTime_ZeroValue_ReturnsEpoch()
    {
        var result = "0".UnixTimeMillisecondsToDateTime();

        Assert.Equal(1970, result.Year);
        Assert.Equal(1, result.Month);
        Assert.Equal(1, result.Day);
    }

    [Fact]
    public void ToUnixTime_EpochDate_ReturnsZero()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = epoch.ToUnixTime();

        Assert.Equal("0", result);
    }

    [Fact]
    public void ToUnixTimeMilliSeconds_EpochDate_ReturnsZero()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = epoch.ToUnixTimeMilliSeconds();

        Assert.Equal("0", result);
    }

    [Fact]
    public void DecompressMessage_InvalidBase64_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => StringExtension.DecompressMessage("!!!not-base64!!!"));
    }

    #endregion
}
