using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class ImportServiceValidationDto
{
    [StringLength(2)]
    [Required]
    public string ORG_CNY_CD { get; set; } = string.Empty;

    [StringLength(2)]
    [Required]
    public string DTN_CNY_CD { get; set; } = string.Empty;

    [StringLength(3)]
    [Required]
    public string SVC_TYP_CD { get; set; } = string.Empty;

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }
}
