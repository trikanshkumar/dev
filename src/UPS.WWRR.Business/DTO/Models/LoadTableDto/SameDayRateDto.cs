#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class SameDayRateDto
{

    [Required, StringLength(4)]
    public string ORG_GPN_CD { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string DTN_GPN_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(6)]
    public string DEL_ZN_NR { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string CCY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string PKG_CHA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CUS_CSF_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(1)]
    public string CNY_RA_SEI_RL_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string WGT_MS_UNT_TYP_CD { get; set; } = string.Empty;

    [Required]
    public decimal WGT_CGY_MAX_WGT_QY { get; set; }

    [Required]
    public decimal WGT_CGY_MIN_WGT_QY { get; set; }

    [Required, StringLength(2)]
    public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;

    [Required]
    public DateTime SVC_RA_CHT_EFF_DT { get; set; }

    [Required]
    public DateTime SVC_RA_CHT_END_DT { get; set; }

    [Required, StringLength(2)]
    public string CCL_MTH_TYP_CD { get; set; } = string.Empty;

    [Required]
    public decimal DTR_CRI_RA_A { get; set; }

    [Required, StringLength(7)]
    public string SVC_RA_CHT_NR { get; set; } = string.Empty;

}
