#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class FuelSurchargeDto
{
    [Required, StringLength(4)]
    public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(1)]
    public string MVM_DRC_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string PKG_CHA_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string PKG_ACQ_MTH_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string CCY_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string BIL_TER_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CCL_MTH_TYP_CD { get; set; } = string.Empty;

    [Required]
    public DateTime ASY_SVC_RA_EFF_DT { get; set; }

    [Required]
    public DateTime ASY_SVC_RA_END_DT { get; set; }

    [Required]
    public decimal ASY_SVC_RA { get; set; }

    [Required]
    public decimal ASY_SVC_MIN_AMT { get; set; }

    [Required, StringLength(2)]
    public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CUS_CSF_TYP_CD { get; set; } = string.Empty;
}
