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

        Task<DataLoadDetail> AddDetailAsync(DataLoadDetail detail, CancellationToken ct = default);

        Task AddErrorsAsync(IEnumerable<DataLoadError> errors, CancellationToken ct = default);

        Task AddExceptionsAsync(IEnumerable<DataLoadException> exceptions, CancellationToken ct = default);

        Task<bool> ExistsAsync(string tableName, long loadVersionNumber, CancellationToken ct = default);

        Task<List<DataLoad>> GetLoadsByStatusAsync(LoadStatus status, CancellationToken ct = default);

        Task<MergeResult> ExecuteMergeStoredProcedureAsync(string storedProcedureName, CancellationToken ct = default);

        Task<int> UpdateLoadReferenceAsync(string stagingTableName, string mainTableName, long dataLoadId, long dataLoadDetailId, CancellationToken ct = default);
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