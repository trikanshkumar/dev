#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TRASTD (Freight Rates) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class FreightRatesDto
    {
        [Required, StringLength(6)]
        public string SVC_RA_CHT_NR { get; set; } = string.Empty;

        [Required]
        public DateTime SVC_RA_CHT_EFF_DT { get; set; }

        [Required, StringLength(2)]
        public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string CMY_CLS_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string CCL_MTH_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string DEL_ZN_NR { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string WGT_MS_UNT_TYP_CD { get; set; } = string.Empty;

        [Required]
        public double WGT_CGY_MIN_WGT_QY { get; set; }

        [Required]
        public double WGT_CGY_MAX_WGT_QY { get; set; }

        [Required]
        public decimal AC_SPL_BIL_TER_PR { get; set; }

        [Required]
        public decimal CNS_SPL_BIL_TER_PR { get; set; }

        [Required]
        public DateTime SVC_RA_CHT_END_DT { get; set; }
    }
}
