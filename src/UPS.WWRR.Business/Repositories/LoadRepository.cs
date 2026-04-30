#nullable enable
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using UPS.WWRR.Business.Common.Constants;
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

        public async Task UpdateTotalBatchNumber(long loadId, int totalBatchNumber, CancellationToken ct = default)
        {
            var load = await _db.DataLoads.FirstOrDefaultAsync(l => l.Id == loadId, ct);
            if (load == null) return;
            load.TotalBatchNumber = totalBatchNumber;
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateFileLocation(long loadId, string fileLocation, CancellationToken ct = default)
        {
            var load = await _db.DataLoads.FirstOrDefaultAsync(l => l.Id == loadId, ct);
            if (load == null) return;
            load.FileLocation = fileLocation;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<DataLoadDetail> AddDetailAsync(DataLoadDetail detail, CancellationToken ct = default)
        {
            detail.CreatedOn = Utc(detail.CreatedOn == default ? DateTime.UtcNow : detail.CreatedOn);
            await _db.DataLoadDetails.AddAsync(detail, ct);
            await _db.SaveChangesAsync(ct);
            return detail;
        }

        public async Task UpdateDetailAsync(DataLoadDetail detail, CancellationToken ct = default)
        {
            detail.UpdatedOn = DateTime.UtcNow;
            _db.DataLoadDetails.Update(detail);
            await _db.SaveChangesAsync(ct);
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

        public Task<bool> ExistsAsync(string tableName, string loadVersion, CancellationToken ct = default)
        => _db.DataLoads.AnyAsync(l => l.LoadTableName == tableName && l.LoadVersion == loadVersion, ct);

        public Task<List<DataLoad>> GetLoadsByStatusAsync(LoadStatus status, string loadVersion, CancellationToken ct = default)
        => _db.DataLoads.Where(l => l.LoadStatusCode == status.ToString() && l.LogFileLocation == loadVersion).ToListAsync(ct);


        public async Task<MergeResult> ExecuteMergeStoredProcedureAsync(string storedProcedureName, CancellationToken ct = default)
        {
            string procLower = storedProcedureName.ToLowerInvariant();

            if (!StoredProcConstant.IsValidProcedureName(procLower))
            {
                throw new ArgumentException($"Procedure name not in list of allowed procedure names: {procLower}", nameof(storedProcedureName));
            }
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

        public async Task<int> MarkStagingCompletedAsync(string stagingTableName, CancellationToken ct = default)
        {
            string callSql = $"CALL sp_update_load_ref(p_staging_table := @p_staging_table, ErrorNumber := NULL, ErrorState := NULL, ErrorProcedure := NULL, ErrorLine := NULL, ErrorMessage := NULL)";

            DbCommand cmd = _db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = callSql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = (int)TimeSpan.FromMinutes(120).TotalSeconds;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "p_staging_table";
            p1.Value = stagingTableName;
            cmd.Parameters.Add(p1);

            int affected = 0;
            await cmd.ExecuteStoredProcedureAsync(async reader =>
            {
                affected = 0;
                return true;
            }, ct);

            return affected;
        }

        public async Task<int> MarkStagingCompletedForMultipleTablesAsync(IEnumerable<string> stagingTableNames, CancellationToken ct = default)
        {
            var tables = stagingTableNames.Where(t => t.EndsWith("_stg", StringComparison.OrdinalIgnoreCase)).ToList();
            if (tables.Count == 0)
                return 0;

            int totalAffected = 0;

            var connection = _db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(ct);

            // Build a single batch SQL that updates all staging tables in one round trip
            var sqlBuilder = new System.Text.StringBuilder();
            foreach (var stagingTable in tables)
            {
                sqlBuilder.AppendLine($@"UPDATE {stagingTable} SET is_completed_ir = 1 WHERE is_completed_ir IS DISTINCT FROM 1;");
            }

            var sql = sqlBuilder.ToString();
            if (string.IsNullOrWhiteSpace(sql))
                return 0;

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = (int)TimeSpan.FromMinutes(60).TotalSeconds;

            try
            {
                totalAffected = await cmd.ExecuteNonQueryAsync(ct);
            }
            catch (Exception)
            {
                // If batch fails, fall back to sequential execution for better error handling
                totalAffected = 0;
                foreach (var table in tables)
                {
                    totalAffected += await MarkStagingCompletedAsync(table, ct);
                }
            }

            return totalAffected;
        }

        public async Task<long> GetStagingTableRowCountAsync(string stagingTableName, CancellationToken ct = default)
        {
            var allowedTableNames = Enum.GetNames(typeof(TableName)).Select(t => $"{t}_stg");
            if (!allowedTableNames.Contains(stagingTableName, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid staging table: {stagingTableName}", nameof(stagingTableName));
            }
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {stagingTableName}";
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = (int)TimeSpan.FromMinutes(5).TotalSeconds;

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Convert.ToInt64(result) : 0;
        }
    }
}
