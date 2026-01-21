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

public class TVPAQMTTests : BatchProcessorTests
{
    [Fact]
    public async Task ValidateLoadsAsync_TVPAQMT_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 9989, LoadTableName = "tvpaqmt", FileLocation = "gs://bucket/TVPAQMT_9989.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAcquisitionMethodDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(Bucket, "TVPAQMT_9989.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(9989, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVPAQMT_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 9988, LoadTableName = "tvpaqmt", FileLocation = "gs://bucket/TVPAQMT_9988.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAcquisitionMethodDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "invalid" }));
        _storage.Setup(s => s.DownloadFile(Bucket, "TVPAQMT_9988.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(9988, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVPAQMT_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 9987, LoadTableName = "tvpaqmt", FileLocation = "gs://bucket/TVPAQMT_9987.csv" };
        _storage.Setup(s => s.DownloadFile(Bucket, "TVPAQMT_9987.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvpaqmt_stg",
                SourceFile = "temp",
                RowsLoaded = 10,
                TotalRowsAttempted = 10,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(9987, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVPAQMT_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 9986, LoadTableName = "tvpaqmt", FileLocation = "gs://bucket/TVPAQMT_9986.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(9986, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVPAQMT_Success_Processed()
    {
        var load = new DataLoad { Id = 9985, LoadTableName = "tvpaqmt", FileLocation = "gs://bucket/TVPAQMT_9985.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(8, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 9985, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(9985, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
