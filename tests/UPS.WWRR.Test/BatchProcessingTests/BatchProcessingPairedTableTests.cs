#nullable enable
using AutoFixture;
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

/// <summary>
/// Tests for paired table processing.
/// These tables must be processed together and use a special multi-step merge process
/// with staging dataset normalization.
/// </summary>
public class BatchProcessingPairedTableTests : BatchProcessorTests
{
    /// <summary>
    /// Returns a list of table pairs. The first element in each pair is the base table, and the second element is its pair
    /// </summary>
    public static TheoryData<string, string> TablePairs = new()
    {
        { "tarclhd", "tarcldt" },
        { "tdoznhd", "tdozndt" },
        { "tasyra", "tchart" },
        { "tinznhd", "tinzndt" }
    };

    /// <summary>
    /// Just returns a list of all the tables in the TablePairs map without their corresponding pairs.
    /// </summary>
    public static TheoryData<string> Tables
    {
        get
        {
            var data = new TheoryData<string>();
            data.AddRange([.. TablePairs.Select(p => p[0]?.ToString() ?? "")]);
            data.AddRange([.. TablePairs.Select(p => p[1]?.ToString() ?? "")]);
            return data;
        }
    }

    /// <summary>
    /// Two table names that are not part of the paired table map
    /// </summary>
    private readonly string[] exampleStandardTables = ["taltccy", "tdecode"];

    private readonly Fixture fixture = new();

    #region BuildLoadsAsync - Paired Table Group Validation Tests

    [Theory]
    [MemberData(nameof(TablePairs))]
    public async Task BuildLoadsAsync_BothTablesPresent_ProcessesBoth(string baseTableName, string pairedTableName)
    {
        var baseTableUpperCase = baseTableName.ToUpper();
        var baseFileName = $"{baseTableUpperCase}_2025_01_15_12345.csv";
        var pairedTableUpperCase = pairedTableName.ToUpper();
        var pairedFileName = $"{pairedTableUpperCase}_2025_01_15_12345.csv";

        // Arrange - Both tables are present in the receipt
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = $"TableName,FileExtractName,Destination\n{baseTableUpperCase},{baseFileName},SRC\n{pairedTableUpperCase},{pairedFileName},SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == baseTableUpperCase);
        Assert.Contains(list, l => l.LoadTableName == pairedTableUpperCase);
        _repo.Verify(r => r.AddLoadsAsync(It.Is<IEnumerable<DataLoad>>(loads => loads.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_OnlyOneTablePresent_SkipsWithWarning(string baseTableName)
    {
        var baseTableUpperCase = baseTableName.ToUpper();
        var baseFileName = $"{baseTableUpperCase}_2025_01_15_12345.csv";

        // Arrange - Only one table is present, pair is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = $"TableName,FileExtractName,Destination\n{baseTableUpperCase},{baseFileName},SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - load should be added to the database with the MissingRequiredPair status, but not returned from the BuildLoadsAsync method
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.Is<IEnumerable<DataLoad>>(list =>
            list != null && list.Count() == 1 && list.First().LoadStatusCode == LoadStatus.MissingRequiredPair.ToString()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_TableWithoutPairPresent_WithOtherStandardTables_ProcessesOthersSkipsTableWithMissingPair(string baseTableName)
    {
        var baseTableUpperCase = baseTableName.ToUpper();
        var baseFileName = $"{baseTableUpperCase}_2025_01_15_12345.csv";

        var standardTableUpperCase = exampleStandardTables[0].ToUpper();
        var standardFileName = $"{standardTableUpperCase}_2025_01_15_12345.csv";

        var otherStandardTableUpperCase = exampleStandardTables[1].ToUpper();
        var otherStandardFileName = $"{otherStandardTableUpperCase}_2025_01_15_12345.csv";


        // Arrange - base table with other tables but no matching pair
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = $"TableName,FileExtractName,Destination\n{standardTableUpperCase},{standardFileName},SRC\n{otherStandardTableUpperCase},{otherStandardFileName},SRC\n{baseTableUpperCase},{baseFileName},SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - standard tables should be processed, table with missing pair should be skipped
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == standardTableUpperCase);
        Assert.Contains(list, l => l.LoadTableName == otherStandardTableUpperCase);
        Assert.DoesNotContain(list, l => l.LoadTableName == baseTableUpperCase);
    }

    [Theory]
    [MemberData(nameof(TablePairs))]
    public async Task BuildLoadsAsync_MixedTablesWithBothPairedTables_ProcessesAll(string baseTableName, string pairedTableName)
    {
        var baseTableUpperCase = baseTableName.ToUpper();
        var baseFileName = $"{baseTableUpperCase}_2025_01_15_12345.csv";

        var pairedTableUpperCase = pairedTableName.ToUpper();
        var pairedFileName = $"{pairedTableUpperCase}_2025_01_15_12345.csv";

        var standardTableUpperCase = exampleStandardTables[0].ToUpper();
        var standardFileName = $"{standardTableUpperCase}_2025_01_15_12345.csv";

        var otherStandardTableUpperCase = exampleStandardTables[1].ToUpper();
        var otherStandardFileName = $"{otherStandardTableUpperCase}_2025_01_15_12345.csv";

        // Arrange - Both paired tables along with other standard tables
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = $"TableName,FileExtractName,Destination\n{standardTableUpperCase},{standardFileName},SRC\n{otherStandardTableUpperCase},{otherStandardFileName},SRC\n{baseTableUpperCase},{baseFileName},SRC\n{pairedTableUpperCase},{pairedFileName},SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - All 4 tables should be processed
        Assert.Equal(4, list.Count);
        Assert.Contains(list, l => l.LoadTableName == standardTableUpperCase);
        Assert.Contains(list, l => l.LoadTableName == otherStandardTableUpperCase);
        Assert.Contains(list, l => l.LoadTableName == baseTableUpperCase);
        Assert.Contains(list, l => l.LoadTableName == pairedTableUpperCase);
    }

    [Theory]
    [MemberData(nameof(TablePairs))]
    public async Task BuildLoadsAsync_CaseInsensitive_BothTablesPresent(string baseTableName, string pairedTableName)
    {
        // use lowercase for base table
        var baseFileName = $"{baseTableName}_2025_01_15_12345.csv";

        var pairedTableUpperCase = pairedTableName.ToUpper();
        var pairedFileName = $"{pairedTableUpperCase}_2025_01_15_12345.csv";

        // Arrange - Mixed case table names
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = $"TableName,FileExtractName,Destination\n{baseTableName},{baseFileName},SRC\n{pairedTableUpperCase},{pairedFileName},SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - Both should be processed (case insensitive matching)
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task BuildLoadsAsync_NoPairedTables_WithOtherStandardTables_ProcessesNormally()
    {
        var standardTableUpperCase = exampleStandardTables[0].ToUpper();
        var standardFileName = $"{standardTableUpperCase}_2025_01_15_12345.csv";

        var otherStandardTableUpperCase = exampleStandardTables[1].ToUpper();
        var otherStandardFileName = $"{otherStandardTableUpperCase}_2025_01_15_12345.csv";

        // Arrange - No paired tables
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = $"TableName,FileExtractName,Destination\n{standardTableUpperCase},{standardFileName},SRC\n{otherStandardTableUpperCase},{otherStandardFileName},SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - Both should be processed normally
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == standardTableUpperCase);
        Assert.Contains(list, l => l.LoadTableName == otherStandardTableUpperCase);
    }

    #endregion

    #region Paired Table Validation Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_PairedTable_SetsReadyToProcess_OnSuccess(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_PairedTable_SetsFailedValidation_OnFailure(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid ORG_CNY_CD format" }));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_PairedTable_MultipleValidationErrors_RecordsAll(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        var errors = new List<string>
    {
        "Invalid ORG_CNY_CD: must be 2 characters",
        "Invalid SVC_TYP_CD: must be 3 characters",
        "Invalid ZCH_NR: must be numeric"
    };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(e => e.Count() == 3), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Paired Table Copy Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_PairedTable_SetsProcessingOnStart(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();
        var stagingTableName = $"{table}_stg";

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = stagingTableName, SourceFile = "temp", RowsLoaded = 1000, TotalRowsAttempted = 1000, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_PairedTable_UsesCorrectStagingTable(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();
        var stagingTableName = $"{table}_stg";

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.Is<TableConfigurationRequest>(r => r.TableName == stagingTableName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = stagingTableName, SourceFile = "temp", RowsLoaded = 100, TotalRowsAttempted = 100, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        _copy.Verify(c => c.CopyAsync(It.IsAny<string>(), It.Is<TableConfigurationRequest>(r => r.TableName == stagingTableName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_PairedTable_CopyFails_SetsFailedStatus(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();
        var stagingTableName = $"{table}_stg";

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = stagingTableName,
            SourceFile = "temp",
            RowsLoaded = 50,
            TotalRowsAttempted = 100,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        failedResult.Errors.Add("Copy operation failed");
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_PairedTable_LargeFile_ProcessesSuccessfully(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();
        var stagingTableName = $"{table}_stg";

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = stagingTableName,
                SourceFile = "temp",
                RowsLoaded = 6800000,
                TotalRowsAttempted = 6800000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        // Should NOT set Failed status when copy succeeds
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Paired Table Merge Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_CallsStagingDatasetProcedure(string table)
    {
        // Arrange
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify stored procedures were called (staging normalization + merge)
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_StagingNormalizationFails_SetsFailedStatus(string table)
    {
        // Arrange
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_pairedtable_stagingdataset_proc", "10", "Staging normalization failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify merge was NOT called (staging failed) and status is Failed
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_MergeFails_SetsFailedStatus(string table)
    {
        // Arrange
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        // First call (staging) succeeds, second call (merge) fails
        _repo.SetupSequence(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null)) // Staging succeeds
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_pairedtable_merge_proc", "20", "Merge failed")); // Merge fails
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_Success_UpdatesMultipleLoadReferences(string table)
    {
        // Arrange
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 100, 50, null, null, null, null, null));
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify MarkStagingCompletedForMultipleTablesAsync was called
        _repo.Verify(r => r.MarkStagingCompletedForMultipleTablesAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_FailedMerge_RecordsError(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        _repo.SetupSequence(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_pairedtable_merge_proc", "", "Duplicate key violation"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_Success_MarksProcessed(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6822838, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_PairedTable_WithInsertUpdateDelete_Success(string table)
    {
        var tableUpper = table.ToUpper();
        var fileName = $"{tableUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = tableUpper, FileLocation = fileLocation };

        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6000000, 500000, 100000, null, null, null, null, null));
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDescriptor Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public void GetDescriptor_PairedTable_ReturnsCorrectDescriptor(string table)
    {
        var sut = CreateSut();
        var descriptor = InvokeSync<object>(sut, "GetDescriptor", table.ToUpper());

        var tableName = descriptor.GetType().GetProperty("TableName")?.GetValue(descriptor)?.ToString();
        var stagingTableName = descriptor.GetType().GetProperty("StagingTableName")?.GetValue(descriptor)?.ToString();
        var requiresNormalization = (bool)(descriptor.GetType().GetProperty("RequiresStagingNormalization")?.GetValue(descriptor) ?? false);

        var expectedStagingName = $"{table}_stg";

        Assert.Equal(table, tableName);
        Assert.Equal(expectedStagingName, stagingTableName);
        Assert.True(requiresNormalization);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public void GetDescriptor_PairedTable_LowercaseTableName_Works(string table)
    {
        var sut = CreateSut();
        var descriptor = InvokeSync<object>(sut, "GetDescriptor", table);

        var tableName = descriptor.GetType().GetProperty("TableName")?.GetValue(descriptor)?.ToString();
        Assert.Equal(table, tableName);
    }

    #endregion

    #region ValidateAndFilterPairedTableGroups Tests for Paired Table

    [Theory]
    [MemberData(nameof(TablePairs))]
    public async Task ValidateAndFilterPairedTableGroups_CompletePair_ReturnsAll(string baseTableName, string pairedTableName)
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = baseTableName.ToUpper() },
            new() { LoadTableName = pairedTableName.ToUpper() }
        };

        var sut = CreateSut();
        var result = await InvokeAsync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, l => l.LoadTableName == baseTableName.ToUpper());
        Assert.Contains(result, l => l.LoadTableName == pairedTableName.ToUpper());
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateAndFilterPairedTableGroups_IncompletePair_PairedTable_Only_RemovesIt(string tableName)
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = tableName.ToUpper() },
            new() { LoadTableName = exampleStandardTables[0].ToUpper() }
        };

        var sut = CreateSut();
        var result = await InvokeAsync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Single(result);
        Assert.Equal(exampleStandardTables[0].ToUpper(), result[0].LoadTableName);
    }

    [Fact]
    public async Task ValidateAndFilterPairedTableGroups_MultiplePairedGroups_AllComplete_ReturnsAll()
    {
        var tablePairs = TablePairs.Take(2).ToArray();
        var firstPair = tablePairs[0];
        var secondPair = tablePairs[1];

        var firstPairBaseTable = firstPair[0].ToString();
        var firstPairPairTable = firstPair[1].ToString();
        var secondPairBaseTable = secondPair[0].ToString();
        var secondPairPairTable = secondPair[1].ToString();

        Assert.NotNull(firstPairBaseTable);
        Assert.NotNull(firstPairPairTable);
        Assert.NotNull(secondPairBaseTable);
        Assert.NotNull(secondPairPairTable);

        var loads = new List<DataLoad>
        {
            new() { LoadTableName = firstPairBaseTable.ToUpper()},
            new() { LoadTableName = firstPairPairTable.ToUpper()},
            new() { LoadTableName = secondPairBaseTable.ToUpper()},
            new() { LoadTableName = secondPairPairTable.ToUpper()},
            new() { LoadTableName = exampleStandardTables[0].ToUpper()}
        };

        var sut = CreateSut();
        var result = await InvokeAsync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task ValidateAndFilterPairedTableGroups_MultiplePairedGroups_OneIncomplete_RemovesIncomplete()
    {
        var tablePairs = TablePairs.Take(2).ToArray();
        var firstPair = tablePairs[0];
        var secondPair = tablePairs[1];

        var firstPairBaseTable = firstPair[0].ToString();
        var firstPairPairTable = firstPair[1].ToString();
        var secondPairBaseTable = secondPair[0].ToString();

        Assert.NotNull(firstPairBaseTable);
        Assert.NotNull(firstPairPairTable);
        Assert.NotNull(secondPairBaseTable);

        var loads = new List<DataLoad>
        {
            new() { LoadTableName = firstPairBaseTable.ToUpper()},
            new() { LoadTableName = firstPairPairTable.ToUpper()},
            new() { LoadTableName = secondPairBaseTable.ToUpper()}, // missing pair
            new() { LoadTableName = exampleStandardTables[0].ToUpper()}
        };

        var sut = CreateSut();
        var result = await InvokeAsync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, l => l.LoadTableName == firstPairBaseTable.ToUpper());
        Assert.Contains(result, l => l.LoadTableName == firstPairPairTable.ToUpper());
        Assert.Contains(result, l => l.LoadTableName == exampleStandardTables[0].ToUpper());
        Assert.DoesNotContain(result, l => l.LoadTableName == secondPairBaseTable.ToUpper());
    }

    #endregion

    #region End-to-End Flow Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task EndToEnd_PairedTable_FullSuccessFlow(string tableName)
    {
        var loadId = fixture.Create<int>();
        var tableNameUpper = tableName.ToUpper();
        var fileName = $"{tableNameUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{tableName}_stg";

        // Arrange
        var load = new DataLoad { Id = loadId, LoadTableName = tableNameUpper, FileLocation = fileLocation };

        // Validation succeeds
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Copy succeeds
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = stagingTableName, SourceFile = "temp", RowsLoaded = 6800000, TotalRowsAttempted = 6800000, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });

        // Merge succeeds
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6822838, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();

        // Act - Validate
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Act - Copy
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);

        // Act - Merge
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task EndToEnd_PairedTable_ValidationFails_DoesNotProceed(string tableName)
    {
        var loadId = fixture.Create<int>();
        var tableNameUpper = tableName.ToUpper();
        var fileName = $"{tableNameUpper}_2026_03_13_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{tableName}_stg";

        // Arrange
        var load = new DataLoad { Id = loadId, LoadTableName = tableNameUpper, FileLocation = fileLocation };

        // Validation fails
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid data format" }));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act - Validate
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        // Assert - Validation failed, no copy or merge should happen
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _copy.Verify(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static T InvokeSync<T>(object target, string name, params object[] args)
    {
        var mi = target.GetType().GetMethod(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mi == null)
        {
            throw new InvalidOperationException($"Method {name} not found on {target.GetType().Name}.");
        }

        var ret = mi.Invoke(target, args);
        return ret is T value ? value : default!;
    }

    #endregion
}
