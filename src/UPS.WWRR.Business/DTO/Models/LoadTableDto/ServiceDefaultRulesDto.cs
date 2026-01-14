#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO mirroring TSVCDFL staging rows for CSV validation.
/// </summary>
public class ServiceDefaultRulesDto
{
    [Required, StringLength(2)]
    public string CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true), StringLength(4)]
    public string SVC_DFL_UNT_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string SVC_DFL_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(6)]
    public string SVC_DFL_VLU_TE { get; set; } = string.Empty;

    [Required]
    public DateTime UDT_TS { get; set; }

    [Required, StringLength(1)]
    public string MVM_DRC_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }

    [Required, StringLength(3)]
    public string DTR_CHA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;

    [Required]
    public decimal CRI_VLU_RNG_LO_QY { get; set; }

    [Required]
    public decimal CRI_VLU_RNG_HI_QY { get; set; }
}
