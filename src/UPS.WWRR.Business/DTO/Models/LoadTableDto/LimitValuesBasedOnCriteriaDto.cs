#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO representing TLMTVLU staging data for validation and load orchestration.
/// </summary>
public class LimitValuesBasedOnCriteriaDto
{
    [Required, StringLength(2)]
    public string CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CRI_GRP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CRI_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string CCY_CD { get; set; } = string.Empty;

    [Required]
    public DateTime DTR_CRI_EFF_DT { get; set; }

    [Required]
    public DateTime DTR_CRI_END_DT { get; set; }

    [Required]
    public decimal DCL_VLU_MAX_A { get; set; }
}
