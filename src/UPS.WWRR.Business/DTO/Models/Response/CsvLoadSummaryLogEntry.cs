namespace UPS.WWRR.Business.DTO.Models.Response
{
    /// <summary>
    /// Summary structured log entry containing all CSV file load results for a single receipt processing cycle.
    /// Written as structured JSON to stdout; GKE forwards these to GCP Log Explorer.
    /// The Summary property contains a fixed-width formatted table for email alert display.
    /// </summary>
    public class CsvLoadSummaryLogEntry
    {
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
