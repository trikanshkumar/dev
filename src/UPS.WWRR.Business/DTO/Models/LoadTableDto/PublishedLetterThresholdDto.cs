#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TWGTTRH (Published Letter Thresholds and Scan Tolerances) table with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class PublishedLetterThresholdDto
    {
        [Required]
        [StringLength(2)]
        public string CNY_CD { get; set; } = string.Empty; 

        [Required]
        [StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty; 

        [Required]
        [StringLength(3)]
        public string PKG_CHA_TYP_CD { get; set; } = string.Empty; 

        [Required]
        [StringLength(2)]
        public string WGT_MS_UNT_TYP_CD { get; set; } = string.Empty; 

        [Required]
        public decimal PCE_MAX_ALW_WGT_QY { get; set; } 

        [Required]
        public decimal TRH_MAX_WGT_QY { get; set; } 

        [Required]
        public decimal SN_TLN_WGT_QY { get; set; } 

        [Required]
        [StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty; 

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; } 

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }
    }
}
