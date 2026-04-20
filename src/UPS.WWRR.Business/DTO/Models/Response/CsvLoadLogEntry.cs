#nullable enable
namespace UPS.WWRR.Business.DTO.Models.Response
{
    /// <summary>
    /// Structured log entry for CSV file load operations.
    /// Written as structured JSON to stdout; GKE forwards these to GCP Log Explorer.
    /// </summary>
    public class CsvLoadLogEntry
    {
        public string TableName { get; set; } = string.Empty;
        public string CsvFileName { get; set; } = string.Empty;
        public string LoadStatus { get; set; } = string.Empty;
        public string LoadVersion { get; set; } = string.Empty;
        public int RecordsInserted { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsDeleted { get; set; }
        public string? ErrorDetails { get; set; }
    }
}
