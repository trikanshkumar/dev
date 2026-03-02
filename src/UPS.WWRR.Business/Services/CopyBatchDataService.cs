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

                await foreach (var chunk in _csvSplitter.SplitAsync(csvFilePath, _chunkSize, _hasHeader, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    chunkIndex++;

                    // Initialize COPY command with column list on first chunk
                    if (copySql == null)
                    {
                        string headerLine = string.Empty;
                        using (var sr = new StringReader(chunk))
                            headerLine = sr.ReadLine() ?? string.Empty;
                        // Normalize header columns to lowercase (schema now lowercase) and remove quotes
                        var headerColsRaw = headerLine.Split(_delimiter, StringSplitOptions.TrimEntries | StringSplitOptions.None)
                            .Select(c => c.Replace("\"", string.Empty).Trim())
                            .Where(c => c.Length > 0)
                            .ToList();
                        var headerColsLower = headerColsRaw.Where(c => !excluded.Contains(c))
                                                           .Select(c => c.ToLowerInvariant())
                                                           .ToList();
                        copySql = SqlCommandHelper.BuildCopyCommand(configuration.TableName, headerColsLower, _delimiter, hasHeader: true);
                        singleRowCopySql = SqlCommandHelper.BuildCopyCommand(configuration.TableName, headerColsLower, _delimiter, hasHeader: false); // no HEADER
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
                                await writer.WriteLineAsync(line); // header as is
                                first = false;
                                continue;
                            }

                            // Normalize any timestamp formatted as yyyy-MM-dd-HH.mm.ss.ffffff within fields (quoted or not)
                            line = NormalizeTimestampFields(line, tsRegex);
                            // Normalize Oracle-style timestamps (dd-MMM-yy hh.mm.ss.ffffff AM/PM) to PostgreSQL format
                            line = NormalizeOracleTimestampFields(line, oracleTsRegex);
                            await writer.WriteLineAsync(line);
                        }
                        loaded = attempted;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"COPY failed for chunk {chunkIndex}. Attempting per-row fallback.");
                        // Batch failed: retry each row individually
                        var fallbackLoaded = await FallbackCopyRowsAsync(chunk, singleRowCopySql!, conn, chunkIndex, result, attempted, tsRegex, oracleTsRegex, cancellationToken);
                        loaded = fallbackLoaded;
                        if (fallbackLoaded < attempted)
                            batchErrors.Add($"Chunk {chunkIndex}: {ex.Message}");
                    }
                    batchSw.Stop();
                    result.TotalRowsAttempted += attempted;
                    result.RowsLoaded += loaded;

                    // Log detail for this batch
                    var detail = new DataLoadDetail
                    {
                        DataLoadId = configuration.DataLoadId,
                        DataLoadType = "STG",
                        ErrorIndicator = (short)(batchErrors.Count > 0 ? 1 : 0),
                        TimeProcessValue = (int)batchSw.Elapsed.TotalSeconds,
                        TimePeriodTypeCode = "SECONDS",
                        RecordsInserted = loaded - 1,
                        RecordsUpdated = 0,
                        RecordsDeleted = 0,
                        BatchNumber = chunkIndex
                    };
                    await _loadRepository.AddDetailAsync(detail, cancellationToken);
                    if (batchErrors.Count > 0)
                    {
                        // Record exceptions (row-level issues summary)
                        var exceptions = batchErrors.Select(msg => new DataLoadException
                        {
                            DataLoadDetailId = detail.Id,
                            TableName = configuration.TableName,
                            TableKey = $"BATCH:{chunkIndex}",
                            ErrorFieldName = "ROW", // generic
                            ErrorFieldValue = msg,
                            CreatedOn = DateTime.UtcNow
                        });
                        await _loadRepository.AddExceptionsAsync(exceptions, cancellationToken);
                    }
                }
                totalSw.Stop();
                _logger.LogInformation($"Load complete for {configuration.TableName}. Loaded {result.RowsLoaded}/{result.TotalRowsAttempted} rows. Errors={result.Errors.Count}. Elapsed={totalSw.Elapsed.TotalSeconds:F2}s");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Batch load failed for table {configuration.TableName}");
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Retry COPY for each row individually in case of a batch failure.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="singleRowCopySql"></param>
        /// <param name="conn"></param>
        /// <param name="chunkIndex"></param>
        /// <param name="result"></param>
        /// <param name="attempted"></param>
        /// <param name="tsRegex"></param>
        /// <param name="oracleTsRegex"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> FallbackCopyRowsAsync(string chunk, string singleRowCopySql, NpgsqlConnection conn, int chunkIndex, CopyBatchResultDto result, int attempted, Regex tsRegex, Regex oracleTsRegex, CancellationToken cancellationToken)
        {
            try
            {
                var normalized = chunk.Replace("\r\n", "\n").Replace("\r", "\n");
                var lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (lines.Length == 0) return 0;
                int headerOffset = _hasHeader ? 1 : 0;
                if (_hasHeader && lines.Length <= 1) return 0; // only header present
                int loaded = 0;
                for (int i = headerOffset; i < lines.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var row = lines[i];
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    try
                    {
                        // Normalize any timestamp formatted as yyyy-MM-dd-HH.mm.ss.ffffff within fields (quoted or not)
                        row = NormalizeTimestampFields(row, tsRegex);
                        // Normalize Oracle-style timestamps (dd-MMM-yy hh.mm.ss.ffffff AM/PM) to PostgreSQL format
                        row = NormalizeOracleTimestampFields(row, oracleTsRegex);

                        await using var writer = await _connectionHelper.BeginTextImportAsync(conn, singleRowCopySql, cancellationToken);
                        await writer.WriteLineAsync(row);
                        loaded++;
                    }
                    catch (Exception rowEx)
                    {
                        result.Errors.Add($"Row COPY failed (chunk {chunkIndex} row {i - headerOffset + 1}): {rowEx.Message}");
                    }
                }
                return loaded;
            }
            catch (Exception fbEx)
            {
                _logger.LogError(fbEx, $"Per-row COPY fallback failed for chunk {chunkIndex}");
                result.Errors.Add($"Fallback COPY failed for chunk {chunkIndex}: {fbEx.Message}");
                return 0;
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
            return count;
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

                    // Format as PostgreSQL-compatible timestamp
                    var normalized = $"{fullYear:D4}-{monthNum:D2}-{day:D2} {hour:D2}:{minute:D2}:{second:D2}.{fraction}";
                    tokens[t] = quoted ? $"\"{normalized}\"" : normalized;
                }
            }
            return string.Join(_delimiter, tokens);
        }
    }
}
