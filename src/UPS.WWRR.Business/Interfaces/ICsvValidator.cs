using UPS.WWRR.Business.DTO.Models.Response;

namespace UPS.WWRR.Business.Interfaces;

public interface ICsvValidator
{
    Task<CsvValidationResponse> ValidateCsvAsync<TModel>(string csvFileLocation);
    Task<CsvValidationResponse> ValidateCsvChunkedAsync<TModel>(string csvFileLocation, int chunkSize);
}