using System.Text.Json.Serialization;

namespace UPS.WWRR.Business.DTO.Models.Response
{
    public class PubSubNotificationMessage
    {
        [JsonPropertyName("runId")]
        public Guid RunId { get; set; }

        [JsonPropertyName("isLocal")]
        public bool IsLocal { get; set; }

        [JsonPropertyName("loadVersions")]
        public List<string> LoadVersions { get; set; } = [];

        [JsonPropertyName("bucket")]
        public string Bucket { get; set; } = string.Empty;

        [JsonPropertyName("batchSize")]
        public int BatchSize { get; set; }
    }
}
