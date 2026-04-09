#nullable enable
using Google.Events.Protobuf.Firebase.TestLab.V1;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.Common.Helper;
using UPS.WWRR.Business.DTO.Models.LoadTableDto;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using TableEnum = UPS.WWRR.Business.Common.Constants.TableName;

namespace UPS.WWRR.Business.Services
{
    public class BatchProcessorWorker : IBatchProcessorWorker
    {
        private readonly ILogger<BatchProcessorWorker> _logger;
        private readonly IStorageService _storageService;
        private readonly ICsvValidator _csvValidator;
        private readonly ICopyBatchDataService _copyBatchService;
        private readonly ILoadRepository _loadRepository;
        private readonly IGooglePubSubService? _pubSubService;

        private readonly string _gcpBucketName;
        private readonly string _tableNameFilter;
        private readonly int _defaultChunkSize = 100_000;
        private readonly int _batchLoadChunkSize;
        private readonly bool _tvasylnUseBatchMerge;
        private readonly bool _trastdUseBatchMerge;
        private readonly bool _tsubchgUseBatchMerge;
        private readonly HashSet<string> _movedObjects = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Accumulates load versions that were successfully processed during the current cycle.
        /// Used to build the Pub/Sub notification message at the end of the job.
        /// </summary>
        private readonly List<string> _processedLoadVersions = [];

        /// <summary>
        /// Accumulates CSV load log entries during a processing cycle.
        /// A single summary log is emitted at the end of the cycle.
        /// </summary>
        private readonly List<CsvLoadLogEntry> _csvLoadLogEntries = [];
        
        /// <summary>
        /// Tracks which paired table groups have already executed their merge SP in the current processing cycle.
        /// Key is the merge stored procedure name. For paired tables sharing the same SP, only the first table runs the merge.
        /// </summary>
        private readonly HashSet<string> _processedMergeSPs = new(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Stores the merge result from the first table in a paired group.
        /// Key is the merge stored procedure name. Used to copy record counts to the second table's detail.
        /// </summary>
        private readonly Dictionary<string, MergeResult> _pairedMergeResults = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Defines groups of tables that must be processed together.
        /// All tables in a group must be present in the receipt file for any of them to be processed.
        /// </summary>
        private static readonly List<HashSet<string>> _pairedTableGroups =
        [
            new(StringComparer.OrdinalIgnoreCase) { nameof(TableEnum.TARCLHD), nameof(TableEnum.TARCLDT) },
            new(StringComparer.OrdinalIgnoreCase) { nameof(TableEnum.TDOZNHD), nameof(TableEnum.TDOZNDT) },
            new(StringComparer.OrdinalIgnoreCase) { nameof(TableEnum.TASYRA), nameof(TableEnum.TCHART) },
            new(StringComparer.OrdinalIgnoreCase) { nameof(TableEnum.TINZNHD), nameof(TableEnum.TINZNDT) }
        ];

        public BatchProcessorWorker(ILogger<BatchProcessorWorker> logger,
                                IStorageService storageService,
                                ICsvValidator csvValidator,
                                ICopyBatchDataService copyBatchService,
                                ILoadRepository loadRepository,
                                IGooglePubSubService? pubSubService = null)
        {
            _logger = logger;
            _storageService = storageService;
            _csvValidator = csvValidator;
            _copyBatchService = copyBatchService;
            _loadRepository = loadRepository;
            _pubSubService = pubSubService;
            _gcpBucketName = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_STORAGE_BUCKET_NAME")
                        ?? throw new InvalidOperationException("GOOGLE_CLOUD_STORAGE_BUCKET_NAME environment variable is not set.");
            _tableNameFilter = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "ALL";
            _batchLoadChunkSize = int.TryParse(Environment.GetEnvironmentVariable("BatchLoad_ChunkSize"), out var cs) ? cs : _defaultChunkSize;
            _tvasylnUseBatchMerge = bool.TryParse(Environment.GetEnvironmentVariable("TVASYLN_USE_BATCH_MERGE"), out var tvasylnUseBatch) && tvasylnUseBatch;
            _trastdUseBatchMerge = bool.TryParse(Environment.GetEnvironmentVariable("TRASTD_USE_BATCH_MERGE"), out var trastdUseBatch) && trastdUseBatch;
            _tsubchgUseBatchMerge = bool.TryParse(Environment.GetEnvironmentVariable("TSUBCHG_USE_BATCH_MERGE"), out var tsubchgUseBatch) && tsubchgUseBatch;
        }

        /// <summary>
		/// Processor execution loop
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
        public async Task ProcessAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BatchProcessorWorker started at {Time}", DateTimeOffset.UtcNow);

            try
            {
                _logger.LogInformation("Starting processing cycle at {time}", DateTimeOffset.UtcNow);

                // Clear trackers at the start of each cycle
                _movedObjects.Clear();
                _processedMergeSPs.Clear();
                _pairedMergeResults.Clear();
                _csvLoadLogEntries.Clear();
                _processedLoadVersions.Clear();

                // Discover and validate new loads (creates DataLoad entries)
                var newLoads = await BuildLoadsAsync(stoppingToken);
                if (newLoads.Count == 0)
                {
                    _logger.LogInformation("No new loads discovered in this cycle.");
                }
                else
                {
                    // Use a unique key per file (object path from FileLocation)
                    var tempFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    // Validate file contents with cache
                    await ValidateLoadsAsync(newLoads, stoppingToken, tempFiles);

                    // Copy batches to staging for ReadyToProcess loads
                    string loadBatch = newLoads.Select(lv => lv.LogFileLocation).FirstOrDefault() ?? string.Empty;
                    var readyAfterValidation = await _loadRepository.GetLoadsByStatusAsync(LoadStatus.ReadyToProcess, loadBatch, stoppingToken);
                    if (readyAfterValidation.Count > 0)
                    {
                        readyAfterValidation = FilterByTableName(readyAfterValidation);
                        await CopyBatchLoadAsync(readyAfterValidation, stoppingToken, tempFiles);
                        tempFiles.Clear(); // Clear references after copy

                        //  main table for loads that are still Processing after copy
                        var processingLoads = await _loadRepository.GetLoadsByStatusAsync(LoadStatus.Processing, loadBatch, stoppingToken);
                        if (processingLoads.Count > 0)
                        {
                            processingLoads = FilterByTableName(processingLoads);
                            await PerformMergeLoadAsync(processingLoads, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("BatchProcessorWorker ended with errors at {Time}", DateTimeOffset.UtcNow);
                _logger.LogError(ex, "Unhandled error in Processor");
            }
            finally
            {
                LogCsvLoadSummary();
                await PublishPubSubNotificationAsync();
            }

            _logger.LogInformation("BatchProcessorWorker ended at {Time}", DateTimeOffset.UtcNow);
        }

        private List<DataLoad> FilterByTableName(List<DataLoad> loads) => _tableNameFilter.Equals("ALL", StringComparison.OrdinalIgnoreCase)
        ? loads
        : loads.Where(l => string.Equals(l.LoadTableName, _tableNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Validates that all tables in paired groups are present together.
        /// If a group has some but not all tables present, those tables are removed and a warning is logged.
        /// </summary>
        /// <param name="loads">The list of loads to validate</param>
        /// <returns>Filtered list with incomplete paired groups removed</returns>
        private async Task<List<DataLoad>> ValidateAndFilterPairedTableGroups(List<DataLoad> loads)
        {
            if (loads.Count == 0) return loads;

            var tableNamesInLoads = new HashSet<string>(
                loads.Select(l => l.LoadTableName),
                StringComparer.OrdinalIgnoreCase);

            var tablesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in _pairedTableGroups)
            {
                var presentTables = group.Where(t => tableNamesInLoads.Contains(t)).ToList();
                var missingTables = group.Where(t => !tableNamesInLoads.Contains(t)).ToList();

                // If some but not all tables in the group are present, remove the present ones
                if (presentTables.Count > 0 && missingTables.Count > 0)
                {
                    _logger.LogWarning(
                        "Paired table group validation failed. Present: [{presentTables}], Missing: [{missingTables}]. " +
                        "All tables in the group must be present together. Skipping present tables.",
                        string.Join(", ", presentTables),
                        string.Join(", ", missingTables));

                    foreach (var table in presentTables)
                    {
                        tablesToRemove.Add(table);
                    }
                }
            }

            if (tablesToRemove.Count > 0)
            {
                // record as failed data load and move the file to processed
                var loadsToRemove = loads.Where(l => tablesToRemove.Contains(l.LoadTableName)).ToList();
                foreach (var load in loadsToRemove)
                {
                    load.LoadStatusCode = LoadStatus.MissingRequiredPair.ToString();
                    load.ProcessedOn = DateTime.UtcNow;
                    BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Missing paired table(s) of {load.LoadTableName}");
                    // The file may not exist, we didn't check for that yet, but the MoveObjectToProcessedAsync method handles and logs that exception gracefully
                    var fileName = Path.GetFileName(load.FileLocation);
                    await MoveObjectToProcessedAsync(fileName);
                }
                await _loadRepository.AddLoadsAsync(loadsToRemove);

                // filter out and don't continue processing the failed loads
                return loads.Except(loadsToRemove).ToList();
            }

            return loads;
        }

        /// <summary>
        /// Discover the receipt CSV directly from GCS bucket
        /// Read and insert new DataLoad records based on the receipt file content
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>List of newly created DataLoad records</returns>
        private async Task<List<DataLoad>> BuildLoadsAsync(CancellationToken ct)
        {
            var newLoads = new List<DataLoad>();
            // Track table/version combinations in this receipt to prevent duplicates
            var processedInReceipt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Discover the new  receipt CSV directly from GCS bucket
            string? dynamicReceiptName = await _storageService.DiscoverReceiptLogFileAsync(ct);
            if (string.IsNullOrWhiteSpace(dynamicReceiptName))
            {
                _logger.LogInformation("No receipt log file found matching convention in bucket {bucket}.", _gcpBucketName);
                return newLoads;
            }
            string logContent;
            try
            {
                logContent = await _storageService.GetFileAsString(dynamicReceiptName);
                if (string.IsNullOrWhiteSpace(logContent))
                {
                    _logger.LogWarning("Receipt file {file} empty or unreadable.", dynamicReceiptName);
                    return newLoads;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading receipt file {file} from bucket {bucket}", dynamicReceiptName, _gcpBucketName);
                return newLoads;
            }

            var rows = ParseCsv(logContent);
            if (rows == null)
            {
                // ParseCsv already logged the error (e.g., unexpected header)
                await MoveObjectToProcessedAsync(dynamicReceiptName);
                return newLoads;
            }
            if (rows.Count <= 1)
            {
                _logger.LogInformation("Receipt file {file} contains no data rows", dynamicReceiptName);
                await MoveObjectToProcessedAsync(dynamicReceiptName);
                return newLoads;
            }

            // Inline former BuildLoadsFromReceiptAsync logic
            var header = rows[0];
            // Track all data file names referenced in the receipt for cleanup
            var allReceiptFileNames = new List<string>();
            int fileExtractNameIndex = Array.FindIndex(header, h => h.Equals(ServiceConstants.fileExtractName, StringComparison.OrdinalIgnoreCase));

            int destinationNameIndex = Array.FindIndex(header, h => h.Equals(ServiceConstants.destinationColumn, StringComparison.OrdinalIgnoreCase));

            var batchSizeEnv = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out var bs) ? bs : _defaultChunkSize;
            foreach (var r in rows.Skip(1))
            {
                if (r.Length <= fileExtractNameIndex) continue;
                var fileExtractName = r[fileExtractNameIndex];

                var destination = destinationNameIndex < 0 || r.Length <= destinationNameIndex || string.IsNullOrWhiteSpace(r[destinationNameIndex])
                    ? ServiceConstants.defaultDestinationValue
                    : r[destinationNameIndex].Trim();

                if (string.IsNullOrWhiteSpace(fileExtractName)) continue;

                allReceiptFileNames.Add(fileExtractName);

                var parts = fileExtractName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                {
                    _logger.LogWarning("Cannot parse table/load id from {val}", fileExtractName);
                    continue;
                }

                var tableNamePart = parts[0];
                var yearPart = parts[1];
                var monthPart = parts[2];
                var dayPart = parts[3];
                var loadIdPart = parts[4];



                int year, month, day;
                long loadId;

                try
                {
                    year = int.Parse(yearPart);
                    month = int.Parse(monthPart);
                    day = int.Parse(dayPart);

                    // get just the digits to remove the ".csv" portion
                    var loadIdDigits = new string(loadIdPart.Where(char.IsDigit).ToArray());

                    loadId = long.Parse(loadIdDigits);
                }
                catch
                {
                    _logger.LogWarning("Date/LoadId has invalid value(s) in file {fileExtractName}", fileExtractName);
                    continue;
                }

                var loadVersion = $"{year}_{month}_{day}_{loadId}";

                if (await _loadRepository.ExistsAsync(tableNamePart, loadVersion, ct))
                {
                    _logger.LogInformation("DataLoad already exists for {tbl} version {ver}. Skipping insert.", tableNamePart, loadVersion);
                    continue;
                }

                // Check if this table/version combination is already in the current receipt
                var loadKey = $"{tableNamePart}|{loadVersion}";
                if (processedInReceipt.Contains(loadKey))
                {
                    _logger.LogWarning("Duplicate entry found in receipt file for {tbl} version {ver}. Skipping duplicate.", tableNamePart, loadVersion);
                    continue;
                }

                newLoads.Add(new DataLoad
                {
                    LoadTableName = tableNamePart,
                    LoadVersionNumber = loadId,
                    LoadVersion = loadVersion,
                    LoadStatusCode = LoadStatus.ReadyForValidation.ToString(),
                    FileLocation = $"gs://{_gcpBucketName}/{_storageService.PrependBaseDirectory(fileExtractName)}",
                    CreatedOn = DateTime.UtcNow,
                    LogFileLocation = $"gs://{_gcpBucketName}/{_storageService.PrependBaseDirectory(dynamicReceiptName)}",
                    TotalBatchNumber = 0,
                    BatchSize = _batchLoadChunkSize,
                    DataSource = destination[..Math.Min(destination.Length, 25)]
                });

                // Mark this table/version as processed in this receipt
                processedInReceipt.Add(loadKey);
            }

            if (newLoads.Count > 0)
            {
                newLoads = FilterByTableName(newLoads);
                
                // Validate paired table groups - all tables in a group must be present together
                newLoads = await ValidateAndFilterPairedTableGroups(newLoads);
                
                if (newLoads.Count > 0)
                    await _loadRepository.AddLoadsAsync(newLoads, ct);
            }

            // If all loads from this receipt were already processed (no new loads created),
            // move the receipt and its data files to the processed folder so the service
            // can discover the next receipt file in subsequent cycles.
            if (newLoads.Count == 0)
            {
                _logger.LogInformation("All loads from receipt {receipt} already exist. Moving receipt and data files to processed folder.", dynamicReceiptName);
                foreach (var fileName in allReceiptFileNames)
                {
                    await MoveObjectToProcessedAsync(fileName);
                }
                await MoveObjectToProcessedAsync(dynamicReceiptName);
            }

            return newLoads;
        }

        /// <summary>
        /// copy batch to staging table
        /// updates status to Processing if successful
        /// </summary>
        private async Task CopyBatchLoadAsync(List<DataLoad> loads, CancellationToken ct, Dictionary<string, string> tempFiles)
        {
            foreach (var load in loads)
            {
                var descriptor = GetDescriptor(load.LoadTableName);
                var gcsFileName = Path.GetFileName(load.FileLocation);
                if (!descriptor.IsSupported) continue;
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processing, null, ct);

                // Key from FileLocation object path
                var key = ExtractObjectName(load.FileLocation);
                string tempFile;
                if (!tempFiles.TryGetValue(key, out tempFile!))
                {
                    tempFile = Path.GetTempFileName();
                    await _storageService.DownloadFile(gcsFileName, tempFile, ct);
                }

                try
                {
                    var (copySuccess, stagingDetail, rowsLoaded, copyErrors) = await CopyBatchAsync(load, tempFile, descriptor, ct);
                    if (!copySuccess)
                    {
                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, DateTime.UtcNow, ct);
                        BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Staging Load Failed: {string.Join("; ", copyErrors)}");
                        // Move failed file to processed folder
                        await MoveObjectToProcessedAsync(gcsFileName);
                    }
                    else if (rowsLoaded == 0)
                    {
                        // CSV file has a header but no data rows — do NOT run the MERGE SP
                        // because an empty staging table would cause all records in the main table to be deleted.
                        _logger.LogWarning("Empty .csv file found. No data to process. Table: {tbl}, Load: {id}, File: {file}.",
                            load.LoadTableName, load.Id, gcsFileName);

                        DataLoadDetail emptyDetail = await CreateDataLoadDetailForError(load, ct);
                        var emptyException = new DataLoadException
                        {
                            DataLoadDetailId = emptyDetail.Id,
                            TableName = descriptor.TableName,
                            TableKey = $"LOAD:{load.Id}",
                            ErrorFieldName = ServiceConstants.emptyDataFileError,
                            ErrorFieldValue = $"Empty .csv file found. No data to process: {gcsFileName}",
                            CreatedOn = DateTime.UtcNow
                        };
                        await _loadRepository.AddExceptionsAsync([emptyException], ct);

                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedMissingData, DateTime.UtcNow, ct);
                        BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Empty .csv file: {gcsFileName}");
                        await MoveObjectToProcessedAsync(gcsFileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CopyBatch failed for table {tbl} load {id}", load.LoadTableName, load.Id);
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, DateTime.UtcNow, ct);

                    DataLoadDetail dataLoadDetail = await CreateDataLoadDetailForError(load, ct);
                    var emptyException = new DataLoadException
                    {
                        DataLoadDetailId = dataLoadDetail.Id,
                        TableName = descriptor.TableName,
                        TableKey = $"LOAD:{load.Id}",
                        ErrorFieldName = ServiceConstants.copyBatchLoadError,
                        ErrorFieldValue = ex.Message.Length > 100 ? ex.Message[..100] : ex.Message,
                        CreatedOn = DateTime.UtcNow
                    };
                    await _loadRepository.AddExceptionsAsync([emptyException], ct);

                    BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Staging Load failed: {ex.Message}");
                    // Move failed file to processed folder
                    await MoveObjectToProcessedAsync(gcsFileName);
                }
                finally
                {
                    try
                    {
                        // Delete and remove from cache
                        File.Delete(tempFile);
                        tempFiles.Remove(key);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// merge from staging to main table
        /// </summary>
        /// <param name="loads"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task PerformMergeLoadAsync(List<DataLoad> loads, CancellationToken ct)
        {
            foreach (var load in loads)
            {
                var descriptor = GetDescriptor(load.LoadTableName);
                if (!descriptor.IsSupported) continue;
                var gcsFileName = Path.GetFileName(load.FileLocation);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    if (descriptor.RequiresStagingNormalization)
                    {
                        await HandleStagingNormalizationLoadAsync(load, descriptor, sw, ct);
                    }
                    else
                    {
                        // For batch merge, get staging count before merge for validation
                        long stagingRowCount = 0;
                        if ((_tvasylnUseBatchMerge && load.LoadTableName.Equals(nameof(TableEnum.TVASYLN), StringComparison.OrdinalIgnoreCase)) ||
                            (_trastdUseBatchMerge && load.LoadTableName.Equals(nameof(TableEnum.TRASTD), StringComparison.OrdinalIgnoreCase)) ||
                            (_tsubchgUseBatchMerge && load.LoadTableName.Equals(nameof(TableEnum.TSUBCHG), StringComparison.OrdinalIgnoreCase)))
                        {
                            stagingRowCount = await _loadRepository.GetStagingTableRowCountAsync(descriptor.StagingTableName, ct);
                            _logger.LogInformation("{loadTableName} batch merge: Staging table row count = {stagingCount}", load.LoadTableName.ToUpper(), stagingRowCount);
                        }

                        var (mergeResult, mergeDetail) = await PerformMergeAsync(load, descriptor, sw, ct);
                        bool hasError = !string.IsNullOrWhiteSpace(mergeResult.ErrorMessage);

                        // Validate batch merge results
                        if (!hasError &&
                            ((_tvasylnUseBatchMerge && load.LoadTableName.Equals(nameof(TableEnum.TVASYLN), StringComparison.OrdinalIgnoreCase)) ||
                            (_trastdUseBatchMerge && load.LoadTableName.Equals(nameof(TableEnum.TRASTD), StringComparison.OrdinalIgnoreCase)) ||
                            (_tsubchgUseBatchMerge && load.LoadTableName.Equals(nameof(TableEnum.TSUBCHG), StringComparison.OrdinalIgnoreCase))))
                        {
                            var totalProcessed = mergeResult.Inserted + mergeResult.Updated;
                            if (totalProcessed != stagingRowCount)
                            {
                                _logger.LogWarning(
                                    "{loadTableName} batch merge record count MISMATCH: Staging={stagingCount}, Processed (Insert+Update)={processedCount}, Inserted={inserted}, Updated={updated}, Deleted={deleted}",
                                    load.LoadTableName.ToUpper(), stagingRowCount, totalProcessed, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted);
                            }
                            else
                            {
                                _logger.LogInformation(
                                    "{loadTableName} batch merge record count VERIFIED: Staging={stagingCount}, Processed={processedCount}, Inserted={inserted}, Updated={updated}, Deleted={deleted}",
                                    load.LoadTableName.ToUpper(), stagingRowCount, totalProcessed, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted);
                            }
                        }

                        if (hasError)
                        {
                            int errorCode = 0;
                            if (!string.IsNullOrWhiteSpace(mergeResult.ErrorNumber) && int.TryParse(mergeResult.ErrorNumber, out var parsed)) errorCode = parsed;
                            var error = new DataLoadError
                            {
                                DataLoadDetailId = mergeDetail.Id,
                                ErrorCode = errorCode,
                                ErrorStoredProcedureName = mergeResult.ErrorProcedure,
                                ErrorMessage = mergeResult.ErrorMessage,
                                CreatedOn = DateTime.UtcNow
                            };
                        await _loadRepository.AddErrorsAsync(new[] { error }, ct);
                            await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, DateTime.UtcNow, ct);
                            BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted, mergeResult.ErrorMessage);
                        }
                        else
                        {
                        await _loadRepository.MarkStagingCompletedAsync(descriptor.StagingTableName, ct);
                                _logger.LogInformation("Staging table {stg} marked as completed for load {id}", descriptor.StagingTableName, load.Id);
                                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processed, DateTime.UtcNow, ct);
                                _logger.LogInformation("Load Process Completed for table {tbl} load {id}", load.LoadTableName, load.Id);
                                BuildCsvLoadLog(load, ServiceConstants.LoadStatusSuccess, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted);
                                _processedLoadVersions.Add(load.LoadVersion);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Merge failed for table {tbl} load {id}", load.LoadTableName, load.Id);
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, DateTime.UtcNow, ct);
                    BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Main Merge Failed: {ex.Message}");

                    DataLoadDetail dataLoadDetail = await CreateDataLoadDetailForError(load, ct);
                    var emptyException = new DataLoadException
                    {
                        DataLoadDetailId = dataLoadDetail.Id,
                        TableName = descriptor.TableName,
                        TableKey = $"LOAD:{load.Id}",
                        ErrorFieldName = ServiceConstants.performMergeLoadError,
                        ErrorFieldValue = ex.Message.Length > 100 ? ex.Message[..100] : ex.Message,
                        CreatedOn = DateTime.UtcNow
                    };
                    await _loadRepository.AddExceptionsAsync([emptyException], ct);
                }
                finally
                {
                    try
                    {
                        await MoveObjectToProcessedAsync(gcsFileName);
                        // Move the associated receipt log file as well, if available, only once per cycle
                        var logObject = ExtractObjectName(Path.GetFileName(load.LogFileLocation) ?? string.Empty);
                        await MoveObjectToProcessedAsync(logObject);
                    }
                    catch (Exception moveEx)
                    {
                        _logger.LogWarning(moveEx, "Failed to move processed file {file}", gcsFileName);
                    }
                }
            }
        }

        /// <summary>
        /// descriptor for each table load
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private LoadTableDescriptor GetDescriptor(string tableName) => tableName.ToUpperInvariant() switch
        {
            nameof(TableEnum.TALTCCY) => new LoadTableDescriptor(
                nameof(TableEnum.TALTCCY).ToLowerInvariant(),
                (nameof(TableEnum.TALTCCY) + "_STG").ToLowerInvariant(),
                StoredProcConstant.AlternateCurrencyMerge,
                async path => await _csvValidator.ValidateCsvAsync<AlternateCurrencyDto>(path)
            ),
            nameof(TableEnum.TASYBRL) => new LoadTableDescriptor(
                nameof(TableEnum.TASYBRL).ToLowerInvariant(),
                (nameof(TableEnum.TASYBRL) + "_STG").ToLowerInvariant(),
                StoredProcConstant.AccessorialExceptionMerge,
                async path => await _csvValidator.ValidateCsvAsync<AccessorialExceptionDTO>(path)
            ),
            nameof(TableEnum.TFPUTRH) => new LoadTableDescriptor(
                nameof(TableEnum.TFPUTRH).ToLowerInvariant(),
                (nameof(TableEnum.TFPUTRH) + "_STG").ToLowerInvariant(),
                StoredProcConstant.AccessorialThresholdMerge,
                async path => await _csvValidator.ValidateCsvAsync<AccessorialThresholdDTO>(path)
            ),

            nameof(TableEnum.TDSTSVP) => new LoadTableDescriptor(
                nameof(TableEnum.TDSTSVP).ToLowerInvariant(),
                (nameof(TableEnum.TDSTSVP) + "_STG").ToLowerInvariant(),
                StoredProcConstant.DestinationZipSvcAsyValidationMerge,
                async path => await _csvValidator.ValidateCsvAsync<DestinationZipSvcAsyValidationDto>(path)
            ),
            nameof(TableEnum.TDFWTHR) => new LoadTableDescriptor(
                nameof(TableEnum.TDFWTHR).ToLowerInvariant(),
                (nameof(TableEnum.TDFWTHR) + "_STG").ToLowerInvariant(),
                StoredProcConstant.DeficitWeightThresholdMerge,
                async path => await _csvValidator.ValidateCsvAsync<DeficitWeightThresholdDto>(path)
            ),
            nameof(TableEnum.TASYTRH) => new LoadTableDescriptor(
                nameof(TableEnum.TASYTRH).ToLowerInvariant(),
                (nameof(TableEnum.TASYTRH) + "_STG").ToLowerInvariant(),
                StoredProcConstant.AccessorialMinMaxCriteriaMerge,
                async path => await _csvValidator.ValidateCsvAsync<AccessorialMinMaxCriteriaDto>(path)
            ),
            nameof(TableEnum.TBMAVCS) => new LoadTableDescriptor(
                nameof(TableEnum.TBMAVCS).ToLowerInvariant(),
                (nameof(TableEnum.TBMAVCS) + "_STG").ToLowerInvariant(),
                StoredProcConstant.BmaCapAmountMerge,
                async path => await _csvValidator.ValidateCsvAsync<BmaCapAmountDTO>(path)
            ),
            nameof(TableEnum.TCZMSYS) => new LoadTableDescriptor(
                nameof(TableEnum.TCZMSYS).ToLowerInvariant(),
                (nameof(TableEnum.TCZMSYS) + "_STG").ToLowerInvariant(),
                StoredProcConstant.CzmSystemRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<CzmSystemRulesDto>(path)
            ),
            nameof(TableEnum.TBRCHAC) => new LoadTableDescriptor(
                nameof(TableEnum.TBRCHAC).ToLowerInvariant(),
                (nameof(TableEnum.TBRCHAC) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ThresholdSimpleRatesMerge,
                async path => await _csvValidator.ValidateCsvAsync<ThresholdSimpleRatesDTO>(path)
            ),
            nameof(TableEnum.TSIARAV) => new LoadTableDescriptor(
                nameof(TableEnum.TSIARAV).ToLowerInvariant(),
                (nameof(TableEnum.TSIARAV) + "_STG").ToLowerInvariant(),
                StoredProcConstant.SimpleRateVolumeRangeMerge,
                async path => await _csvValidator.ValidateCsvAsync<SimpleRateVolumeRangeDto>(path)
            ),
            nameof(TableEnum.TAUHIST) => new LoadTableDescriptor(
                nameof(TableEnum.TAUHIST).ToLowerInvariant(),
                (nameof(TableEnum.TAUHIST) + "_STG").ToLowerInvariant(),
                StoredProcConstant.AuditHistoryMerge,
                async path => await _csvValidator.ValidateCsvAsync<AuditHistoryDto>(path)
            ),
            nameof(TableEnum.TCNYASY) => new LoadTableDescriptor(
                nameof(TableEnum.TCNYASY).ToLowerInvariant(),
                (nameof(TableEnum.TCNYASY) + "_STG").ToLowerInvariant(),
                StoredProcConstant.AccessorialRatingRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<AccessorialRatingRulesDTO>(path)
            ),
            nameof(TableEnum.TCYBLTY) => new LoadTableDescriptor(
                nameof(TableEnum.TCYBLTY).ToLowerInvariant(),
                (nameof(TableEnum.TCYBLTY) + "_STG").ToLowerInvariant(),
                StoredProcConstant.CountryBillTypeMerge,
                async path => await _csvValidator.ValidateCsvAsync<CountryBillTypeDto>(path)
            ),
            nameof(TableEnum.TIMPSVC) => new LoadTableDescriptor(
                nameof(TableEnum.TIMPSVC).ToLowerInvariant(),
                (nameof(TableEnum.TIMPSVC) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ImportServiceValidationMerge,
                async path => await _csvValidator.ValidateCsvAsync<ImportServiceValidationDto>(path)
            ),
            nameof(TableEnum.TINFTRH) => new LoadTableDescriptor(
                nameof(TableEnum.TINFTRH).ToLowerInvariant(),
                (nameof(TableEnum.TINFTRH) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InformationalAccessorialThresholdMerge,
                async path => await _csvValidator.ValidateCsvAsync<InformationalAccessorialThresholdDto>(path)
            ),
            nameof(TableEnum.TPSLBUR) => new LoadTableDescriptor(
                nameof(TableEnum.TPSLBUR).ToLowerInvariant(),
                (nameof(TableEnum.TPSLBUR) + "_STG").ToLowerInvariant(),
                StoredProcConstant.PostalExceptionMerge,
                async path => await _csvValidator.ValidateCsvAsync<PostalExceptionDTO>(path)
            ),
            nameof(TableEnum.TINFCHG) => new LoadTableDescriptor(
                nameof(TableEnum.TINFCHG).ToLowerInvariant(),
                (nameof(TableEnum.TINFCHG) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InformationalAccessorialChargeMerge,
                async path => await _csvValidator.ValidateCsvAsync<InformationalAccessorialChargeDto>(path)
            ),
            nameof(TableEnum.TINSCRI) => new LoadTableDescriptor(
                nameof(TableEnum.TINSCRI).ToLowerInvariant(),
                (nameof(TableEnum.TINSCRI) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InsuranceCriteriaMerge,
                async path => await _csvValidator.ValidateCsvAsync<InsuranceCriteriaDto>(path)
            ),
            nameof(TableEnum.TIRACCY) => new LoadTableDescriptor(
                nameof(TableEnum.TIRACCY).ToLowerInvariant(),
                (nameof(TableEnum.TIRACCY) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InternationalRatingCurrencyMerge,
                async path => await _csvValidator.ValidateCsvAsync<InternationalRatingCurrencyDTO>(path)
            ),
            nameof(TableEnum.TLMTVLU) => new LoadTableDescriptor(
                nameof(TableEnum.TLMTVLU).ToLowerInvariant(),
                (nameof(TableEnum.TLMTVLU) + "_STG").ToLowerInvariant(),
                StoredProcConstant.LimitValuesBasedOnCriteriaMerge,
                async path => await _csvValidator.ValidateCsvAsync<LimitValuesBasedOnCriteriaDto>(path)
            ),
            nameof(TableEnum.TSVCDFL) => new LoadTableDescriptor(
                nameof(TableEnum.TSVCDFL).ToLowerInvariant(),
                (nameof(TableEnum.TSVCDFL) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ServiceDefaultRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<ServiceDefaultRulesDto>(path)
            ),
            nameof(TableEnum.TSVCACP) => new LoadTableDescriptor(
                nameof(TableEnum.TSVCACP).ToLowerInvariant(),
                (nameof(TableEnum.TSVCACP) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ServiceDowngradeValidAccessorialRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<ServiceDowngradeValidAccessorialRulesDto>(path)
            ),
            nameof(TableEnum.TSVCDGR) => new LoadTableDescriptor(
                nameof(TableEnum.TSVCDGR).ToLowerInvariant(),
                (nameof(TableEnum.TSVCDGR) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ServiceDowngradeRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<ServiceDowngradeRulesDto>(path)
            ),
            nameof(TableEnum.TMINCRI) => new LoadTableDescriptor(
                nameof(TableEnum.TMINCRI).ToLowerInvariant(),
                (nameof(TableEnum.TMINCRI) + "_STG").ToLowerInvariant(),
                StoredProcConstant.MinimumCriteriaMerge,
                async path => await _csvValidator.ValidateCsvAsync<MinimumCriteriaDTO>(path)
            ),
            nameof(TableEnum.TINFRAT) => new LoadTableDescriptor(
                nameof(TableEnum.TINFRAT).ToLowerInvariant(),
                (nameof(TableEnum.TINFRAT) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InformationalAccessorialRateMerge,
                async path => await _csvValidator.ValidateCsvAsync<InformationalAccessorialRateDto>(path)
            ),
            nameof(TableEnum.TVDSTBT) => new LoadTableDescriptor(
                nameof(TableEnum.TVDSTBT).ToLowerInvariant(),
                (nameof(TableEnum.TVDSTBT) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ValidDestinationBillTermMerge,
                async path => await _csvValidator.ValidateCsvAsync<ValidDestinationBillTermDto>(path)
            ),
            nameof(TableEnum.TRATRUL) => new LoadTableDescriptor(
                nameof(TableEnum.TRATRUL).ToLowerInvariant(),
                (nameof(TableEnum.TRATRUL) + "_STG").ToLowerInvariant(),
                StoredProcConstant.FreightRatingRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<FreightRatingRulesDto>(path)
            ),
            nameof(TableEnum.TVDSVCF) => new LoadTableDescriptor(
                nameof(TableEnum.TVDSVCF).ToLowerInvariant(),
                (nameof(TableEnum.TVDSVCF) + "_STG").ToLowerInvariant(),
                StoredProcConstant.DestinationServiceFeatureTypeMerge,
                async path => await _csvValidator.ValidateCsvAsync<DestinationServiceFeatureTypeDto>(path)
            ),
            nameof(TableEnum.TSDRWSF) => new LoadTableDescriptor(
                nameof(TableEnum.TSDRWSF).ToLowerInvariant(),
                (nameof(TableEnum.TSDRWSF) + "_STG").ToLowerInvariant(),
                StoredProcConstant.SameDayRateMerge,
                async path => await _csvValidator.ValidateCsvAsync<SameDayRateDto>(path)
            ),
            nameof(TableEnum.TVORGBT) => new LoadTableDescriptor(
                nameof(TableEnum.TVORGBT).ToLowerInvariant(),
                (nameof(TableEnum.TVORGBT) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ValidOriginBillTermMerge,
                async path => await _csvValidator.ValidateCsvAsync<ValidOriginBillTermDto>(path)
            ),
            nameof(TableEnum.TSPMYCD) => new LoadTableDescriptor(
                nameof(TableEnum.TSPMYCD).ToLowerInvariant(),
                (nameof(TableEnum.TSPMYCD) + "_STG").ToLowerInvariant(),
                StoredProcConstant.TemplateAccessorialRulesMerge,
                async path => await _csvValidator.ValidateCsvAsync<TemplateAccessorialRulesDto>(path)
            ),
            nameof(TableEnum.TVLNSVC) => new LoadTableDescriptor(
                nameof(TableEnum.TVLNSVC).ToLowerInvariant(),
                (nameof(TableEnum.TVLNSVC) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ValidLaneServiceMerge,
                async path => await _csvValidator.ValidateCsvAsync<ValidLaneServiceDto>(path)
            ),
            nameof(TableEnum.TVOSVCF) => new LoadTableDescriptor(
                nameof(TableEnum.TVOSVCF).ToLowerInvariant(),
                (nameof(TableEnum.TVOSVCF) + "_STG").ToLowerInvariant(),
                StoredProcConstant.OriginServiceFeatureTypeMerge,
                async path => await _csvValidator.ValidateCsvAsync<OriginServiceFeatureTypesDto>(path)
            ),
            nameof(TableEnum.TWGTTRH) => new LoadTableDescriptor(
                nameof(TableEnum.TWGTTRH).ToLowerInvariant(),
                (nameof(TableEnum.TWGTTRH) + "_STG").ToLowerInvariant(),
                StoredProcConstant.PublishedLetterThresholdMerge,
                async path => await _csvValidator.ValidateCsvAsync<PublishedLetterThresholdDto>(path)
            ),
            nameof(TableEnum.TVPAQMT) => new LoadTableDescriptor(
                nameof(TableEnum.TVPAQMT).ToLowerInvariant(),
                (nameof(TableEnum.TVPAQMT) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ValidAcquisitionMethodMerge,
                async path => await _csvValidator.ValidateCsvAsync<ValidAcquisitionMethodDto>(path)
            ),
            nameof(TableEnum.TCOLDEC) => new LoadTableDescriptor(
                nameof(TableEnum.TCOLDEC).ToLowerInvariant(),
                (nameof(TableEnum.TCOLDEC) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ColumnDecodeMerge,
                async path => await _csvValidator.ValidateCsvAsync<ColumnDecodeDto>(path)
            ),
            nameof(TableEnum.TVSVCPK) => new LoadTableDescriptor(
                nameof(TableEnum.TVSVCPK).ToLowerInvariant(),
                (nameof(TableEnum.TVSVCPK) + "_STG").ToLowerInvariant(),
                StoredProcConstant.ValidOriginServicePackageMerge,
                async path => await _csvValidator.ValidateCsvAsync<ValidOriginServicePackageDto>(path)
            ),
            nameof(TableEnum.TDECODE) => new LoadTableDescriptor(
                nameof(TableEnum.TDECODE).ToLowerInvariant(),
                (nameof(TableEnum.TDECODE) + "_STG").ToLowerInvariant(),
                StoredProcConstant.DecodeValuesMerge,
                async path => await _csvValidator.ValidateCsvAsync<DecodeValuesDto>(path)
            ),
            nameof(TableEnum.TSUBCHG) => new LoadTableDescriptor(
                nameof(TableEnum.TSUBCHG).ToLowerInvariant(),
                (nameof(TableEnum.TSUBCHG) + "_STG").ToLowerInvariant(),
                _tsubchgUseBatchMerge ? StoredProcConstant.FuelSurchargeBatchMerge : StoredProcConstant.FuelSurchargeMerge,
                async path => await _csvValidator.ValidateCsvAsync<FuelSurchargeDto>(path)
            ),
            nameof(TableEnum.TVASYLN) => new LoadTableDescriptor(
                    nameof(TableEnum.TVASYLN).ToLowerInvariant(),
                    (nameof(TableEnum.TVASYLN) + "_STG").ToLowerInvariant(),
                    _tvasylnUseBatchMerge ? StoredProcConstant.ValidAccessorialLaneBatchMerge : StoredProcConstant.ValidAccessorialLaneMerge,
                    async path => await _csvValidator.ValidateCsvAsync<ValidAccessorialLaneDto>(path)
                ),
                nameof(TableEnum.TRASTD) => new LoadTableDescriptor(
                   nameof(TableEnum.TRASTD).ToLowerInvariant(),
                   (nameof(TableEnum.TRASTD) + "_STG").ToLowerInvariant(),
                   _trastdUseBatchMerge ? StoredProcConstant.FreightRatesBatchMarge : StoredProcConstant.FreightRatesMerge,
                   async path => await _csvValidator.ValidateCsvChunkedAsync<FreightRatesDto>(path, _batchLoadChunkSize)
               ),
                // Area Classification Header - uses special multi-step process with staging dataset normalization
                nameof(TableEnum.TARCLHD) => new LoadTableDescriptor(
                    nameof(TableEnum.TARCLHD).ToLowerInvariant(),
                    (nameof(TableEnum.TARCLHD) + "_STG").ToLowerInvariant(),
                    StoredProcConstant.AreaClassificationHeaderMerge,
                    async path => await _csvValidator.ValidateCsvAsync<AreaClassificationHeaderDto>(path),
                    StoredProcConstant.AreaClassificationHeaderNormalizeStaging,
                    GetAreaClassificationTableMapping()
                ),
                // Area Classification Detail - uses special multi-step process with staging dataset normalization
                nameof(TableEnum.TARCLDT) => new LoadTableDescriptor(
                    nameof(TableEnum.TARCLDT).ToLowerInvariant(),
                    (nameof(TableEnum.TARCLDT) + "_STG").ToLowerInvariant(),
                    StoredProcConstant.AreaClassificationHeaderMerge,
                    async path => await _csvValidator.ValidateCsvAsync<AreaClassificationDetailDto>(path),
                    StoredProcConstant.AreaClassificationHeaderNormalizeStaging,
                    GetAreaClassificationTableMapping()
                ),
            // Domestic Zone Header - uses special multi-step process with staging dataset normalization
            nameof(TableEnum.TDOZNHD) => new LoadTableDescriptor(
                nameof(TableEnum.TDOZNHD).ToLowerInvariant(),
                (nameof(TableEnum.TDOZNHD) + "_STG").ToLowerInvariant(),
                StoredProcConstant.DomesticZoneMerge,
                async path => await _csvValidator.ValidateCsvAsync<DomesticZoneHeaderDto>(path),
                StoredProcConstant.DomesticZoneNormalizeStaging,
                GetDomesticZoneTableMapping()
            ),
            // Domestic Zone Detail - uses special multi-step process with staging dataset normalization
            nameof(TableEnum.TDOZNDT) => new LoadTableDescriptor(
                nameof(TableEnum.TDOZNDT).ToLowerInvariant(),
                (nameof(TableEnum.TDOZNDT) + "_STG").ToLowerInvariant(),
                StoredProcConstant.DomesticZoneMerge,
                async path => await _csvValidator.ValidateCsvAsync<DomesticZoneDetailDto>(path),
                StoredProcConstant.DomesticZoneNormalizeStaging,
                GetDomesticZoneTableMapping()
            ),
            nameof(TableEnum.TFSCIDX) => new LoadTableDescriptor(
                nameof(TableEnum.TFSCIDX).ToLowerInvariant(),
                (nameof(TableEnum.TFSCIDX) + "_STG").ToLowerInvariant(),
                StoredProcConstant.FuelSurchargeIndexMerge,
                async path => await _csvValidator.ValidateCsvAsync<FuelSurchargeIndexDto>(path)
            ),
            nameof(TableEnum.TFSCMAP) => new LoadTableDescriptor(
                nameof(TableEnum.TFSCMAP).ToLowerInvariant(),
                (nameof(TableEnum.TFSCMAP) + "_STG").ToLowerInvariant(),
                StoredProcConstant.FuelSurchargeCategoryMapMerge,
                async path => await _csvValidator.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(path)
            ),
            // Accessorial Rates - uses special multi-step process with staging dataset normalization
            nameof(TableEnum.TASYRA) => new LoadTableDescriptor(
                nameof(TableEnum.TASYRA).ToLowerInvariant(),
                (nameof(TableEnum.TASYRA) + "_STG").ToLowerInvariant(),
                StoredProcConstant.RateChartAccessorialRatesMerge,
                async path => await _csvValidator.ValidateCsvAsync<AccessorialRatesDto>(path),
                StoredProcConstant.RateChartAccessorialRatesNormalizeStaging,
                GetRateChartAccessorialRateTableMapping()
            ),
            // Rate Chart Header - uses special multi-step process with staging dataset normalization
            nameof(TableEnum.TCHART) => new LoadTableDescriptor(
                nameof(TableEnum.TCHART).ToLowerInvariant(),
                (nameof(TableEnum.TCHART) + "_STG").ToLowerInvariant(),
                StoredProcConstant.RateChartAccessorialRatesMerge,
                async path => await _csvValidator.ValidateCsvAsync<RateChartHeaderDto>(path),
                StoredProcConstant.RateChartAccessorialRatesNormalizeStaging,
                GetRateChartAccessorialRateTableMapping()
            ),
            // International Zone Header - uses special multi-step process with staging dataset normalization
            nameof(TableEnum.TINZNHD) => new LoadTableDescriptor(
                nameof(TableEnum.TINZNHD).ToLowerInvariant(),
                (nameof(TableEnum.TINZNHD) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InternationalZoneMerge,
                async path => await _csvValidator.ValidateCsvAsync<InternationalZoneHeaderDto>(path),
                StoredProcConstant.InternationalZoneNormalizeStaging,
                GetInternationalZoneTableMapping()
            ),
            // International Zone Detail - uses special multi-step process with staging dataset normalization
            nameof(TableEnum.TINZNDT) => new LoadTableDescriptor(
                nameof(TableEnum.TINZNDT).ToLowerInvariant(),
                (nameof(TableEnum.TINZNDT) + "_STG").ToLowerInvariant(),
                StoredProcConstant.InternationalZoneMerge,
                async path => await _csvValidator.ValidateCsvAsync<InternationalZoneDetailDto>(path),
                StoredProcConstant.InternationalZoneNormalizeStaging,
                GetInternationalZoneTableMapping()
            ),
            //Add other table descriptors here as needed
            _ => LoadTableDescriptor.Unsupported
            };

        /// <summary>
        /// validate loads
        /// </summary>
        /// <param name="loads"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ValidateLoadsAsync(List<DataLoad> loads, CancellationToken ct, Dictionary<string, string> tempFiles)
        {
            foreach (var load in loads)
            {
                var descriptor = GetDescriptor(load.LoadTableName);
                if (!descriptor.IsSupported)
                {
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, DateTime.UtcNow, ct);
                    _logger.LogWarning("Validation unsupported for table {tbl}", load.LoadTableName);
                    BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Unsupported table {load.LoadTableName}");
                    
                    // Move unsupported file to processed folder
                    var unsupportedFileName = Path.GetFileName(load.FileLocation);
                    await MoveObjectToProcessedAsync(unsupportedFileName);
                    continue;
                }
                
                string? tempFile = null;
                try
                {
                    // Download once and keep for copy
                    tempFile = Path.GetTempFileName();
                    var gcsFileName = Path.GetFileName(load.FileLocation);

                    // Check if file exists before attempting to download
                    if (!await _storageService.FileExistsAsync(gcsFileName, ct))
                    {
                        // See if the filename was correct but the casing is wrong.
                        var actualName = _storageService.GetFilenameCaseInsensitive(gcsFileName);
                        if (actualName is not null)
                        {
                            // Still skip processing and log this as an error, but with a special FilenameCaseMismatch status
                            var errorMessage = $"Filename has incorrect casing in receipt file. Searched for {gcsFileName} but found {actualName}. Bucket: {_gcpBucketName}, FileLocation: {load.FileLocation}";
                            _logger.LogError(errorMessage);
                            await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, DateTime.UtcNow, ct);

                            DataLoadDetail detail = await CreateDataLoadDetailForError(load, ct);
                            var exception = new DataLoadException
                            {
                                DataLoadDetailId = detail.Id,
                                TableName = descriptor.TableName,
                                TableKey = $"LOAD:{load.Id}",
                                ErrorFieldName = ServiceConstants.filenameCaseMismatchError,
                                ErrorFieldValue = errorMessage.Length > 100 ? errorMessage[..100] : errorMessage,
                                CreatedOn = DateTime.UtcNow
                            };

                            await _loadRepository.AddExceptionsAsync([exception], ct);
                            
                            // Move the file with actual name to processed folder
                            await MoveObjectToProcessedAsync(actualName);
                            BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: errorMessage);

                            // Clean up temp file
                            if (tempFile != null)
                            {
                                try { File.Delete(tempFile); } catch { }
                            }
                            continue;
                        }
                        
                        _logger.LogError("File not found in GCS bucket. Bucket: {bucket}, Object: {object}, FileLocation: {fileLocation}",
                            _gcpBucketName, gcsFileName, load.FileLocation);
                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, DateTime.UtcNow, ct);
                        
                        DataLoadDetail fileNotFoundDetail = await CreateDataLoadDetailForError(load, ct);
                        var fileNotFoundMessage = $"File not found: {load.FileLocation}";
                        var fileNotFoundException = new DataLoadException
                        {
                            DataLoadDetailId = fileNotFoundDetail.Id,
                            TableName = descriptor.TableName,
                            TableKey = $"LOAD:{load.Id}",
                            ErrorFieldName = ServiceConstants.fileNotFoundError,
                            ErrorFieldValue = fileNotFoundMessage.Length > 100 ? fileNotFoundMessage[..100] : fileNotFoundMessage,
                            CreatedOn = DateTime.UtcNow
                        };

                        await _loadRepository.AddExceptionsAsync([fileNotFoundException], ct);

                        await _loadRepository.UpdateFileLocation(load.Id, "", ct);
                        BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: fileNotFoundMessage);

                        // Clean up temp file (even though download didn't happen, the temp file was created)
                        if (tempFile != null)
                        {
                            try { File.Delete(tempFile); } catch { }
                        }
                        continue;
                    }
                    await _storageService.DownloadFile(gcsFileName, tempFile, ct);

                    // Verify downloaded file size matches remote file size
                    if (!await _storageService.VerifyFileSizeAsync(gcsFileName, tempFile, ct))
                    {
                        _logger.LogError("Downloaded file size mismatch. Remote file: {remoteFile}, Local file: {localFile}. Skipping load.",
                            gcsFileName, tempFile);
                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, DateTime.UtcNow, ct);
                        BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Download file size mismatch for {gcsFileName}");
                        
                        // Clean up temp file
                        try { File.Delete(tempFile); } catch { }
                        
                        // Move mismatched file to processed folder
                        await MoveObjectToProcessedAsync(gcsFileName);
                        continue;
                    }

                    var validation = await descriptor.ValidateAsync(tempFile);
                    bool valid = validation.Success;
                    // Keep the temp file in cache for copy step if valid
                    if (valid)
                    {
                        var key = ExtractObjectName(load.FileLocation);
                        tempFiles[key] = tempFile;
                    }
                    else
                    {
                        // Record validation errors
                        if (validation.ValidationErrors?.Count > 0)
                        {
                            DataLoadDetail detail = await CreateDataLoadDetailForError(load, ct);

                            var exceptions = validation.ValidationErrors.Select((msg, idx) => new DataLoadException
                            {
                                DataLoadDetailId = detail.Id,
                                TableName = descriptor.TableName,
                                TableKey = $"LOAD:{load.Id}",
                                ErrorFieldName = ServiceConstants.csvValidationError,
                                ErrorFieldValue = msg,
                                CreatedOn = DateTime.UtcNow
                            });
                            await _loadRepository.AddExceptionsAsync(exceptions, ct);
                        }
                        // Move invalid file to processed folder
                        await MoveObjectToProcessedAsync(gcsFileName);
                        // Delete temp
                        try { File.Delete(tempFile); } catch { }
                    }

                    await _loadRepository.UpdateStatusAsync(load.Id, valid ? LoadStatus.ReadyToProcess : LoadStatus.FailedValidation, valid ? null : DateTime.UtcNow, ct);
                    _logger.LogInformation("Validation {result} for table {tbl} load {id}", valid ? "passed" : "failed", load.LoadTableName, load.Id);
                    if (!valid)
                    {
                        BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"CSV validation failed: {string.Join("; ", validation.ValidationErrors ?? new List<string>())}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Validation error for table {tbl} load {id}", load.LoadTableName, load.Id);
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, DateTime.UtcNow, ct);
                    BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, errorDetails: $"Validation Failed: {ex.Message}");

                    DataLoadDetail exceptionDetail = await CreateDataLoadDetailForError(load, ct);
                    var dataLoadException = new DataLoadException
                    {
                        DataLoadDetailId = exceptionDetail.Id,
                        TableName = load.LoadTableName,
                        TableKey = $"LOAD:{load.Id}",
                        ErrorFieldName = ServiceConstants.validateLoadsError,
                        ErrorFieldValue = ex.Message.Length > 100 ? ex.Message[..100] : ex.Message,
                        CreatedOn = DateTime.UtcNow
                    };

                    await _loadRepository.AddExceptionsAsync([dataLoadException], ct);

                    // Clean up temp file if it was created
                    if (tempFile != null)
                    {
                        try { File.Delete(tempFile); } catch { }
                    }
                    
                    // Move the file to processed folder even on error
                    var errorFileName = Path.GetFileName(load.FileLocation);
                    await MoveObjectToProcessedAsync(errorFileName);
                }
            }
        }

        private async Task<DataLoadDetail> CreateDataLoadDetailForError(DataLoad load, CancellationToken ct)
        {
            // Create a DataLoadDetail to satisfy FK and track validation failure
            var detail = new DataLoadDetail
            {
                DataLoadId = load.Id,
                DataLoadType = "VAL",
                ErrorIndicator = 1,
                TimeProcessValue = 0,
                TimePeriodTypeCode = "SECONDS",
                RecordsInserted = 0,
                RecordsUpdated = 0,
                RecordsDeleted = 0,
                BatchNumber = 1
            };
            await _loadRepository.AddDetailAsync(detail, ct);
            return detail;
        }

        /// <summary>
        /// merge from staging to main table
        /// </summary>
        /// <param name="load"></param>
        /// <param name="descriptor"></param>
        /// <param name="sw"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<(MergeResult MergeResult, DataLoadDetail MergeDetail)> PerformMergeAsync(DataLoad load, LoadTableDescriptor descriptor, System.Diagnostics.Stopwatch sw, CancellationToken ct)
        {
            var merge = await _loadRepository.ExecuteMergeStoredProcedureAsync(descriptor.MergeStoredProcedure, ct);
            bool hasError = !string.IsNullOrWhiteSpace(merge.ErrorMessage);
            sw.Stop();
            var detail = new DataLoadDetail
            {
                DataLoadId = load.Id,
                DataLoadType = "ACL",
                ErrorIndicator = (short)(hasError ? 1 : 0),
                TimeProcessValue = (int)sw.Elapsed.TotalSeconds,
                TimePeriodTypeCode = "SECONDS",
                RecordsInserted = merge.Inserted,
                RecordsUpdated = merge.Updated,
                RecordsDeleted = merge.Deleted,
                BatchNumber = 1
            };
            await _loadRepository.AddDetailAsync(detail, ct);
            return (merge, detail);
        }

        /// <summary>
        /// copy batch to staging table
        /// </summary>
        /// <param name="load"></param>
        /// <param name="tempFile"></param>
        /// <param name="descriptor"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<(bool Success, DataLoadDetail? StagingDetail, int RowsLoaded, List<string> Errors)> CopyBatchAsync(DataLoad load, string tempFile, LoadTableDescriptor descriptor, CancellationToken ct)
        {
            // Build configuration for staging table
            var cfg = new UPS.WWRR.Business.DTO.Models.Request.TableConfigurationRequest { TableName = descriptor.StagingTableName, DataLoadId = load.Id };
            var copyResult = await _copyBatchService.CopyAsync(tempFile, cfg, ct);
            // Per-chunk DataLoadDetail records are already created by CopyBatchDataService.CopyAsync,
            // so no additional summary detail is needed here.
            return (copyResult.Errors.Count == 0, null, copyResult.RowsLoaded, copyResult.Errors);
        }

        /// <summary>
        /// parse csv content into list of string arrays
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private List<string[]>? ParseCsv(string content)
        {
            try
            {
                return ParseCsvWithValidationHelper.ParseCsvWithValidation(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }


        /// <summary>
        /// load table descriptor
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="StagingTableName"></param>
        /// <param name="MergeStoredProcedure"></param>
        /// <param name="ValidateAsync"></param>
        /// <param name="StagingDatasetProcedure">Optional stored procedure to normalize raw staging data before merge (used for Area Classification and Domestic Zone tables)</param>
        /// <param name="NormalizedTableMapping">Optional dictionary mapping normalized staging tables to their main tables for marking staging completion</param>
        private sealed record LoadTableDescriptor(
            string TableName,
            string StagingTableName,
            string MergeStoredProcedure,
            Func<string, Task<CsvValidationResponse>> ValidateAsync,
            string? StagingDatasetProcedure = null,
            Dictionary<string, string>? NormalizedTableMapping = null)
        {
            public bool IsSupported => TableName != "UNSUPPORTED";
            public bool RequiresStagingNormalization => !string.IsNullOrEmpty(StagingDatasetProcedure);
            public static LoadTableDescriptor Unsupported => new("UNSUPPORTED", string.Empty, string.Empty, _ => Task.FromResult(new CsvValidationResponse(false, new List<string> { "Unsupported table" })));
        }

        private static string ExtractObjectName(string gsUrl)
        {
            const string scheme = "gs://";
            if (!gsUrl.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)) return gsUrl;
            var parts = gsUrl.Substring(scheme.Length).Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 2 ? parts[1] : gsUrl; // return object path after bucket
        }

        /// <summary>
        /// Handles the complete staging normalization load process including merge execution and result handling.
        /// Executes staging normalization merge and updates load status based on result.
        /// For paired table groups (e.g., TINZNHD/TINZNDT), the merge SP only runs once for the first table.
        /// </summary>
        /// <param name="load">The data load being processed</param>
        /// <param name="descriptor">The table descriptor with normalization settings</param>
        /// <param name="sw">Stopwatch for timing the operation</param>
        /// <param name="ct">Cancellation token</param>
        private async Task HandleStagingNormalizationLoadAsync(
            DataLoad load, LoadTableDescriptor descriptor, System.Diagnostics.Stopwatch sw, CancellationToken ct)
        {
            // Check if merge SP was already executed for this paired group in this cycle
            // For paired tables (e.g., TINZNHD/TINZNDT), they share the same merge SP, so no need to run again the same SP.
            bool mergeAlreadyExecuted = _processedMergeSPs.Contains(descriptor.MergeStoredProcedure);
            
            if (mergeAlreadyExecuted)
            {
                _logger.LogInformation("Merge SP already Executed for table {tbl} load {id} in this pair load", load.LoadTableName, load.Id);
                
                // Get the merge result from the first table to copy record counts
                var previousMergeResult = _pairedMergeResults.GetValueOrDefault(descriptor.MergeStoredProcedure);
                
                // Create a detail record with the same counts as the first table (for consistency)
                sw.Stop();
                var skipDetail = new DataLoadDetail
                {
                    DataLoadId = load.Id,
                    DataLoadType = "ACL",
                    ErrorIndicator = 0,
                    TimeProcessValue = (int)sw.Elapsed.TotalSeconds,
                    TimePeriodTypeCode = "SECONDS",
                    RecordsInserted = previousMergeResult?.Inserted ?? 0,
                    RecordsUpdated = previousMergeResult?.Updated ?? 0,
                    RecordsDeleted = previousMergeResult?.Deleted ?? 0,
                    BatchNumber = 1
                };
                await _loadRepository.AddDetailAsync(skipDetail, ct);
                
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processed, DateTime.UtcNow, ct);
                _logger.LogInformation("Load Process Completed for second table {tbl} load {id} in this pair load. Records Count: Inserted={ins}, Updated={upd}, Deleted={del}", 
                    load.LoadTableName, load.Id, skipDetail.RecordsInserted, skipDetail.RecordsUpdated, skipDetail.RecordsDeleted);
                BuildCsvLoadLog(load, ServiceConstants.LoadStatusSuccess, skipDetail.RecordsInserted, skipDetail.RecordsUpdated, skipDetail.RecordsDeleted);
                _processedLoadVersions.Add(load.LoadVersion);
                return;
            }

            var (mergeResult, mergeDetail) = await PerformStagingNormalizationMergeAsync(load, descriptor, sw, ct);
            bool hasError = !string.IsNullOrWhiteSpace(mergeResult.ErrorMessage);

            if (hasError)
            {
                int errorCode = 0;
                if (!string.IsNullOrWhiteSpace(mergeResult.ErrorNumber) && int.TryParse(mergeResult.ErrorNumber, out var parsed))
                    errorCode = parsed;

                var error = new DataLoadError
                {
                    DataLoadDetailId = mergeDetail.Id,
                    ErrorCode = errorCode,
                    ErrorStoredProcedureName = mergeResult.ErrorProcedure,
                    ErrorMessage = mergeResult.ErrorMessage,
                    CreatedOn = DateTime.UtcNow
                };
                await _loadRepository.AddErrorsAsync(new[] { error }, ct);
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, DateTime.UtcNow, ct);
                BuildCsvLoadLog(load, ServiceConstants.LoadStatusFailed, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted, mergeResult.ErrorMessage);
            }
            else
            {
                // Mark this merge SP as processed and store the result for paired table
                _processedMergeSPs.Add(descriptor.MergeStoredProcedure);
                _pairedMergeResults[descriptor.MergeStoredProcedure] = mergeResult;
                
                if (descriptor.NormalizedTableMapping != null)
                {
                    await _loadRepository.MarkStagingCompletedForMultipleTablesAsync(
                        descriptor.NormalizedTableMapping.Keys, ct);
                    _logger.LogInformation(
                        "Staging tables marked as completed for {count} tables (DataLoad: {loadId})",
                        descriptor.NormalizedTableMapping.Count, load.Id);
                }
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processed, DateTime.UtcNow, ct);
                _logger.LogInformation("Load Process Completed for table {tbl} load {id}", load.LoadTableName, load.Id);
                BuildCsvLoadLog(load, ServiceConstants.LoadStatusSuccess, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted);
                _processedLoadVersions.Add(load.LoadVersion);
            }
        }

        /// <summary>
        /// Performs the staging normalization merge process for tables that require staging dataset normalization:
        /// 1. Execute staging dataset procedure to normalize data from raw staging tables to normalized staging tables
        /// 2. Execute merge procedure to move data from normalized staging to actual tables
        /// </summary>
        private async Task<(MergeResult MergeResult, DataLoadDetail MergeDetail)> PerformStagingNormalizationMergeAsync(
            DataLoad load, LoadTableDescriptor descriptor, System.Diagnostics.Stopwatch sw, CancellationToken ct)
        {
            _logger.LogInformation("Executing staging dataset normalization for table {tbl} load {id}", load.LoadTableName, load.Id);
            var stagingResult = await _loadRepository.ExecuteMergeStoredProcedureAsync(descriptor.StagingDatasetProcedure!, ct);

            if (!string.IsNullOrWhiteSpace(stagingResult.ErrorMessage))
            {
                _logger.LogError("Staging dataset normalization failed for table {tbl} load {id}: {error}",
                    load.LoadTableName, load.Id, stagingResult.ErrorMessage);
                sw.Stop();
                var stagingDetail = new DataLoadDetail
                {
                    DataLoadId = load.Id,
                    DataLoadType = "NRM",
                    ErrorIndicator = 1,
                    TimeProcessValue = (int)sw.Elapsed.TotalSeconds,
                    TimePeriodTypeCode = "SECONDS",
                    RecordsInserted = stagingResult.Inserted,
                    RecordsUpdated = stagingResult.Updated,
                    RecordsDeleted = stagingResult.Deleted,
                    BatchNumber = 1
                };
                await _loadRepository.AddDetailAsync(stagingDetail, ct);
                return (stagingResult, stagingDetail);
            }

            _logger.LogInformation("Staging dataset normalization completed for table {tbl} load {id}. Inserted: {ins}",
                load.LoadTableName, load.Id, stagingResult.Inserted);

            _logger.LogInformation("Executing merge for table {tbl} load {id}", load.LoadTableName, load.Id);
            var mergeResult = await _loadRepository.ExecuteMergeStoredProcedureAsync(descriptor.MergeStoredProcedure, ct);

            sw.Stop();
            var mergeDetail = new DataLoadDetail
            {
                DataLoadId = load.Id,
                DataLoadType = "ACL",
                ErrorIndicator = (short)(!string.IsNullOrWhiteSpace(mergeResult.ErrorMessage) ? 1 : 0),
                TimeProcessValue = (int)sw.Elapsed.TotalSeconds,
                TimePeriodTypeCode = "SECONDS",
                RecordsInserted = mergeResult.Inserted,
                RecordsUpdated = mergeResult.Updated,
                RecordsDeleted = mergeResult.Deleted,
                BatchNumber = 1
            };
            await _loadRepository.AddDetailAsync(mergeDetail, ct);

            if (!string.IsNullOrWhiteSpace(mergeResult.ErrorMessage))
            {
                _logger.LogError("Merge failed for table {tbl} load {id}: {error}",
                    load.LoadTableName, load.Id, mergeResult.ErrorMessage);
            }
            else
            {
                _logger.LogInformation("Merge completed for table {tbl} load {id}. Inserted: {ins}, Updated: {upd}, Deleted: {del}",
                    load.LoadTableName, load.Id, mergeResult.Inserted, mergeResult.Updated, mergeResult.Deleted);
            }

            return (mergeResult, mergeDetail);
        }

        /// <summary>
        /// Gets the table mapping for Area Classification tables (staging -> main tables)
        /// Used for marking IS_COMPLETED_IR on staging tables after merge
        /// </summary>
        private static Dictionary<string, string> GetAreaClassificationTableMapping() => new(StringComparer.OrdinalIgnoreCase)
        {
            { "tarclhd_stg", "tarclhd_stg" },
            { "tarcldt_stg", "tarcldt_stg" },
            { "zchartsts_stg", "zchartsts" },
            { "zchartlkup_stg", "zchartlkup" },
            { "tarclhd_new_stg", "tarclhd" },
            { "tarcldt_new_stg", "tarcldt" },
            { "zchartdtngeo_stg", "zchartdtngeo" },
            { "zchartdtngpu_stg", "zchartdtngpu" },
            { "zchartorggeo_stg", "zchartorggeo" },
            { "zchartorggpu_stg", "zchartorggpu" },
            { "zchartsvctyp_stg", "zchartsvctyp" }
        };

        /// <summary>
        /// Gets the table mapping for Domestic Zone tables (staging -> main tables)
        /// Used for marking IS_COMPLETED_IR on staging tables after merge
        /// </summary>
        private static Dictionary<string, string> GetDomesticZoneTableMapping() => new(StringComparer.OrdinalIgnoreCase)
        {
            { "tdoznhd_stg", "tdoznhd_stg" },
            { "tdozndt_stg", "tdozndt_stg" },
            { "domzchartsts_stg", "domzchartsts" },
            { "domzchartlkup_stg", "domzchartlkup" },
            { "tdoznhd_new_stg", "tdoznhd" },
            { "tdozndt_new_stg", "tdozndt" },
            { "domzchartdtngeo_stg", "domzchartdtngeo" },
            { "domzchartorggeo_stg", "domzchartorggeo" }
        };

        /// <summary>
        /// Gets the table mapping for Rate Chart / Accessorial Rates tables (staging -> main tables)
        /// Used for marking IS_COMPLETED_IR on staging tables after merge
        /// </summary>
        private static Dictionary<string, string> GetRateChartAccessorialRateTableMapping() => new(StringComparer.OrdinalIgnoreCase)
        {
            { "tasyra_stg", "tasyra_stg" },
            { "tchart_stg", "tchart_stg" },
            { "accrate_stg", "accrate" },
            { "accratecrit_stg", "accratecrit" },
            { "chartacccd_stg", "chartacccd" },
            { "chartorggeo_stg", "chartorggeo" },
            { "chartsts_stg", "chartsts" },
            { "chartsvcpkg_stg", "chartsvcpkg" }
        };

        /// <summary>
        /// Gets the table mapping for International Zone tables (staging -> main tables)
        /// Used for marking IS_COMPLETED_IR on staging tables after merge
        /// </summary>
        private static Dictionary<string, string> GetInternationalZoneTableMapping() => new(StringComparer.OrdinalIgnoreCase)
        {
            { "tinznhd_stg", "tinznhd_stg" },
            { "tinzndt_stg", "tinzndt_stg" },
            { "izchartsts_stg", "izchartsts" },
            { "izchartlkup_stg", "izchartlkup" },
            { "izcharthd_stg", "izcharthd" },
            { "izchartdtl_stg", "izchartdtl" },
            { "izchartorgdtnpst_stg", "izchartorgdtnpst" },
            { "izchartorgpoldiv_stg", "izchartorgpoldiv" },
            { "izchartdtnpoldiv_stg", "izchartdtnpoldiv" }
        };

        /// <summary>
        /// Accumulates a structured log entry for a CSV file load operation.
        /// The entry is added to the collection and emitted as a single summary at the end of the processing cycle.
        /// </summary>
        private void BuildCsvLoadLog(DataLoad load, string loadStatus, int recordsInserted = 0, int recordsUpdated = 0, int recordsDeleted = 0, string? errorDetails = null)
        {
            var logEntry = new CsvLoadLogEntry
            {
                TableName = load.LoadTableName,
                CsvFileName = Path.GetFileName(load.FileLocation),
                LoadStatus = loadStatus,
                LoadVersion = load.LoadVersion,
                RecordsInserted = recordsInserted,
                RecordsUpdated = recordsUpdated,
                RecordsDeleted = recordsDeleted,
                ErrorDetails = errorDetails
            };

            _csvLoadLogEntries.Add(logEntry);
        }

        /// <summary>
        /// Emits a single structured summary log entry containing all CSV load results for the current processing cycle.
        /// </summary>
        private void LogCsvLoadSummary()
        {
            if (_csvLoadLogEntries.Count == 0) return;

            int totalFiles = _csvLoadLogEntries.Count;
            int successCount = _csvLoadLogEntries.Count(e => e.LoadStatus == ServiceConstants.LoadStatusSuccess);
            int failedCount = totalFiles - successCount;

            var summary = new CsvLoadSummaryLogEntry
            {
                TotalFiles = totalFiles,
                SuccessCount = successCount,
                FailedCount = failedCount,
                Summary = BuildSummaryTable(_csvLoadLogEntries, totalFiles, successCount, failedCount)
            };

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CsvLoadSummaryLogEntry"] = true,
                [nameof(summary.TotalFiles)] = summary.TotalFiles,
                [nameof(summary.SuccessCount)] = summary.SuccessCount,
                [nameof(summary.FailedCount)] = summary.FailedCount,
                [nameof(summary.Summary)] = summary.Summary
            }))
            {
                _logger.LogInformation("CsvLoadSummary");
            }
        }

        /// <summary>
        /// Builds a fixed-width formatted summary table from the accumulated CSV load log entries.
        /// The output is suitable for email alert display and structured log Summary field.
        /// </summary>
        private static string BuildSummaryTable(List<CsvLoadLogEntry> entries, int totalFiles, int successCount, int failedCount)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            const string separator = "------------------------------------------------------------------------------------------------------------------------";
            const int maxErrorLength = 50;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"WWRR Data Load Summary \u2014 {timestamp} UTC");
            sb.AppendLine($"Total Files: {totalFiles} | Success: {successCount} | Failed: {failedCount}");
            sb.AppendLine(separator);
            sb.AppendLine($"{"Table",-12}{"CSV File",-29}{"Status",-10}{"Inserted",10}{"Updated",10}{"Deleted",10}   Error");
            sb.AppendLine(separator);

            foreach (var entry in entries)
            {
                var error = entry.ErrorDetails ?? string.Empty;
                if (error.Length > maxErrorLength)
                    error = string.Concat(error.AsSpan(0, maxErrorLength - 3), "...");

                sb.AppendLine($"{entry.TableName,-12}{entry.CsvFileName,-29}{entry.LoadStatus,-10}{entry.RecordsInserted,10}{entry.RecordsUpdated,10}{entry.RecordsDeleted,10}   {error}");
            }

            sb.AppendLine(separator);
            return sb.ToString();
        }

        /// <summary>
        /// Publishes a Pub/Sub notification message after the job completes successfully
        /// and at least one data load was processed.
        /// </summary>
        private async Task PublishPubSubNotificationAsync()
        {
            if (_pubSubService == null || _processedLoadVersions.Count == 0)
                return;

            try
            {
                var message = new PubSubNotificationMessage
                {
                    RunId = Guid.NewGuid(),
                    IsLocal = false,
                    LoadVersions = _processedLoadVersions.ToList(),
                    Bucket = _gcpBucketName,
                    TestSuite = ServiceConstants.TestSuite
                };

                var messageJson = JsonConvert.SerializeObject(message);
                await _pubSubService.PublishMessageAsync(messageJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Pub/Sub notification message.");
            }
        }

        private async Task MoveObjectToProcessedAsync(string objectName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(objectName)) return;
                if (_movedObjects.Contains(objectName)) return;
                var destName = $"{ServiceConstants.processedDataFilesFolder}/{Path.GetFileName(objectName)}";
                await _storageService.MoveFile(objectName, destName);
                _movedObjects.Add(objectName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to move file to processed folder: {file}", objectName);
            }
        }
    }
}
