using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using UPS.WWRR.Business.Common.Constants;


namespace UPS.WWRR.Business.Common.Helper
{
    public static class ParseCsvWithValidationHelper
    {
        // Call like: var rows = ParseCsvWithValidation(csvText);
        public static List<string[]> ParseCsvWithValidation(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException($"CSV {ServiceConstants.receiptLogFilePattern} content is empty.", nameof(content));

            using var reader = new StringReader(content);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = context =>
                {
                    // Optional: capture malformed fields
                    throw new FormatException($"Bad CSV data at row : {context.RawRecord}");
                },
                MissingFieldFound = null, // ignore missing-field events (we'll validate ourselves)
                DetectDelimiter = true
                //DetectDelimiter = false,  // we know it's comma; set true if needed
                //Delimiter = ","
            };

            using var csv = new CsvReader(reader, config);

            // --- Read & validate header ---
            if (!csv.Read() || !csv.ReadHeader())
                throw new FormatException("CSV appears to have no header row.");

            // Header values are already unquoted
            var header = csv.HeaderRecord ?? Array.Empty<string>();

            // Example: enforce exact header names (customize as needed)
            var expected = new[] { ServiceConstants.tableNameColumn, ServiceConstants.fileExtractName, ServiceConstants.destinationColumn };
            if (header.Length != expected.Length ||
                !header.SequenceEqual(expected, StringComparer.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"Unexpected header. Found: [{string.Join(", ", header)}]. " +
                    $"Expected: [{string.Join(", ", expected)}].");
            }

            // --- Read data rows ---
            var rows = new List<string[]>();

            while (csv.Read())
            {
                // Ensure we extract by index (aligned with header length)
                var row = new string[header.Length];

                for (int i = 0; i < header.Length; i++)
                {
                    // Get unquoted string field; CsvHelper will unescape embedded quotes ("")
                    string value;
                    try
                    {
                        value = csv.GetField(i);
                    }
                    catch (TypeConverterException ex)
                    {
                        throw new FormatException(
                            $"Failed to parse field at row {csv.Context.Parser.Row}, column {i + 1}: {ex.Message}", ex);
                    }

                    row[i] = value.Trim();
                }

                // --- Row-level validation examples ---
                // 1) No empty required fields
                if (string.IsNullOrWhiteSpace(row[0]))
                    throw new FormatException($"Row {csv.Context.Parser.Row}: {ServiceConstants.tableNameColumn} is required.");
                if (string.IsNullOrWhiteSpace(row[1]))
                    throw new FormatException($"Row {csv.Context.Parser.Row}: {ServiceConstants.receiptLogFilePattern} is required.");
                if (string.IsNullOrWhiteSpace(row[2]))
                    throw new FormatException($"Row {csv.Context.Parser.Row}: {ServiceConstants.destinationColumn} is required.");

                // 2) Simple shape checks (e.g., FileExtractName ends with .csv)
                if (!row[1].Trim().EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    throw new FormatException($"Row {csv.Context.Parser.Row}: {row[1]} must end with .csv");

                rows.Add(row);
            }
            rows.Insert(0, header);

            return rows;
        }
    }
}