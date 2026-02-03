#nullable enable
namespace UPS.WWRR.Business.Interfaces
{
    public interface IStorageService
    {
        Task DownloadFile(string storageFileName, string localFileName,CancellationToken ct = default);
        Task<string> GetFileAsString(string fileName);
        Task MoveFile(string sourceFileName, string destFileName);
        Task<string?> DiscoverReceiptLogFileAsync(CancellationToken ct = default); // ensure signature present
        /// <summary>
        /// Gets the size of a file in cloud storage in bytes.
        /// </summary>
        Task<long> GetFileSizeAsync(string fileName, CancellationToken ct = default);
        /// <summary>
        /// Checks if a file exists in cloud storage.
        /// </summary>
        Task<bool> FileExistsAsync(string fileName, CancellationToken ct = default);
        /// <summary>
        /// Verifies that the local file size matches the remote file size in cloud storage.
        /// </summary>
        /// <param name="remoteFileName">The name of the file in cloud storage.</param>
        /// <param name="localFilePath">The path to the local file.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the file sizes match, false otherwise.</returns>
        Task<bool> VerifyFileSizeAsync(string remoteFileName, string localFilePath, CancellationToken ct = default);
    }
}