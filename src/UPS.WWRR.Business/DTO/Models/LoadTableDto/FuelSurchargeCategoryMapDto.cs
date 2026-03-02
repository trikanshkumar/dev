#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO for validating Fuel Surcharge Category Map (TFSCMAP) CSV data.
/// </summary>
public class FuelSurchargeCategoryMapDto
{
    [Required, StringLength(4)]
    public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

    [Required, StringLength(1)]
    public string MVM_DRC_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string CUS_CSF_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string CCY_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }

    [Required, StringLength(2)]
    public string PSE_IDX_FU_CGY_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_INS_TS { get; set; }
}
