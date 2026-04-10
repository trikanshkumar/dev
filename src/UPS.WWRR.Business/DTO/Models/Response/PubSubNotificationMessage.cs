using Newtonsoft.Json;

namespace UPS.WWRR.Business.DTO.Models.Response
{
    public class PubSubNotificationMessage
    {
        [JsonProperty("runId")]
        public Guid RunId { get; set; }

        [JsonProperty("isLocal")]
        public bool IsLocal { get; set; }

        [JsonProperty("loadVersions")]
        public List<string> LoadVersions { get; set; } = [];

        [JsonProperty("bucket")]
        public string Bucket { get; set; } = string.Empty;

        [JsonProperty("batchSize")]
        public int BatchSize { get; set; }
    }
}
