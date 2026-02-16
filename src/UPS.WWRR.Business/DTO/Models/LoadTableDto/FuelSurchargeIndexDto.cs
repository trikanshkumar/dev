#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO for validating Fuel Surcharge Index (TFSCIDX) CSV data.
/// </summary>
public class FuelSurchargeIndexDto
{
    [Required, StringLength(2)]
    public string PSE_IDX_FU_CGY_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }

    [Required]
    public decimal PSE_IDX_FU_RA_PR { get; set; }

    [Required, StringLength(3)]
    public string CCY_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string FU_MS_UNT_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string PSE_IDX_FU_CGY_TE { get; set; } = string.Empty;

    [Required]
    public decimal FU_SUR_PBH_RT_A { get; set; }
}
