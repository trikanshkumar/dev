#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TINFTRH table with property names matching column names.
    /// </summary>
    public class InformationalAccessorialThresholdDto
    {
        [Required]
        [StringLength(2)]
        public string CNY_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string TM_PRD_TYP_CD { get; set; } = string.Empty;

        [Required]
        public DateTime DTR_CRI_EFF_DT { get; set; }

        [Required]
        public DateTime DTR_CRI_END_DT { get; set; }

        [Required]
        public int DTR_CRI_VLU_QY { get; set; }

        [Required]
        [StringLength(2)]
        public string CUS_CSF_TYP_CD { get; set; } = string.Empty;
    }
}
