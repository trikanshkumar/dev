#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TFPUTRH (Accessorial Threshold / Pick Ups) table with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class AccessorialThresholdDTO
    {
        [Required]
        [StringLength(2)]
        public string CNY_CD { get; set; } = string.Empty; // Country

        [Required]
        [StringLength(3)]
        public string ASY_SVC_TYP_CD { get; set; } = string.Empty; // Accessorial code

        [Required]
        public int ASY_DLY_TRH_QY { get; set; } // Daily Threshold

        [Required]
        public int ASY_WKY_TRH_QY { get; set; } // Weekly Threshold

        [Required]
        [StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty; // Status Code

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; } // Effective Date

        [Required]
        public DateTime REC_EFF_END_DT { get; set; } // End Date

        [StringLength(100)]
        public string? LOAD_REF_TE { get; set; } // Load reference (optional, populated post-merge)
    }
}
