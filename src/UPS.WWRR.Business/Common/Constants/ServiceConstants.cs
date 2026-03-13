namespace UPS.WWRR.Business.Common.Constants
{
    public static class ServiceConstants
    {
        public const string receiptLogFilePattern = "WWRR_MOD_RECEIPT_FILES_";
        public const string fileExtractName = "FileExtractName";
        public const string tableNameColumn = "TableName";
        public const string destinationColumn = "Destination";
        public const string defaultDestinationValue = "No target env. found";
        public const string source = "Source";
        public const string processedDataFilesFolder = "ProcessedDataFiles";
        public const string csvValidationError = "CSV_Validation_Failed";
        public const string filenameCaseMismatchError = "Filename_Case_Mismatch";
        public const string fileNotFoundError = "File_Not_Found";
        public const string emptyDataFileError = "Empty_Data_File";
        public const string LoadStatusSuccess = "Success";
        public const string LoadStatusFailed = "Failed";
        // Regex pattern that matches timestamps like yyyy-MM-dd-HH.mm.ss.ffffff (date with dashes, time with dots, 1–6 fractional digits)
        public const string DashDotTimestampRegexPattern = @"(\d{4}-\d{2}-\d{2})-(\d{2})\.(\d{2})\.(\d{2})\.(\d{1,6})";

        // Regex pattern that matches Oracle-style timestamps like dd-MMM-yy hh.mm.ss.ffffff AM/PM (e.g., "20-NOV-18 03.41.58.000000 PM")
        public const string OracleTimestampRegexPattern = @"(\d{1,2})-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)-(\d{2})\s+(\d{1,2})\.(\d{2})\.(\d{2})\.(\d{1,6})\s*(AM|PM)";

        // Tables that require Oracle-style timestamp conversion (dd-MMM-yy hh.mm.ss.ffffff AM/PM format)
        public static readonly HashSet<string> OracleTimestampTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "tfscmap_stg"
        };

        // Centralized CSV DateTime parse formats used across the app (CsvHelper and other parsers)
        public static readonly string[] CsvDateTimeFormats = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd-HH.mm.ss.ffffff",
            "yyyy-MM-dd-HH.mm.ss.FFFFFF",
            "yyyy-MM-dd-HH.mm.ss.fff",
            "yyyy-MM-dd-HH.mm.ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "M/d/yyyy",
            "M/d/yyyy HH:mm:ss",
            "M/d/yyyy h:mm:ss tt",
            "dd-MMM-yyyy",          
            "dd-MMM-yyyy HH:mm:ss",
            "dd-MMM-yy hh.mm.ss.ffffff tt"
        };
    }
}
