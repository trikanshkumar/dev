#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

/// <summary>
/// DTO representing TSVCDGR staging rows for CSV validation and batch loads.
/// </summary>
public class ServiceDowngradeRulesDto
{
    [Required, StringLength(3)]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(3)]
    public string NXT_LWR_SVC_TYP_CD { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string APV_STS_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }
}
