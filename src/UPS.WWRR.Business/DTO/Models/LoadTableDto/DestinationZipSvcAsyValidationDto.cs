using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto;

public class DestinationZipSvcAsyValidationDto
{
    [StringLength(2)]
    [Required]
    public string CNY_CD { get; set; }

    [StringLength(9)]
    [Required]
    public string DTN_PSL_CD { get; set; }

    [Required]
    public decimal PRC_PGM_PRM_VLU_TE { get; set; }

    [Required]
    public DateTime REC_EFF_STT_DT { get; set; }

    [Required]
    public DateTime REC_EFF_END_DT { get; set; }
}
