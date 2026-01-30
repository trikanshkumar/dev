#nullable enable
using Google.Cloud.Storage.V1;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.API.Infrastructure;
public class GoogleCloudStorageService(StorageClient storageClient, string bucketName, string baseDirectory) : IStorageService
{

    public async Task<string> GetFileAsString(string fileName)
    {
        using var stream = new MemoryStream();
        await storageClient.DownloadObjectAsync(bucketName, PrependBaseDirectory(fileName), stream);
        using var reader = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var content = await reader.ReadToEndAsync();
        return content;
    }

    public async Task DownloadFile(string storageFileName, string localFileName)
    {
        using var stream = File.OpenWrite(localFileName);
        await storageClient.DownloadObjectAsync(bucketName, PrependBaseDirectory(storageFileName), stream);
    }

    public async Task MoveFile(string sourceFileName, string destFileName)
    {
        await storageClient.MoveObjectAsync(bucketName, PrependBaseDirectory(sourceFileName), PrependBaseDirectory(destFileName));
    }

    public async Task<string?> DiscoverReceiptLogFileAsync(CancellationToken ct = default)
    {
        var receiptLogFilePattern = PrependBaseDirectory(ServiceConstants.receiptLogFilePattern);
        var matches = new List<(string FileName, long LoadId, DateTimeOffset CreatedAt)>();

        await foreach (var obj in storageClient.ListObjectsAsync(bucketName).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            var receiptFileName = obj.Name;
            if (!receiptFileName.StartsWith(receiptLogFilePattern, StringComparison.OrdinalIgnoreCase)) continue;
            if (!receiptFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) continue;
            // New format: WWRR_MOD_RECEIPT_FILES_YYYY_MM_DD_LOADID.csv (8 parts)
            var parts = Path.GetFileName(receiptFileName).Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8) continue;
            if (int.TryParse(parts[4], out var y) && int.TryParse(parts[5], out var m) && int.TryParse(parts[6], out var d))
            {
                var loadIdPart = parts[7];
                if (loadIdPart.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) loadIdPart = loadIdPart[..^4];
                if (!long.TryParse(loadIdPart, out var loadId)) continue;

                // Get object creation timestamp for secondary sorting when loadIds are the same
                var createdAt = obj.TimeCreatedDateTimeOffset.HasValue
                    ? obj.TimeCreatedDateTimeOffset.Value
                    : DateTimeOffset.MaxValue;
                matches.Add((Path.GetFileName(receiptFileName), loadId, createdAt));
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

    private string PrependBaseDirectory(string objectName) => !string.IsNullOrWhiteSpace(baseDirectory) ? $"{baseDirectory}/{objectName}" : objectName;
}
