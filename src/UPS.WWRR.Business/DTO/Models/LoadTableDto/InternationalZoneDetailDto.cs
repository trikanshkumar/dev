#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TINZNDT (International Zone Detail) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class InternationalZoneDetailDto
    {
        [Required, StringLength(2)]
        public string ORG_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string ORG_GPU_NR { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [StringLength(3)]
        [Required(AllowEmptyStrings = true)]
        public string PKG_CHA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DTN_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string DTN_GPU_NR { get; set; } = string.Empty;

        [Required]
        public int ZCH_NR { get; set; }

        [Required]
        public DateTime INL_ZN_HDR_STT_DT { get; set; }

        [StringLength(9)]
        [Required(AllowEmptyStrings = true)]
        public string ORG_RNG_LO_PSL_CD { get; set; } = string.Empty;

        [StringLength(9)]
        [Required(AllowEmptyStrings = true)]
        public string ORG_RNG_HI_PSL_CD { get; set; } = string.Empty;

        [StringLength(50)]
        [Required(AllowEmptyStrings = true)]
        public string ORG_POL_DIV_2_NA { get; set; } = string.Empty;

        [StringLength(9)]
        [Required(AllowEmptyStrings = true)]
        public string DTN_RNG_LO_PSL_CD { get; set; } = string.Empty;

        [StringLength(9)]
        [Required(AllowEmptyStrings = true)]
        public string DTN_RNG_HI_PSL_CD { get; set; } = string.Empty;

        [StringLength(50)]
        [Required(AllowEmptyStrings = true)]
        public string DTN_POL_DIV_2_NA { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string DEL_ZN_NR { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string ORG_GEO_ARA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DTN_GEO_ARA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string BUS_ENY_ACS_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime INL_ZN_DTL_END_DT { get; set; }

        [StringLength(5)]
        [Required(AllowEmptyStrings = true)]
        public string ORG_POL_DIV_1_CD { get; set; } = string.Empty;

        [StringLength(5)]
        [Required(AllowEmptyStrings = true)]
        public string DTN_POL_DIV_1_CD { get; set; } = string.Empty;

        [StringLength(2)]
        [Required(AllowEmptyStrings = true)]
        public string ZN_NCV_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(1)]
        public string MVM_DRC_CD { get; set; } = string.Empty;
    }
}
