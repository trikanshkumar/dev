#nullable enable
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Data.Common;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.Extensions;
using UPS.WWRR.Data.Models;

namespace UPS.WWRR.Business.Repositories
{
    /// <summary>
    /// Load Repository implementation
    /// </summary>
    public class LoadRepository : ILoadRepository
    {
        private readonly DataContext _db;
        public LoadRepository(DataContext db) => _db = db;

        private static DateTime Utc(DateTime dt) => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        public async Task<List<DataLoad>> AddLoadsAsync(IEnumerable<DataLoad> loads, CancellationToken ct = default)
        {
            foreach (var l in loads)
            {
                l.CreatedOn = Utc(l.CreatedOn == default ? DateTime.UtcNow : l.CreatedOn);
            }
            await _db.DataLoads.AddRangeAsync(loads, ct);
            await _db.SaveChangesAsync(ct);
            return loads.ToList();
        }

        public async Task UpdateStatusAsync(long loadId, LoadStatus status, DateTime? processedOn = null, CancellationToken ct = default)
        {
            var load = await _db.DataLoads.FirstOrDefaultAsync(l => l.Id == loadId, ct);
            if (load == null) return;
            load.LoadStatusCode = status.ToString();
            if (processedOn.HasValue) load.ProcessedOn = Utc(processedOn.Value);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<DataLoadDetail> AddDetailAsync(DataLoadDetail detail, CancellationToken ct = default)
        {
            await _db.DataLoadDetails.AddAsync(detail, ct);
            await _db.SaveChangesAsync(ct);
            return detail;
        }

        public async Task AddErrorsAsync(IEnumerable<DataLoadError> errors, CancellationToken ct = default)
        {
            var utcNow = DateTime.UtcNow;
            foreach (var e in errors) e.CreatedOn = Utc(e.CreatedOn == default ? utcNow : e.CreatedOn);
            await _db.DataLoadErrors.AddRangeAsync(errors, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task AddExceptionsAsync(IEnumerable<DataLoadException> exceptions, CancellationToken ct = default)
        {
            var utcNow = DateTime.UtcNow;
            foreach (var e in exceptions) e.CreatedOn = Utc(e.CreatedOn == default ? utcNow : e.CreatedOn);
            await _db.DataLoadExceptions.AddRangeAsync(exceptions, ct);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> AnyProcessingAsync(CancellationToken ct = default)
        => _db.DataLoads.AnyAsync(l => l.LoadStatusCode == LoadStatus.Processing.ToString(), ct);

        public Task<bool> ExistsAsync(string tableName, long loadVersionNumber, CancellationToken ct = default)
        => _db.DataLoads.AnyAsync(l => l.LoadTableName == tableName && l.LoadVersionNumber == loadVersionNumber, ct);

        public Task<List<DataLoad>> GetLoadsByStatusAsync(LoadStatus status, CancellationToken ct = default)
        => _db.DataLoads.Where(l => l.LoadStatusCode == status.ToString()).ToListAsync(ct);


        public async Task<MergeResult> ExecuteMergeStoredProcedureAsync(string storedProcedureName, CancellationToken ct = default)
        {
            string procLower = storedProcedureName.ToLowerInvariant();
            string callSql = $"CALL {procLower}(InsertCount := NULL, UpdateCount := NULL, DeleteCount := NULL, ErrorNumber := NULL, ErrorState := NULL, ErrorProcedure := NULL, ErrorLine := NULL, ErrorMessage := NULL)";

            int ins = 0, upd = 0, del = 0;
            string? errNum = null, errState = null, errProc = null, errLine = null, errMsg = null;

            try
            {
                DbCommand cmd = _db.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = callSql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = (int)TimeSpan.FromMinutes(120).TotalSeconds;

                var result = await cmd.ExecuteStoredProcedureAsync(async reader =>
                {
                    if (await reader.ReadAsync(ct))
                    {
                        ins = SafeGetInt(reader, 0);
                        upd = SafeGetInt(reader, 1);
                        del = SafeGetInt(reader, 2);
                        errNum = SafeGetString(reader, 3);
                        errState = SafeGetString(reader, 4);
                        errProc = SafeGetString(reader, 5);
                        errLine = SafeGetString(reader, 6);
                        errMsg = SafeGetString(reader, 7);
                    }
                    else
                    {
                        errMsg = "No result row returned.";
                    }
                    return true;
                }, ct);
            }
            catch (PostgresException px) when (px.SqlState == "42883")
            {
                return new MergeResult(0, 0, 0, px.SqlState, px.SqlState, storedProcedureName, null, $"Procedure not found using call: {callSql} - {px.MessageText}");
            }
            catch (Exception ex)
            {
                return new MergeResult(0, 0, 0, null, null, storedProcedureName, null, ex.Message);
            }

            var mergeResult = new MergeResult(ins, upd, del, errNum, errState, errProc, errLine, errMsg)
            {
            };
            return mergeResult;

            static int SafeGetInt(DbDataReader r, int ord) => ord < r.FieldCount && !r.IsDBNull(ord) ? r.GetInt32(ord) : 0;
            static string? SafeGetString(DbDataReader r, int ord) => ord < r.FieldCount && !r.IsDBNull(ord) ? r.GetString(ord) : null;
        }

        public async Task<int> UpdateLoadReferenceAsync(string stagingTableName, string mainTableName, long dataLoadId, long dataLoadDetailId, CancellationToken ct = default)
        {
            string value = $"{dataLoadId}|{dataLoadDetailId}";
            string proc = "sp_update_load_ref";
            string callSql = $"CALL {proc}(p_staging_table := @p_staging_table, p_main_table := @p_main_table, p_load_ref_te := @p_load_ref_te, ErrorNumber := NULL, ErrorState := NULL, ErrorProcedure := NULL, ErrorLine := NULL, ErrorMessage := NULL)";

            DbCommand cmd = _db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = callSql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = (int)TimeSpan.FromMinutes(120).TotalSeconds;


            var p1 = cmd.CreateParameter();
            p1.ParameterName = "p_staging_table";
            p1.Value = stagingTableName;
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "p_main_table";
            p2.Value = mainTableName;
            cmd.Parameters.Add(p2);

            var p3 = cmd.CreateParameter();
            p3.ParameterName = "p_load_ref_te";
            p3.Value = value;
            cmd.Parameters.Add(p3);

            int affected = 0;
            await cmd.ExecuteStoredProcedureAsync(async reader =>
            {
                // procedure returns only OUT error fields, no row count; we keep affected unknown
                affected = 0;
                return true;
            }, ct);

            return affected;
        }

        public async Task<int> UpdateLoadReferenceForMultipleTablesAsync(Dictionary<string, string> tableMapping, long dataLoadId, long dataLoadDetailId, CancellationToken ct = default)
        {
            int totalAffected = 0;
            foreach (var kvp in tableMapping)
            {
                totalAffected += await UpdateLoadReferenceAsync(kvp.Key, kvp.Value, dataLoadId, dataLoadDetailId, ct);
            }
            return totalAffected;
        }
    }
}
