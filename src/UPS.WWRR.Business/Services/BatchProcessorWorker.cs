#nullable enable
using Microsoft.Extensions.Logging;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.Common.Enum;
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

        private readonly string _gcpBucketName;
        private readonly string _tableNameFilter;
        private readonly int _defaultChunkSize = 100_000;
        private readonly int _batchLoadChunkSize;
        private readonly HashSet<string> _movedObjects = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Defines groups of tables that must be processed together.
        /// All tables in a group must be present in the receipt file for any of them to be processed.
        /// </summary>
        private static readonly List<HashSet<string>> _pairedTableGroups =
        [
            new(StringComparer.OrdinalIgnoreCase) { nameof(TableEnum.TARCLHD), nameof(TableEnum.TARCLDT) },
            new(StringComparer.OrdinalIgnoreCase) { nameof(TableEnum.TDOZNHD), nameof(TableEnum.TDOZNDT) }
        ];

        public BatchProcessorWorker(ILogger<BatchProcessorWorker> logger,
                                IStorageService storageService,
                                ICsvValidator csvValidator,
                                ICopyBatchDataService copyBatchService,
                                ILoadRepository loadRepository)
        {
            _logger = logger;
            _storageService = storageService;
            _csvValidator = csvValidator;
            _copyBatchService = copyBatchService;
            _loadRepository = loadRepository;
            _gcpBucketName = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_STORAGE_BUCKET_NAME")
                        ?? throw new InvalidOperationException("GOOGLE_CLOUD_STORAGE_BUCKET_NAME environment variable is not set.");
            _tableNameFilter = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "ALL";
            _batchLoadChunkSize = int.TryParse(Environment.GetEnvironmentVariable("BatchLoad_ChunkSize"), out var cs) ? cs : _defaultChunkSize;
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

                // Clear moved objects tracker at the start of each cycle to allow re-processing of files with same names
                _movedObjects.Clear();

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
                    var readyAfterValidation = await _loadRepository.GetLoadsByStatusAsync(LoadStatus.ReadyToProcess, stoppingToken);
                    if (readyAfterValidation.Count > 0)
                    {
                        readyAfterValidation = FilterByTableName(readyAfterValidation);
                        await CopyBatchLoadAsync(readyAfterValidation, stoppingToken, tempFiles);
                        tempFiles.Clear(); // Clear references after copy

                        //  main table for loads that are still Processing after copy
                        var processingLoads = await _loadRepository.GetLoadsByStatusAsync(LoadStatus.Processing, stoppingToken);
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
                _logger.LogError(ex, "Unhandled error in Processor");
            }


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
            private List<DataLoad> ValidateAndFilterPairedTableGroups(List<DataLoad> loads)
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
                    return loads.Where(l => !tablesToRemove.Contains(l.LoadTableName)).ToList();
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
            if (rows.Count <= 1)
            {
                _logger.LogInformation("Receipt file {file} contains no data rows", dynamicReceiptName);
                return newLoads;
            }

            // Inline former BuildLoadsFromReceiptAsync logic
            var header = rows[0];
            int fileExtractNameIndex = Array.FindIndex(header, h => h.Equals(ServiceConstants.fileExtractName, StringComparison.OrdinalIgnoreCase));
            if (fileExtractNameIndex < 0)
            {
                _logger.LogError("Required columns (FileExtractName, Source) not found in receipt file. Columns: {cols}", string.Join("|", header));
            }
            else
            {
                var batchSizeEnv = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out var bs) ? bs : _defaultChunkSize;
                foreach (var r in rows.Skip(1))
                {
                    if (r.Length <= fileExtractNameIndex) continue;
                    var fileExtractName = r[fileExtractNameIndex];
                    if (string.IsNullOrWhiteSpace(fileExtractName)) continue;

                    var parts = fileExtractName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        _logger.LogWarning("Cannot parse table/load id from {val}", fileExtractName);
                        continue;
                    }

                    var tableNamePart = parts[0];
                    var loadIdDigits = new string(parts[^1].Where(char.IsDigit).ToArray());
                    long.TryParse(loadIdDigits, out var loadId);
                    if (loadId == 0) loadId = DateTime.UtcNow.Ticks;

                    if (await _loadRepository.ExistsAsync(tableNamePart, loadId, ct))
                    {
                        _logger.LogInformation("DataLoad already exists for {tbl} version {ver}. Skipping insert.", tableNamePart, loadId);
                        continue;
                    }

                    newLoads.Add(new DataLoad
                    {
                        LoadTableName = tableNamePart,
                        LoadVersionNumber = loadId,
                        LoadStatusCode = LoadStatus.ReadyForValidation.ToString(),
                        FileLocation = $"gs://{_gcpBucketName}/{_storageService.PrependBaseDirectory(fileExtractName)}",
                        CreatedOn = DateTime.UtcNow,
                        LogFileLocation = $"gs://{_gcpBucketName}/{_storageService.PrependBaseDirectory(dynamicReceiptName)}",
                        TotalBatchNumber = 0,
                        BatchSize = batchSizeEnv
                    });
                }
            }

            if (newLoads.Count > 0)
            {
                newLoads = FilterByTableName(newLoads);
                
                // Validate paired table groups - all tables in a group must be present together
                newLoads = ValidateAndFilterPairedTableGroups(newLoads);
                
                if (newLoads.Count > 0)
                    await _loadRepository.AddLoadsAsync(newLoads, ct);
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
                if (!descriptor.IsSupported) continue;
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processing, null, ct);

                // Key from FileLocation object path
                var key = ExtractObjectName(load.FileLocation);
                string tempFile;
                if (!tempFiles.TryGetValue(key, out tempFile!))
                {
                    tempFile = Path.GetTempFileName();
                    var gcsFileName = Path.GetFileName(load.FileLocation);
                    await _storageService.DownloadFile(gcsFileName, tempFile, ct);
                }

                try
                {
                    var (copySuccess, stagingDetail, rowsLoaded, copyErrors) = await CopyBatchAsync(load, tempFile, descriptor, ct);
                    if (!copySuccess)
                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, null, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CopyBatch failed for table {tbl} load {id}", load.LoadTableName, load.Id);
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, null, ct);
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
                        var (mergeResult, mergeDetail) = await PerformMergeAsync(load, descriptor, sw, ct);
                        bool hasError = !string.IsNullOrWhiteSpace(mergeResult.ErrorMessage);
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
                            await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, null, ct);
                        }
                        else
                        {
                            var updatedRef = await _loadRepository.UpdateLoadReferenceAsync(descriptor.StagingTableName, descriptor.TableName, load.Id, mergeDetail.Id, ct);
                            _logger.LogInformation("LOAD_REF_TE updated via stored procedure for staging {stg} and main {main} (value: {val})", descriptor.StagingTableName, descriptor.TableName, $"{load.Id}|{mergeDetail.Id}");
                            await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processed, DateTime.UtcNow, ct);
                            _logger.LogInformation("Load Process Completed for table {tbl} load {id}", load.LoadTableName, load.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Merge failed for table {tbl} load {id}", load.LoadTableName, load.Id);
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, null, ct);
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
                StoredProcConstant.FuelSurchargeMerge,
                async path => await _csvValidator.ValidateCsvAsync<FuelSurchargeDto>(path)
            ),
            nameof(TableEnum.TVASYLN) => new LoadTableDescriptor(
                    nameof(TableEnum.TVASYLN).ToLowerInvariant(),
                    (nameof(TableEnum.TVASYLN) + "_STG").ToLowerInvariant(),
                    StoredProcConstant.ValidAccessorialLaneMerge,
                    async path => await _csvValidator.ValidateCsvAsync<ValidAccessorialLaneDto>(path)
                ),
                nameof(TableEnum.TRASTD) => new LoadTableDescriptor(
                   nameof(TableEnum.TRASTD).ToLowerInvariant(),
                   (nameof(TableEnum.TRASTD) + "_STG").ToLowerInvariant(),
                   StoredProcConstant.FreightRatesMerge,
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
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, null, ct);
                    _logger.LogWarning("Validation unsupported for table {tbl}", load.LoadTableName);
                    continue;
                }
                try
                {
                    // Download once and keep for copy
                    var tempFile = Path.GetTempFileName();
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
                            await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, null, ct);

                            DataLoadDetail detail = await CreateDataLoadDetailForError(load, ct);
                            var exception = new DataLoadException
                            {
                                DataLoadDetailId = detail.Id,
                                TableName = descriptor.TableName,
                                TableKey = $"LOAD:{load.Id}",
                                ErrorFieldName = ServiceConstants.filenameCaseMismatchError,
                                ErrorFieldValue = errorMessage[..100],
                                CreatedOn = DateTime.UtcNow
                            };

                            await _loadRepository.AddExceptionsAsync([exception], ct);
                            continue;
                        }
                        _logger.LogError("File not found in GCS bucket. Bucket: {bucket}, Object: {object}, FileLocation: {fileLocation}",
                            _gcpBucketName, gcsFileName, load.FileLocation);
                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, null, ct);
                        continue;
                    }
                    await _storageService.DownloadFile(gcsFileName, tempFile, ct);

                    // Verify downloaded file size matches remote file size
                    if (!await _storageService.VerifyFileSizeAsync(gcsFileName, tempFile, ct))
                    {
                        _logger.LogError("Downloaded file size mismatch. Remote file: {remoteFile}, Local file: {localFile}. Skipping load.",
                            gcsFileName, tempFile);
                        await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, null, ct);
                        try { File.Delete(tempFile); } catch { }
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

                    await _loadRepository.UpdateStatusAsync(load.Id, valid ? LoadStatus.ReadyToProcess : LoadStatus.FailedValidation, null, ct);
                    _logger.LogInformation("Validation {result} for table {tbl} load {id}", valid ? "passed" : "failed", load.LoadTableName, load.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Validation error for table {tbl} load {id}", load.LoadTableName, load.Id);
                    await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.FailedValidation, null);
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
            DataLoadDetail? stagingDetail = null;
            if (copyResult.Errors.Count > 0)
            {
                stagingDetail = new DataLoadDetail
                {
                    DataLoadId = load.Id,
                    DataLoadType = "STG",
                    ErrorIndicator = 1,
                    TimeProcessValue = (int)(copyResult.CompletedAt - copyResult.StartedAt).TotalSeconds,
                    TimePeriodTypeCode = "SECONDS",
                    RecordsInserted = copyResult.RowsLoaded,
                    RecordsUpdated = 0,
                    RecordsDeleted = 0,
                    BatchNumber = 1
                };
                await _loadRepository.AddDetailAsync(stagingDetail, ct);
            }
            return (copyResult.Errors.Count == 0, stagingDetail, copyResult.RowsLoaded, copyResult.Errors);
        }

        /// <summary>
        /// parse csv content into list of string arrays
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static List<string[]> ParseCsv(string content)
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<string[]>();
            foreach (var l in lines)
                list.Add(l.Split(',', StringSplitOptions.None));
            return list;
        }

        /// <summary>
        /// load table descriptor
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="StagingTableName"></param>
        /// <param name="MergeStoredProcedure"></param>
        /// <param name="ValidateAsync"></param>
        /// <param name="StagingDatasetProcedure">Optional stored procedure to normalize raw staging data before merge (used for Area Classification and Domestic Zone tables)</param>
        /// <param name="NormalizedTableMapping">Optional dictionary mapping normalized staging tables to their main tables for load reference update</param>
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
        /// </summary>
        /// <param name="load">The data load being processed</param>
        /// <param name="descriptor">The table descriptor with normalization settings</param>
        /// <param name="sw">Stopwatch for timing the operation</param>
        /// <param name="ct">Cancellation token</param>
        private async Task HandleStagingNormalizationLoadAsync(
            DataLoad load, LoadTableDescriptor descriptor, System.Diagnostics.Stopwatch sw, CancellationToken ct)
        {
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
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Failed, null, ct);
            }
            else
            {
                if (descriptor.NormalizedTableMapping != null)
                {
                    await _loadRepository.UpdateLoadReferenceForMultipleTablesAsync(
                        descriptor.NormalizedTableMapping, load.Id, mergeDetail.Id, ct);
                    _logger.LogInformation(
                        "LOAD_REF_TE updated for {count} tables (DataLoad: {loadId}, Detail: {detailId})",
                        descriptor.NormalizedTableMapping.Count, load.Id, mergeDetail.Id);
                }
                await _loadRepository.UpdateStatusAsync(load.Id, LoadStatus.Processed, DateTime.UtcNow, ct);
                _logger.LogInformation("Load Process Completed for table {tbl} load {id}", load.LoadTableName, load.Id);
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
        /// Used for updating LOAD_REF_TE and IS_COMPLETED_IR across all related tables after merge
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
        /// Used for updating LOAD_REF_TE and IS_COMPLETED_IR across all related tables after merge
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
