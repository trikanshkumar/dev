#nullable enable
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using UPS.WWRR.Business.Interfaces;

namespace UPS.WWRR.Business.Services
{
    public class GooglePubSubService : IGooglePubSubService
    {
        private readonly PublisherClient _publisherClient;
        private readonly ILogger<GooglePubSubService> _logger;

        public GooglePubSubService(PublisherClient publisherClient, ILogger<GooglePubSubService> logger)
        {
            _publisherClient = publisherClient;
            _logger = logger;
        }

        public async Task PublishMessageAsync(string messageJson, CancellationToken ct = default)
        {
            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(messageJson)
            };

            var messageId = await _publisherClient.PublishAsync(pubsubMessage);
            _logger.LogInformation("Published Pub/Sub message with ID: {MessageId}", messageId);
        }
    }
}
