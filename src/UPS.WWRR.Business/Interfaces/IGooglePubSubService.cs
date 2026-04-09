namespace UPS.WWRR.Business.Interfaces
{
    public interface IGooglePubSubService
    {
        Task PublishMessageAsync(string messageJson, CancellationToken ct = default);
    }
}
