#nullable enable
namespace UPS.WWRR.Business.Common.Helper
{
    /// <summary>
    /// static helper to build SQL commands for data loading
    /// </summary>
    public static class SqlCommandHelper
    {

        /// <summary>
        /// Builds a COPY command with an explicit column list. Uses NULL 'NULL' so empty cells remain empty strings.
        /// </summary>
        public static string BuildCopyCommand(string tableName, IEnumerable<string> columns, string delimiter, bool hasHeader)
        {
            var qualifiedTable = tableName.ToLowerInvariant();
            var columnList = string.Join(',', columns);
            var opts = BuildOptions(delimiter, hasHeader);
            return $"COPY {qualifiedTable} ({columnList}) FROM STDIN WITH ({opts})";
        }

        /// <summary>
        /// Builds a TRUNCATE TABLE command for the specified table.
        /// </summary>
        public static string BuildTruncateCommand(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));

            var qualifiedTable = tableName.ToLowerInvariant();
            return $"TRUNCATE TABLE {qualifiedTable}";
        }


        // COPY options including an explicit non-empty NULL marker, so empty cells are not treated as NULL
        private static string BuildOptions(string delimiter, bool hasHeader)
        {
            var options = new List<string>
            {
                "FORMAT CSV",
                $"DELIMITER '{delimiter}'",
                "QUOTE '\"'",
                "ESCAPE '\"'",
                "NULL 'NULL'"
            };
            if (hasHeader) options.Add("HEADER");
            return string.Join(", ", options);
        }
    }
}
