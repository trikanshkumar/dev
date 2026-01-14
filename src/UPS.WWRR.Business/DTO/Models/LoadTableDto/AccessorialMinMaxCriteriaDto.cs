#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Data.Models;

public class AccessorialMinMaxCriteriaDto
{
    [StringLength(2)]
    [Required]
    public string CNY_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string CCY_CD { get; set; } = string.Empty;

    [Required]
    public decimal ASY_TRH_A { get; set; }

    [StringLength(2)]
    [Required]
    public string ASY_TRH_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string CCL_MTH_TYP_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }

    [StringLength(2)]
    [Required]
    public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;

}
