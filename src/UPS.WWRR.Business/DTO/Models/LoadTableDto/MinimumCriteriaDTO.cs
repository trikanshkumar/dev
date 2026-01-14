#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO representing TMINCRI staging rows with column-aligned property names
/// to support CSV validation and load orchestration.
/// </summary>
public class MinimumCriteriaDTO
{
    [Required, StringLength(4)]
    public string GPU_XPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string PKG_CHA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVM_TYP_CD { get; set; } = string.Empty;

    [Required]
    public short DTR_CRI_TYP_CD { get; set; }

    [Required, StringLength(3)]
    public string DTR_CRI_UNT_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string CMY_CLS_CD { get; set; } = string.Empty;

    [Required, StringLength(6)]
    public string DEL_ZN_NR { get; set; } = string.Empty;

    [Required]
    public DateTime DTR_CRI_EFF_DT { get; set; }

    [Required]
    public DateTime DTR_CRI_END_DT { get; set; }

    [Required]
    public decimal DTR_CRI_VLU_TE { get; set; }

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;
}
