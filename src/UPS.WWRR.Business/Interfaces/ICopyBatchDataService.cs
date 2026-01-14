using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;

namespace UPS.WWRR.Business.Interfaces
{
    /// <summary>
    /// Interface for batch data loader service
    /// </summary>
    public interface ICopyBatchDataService
    {
        /// <summary>
        /// Copy data from the specified CSV file into the target table as per the provided configuration.
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="configuration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<CopyBatchResultDto> CopyAsync(string csvFilePath, TableConfigurationRequest configuration, CancellationToken cancellationToken = default);
    }
}
