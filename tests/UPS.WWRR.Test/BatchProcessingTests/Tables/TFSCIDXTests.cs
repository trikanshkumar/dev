#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.LoadTableDto;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

/// <summary>
/// Unit tests for TFSCIDX (Fuel Surcharge Index) table batch processing.
/// </summary>
public class TFSCIDXTests : BatchProcessorTests
{
    [Fact]
    public async Task CopyBatchLoadAsync_TFSCIDX_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 4111, LoadTableName = "tfscidx", FileLocation = "gs://bucket/TFSCIDX_4111.csv" };
        _storage.Setup(s => s.DownloadFile("TFSCIDX_4111.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tfscidx_stg", SourceFile = "temp", RowsLoaded = 8, TotalRowsAttempted = 8, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(4111, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCIDX_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 4130, LoadTableName = "tfscidx", FileLocation = "gs://bucket/TFSCIDX_4130.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", "", "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(4130, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCIDX_Success_Processed()
    {
        var load = new DataLoad { Id = 4131, LoadTableName = "tfscidx", FileLocation = "gs://bucket/TFSCIDX_4131.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(3, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(4131, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
