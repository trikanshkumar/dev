#nullable enable
using Google.Cloud.Storage.V1;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.API.Infrastructure;
public class GoogleCloudStorageService(StorageClient storageClient, string bucketName, string baseDirectory, int chunkSizeBytes) : IStorageService
{
    public async Task<long> GetFileSizeAsync(string fileName, CancellationToken ct = default)
    {
        var obj = await storageClient.GetObjectAsync(bucketName, fileName, cancellationToken: ct);
        return (long)(obj.Size ?? 0);
    }
    public async Task<string> GetFileAsString(string fileName)
    {
        using var stream = new MemoryStream();
        await storageClient.DownloadObjectAsync(bucketName, PrependBaseDirectory(fileName), stream);
        using var reader = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var content = await reader.ReadToEndAsync();
        return content;
    }

    public async Task DownloadFile(string storageFileName, string localFileName, CancellationToken ct = default)
    {
        try
        {
            // Get file size for parallel chunked download
            var fileSize = await GetFileSizeAsync(storageFileName, ct);

            // Use parallel download for all files
            await DownloadFileParallelAsync(bucketName, storageFileName, localFileName, fileSize, ct);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File '{storageFileName}' not found in bucket '{bucketName}'.", storageFileName, ex);
        }
    }

    private async Task DownloadFileParallelAsync(string bucketName, string storageFileName, string localFileName, long fileSize, CancellationToken ct)
    {
        // Calculate number of chunks
        var chunkCount = (int)Math.Ceiling((double)fileSize / chunkSizeBytes);

        // Handle empty files
        if (chunkCount == 0)
        {
            await using var fs = new FileStream(localFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            return;
        }

        // Pre-allocate the file with the correct size
        await using (var fs = new FileStream(localFileName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fs.SetLength(fileSize);
        }

        // Download all chunks in parallel
        var tasks = new Task[chunkCount];
        for (int i = 0; i < chunkCount; i++)
        {
            var chunkIndex = i;
            var offset = (long)chunkIndex * chunkSizeBytes;
            var chunkEnd = Math.Min(offset + chunkSizeBytes - 1, fileSize - 1);

            tasks[i] = DownloadChunkAsync(bucketName, storageFileName, localFileName, offset, chunkEnd, ct);
        }

        await Task.WhenAll(tasks);
    }

    private async Task DownloadChunkAsync(string bucketName, string storageFileName, string localFileName, long offset, long chunkEnd, CancellationToken ct)
    {
        int attempt = 0;
        int MaxRetriesPerRange = 5;

        var options = new DownloadObjectOptions
        {
            Range = new System.Net.Http.Headers.RangeHeaderValue(offset, chunkEnd)
        };

        while (true)
        {
            try
            {
                using var chunkStream = new MemoryStream();
                await storageClient.DownloadObjectAsync(bucketName, storageFileName, chunkStream, options, ct);

                await using var fs = new FileStream(
                    localFileName,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.Write,
                    bufferSize: 1 << 20,
                    useAsync: true);

                fs.Seek(offset, SeekOrigin.Begin);
                chunkStream.Position = 0;
                await chunkStream.CopyToAsync(fs, ct);
                await fs.FlushAsync(ct);

                break; // success -> exit loop
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Always respect cancellation
                throw;
            }
            catch (Exception ex) when (attempt < MaxRetriesPerRange)
            {
                attempt++;

                // Exponential backoff with jitter
                var delayMs = 200 * Math.Pow(2, attempt) + Random.Shared.Next(0, 250);
                var delay = TimeSpan.FromMilliseconds(delayMs);

                // Optional: log
                //_logger.LogWarning(ex, "Chunk download failed (attempt {Attempt}/{Max}). Retrying in {Delay}.", attempt, MaxRetriesPerRange, delay);

                await Task.Delay(delay, ct);
                continue;
            }
        }
    }

    public async Task MoveFile(string sourceFileName, string destFileName)
    {
        await storageClient.MoveObjectAsync(bucketName, PrependBaseDirectory(sourceFileName), PrependBaseDirectory(destFileName));
    }
    public async Task<bool> FileExistsAsync(string fileName, CancellationToken ct = default)
    {
        try
        {
            await storageClient.GetObjectAsync(bucketName, fileName, cancellationToken: ct);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
    public async Task<bool> VerifyFileSizeAsync(string remoteFileName, string localFilePath, CancellationToken ct = default)
    {
        var remoteSize = await GetFileSizeAsync(remoteFileName, ct);
        var localFileInfo = new FileInfo(localFilePath);
        if (!localFileInfo.Exists)
        {
            return false;
        }
        var localSize = localFileInfo.Length;
        return remoteSize == localSize;
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
