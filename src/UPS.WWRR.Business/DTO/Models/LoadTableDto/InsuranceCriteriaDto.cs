#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO representing TINSCRI (Insurance Criteria) table for CSV validation scenarios.
/// Property names mirror the column names expected in the load files.
/// </summary>
public class InsuranceCriteriaDto
{
    [Required, StringLength(2)]
    public string CUS_CLS_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string WGT_MS_UNT_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string CCY_CD { get; set; } = string.Empty;

    [Required]
    public decimal INS_BSS_A { get; set; }

    [Required]
    public decimal MIN_INS_AVAIL { get; set; }

    [Required]
    public decimal INS_MIN_DCL_VLU { get; set; }

    [Required]
    public decimal INS_MIN_WGT_QY { get; set; }

    [Required]
    public decimal INS_MAX_WGT_QY { get; set; }

    [Required]
    public DateTime INS_CRI_EFF_STT_DT { get; set; }

    [Required]
    public DateTime INS_CRI_EFF_END_DT { get; set; }

    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required]
    public decimal ISN_MAX_DCL_VLU_A { get; set; }

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;
}
