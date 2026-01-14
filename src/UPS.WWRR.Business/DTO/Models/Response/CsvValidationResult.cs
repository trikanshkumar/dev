namespace UPS.WWRR.Business.DTO.Models.Response;

public record CsvValidationResponse(
    bool Success,
    List<string> ValidationErrors
);
