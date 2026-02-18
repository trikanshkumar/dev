using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class RateChartHeaderDto
{
    [StringLength(6)]
    [Required]
    public string SVC_RA_CHT_NR { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string PKG_CHA_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string PKG_ACQ_MTH_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string XPT_CNY_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string IPT_CNY_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string BIL_TER_TYP_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string CUS_CLS_TYP_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string CCY_CD { get; set; } = string.Empty;

    [Required]
    public DateTime SVC_RA_CHT_EFF_DT { get; set; }

    [StringLength(2)]
    [Required]
    public string NA_NRS_CD { get; set; } = string.Empty;

    [Required]
    public DateTime SVC_RA_CHT_END_DT { get; set; }

    [StringLength(4)]
    [Required]
    public string GPU_XPT_CNY_CD { get; set; } = string.Empty;

    [StringLength(4)]
    [Required]
    public string GPU_IPT_CNY_CD { get; set; } = string.Empty;

    [Required]
    public decimal SVC_RA_CHT_SEQ_NR { get; set; }

    [StringLength(3)]
    [Required]
    public string PKG_ACQ_MTH_CSF_CD { get; set; } = string.Empty;

    [StringLength(1)]
    [Required]
    public string MVM_DRC_CD { get; set; } = string.Empty;

}
