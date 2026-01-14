#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TCZMSYS table with property names matching column names.
    /// </summary>
    public class CzmSystemRulesDto
    {
        [Required]
        [StringLength(3)]
        public string CD_TBL_TYP_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CD_TBL_CD { get; set; } = string.Empty;

        [StringLength(80)]
        public string DCO_CD_DSC_TE { get; set; } = string.Empty;

        [Required]
        public DateTime CD_TBL_STT_DT { get; set; }

        [Required]
        public DateTime CD_TBL_END_DT { get; set; }

        [Required]
        [StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LOAD_REF_TE { get; set; }
    }
}
