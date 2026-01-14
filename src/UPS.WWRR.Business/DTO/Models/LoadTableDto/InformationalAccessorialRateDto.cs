#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class InformationalAccessorialRateDto
{
    [StringLength(2)]
    [Required]
    public string CNY_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string TM_PRD_TYP_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string CCY_CD { get; set; } = string.Empty;

    [Required]
    public DateTime DTR_CRI_EFF_DT { get; set; }

    [Required]
    public decimal DTR_CRI_LOW_RNG_TE { get; set; }

    [Required]
    public decimal DTR_CRI_HI_RNG_TE { get; set; }

    [Required]
    public DateTime DTR_CRI_END_DT { get; set; }

    [StringLength(2)]
    [Required]
    public string CCL_MTH_TYP_CD { get; set; } = string.Empty;

    [Required]
    public decimal DTR_CRI_RA_A { get; set; }

    [StringLength(2)]
    [Required]
    public string CUS_CSF_TYP_CD { get; set; } = string.Empty;
}
