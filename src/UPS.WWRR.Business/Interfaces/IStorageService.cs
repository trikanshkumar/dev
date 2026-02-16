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
        /// Checks to see if a file exists in cloud storage matching a case-insensive expected name, and
        /// returns the actual filename with the correct casing.
        /// </summary>
        /// <remarks>
        /// If the casing of the filename in the receipt file does not match the casing of the file in
        /// google cloud storage, that is an error and the load should still not be processed. This method
        /// is provided strictly so that we can log a special error if the exact file wasn't found but a
        /// file with different casing was found.
        /// 
        /// If more than one matching file exists, it will return the first one in the list. It will
        /// not prioritize the existing file with the correct casing.
        /// </remarks>
        /// <param name="expectedName">Case-insensitive filename of the file to search for</param>
        /// <returns>The filename with the exact casing, or null if no matching filename found.</returns>
        public string? GetFilenameCaseInsensitive(string expectedName);

        /// <summary>
        /// Verifies that the local file size matches the remote file size in cloud storage.
        /// </summary>
        /// <param name="remoteFileName">The name of the file in cloud storage.</param>
        /// <param name="localFilePath">The path to the local file.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the file sizes match, false otherwise.</returns>
        Task<bool> VerifyFileSizeAsync(string remoteFileName, string localFilePath, CancellationToken ct = default);

        string PrependBaseDirectory(string objectName);
    }
}