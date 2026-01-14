using UPS.WWRR.Business.Services;
using UPS.WWRR.UnitTests.CsvMockModels;

namespace UPS.WWRR.UnitTests.ServicesTests;

public class CsvValidatorTests
{
    [Fact]
    public async Task Validate_ReturnsSuccess_WhenGivenValidCsv()
    {
        var service = new CsvValidator();

        var result = await service.ValidateCsvAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-valid.csv");

        Assert.True(result.Success);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public async Task Validate_ReturnsError_WhenCsvContainsInvalidField()
    {
        var service = new CsvValidator();

        var result = await service.ValidateCsvAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-invalid-field.csv");

        Assert.False(result.Success);
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task Validate_ReturnsError_WhenCsvIsNotCorrectlyFormatted()
    {
        var service = new CsvValidator();

        var result = await service.ValidateCsvAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-bad-csv.csv");

        Assert.False(result.Success);
        Assert.Single(result.ValidationErrors);
    }

    // Tests for ValidateCsvChunkedAsync
    [Fact]
    public async Task ValidateChunked_ReturnsSuccess_WhenGivenValidCsv()
    {
        var service = new CsvValidator();

        var result = await service.ValidateCsvChunkedAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-valid.csv", chunkSize: 2);

        Assert.True(result.Success);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public async Task ValidateChunked_ReturnsError_WhenCsvContainsInvalidField()
    {
        var service = new CsvValidator();

        var result = await service.ValidateCsvChunkedAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-invalid-field.csv", chunkSize: 2);

        Assert.False(result.Success);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task ValidateChunked_ReturnsError_WhenCsvIsNotCorrectlyFormatted()
    {
        var service = new CsvValidator();

        var result = await service.ValidateCsvChunkedAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-bad-csv.csv", chunkSize: 2);

        Assert.False(result.Success);
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task ValidateChunked_ProcessesMultipleChunks_WhenDataExceedsChunkSize()
    {
        var service = new CsvValidator();

        // Using chunk size of 1 to ensure multiple chunks are processed
        var result = await service.ValidateCsvChunkedAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-valid.csv", chunkSize: 1);

        Assert.True(result.Success);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public async Task ValidateChunked_HandlesRemainingRecords_WhenNotExactMultipleOfChunkSize()
    {
        var service = new CsvValidator();

        // Chunk size that doesn't evenly divide the record count
        var result = await service.ValidateCsvChunkedAsync<AreaClassificationHeaderCsvModel>("Resources/tarclhd-valid.csv", chunkSize: 3);

        Assert.True(result.Success);
        Assert.Empty(result.ValidationErrors);
    }
}
