#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TPSLBUR_STG (Postal Exception) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class PostalExceptionDTO
    {
        [Required, StringLength(2)]
        public string BRL_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(12)]
        public string RNG_LOW_PSL_CD { get; set; } = string.Empty;

        [Required, StringLength(12)]
        public string RNG_HI_PSL_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }

        [Required, StringLength(50)]
        public string POL_DIV_2_NA { get; set; } = string.Empty;

        [Required]
        public DateTime REC_INS_TS { get; set; }
    }
}
