namespace UPS.WWRR.Business.Interfaces
{
    public interface IStorageService
    {
        Task DownloadFile(string storageFileName, string localFileName);
        Task<string> GetFileAsString(string fileName);
        Task MoveFile(string sourceFileName, string destFileName);
        Task<string?> DiscoverReceiptLogFileAsync(CancellationToken ct = default); // ensure signature present
    }
}