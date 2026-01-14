#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TDFWTHR table with property names matching column names.
    /// </summary>
    public class DeficitWeightThresholdDto
    {
        [Required]
        [StringLength(2)]
        public string CNY_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string WGT_MS_UNT_TYP_CD { get; set; } = string.Empty;

        [Required]
        public decimal DFW_RTG_MIN_WGT_QY { get; set; }

        [Required]
        [StringLength(1)]
        public string WGT_DAT_PPN_IR { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }
    }
}
