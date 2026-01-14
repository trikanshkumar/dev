using AutoFixture;
using Moq;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Services;

namespace UPS.WWRR.UnitTests;

public class CsvSplitterServiceTest
{
    private readonly Fixture _fixture;
    private readonly ICsvSplitterService _splitterConcrete;
    private readonly Mock<ICsvSplitterService> _splitterMock;

    public CsvSplitterServiceTest()
    {
        _fixture = new Fixture();
        _splitterConcrete = new CsvSplitterService();
        _splitterMock = new Mock<ICsvSplitterService>();
    }

    [Fact]
    public async Task SplitAsync_ShouldReturnExpectedChunks_WhenHeaderPresent()
    {
        // Arrange
        var lines = MockDataStore.BuildSequentialLines(5);
        var tempFile = MockDataStore.CreateTempCsv(true, lines);
        int chunkSize = 2;

        // Act
        var chunks = await MockDataStore.CollectAsync(_splitterConcrete.SplitAsync(tempFile, chunkSize, true));

        // Assert
        Assert.Equal(3, chunks.Count); //5 =>2 +2 +1
        Assert.Equal(new[] { "Id,Name", lines[0], lines[1] }, MockDataStore.ChunkLines(chunks[0]));
        Assert.Equal(new[] { "Id,Name", lines[2], lines[3] }, MockDataStore.ChunkLines(chunks[1]));
        Assert.Equal(new[] { "Id,Name", lines[4] }, MockDataStore.ChunkLines(chunks[2]));
    }

    [Fact]
    public async Task SplitAsync_ShouldReturnExpectedChunks_WhenNoHeader()
    {
        // Arrange
        var lines = MockDataStore.BuildSequentialLines(7);
        var tempFile = MockDataStore.CreateTempCsv(false, lines);
        int chunkSize = 3;

        // Act
        var chunks = await MockDataStore.CollectAsync(_splitterConcrete.SplitAsync(tempFile, chunkSize, false));

        // Assert
        Assert.Equal(3, chunks.Count); //7 =>3 +3 +1
        Assert.Equal(lines[..3], MockDataStore.ChunkLines(chunks[0]));
        Assert.Equal(lines[3..6], MockDataStore.ChunkLines(chunks[1]));
        Assert.Equal(new[] { lines[6] }, MockDataStore.ChunkLines(chunks[2]));
    }

    [Fact]
    public async Task SplitAsync_ShouldThrow_WhenCancelled()
    {
        // Arrange
        var lines = MockDataStore.BuildSequentialLines(10);
        var tempFile = MockDataStore.CreateTempCsv(false, lines);
        using var cts = new CancellationTokenSource();
        var asyncEnum = _splitterConcrete.SplitAsync(tempFile, 1, false, cts.Token).GetAsyncEnumerator();

        // Act / Assert
        Assert.True(await asyncEnum.MoveNextAsync()); // first line ok
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await asyncEnum.MoveNextAsync());
    }

    [Fact]
    public async Task SplitAsync_ShouldReturnEmpty_WhenHeaderOnlyFile()
    {
        // Arrange
        var tempFile = MockDataStore.CreateTempCsv(true, Array.Empty<string>());

        // Act
        var chunks = await MockDataStore.CollectAsync(_splitterConcrete.SplitAsync(tempFile, 2, true));

        // Assert
        Assert.Empty(chunks);
    }

    [Fact]
    public async Task SplitAsync_ShouldInvokeInterfaceMock_WhenUsingMock()
    {
        // Arrange
        var path = _fixture.Create<string>();
        _splitterMock.Setup(s => s.SplitAsync(path, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { "chunk1", "chunk2" }));

        // Act
        var chunks = await MockDataStore.CollectAsync(_splitterMock.Object.SplitAsync(path, 2, true));

        // Assert
        Assert.Equal(2, chunks.Count);
        _splitterMock.Verify(s => s.SplitAsync(path, 2, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
