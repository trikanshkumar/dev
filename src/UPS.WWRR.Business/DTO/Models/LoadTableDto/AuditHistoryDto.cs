#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class AuditHistoryDto
{
    [StringLength(3)]
    [Required]
    public string AUD_TYP_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string AUD_ACN_CD { get; set; } = string.Empty;

    [StringLength(10)]
    [Required]
    public string AUD_TRS_NR { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string AUD_TRS_TYP_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_CRT_TS { get; set; }

    [StringLength(8)]
    [Required]
    public string REC_CRT_USR_NR { get; set; } = string.Empty;

    [Required]
    public DateTime DAT_TMS_SND_TS { get; set; }

    [StringLength(100)]
    [Required(AllowEmptyStrings = true)]
    public string AUD_RMK_TE { get; set; } = string.Empty;

    [Required]
    public decimal AUD_HDR_UDT_QY { get; set; }

    [Required]
    public decimal AUD_DTL_UDT_QY { get; set; }

    [StringLength(2)]
    [Required(AllowEmptyStrings = true)]
    public string AUD_RPT_RSL_CD { get; set; } = string.Empty;
}
