#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Common.Helper;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;

namespace UPS.WWRR.Business.Services
{
    public class CopyBatchDataService : ICopyBatchDataService
    {
        private readonly ICsvSplitterService _csvSplitter;
        private readonly ILogger<CopyBatchDataService> _logger;
        private readonly INpgsqlConnectionHelper _connectionHelper;
        private readonly ILoadRepository _loadRepository;
        private readonly int _chunkSize;
        private readonly string _delimiter;
        private readonly bool _hasHeader;

        public CopyBatchDataService(ICsvSplitterService csvSplitter, ILogger<CopyBatchDataService> logger, IConfiguration configuration, INpgsqlConnectionHelper connectionHelper, ILoadRepository loadRepository)
        {
            _csvSplitter = csvSplitter;
            _logger = logger;
            _connectionHelper = connectionHelper;
            _loadRepository = loadRepository;

            var section = configuration.GetSection("BatchLoad");

            var chunkSizeRaw = (string.IsNullOrWhiteSpace(section["ChunkSize"]) ? null : section["ChunkSize"]) ?? (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BatchLoad_ChunkSize")) ? null : Environment.GetEnvironmentVariable("BatchLoad_ChunkSize"));
            _chunkSize = int.TryParse(chunkSizeRaw, out var parsedSize) ? parsedSize : 10_000;

            _delimiter = (string.IsNullOrWhiteSpace(section["Delimiter"]) ? null : section["Delimiter"]) ?? (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BatchLoad_Delimiter")) ? null : Environment.GetEnvironmentVariable("BatchLoad_Delimiter")) ?? ",";

            var hasHeaderRaw = (string.IsNullOrWhiteSpace(section["HasHeader"]) ? null : section["HasHeader"]) ?? (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BatchLoad_HasHeader")) ? null : Environment.GetEnvironmentVariable("BatchLoad_HasHeader"));
            _hasHeader = bool.TryParse(hasHeaderRaw, out var parsedHeader) ? parsedHeader : true;
        }

        /// <summary>
        /// Copies data from the specified CSV file into the target table using PostgreSQL COPY command in chunks.
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="configuration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CopyBatchResultDto> CopyAsync(string csvFilePath, TableConfigurationRequest configuration, CancellationToken cancellationToken = default)
        {
            var result = new CopyBatchResultDto
            {
                TableName = configuration.TableName,
                SourceFile = csvFilePath,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            };

            try
            {
                await using var conn = await _connectionHelper.OpenConnectionAsync(cancellationToken);

                var truncateSql = SqlCommandHelper.BuildTruncateCommand(configuration.TableName);
                await _connectionHelper.ExecuteNonQueryAsync(conn, truncateSql, cancellationToken);

                var chunkIndex = 0;
                var totalSw = Stopwatch.StartNew();

                string? copySql = null;
                string? singleRowCopySql = null;
                var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "is_completed_ir", "load_ref_te" };
                // Matches timestamps like yyyy-MM-dd-HH.mm.ss.ffffff (date with dashes, time with dots, 1–6 fractional digits)
                // Capturing groups: 1=date (yyyy-MM-dd), 2=HH, 3=mm, 4=ss, 5=fractional seconds
                var tsRegex = new Regex(ServiceConstants.DashDotTimestampRegexPattern, RegexOptions.Compiled);
                // Matches Oracle-style timestamps like dd-MMM-yy hh.mm.ss.ffffff AM/PM
                var oracleTsRegex = new Regex(ServiceConstants.OracleTimestampRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                // Determine if this table requires Oracle timestamp conversion
                var useOracleTimestamp = ServiceConstants.OracleTimestampTables.Contains(configuration.TableName);

                await foreach (var chunk in _csvSplitter.SplitAsync(csvFilePath, _chunkSize, _hasHeader, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    chunkIndex++;

                    // Create a per-chunk DataLoadDetail so each chunk gets its own load_ref_te = "data_load_seq_nr|seq_nr"
                    var chunkDetail = new DataLoadDetail
                    {
                        DataLoadId = configuration.DataLoadId,
                        DataLoadType = "STG",
                        ErrorIndicator = 0,
                        TimeProcessValue = 0,
                        TimePeriodTypeCode = "SECONDS",
                        RecordsInserted = 0,
                        RecordsUpdated = 0,
                        RecordsDeleted = 0,
                        BatchNumber = chunkIndex
                    };
                    await _loadRepository.AddDetailAsync(chunkDetail, cancellationToken);
                    var loadRefValue = $"{configuration.DataLoadId}|{chunkDetail.Id}";

                    // Initialize COPY command with column list on first chunk
                    if (copySql == null)
                    {
                        (copySql, singleRowCopySql) = BuildCopySqlFromHeader(chunk, configuration.TableName, excluded);
                    }

                    var attempted = CountLinesStreaming(chunk) - (_hasHeader ? 1 : 0);
                    var batchSw = Stopwatch.StartNew();
                    int loaded = 0;
                    var batchErrors = new List<string>();
                    try
                    {
                        await using var writer = await _connectionHelper.BeginTextImportAsync(conn, copySql, cancellationToken);
                        using var reader = new StringReader(chunk);
                        string? line;
                        bool first = true;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (first && _hasHeader)
                            {
                                // Append load_ref_te header column
                                await writer.WriteLineAsync(line + _delimiter + "load_ref_te");
                                first = false;
                                continue;
                            }

                            line = NormalizeLineTimestamp(line, tsRegex, oracleTsRegex, useOracleTimestamp);
                            // Append load_ref_te value to each data row
                            await writer.WriteLineAsync(line + _delimiter + loadRefValue);
                        }
                        loaded = attempted;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"COPY failed for chunk {chunkIndex}. Attempting per-row fallback.");
                        // Batch failed: retry each row individually
                        var (fallbackLoaded, rowErrors) = await FallbackCopyRowsAsync(chunk, singleRowCopySql!, conn, chunkIndex, attempted, tsRegex, oracleTsRegex, loadRefValue, useOracleTimestamp, cancellationToken);
                        loaded = fallbackLoaded;
                        // Propagate per-row errors to result and batch tracking
                        result.Errors.AddRange(rowErrors);
                        batchErrors.AddRange(rowErrors);
                        if (fallbackLoaded < attempted && rowErrors.Count == 0)
                            batchErrors.Add($"Chunk {chunkIndex}: {ex.Message}");
                    }
                    batchSw.Stop();
                    result.TotalRowsAttempted += attempted;
                    result.RowsLoaded += loaded;

                    if (batchErrors.Count > 0)
                    {
                        // Record each error as a DataLoadException
                        var exceptions = batchErrors.Select((msg, idx) => new DataLoadException
                        {
                            DataLoadDetailId = chunkDetail.Id,
                            TableName = configuration.TableName,
                            TableKey = $"BATCH:{chunkIndex}",
                            ErrorFieldName = "ROW",
                            ErrorFieldValue = msg.Length > 100 ? msg[..100] : msg,
                            CreatedOn = DateTime.UtcNow
                        });
                        await _loadRepository.AddExceptionsAsync(exceptions, cancellationToken);
                    }

                    // Update the per-chunk DataLoadDetail with chunk metrics
                    chunkDetail.ErrorIndicator = (short)(batchErrors.Count > 0 ? 1 : 0);
                    chunkDetail.TimeProcessValue = (int)batchSw.Elapsed.TotalSeconds;
                    chunkDetail.RecordsInserted = loaded;
                    await _loadRepository.UpdateDetailAsync(chunkDetail, cancellationToken);
                }
                await _loadRepository.UpdateTotalBatchNumber(configuration.DataLoadId, chunkIndex, cancellationToken);
                totalSw.Stop();

                _logger.LogInformation($"Load complete for {configuration.TableName}. Loaded {result.RowsLoaded}/{result.TotalRowsAttempted} rows in {chunkIndex} chunk(s). Errors={result.Errors.Count}. Elapsed={totalSw.Elapsed.TotalSeconds:F2}s");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Batch load failed for table {configuration.TableName}");
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Builds the COPY SQL commands from the first chunk's header line.
        /// </summary>
        private (string CopySql, string SingleRowCopySql) BuildCopySqlFromHeader(string chunk, string tableName, HashSet<string> excluded)
        {
            string headerLine = string.Empty;
            using (var sr = new StringReader(chunk))
                headerLine = sr.ReadLine() ?? string.Empty;

            var headerColsRaw = headerLine.Split(_delimiter, StringSplitOptions.TrimEntries | StringSplitOptions.None)
                .Select(c => c.Replace("\"", string.Empty).Trim())
                .Where(c => c.Length > 0)
                .ToList();

            var headerColsLower = headerColsRaw.Where(c => !excluded.Contains(c))
                                               .Select(c => c.ToLowerInvariant())
                                               .ToList();

            headerColsLower.Add("load_ref_te");

            var copySql = SqlCommandHelper.BuildCopyCommand(tableName, headerColsLower, _delimiter, hasHeader: true);
            var singleRowCopySql = SqlCommandHelper.BuildCopyCommand(tableName, headerColsLower, _delimiter, hasHeader: false);
            return (copySql, singleRowCopySql);
        }

        /// <summary>
        /// Retry COPY for each row individually in case of a batch failure.
        /// Returns the number of successfully loaded rows and a list of per-row error messages.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="singleRowCopySql"></param>
        /// <param name="conn"></param>
        /// <param name="chunkIndex"></param>
        /// <param name="attempted"></param>
        /// <param name="tsRegex"></param>
        /// <param name="oracleTsRegex"></param>
        /// <param name="loadRefValue"></param>
        /// <param name="useOracleTimestamp"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A tuple of (loaded row count, list of per-row error messages)</returns>
        private async Task<(int Loaded, List<string> RowErrors)> FallbackCopyRowsAsync(string chunk, string singleRowCopySql, NpgsqlConnection conn, int chunkIndex, int attempted, Regex tsRegex, Regex oracleTsRegex, string loadRefValue, bool useOracleTimestamp, CancellationToken cancellationToken)
        {
            var rowErrors = new List<string>();
            try
            {
                var normalized = chunk.Replace("\r\n", "\n").Replace("\r", "\n");
                var lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (lines.Length == 0) return (0, rowErrors);
                int headerOffset = _hasHeader ? 1 : 0;
                if (_hasHeader && lines.Length <= 1) return (0, rowErrors); // only header present
                int loaded = 0;
                for (int i = headerOffset; i < lines.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var row = lines[i];
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    try
                    {
                        row = NormalizeLineTimestamp(row, tsRegex, oracleTsRegex, useOracleTimestamp);
                        // Append load_ref_te value
                        row = row + _delimiter + loadRefValue;

                        await using var writer = await _connectionHelper.BeginTextImportAsync(conn, singleRowCopySql, cancellationToken);
                        await writer.WriteLineAsync(row);
                        loaded++;
                    }
                    catch (Exception rowEx)
                    {
                        rowErrors.Add($"Row COPY failed (chunk {chunkIndex} row {i - headerOffset + 1}): {rowEx.Message}");
                    }
                }
                return (loaded, rowErrors);
            }
            catch (Exception fbEx)
            {
                _logger.LogError(fbEx, $"Per-row COPY fallback failed for chunk {chunkIndex}");
                rowErrors.Add($"Fallback COPY failed for chunk {chunkIndex}: {fbEx.Message}");
                return (0, rowErrors);
            }
        }

        /// <summary>
        /// Counts lines in a chunk string by streaming through characters
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private static int CountLinesStreaming(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return 0;
            var count = 1;
            for (int i = 0; i < chunk.Length; i++)
                if (chunk[i] == '\n') count++;
            // If the chunk ends with a newline, the trailing newline does not indicate an additional line
            if (chunk[^1] == '\n') count--;
            return count;
        }

        /// <summary>
        /// Applies the correct timestamp normalization (Oracle or standard) based on the table type.
        /// </summary>
        private string NormalizeLineTimestamp(string line, Regex tsRegex, Regex oracleTsRegex, bool useOracleTimestamp)
        {
            return useOracleTimestamp
                ? NormalizeOracleTimestampFields(line, oracleTsRegex)
                : NormalizeTimestampFields(line, tsRegex);
        }

        // Normalizes any timestamp formatted as yyyy-MM-dd-HH.mm.ss.ffffff within fields (quoted or not)
        private string NormalizeTimestampFields(string line, Regex tsRegex)
        {
            if (string.IsNullOrEmpty(line)) return line;
            var tokens = line.Split(_delimiter);
            for (int t = 0; t < tokens.Length; t++)
            {
                var tok = tokens[t];
                var quoted = tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"';
                var inner = quoted ? tok.Substring(1, tok.Length - 2) : tok;
                var replaced = tsRegex.Replace(inner, "$1 $2:$3:$4.$5");
                if (!ReferenceEquals(inner, replaced) && !inner.Equals(replaced, StringComparison.Ordinal))
                {
                    tokens[t] = quoted ? $"\"{replaced}\"" : replaced;
                }
            }
            return string.Join(_delimiter, tokens);
        }

        // Normalizes Oracle-style timestamps (dd-MMM-yy hh.mm.ss.ffffff AM/PM) to PostgreSQL format (yyyy-MM-dd HH:mm:ss.ffffff)
        private string NormalizeOracleTimestampFields(string line, Regex oracleTsRegex)
        {
            if (string.IsNullOrEmpty(line)) return line;
            var tokens = line.Split(_delimiter);
            for (int t = 0; t < tokens.Length; t++)
            {
                var tok = tokens[t];
                var quoted = tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"';
                var inner = quoted ? tok.Substring(1, tok.Length - 2) : tok.Trim();
                var match = oracleTsRegex.Match(inner);
                if (match.Success)
                {
                    var normalized = ConvertOracleTimestampToPostgres(match);
                    tokens[t] = quoted ? $"\"{normalized}\"" : normalized;
                }
            }
            return string.Join(_delimiter, tokens);
        }

        /// <summary>
        /// Converts a single Oracle-style timestamp regex match to PostgreSQL format (yyyy-MM-dd HH:mm:ss.ffffff).
        /// </summary>
        private static string ConvertOracleTimestampToPostgres(Match match)
        {
            var day = int.Parse(match.Groups[1].Value);
            var monthStr = match.Groups[2].Value.ToUpperInvariant();
            var year = int.Parse(match.Groups[3].Value);
            var hour = int.Parse(match.Groups[4].Value);
            var minute = int.Parse(match.Groups[5].Value);
            var second = int.Parse(match.Groups[6].Value);
            var fraction = match.Groups[7].Value;
            var ampm = match.Groups[8].Value.ToUpperInvariant();

            // Convert 2-digit year to 4-digit (assume 2000s for years < 50, 1900s otherwise)
            var fullYear = year < 50 ? 2000 + year : 1900 + year;

            // Convert month abbreviation to month number
            var monthNum = DateTime.ParseExact(monthStr, "MMM", CultureInfo.InvariantCulture).Month;

            // Convert 12-hour to 24-hour format
            if (ampm == "PM" && hour != 12)
                hour += 12;
            else if (ampm == "AM" && hour == 12)
                hour = 0;

            return $"{fullYear:D4}-{monthNum:D2}-{day:D2} {hour:D2}:{minute:D2}:{second:D2}.{fraction}";
        }
    }
}
