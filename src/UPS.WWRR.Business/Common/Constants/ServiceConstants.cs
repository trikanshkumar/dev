namespace UPS.WWRR.Business.Common.Constants
{
    public static class ServiceConstants
    {
        public const string receiptLogFilePattern = "WWRR_MOD_RECEIPT_FILES_";
        public const string fileExtractName = "FileExtractName";
        public const string source = "Source";
        public const string processedDataFilesFolder = "ProcessedDataFiles";
        public const string csvValidationError = "CSV_Validation_Failed";
        // Regex pattern that matches timestamps like yyyy-MM-dd-HH.mm.ss.ffffff (date with dashes, time with dots, 1–6 fractional digits)
        public const string DashDotTimestampRegexPattern = @"(\d{4}-\d{2}-\d{2})-(\d{2})\.(\d{2})\.(\d{2})\.(\d{1,6})";

        // Centralized CSV DateTime parse formats used across the app (CsvHelper and other parsers)
        public static readonly string[] CsvDateTimeFormats = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd-HH.mm.ss.ffffff",
            "yyyy-MM-dd-HH.mm.ss.FFFFFF",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "M/d/yyyy",
            "M/d/yyyy HH:mm:ss",
            "M/d/yyyy h:mm:ss tt"
        };
    }
}
