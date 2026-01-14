namespace UPS.WWRR.Business.DTO.Models.Response
{
    /// <summary>
    /// Data Model representing the result of a copy batch operation.
    /// </summary>
    public class CopyBatchResultDto
    {
        public required string TableName { get; init; }
        public required string SourceFile { get; init; }
        public int TotalRowsAttempted { get; set; }
        public int RowsLoaded { get; set; }
        public int RowsFailed => TotalRowsAttempted - RowsLoaded;
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset CompletedAt { get; set; }
        public bool Success => RowsFailed == 0 && Errors.Count == 0;
        public List<string> Errors { get; } = new();
    }
}
