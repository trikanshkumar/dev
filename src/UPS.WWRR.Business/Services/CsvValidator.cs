using CsvHelper;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using UPS.WWRR.Business.Common.Constants;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.Business.Services;

public class CsvValidator : ICsvValidator
{

    public async Task<CsvValidationResponse> ValidateCsvAsync<TModel>(string csvFileLocation)
    {
        await using var file = File.OpenRead(csvFileLocation);
        using var reader = new StreamReader(file);
        using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null

        });

        List<string> validationErrors = [];

        await csvReader.ReadAsync();
        csvReader.ReadHeader();

        // Validate CSV header columns match the expected schema
        var headerErrors = ValidateHeaderColumns<TModel>(csvReader.HeaderRecord);
        if (headerErrors.Count > 0)
        {
            return new CsvValidationResponse(Success: false, ValidationErrors: headerErrors);
        }

        ConfigureDateTimeFormats(csvReader);

        //loop through the records inorder to have the model be validated. Start line number at 2 because we skip the header
        for (int lineNumber = 2; csvReader.Read(); lineNumber++)
        {
            TModel record;
            try
            {
                record = csvReader.GetRecord<TModel>();
            }
            catch (CsvHelperException)
            {
                validationErrors.Add($"Line: {lineNumber} - Error: Cannot parse line");
                var errorResponse = new CsvValidationResponse(Success: false, ValidationErrors: validationErrors);
                return errorResponse;
            }

            var context = new ValidationContext(record);
            List<ValidationResult> results = [];
            bool isValid = Validator.TryValidateObject(record, context, results, true);

            if (!isValid)
            {
                foreach (ValidationResult validationResult in results)
                {
                    validationErrors.Add($"Line: {lineNumber} - Field: {validationResult.MemberNames.First()} - Error: {validationResult.ErrorMessage}");
                }
            }
        }

        var success = validationErrors.Count == 0;
        var response = new CsvValidationResponse(Success: success, ValidationErrors: validationErrors);
        return response;
    }

    public async Task<CsvValidationResponse> ValidateCsvChunkedAsync<TModel>(string csvFileLocation, int chunkSize)
    {
        await using var file = File.OpenRead(csvFileLocation);
        using var reader = new StreamReader(file);
        using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null
        });

        ConfigureDateTimeFormats(csvReader);

        var errorBag = new ConcurrentBag<string>();

        var records = new List<TModel>(chunkSize);

        var tasks = new List<Task>();

        await csvReader.ReadAsync();
        csvReader.ReadHeader();

        // Validate CSV header columns match the expected schema
        var headerErrors = ValidateHeaderColumns<TModel>(csvReader.HeaderRecord);
        if (headerErrors.Count > 0)
        {
            return new CsvValidationResponse(Success: false, ValidationErrors: headerErrors);
        }

        //Start line number at 2 because we skip the header
        int lineNumber = 2;

        while (csvReader.Read())
        {
            TModel record;
            try
            {
                record = csvReader.GetRecord<TModel>();
            }
            catch (CsvHelperException)
            {
                var parseError = $"Line: {lineNumber} - Error: Cannot parse line";
                var errorResponse = new CsvValidationResponse(Success: false, ValidationErrors: [parseError]);
                return errorResponse;
            }
            records.Add(record);

            if (records.Count >= chunkSize)
            {
                var thisChunk = records.ToList();
                // Pass errorBag to each Task for concurrent writing
                tasks.Add(Task.Run(() => ValidateChunk(thisChunk, lineNumber, errorBag)));
                records.Clear();
            }
            lineNumber++;
        }
        // Process any remaining records
        if (records.Count != 0)
        {
            var lastChunk = records.ToList();
            tasks.Add(Task.Run(() => ValidateChunk(lastChunk, lineNumber, errorBag)));
        }

        await Task.WhenAll(tasks);

        var success = errorBag.Count == 0;
        var response = new CsvValidationResponse(
            Success: success,
            ValidationErrors: [.. errorBag]
        );

        return response;
    }

    private static void ValidateChunk<TModel>(List<TModel> chunk, int startLineNumber, ConcurrentBag<string> errorBag)
    {
        for (int i = 0; i < chunk.Count; i++)
        {
            var record = chunk[i];
            var context = new ValidationContext(record);
            List<ValidationResult> results = [];
            bool isValid = Validator.TryValidateObject(record, context, results, true);

            if (!isValid)
            {
                foreach (ValidationResult validationResult in results)
                {
                    var lineNumber = startLineNumber + i;
                    var errorMessage = $"Line: {lineNumber} - Field: {validationResult.MemberNames.First()} - Error: {validationResult.ErrorMessage}";
                    errorBag.Add(errorMessage);
                }
            }
        }
    }
    private static void ConfigureDateTimeFormats(CsvReader csvReader)
    {
        var optsNonNull = csvReader.Context.TypeConverterOptionsCache.GetOptions<DateTime>();
        optsNonNull.Formats = ServiceConstants.CsvDateTimeFormats;
        optsNonNull.DateTimeStyle = DateTimeStyles.AllowWhiteSpaces;

        var optsNull = csvReader.Context.TypeConverterOptionsCache.GetOptions<DateTime?>();
        optsNull.Formats = ServiceConstants.CsvDateTimeFormats;
        optsNull.DateTimeStyle = DateTimeStyles.AllowWhiteSpaces;
    }

    /// <summary>
    /// Validates that CSV header columns match the expected DTO model properties.
    /// Returns a list of error messages for any extra columns not found in the schema.
    /// </summary>
    private static List<string> ValidateHeaderColumns<TModel>(string[]? headerRecord)
    {
        var errors = new List<string>();

        if (headerRecord == null || headerRecord.Length == 0)
        {
            errors.Add("CSV file has no header row.");
            return errors;
        }

        // Get all valid column names from the DTO model:
        // - Property names (default)
        // - CsvHelper Name attribute values (if specified)
        var validColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Add property name as valid
            validColumnNames.Add(property.Name);

            // Also add any CsvHelper Name attribute values
            var nameAttribute = property.GetCustomAttribute<NameAttribute>();
            if (nameAttribute?.Names != null)
            {
                foreach (var name in nameAttribute.Names)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                        validColumnNames.Add(name);
                }
            }
        }

        // Find columns in CSV that don't match any property or Name attribute in the DTO
        var extraColumns = headerRecord
            .Select(h => h.Trim())
            .Where(h => !string.IsNullOrWhiteSpace(h) && !validColumnNames.Contains(h))
            .ToList();

        if (extraColumns.Count > 0)
        {
            errors.Add($"CSV contains column(s) not matching table schema: [{string.Join(", ", extraColumns)}]");
        }

        return errors;
    }

}

