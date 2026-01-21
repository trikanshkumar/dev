#nullable enable
using Google.Cloud.Storage.V1;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.API.Infrastructure;
public class GoogleCloudStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    public GoogleCloudStorageService(StorageClient storageClient)
    {
        _storageClient = storageClient;
    }

    public async Task<string> GetFileAsString(string bucketName, string fileName)
    {
        using var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(bucketName, fileName, stream);
        using var reader = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var content = await reader.ReadToEndAsync();
        return content;
    }

    public async Task DownloadFile(string bucketName, string storageFileName, string localFileName)
    {
        using var stream = File.OpenWrite(localFileName);
        await _storageClient.DownloadObjectAsync(bucketName, storageFileName, stream);
    }

    public async Task MoveFile(string bucketName, string sourceFileName, string destFileName)
    {
        await _storageClient.MoveObjectAsync(bucketName, sourceFileName, destFileName);
    }

    public async Task<string?> DiscoverReceiptLogFileAsync(string bucketName, CancellationToken ct = default)
    {
        var receiptLogFilePattern = ServiceConstants.receiptLogFilePattern;
        var matches = new List<(string FileName, long LoadId, DateTimeOffset CreatedAt)>();

        await foreach (var obj in _storageClient.ListObjectsAsync(bucketName).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            var receiptFileName = obj.Name;
            if (!receiptFileName.StartsWith(receiptLogFilePattern, StringComparison.OrdinalIgnoreCase)) continue;
            if (!receiptFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) continue;
            var parts = Path.GetFileName(receiptFileName).Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 9) continue;
            if (int.TryParse(parts[5], out var y) && int.TryParse(parts[6], out var m) && int.TryParse(parts[7], out var d))
            {
                var loadIdPart = parts[8];
                if (loadIdPart.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) loadIdPart = loadIdPart[..^4];
                if (!long.TryParse(loadIdPart, out var loadId)) continue;

                // Get object creation timestamp for secondary sorting when loadIds are the same
                var createdAt = obj.TimeCreatedDateTimeOffset.HasValue
                    ? obj.TimeCreatedDateTimeOffset.Value
                    : DateTimeOffset.MaxValue;
                matches.Add((receiptFileName, loadId, createdAt));
            }
        }

        if (matches.Count == 0)
            return null;

        // Return the receipt file ordered by loadId ascending, then by creation date ascending
        var receiptByLoadIdAndDate = matches
            .OrderBy(x => x.LoadId)
            .ThenBy(x => x.CreatedAt)
            .First();

        return receiptByLoadIdAndDate.FileName;
    }
}
