namespace UPS.WWRR.Business.Interfaces
{
    public interface IStorageService
    {
        Task DownloadFile(string bucketName, string storageFileName, string localFileName);
        Task<string> GetFileAsString(string bucketName, string fileName);
        Task MoveFile(string bucketName, string sourceFileName, string destFileName);
        Task<string?> DiscoverReceiptLogFileAsync(string bucketName, CancellationToken ct = default); // ensure signature present
    }
}