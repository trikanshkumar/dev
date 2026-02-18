using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class AccessorialRatesDto
{
    [StringLength(6)]
    [Required]
    public string SVC_RA_CHT_NR { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string CCL_MTH_TYP_CD { get; set; } = string.Empty;

    [StringLength(6)]
    [Required]
    public string DEL_ZN_NR { get; set; } = string.Empty;

    [Required]
    public decimal ASY_SVC_RA { get; set; }

    [Required]
    public DateTime ASY_SVC_RA_EFF_DT { get; set; }

    [Required]
    public DateTime ASY_SVC_RA_END_DT { get; set; }

    [StringLength(2)]
    [Required(AllowEmptyStrings = true)]
    public string DTR_CRI_VLU_TYP_CD { get; set; } = string.Empty;

    [Required]
    public decimal DTR_CRI_LO_RNG_TE { get; set; }

    [Required]
    public decimal DTR_CRI_HI_RNG_TE { get; set; }

    [StringLength(3)]
    [Required(AllowEmptyStrings = true)]
    public string RA_CHG_CSF_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;
}
