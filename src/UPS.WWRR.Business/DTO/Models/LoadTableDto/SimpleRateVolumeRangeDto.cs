#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TSIARAV (Simple Rate Volume Range) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class SimpleRateVolumeRangeDto
    {
        [Required, StringLength(4)]
        public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string PKG_CHA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string BIL_TER_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(1)]
        public string MVM_DRC_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string CUS_CSF_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string WGT_MS_UNT_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }

        [Required]
        public decimal VOL_RNG_MIN_QY { get; set; }

        [Required]
        public decimal VOL_RNG_MAX_QY { get; set; }

        [Required, StringLength(3)]
        public string MS_UNT_TYP_CD { get; set; } = string.Empty;

        [Required]
        public decimal DW_MIN_QY { get; set; }

        [Required]
        public decimal DW_MAX_QY { get; set; }

        [Required]
        public decimal PBH_MAX_WGT_QY { get; set; }

        [StringLength(100)]
        public string? LOAD_REF_TE { get; set; }
    }
}
