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
            var config = CreateCsvConfiguration();
            using var csv = new CsvReader(reader, config);

            var header = ReadAndValidateHeader(csv);
            var rows = ReadRows(csv, header);

            rows.Insert(0, header);

            return rows;
        }

        private static CsvConfiguration CreateCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = context =>
                {
                    throw new FormatException($"Bad CSV data at row : {context.RawRecord}");
                },
                MissingFieldFound = null,
                DetectDelimiter = true
            };
        }

        private static string[] ReadAndValidateHeader(CsvReader csv)
        {
            if (!csv.Read() || !csv.ReadHeader())
                throw new FormatException("CSV appears to have no header row.");

            var header = csv.HeaderRecord ?? Array.Empty<string>();

            var expectedWithoutDestination = new[] { ServiceConstants.tableNameColumn, ServiceConstants.fileExtractName };
            var expectedWithDestination = new[] { ServiceConstants.tableNameColumn, ServiceConstants.fileExtractName, ServiceConstants.destinationColumn };

            if (!IsExpectedHeader(header, expectedWithoutDestination, expectedWithDestination))
            {
                throw new FormatException(
                    $"Unexpected header. Found: [{string.Join(", ", header)}]. " +
                    $"Expected: [{string.Join(", ", expectedWithoutDestination)}] OR [{string.Join(", ", expectedWithDestination)}.");
            }

            return header;
        }

        private static bool IsExpectedHeader(string[] header, string[] expectedWithoutDestination, string[] expectedWithDestination)
        {
            return header.SequenceEqual(expectedWithoutDestination, StringComparer.OrdinalIgnoreCase)
                || header.SequenceEqual(expectedWithDestination, StringComparer.OrdinalIgnoreCase);
        }

        private static List<string[]> ReadRows(CsvReader csv, string[] header)
        {
            var rows = new List<string[]>();

            while (csv.Read())
            {
                var row = ReadRow(csv, header.Length);
                ValidateRow(csv, row);
                rows.Add(row);
            }

            return rows;
        }

        private static string[] ReadRow(CsvReader csv, int headerLength)
        {
            var row = new string[headerLength];

            for (int i = 0; i < headerLength; i++)
            {
                row[i] = (GetFieldValue(csv, i) ?? string.Empty).Trim();
            }

            return row;
        }

        private static string? GetFieldValue(CsvReader csv, int index)
        {
            try
            {
                return csv.GetField(index);
            }
            catch (TypeConverterException ex)
            {
                throw new FormatException(
                    $"Failed to parse field at row {csv.Context.Parser.Row}, column {index + 1}: {ex.Message}", ex);
            }
        }

        private static void ValidateRow(CsvReader csv, string[] row)
        {
            if (string.IsNullOrWhiteSpace(row[0]))
                throw new FormatException($"Row {csv.Context.Parser.Row}: {ServiceConstants.tableNameColumn} is required.");

            if (string.IsNullOrWhiteSpace(row[1]))
                throw new FormatException($"Row {csv.Context.Parser.Row}: {ServiceConstants.fileExtractName} is required.");

            if (!row[1].Trim().EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"Row {csv.Context.Parser.Row}: {row[1]} must end with .csv");
        }

    }
}