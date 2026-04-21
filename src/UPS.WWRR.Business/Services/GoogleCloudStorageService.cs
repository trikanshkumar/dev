#nullable enable
using Google.Cloud.Storage.V1;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.API.Infrastructure;
public class GoogleCloudStorageService(StorageClient storageClient, string bucketName, string baseDirectory, int chunkSizeBytes) : IStorageService
{
    public async Task<long> GetFileSizeAsync(string fileName, CancellationToken ct = default)
    {
        var obj = await storageClient.GetObjectAsync(bucketName, PrependBaseDirectory(fileName), cancellationToken: ct);
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
        const int maxRetriesPerRange = 5;

        var options = new DownloadObjectOptions
        {
            Range = new System.Net.Http.Headers.RangeHeaderValue(offset, chunkEnd)
        };

        while (true)
        {
            try
            {
                using var chunkStream = new MemoryStream();
                await storageClient.DownloadObjectAsync(bucketName, PrependBaseDirectory(storageFileName), chunkStream, options, ct);

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
            catch (Exception) when (attempt < maxRetriesPerRange)
            {
                attempt++;

                // Exponential backoff with jitter
                var delayMs = 200 * Math.Pow(2, attempt) + Random.Shared.Next(0, 250);
                var delay = TimeSpan.FromMilliseconds(delayMs);

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
            await storageClient.GetObjectAsync(bucketName, PrependBaseDirectory(fileName), cancellationToken: ct);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public string? GetFilenameCaseInsensitive(string expectedName)
    {
        var objects = storageClient.ListObjects(bucketName);

        var expectedNameWithBaseDirectory = PrependBaseDirectory(expectedName);

        var match = objects.FirstOrDefault(o => o.Name.Equals(expectedNameWithBaseDirectory, StringComparison.OrdinalIgnoreCase));

        if (match == null) return null;

        var resolvedName = match.Name;

        var resolvedNameWithoutBaseDirectory =
            !string.IsNullOrWhiteSpace(baseDirectory) ? resolvedName[(baseDirectory.Length + 1)..] : resolvedName;

        return resolvedNameWithoutBaseDirectory;
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

    public string PrependBaseDirectory(string objectName) => !string.IsNullOrWhiteSpace(baseDirectory) ? $"{baseDirectory}/{objectName}" : objectName;

    public async Task<string?> DiscoverReceiptLogFileAsync(CancellationToken ct = default)
    {
        var receiptLogFilePattern = PrependBaseDirectory(ServiceConstants.receiptLogFilePattern);
        var matches = new List<(string FileName, DateOnly FileDate, long LoadId)>();

        await foreach (var obj in storageClient.ListObjectsAsync(bucketName).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();

            if (TryParseReceiptLogMatch(obj.Name, receiptLogFilePattern, out var match))
            {
                matches.Add(match);
            }
        }

        if (matches.Count == 0)
            return null;

        var receiptByDateAndLoadId = matches
            .OrderBy(x => x.FileDate)
            .ThenBy(x => x.LoadId)
            .First();

        return receiptByDateAndLoadId.FileName;
    }
    private bool TryParseReceiptLogMatch(
        string receiptFileName,
        string receiptLogFilePattern,
        out (string FileName, DateOnly FileDate, long LoadId) match)
    {
        match = default;

        if (!IsReceiptLogCandidate(receiptFileName, receiptLogFilePattern))
            return false;

        var fileName = Path.GetFileName(receiptFileName);
        var parts = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 8)
            return false;

        if (!TryParseFileDate(parts, out var fileDate))
            return false;

        if (!TryParseLoadId(parts[7], out var loadId))
            return false;

        match = (fileName, fileDate, loadId);
        return true;
    }

    private bool IsReceiptLogCandidate(string receiptFileName, string receiptLogFilePattern)
    {
        return receiptFileName.StartsWith(receiptLogFilePattern, StringComparison.OrdinalIgnoreCase)
            && receiptFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryParseFileDate(string[] parts, out DateOnly fileDate)
    {
        fileDate = default;

        if (!int.TryParse(parts[4], out var year)
            || !int.TryParse(parts[5], out var month)
            || !int.TryParse(parts[6], out var day))
        {
            return false;
        }

        if (year < 1 || year > 9999
            || month < 1 || month > 12
            || day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        fileDate = new DateOnly(year, month, day);
        return true;
    }

    private bool TryParseLoadId(string loadIdPart, out long loadId)
    {
        if (loadIdPart.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            loadIdPart = loadIdPart[..^4];
        }

        return long.TryParse(loadIdPart, out loadId);
    }
}
