
namespace UPS.WWRR.Business.Interfaces
{
    public interface IBatchProcessorWorker
    {
        Task ProcessAsync(CancellationToken ct);
    }
}
