#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO mirroring TIRACCY staging columns for CSV validation and batch load scenarios.
/// </summary>
public class InternationalRatingCurrencyDTO
{
    [Required, StringLength(2)]
    public string CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string RTG_CCY_CD { get; set; } = string.Empty;

    [Required]
    public DateTime RTG_CCY_STT_DT { get; set; }

    [Required]
    public DateTime RTG_CCY_END_DT { get; set; }

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;
}
