#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class FreightRatingRulesDto
{
    [StringLength(2)]
    [Required]
    public string RTG_CNY_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string CNY_RA_SEI_RL_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required(AllowEmptyStrings = true)]
    public string CUS_CSF_TYP_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required(AllowEmptyStrings = true)]
    public string MPS_PKG_IR { get; set; } = string.Empty;

    [Required]
    public DateTime TBL_ROW_EFF_DT { get; set; }

    [Required]
    public DateTime TBL_ROW_END_DT { get; set; }

    [StringLength(6)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_1_TE { get; set; } = string.Empty;

    [StringLength(6)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_2_TE { get; set; } = string.Empty;

    [StringLength(6)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_3_TE { get; set; } = string.Empty;

    [StringLength(6)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_4_TE { get; set; } = string.Empty;

    [StringLength(6)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_5_TE { get; set; } = string.Empty;

    [StringLength(6)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_6_TE { get; set; } = string.Empty;

    [StringLength(35)]
    [Required(AllowEmptyStrings = true)]
    public string CTL_VLU_DSC_TE { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string APV_STS_CD { get; set; } = string.Empty;

    [StringLength(20)]
    [Required(AllowEmptyStrings = true)]
    public string RA_TYP_CD_ARY_TE { get; set; } = string.Empty;
}
