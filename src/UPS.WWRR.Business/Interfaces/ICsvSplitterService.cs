namespace UPS.WWRR.Business.Interfaces
{
    /// <summary>
    /// Interface for CSV chunker service
    /// </summary>
    public interface ICsvSplitterService
    {
        /// <summary>
        /// Splits a CSV file into successive stream of string chunks of up to chunkSize data lines.
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="chunkSize"></param>
        /// <param name="hasHeader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IAsyncEnumerable<string> SplitAsync(string csvFilePath, int chunkSize, bool hasHeader, CancellationToken cancellationToken = default);
    }
}
