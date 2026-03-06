#nullable enable
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Data.Models;

namespace UPS.WWRR.Business.Repositories
{
    /// <summary>
    /// interface for Load Repository
    /// </summary>
    public interface ILoadRepository
    {
        Task<List<DataLoad>> AddLoadsAsync(IEnumerable<DataLoad> loads, CancellationToken ct = default);

        Task UpdateStatusAsync(long loadId, LoadStatus status, DateTime? processedOn = null, CancellationToken ct = default);

        Task UpdateTotalBatchNumber(long loadId, int totalBatchNumber, CancellationToken ct = default);

        Task UpdateFileLocation(long loadId, string fileLocation, CancellationToken ct = default);

        Task<DataLoadDetail> AddDetailAsync(DataLoadDetail detail, CancellationToken ct = default);

        Task UpdateDetailAsync(DataLoadDetail detail, CancellationToken ct = default);

        Task AddErrorsAsync(IEnumerable<DataLoadError> errors, CancellationToken ct = default);

        Task AddExceptionsAsync(IEnumerable<DataLoadException> exceptions, CancellationToken ct = default);

        Task<bool> ExistsAsync(string tableName, string loadVersion, CancellationToken ct = default);

        Task<List<DataLoad>> GetLoadsByStatusAsync(LoadStatus status, CancellationToken ct = default);

        Task<MergeResult> ExecuteMergeStoredProcedureAsync(string storedProcedureName, CancellationToken ct = default);

        /// <summary>
        /// Marks staging table rows as completed (is_completed_ir = 1) via the sp_update_load_ref stored procedure.
        /// </summary>
        /// <param name="stagingTableName">The staging table name</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<int> MarkStagingCompletedAsync(string stagingTableName, CancellationToken ct = default);

        /// <summary>
        /// Marks staging table rows as completed (is_completed_ir = 1) for multiple staging tables.
        /// </summary>
        /// <param name="stagingTableNames">Collection of staging table names</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Total number of affected records</returns>
        Task<int> MarkStagingCompletedForMultipleTablesAsync(IEnumerable<string> stagingTableNames, CancellationToken ct = default);

        /// <summary>
        /// Gets the row count from a staging table.
        /// </summary>
        /// <param name="stagingTableName">The staging table name</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The number of rows in the staging table</returns>
        Task<long> GetStagingTableRowCountAsync(string stagingTableName, CancellationToken ct = default);
    }

    public record MergeResult(int Inserted, int Updated, int Deleted,
        string? ErrorNumber,
        string? ErrorState,
        string? ErrorProcedure,
        string? ErrorLine,
        string? ErrorMessage)
    {
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    }
}