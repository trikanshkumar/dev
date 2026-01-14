using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class TemplateAccessorialRulesDto
{
    [Required, StringLength(4)]
    public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CNY_RA_SEI_RL_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SPM_LIN_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string DTR_CRI_STS_CD { get; set; } = string.Empty;

    [Required]
    public DateTime DTR_CRI_EFF_DT { get; set; }

    [Required]
    public DateTime DTR_CRI_END_DT { get; set; }

    [Required, StringLength(2)]
    public string CHG_CCL_RUL_CD { get; set; } = string.Empty;

    [Required, StringLength(1)]
    public string SPM_CHG_RFD_ELG_IR { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true), StringLength(2)]
    public string SPM_TYP_CD { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true), StringLength(1)]
    public string INF_XMP_IR { get; set; } = string.Empty;
}
