#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TDOZNDT (Domestic Zone Detail) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class DomesticZoneDetailDto
    {
        [Required, StringLength(2)]
        public string ORG_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string ORG_GPU_NR { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DTN_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string DTN_GPU_NR { get; set; } = string.Empty;

        [Required]
        public int ZCH_NR { get; set; }

        [Required]
        public DateTime DOM_ZN_HDR_STT_DT { get; set; }

        [Required, StringLength(9)]
        public string ORG_RNG_LO_PSL_CD { get; set; } = string.Empty;

        [Required, StringLength(9)]
        public string ORG_RNG_HI_PSL_CD { get; set; } = string.Empty;

        [Required, StringLength(9)]
        public string DTN_RNG_LO_PSL_CD { get; set; } = string.Empty;

        [Required, StringLength(9)]
        public string DTN_RNG_HI_PSL_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string BUS_ENY_ACS_STS_CD { get; set; } = string.Empty;

        [StringLength(2)]
        [Required(AllowEmptyStrings = true)]
        public string ZN_NCV_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string DEL_ZN_NR { get; set; } = string.Empty;

        [Required]
        public DateTime DOM_ZN_DTL_END_DT { get; set; }

        [Required, StringLength(1)]
        public string MVM_DRC_CD { get; set; } = string.Empty;
    }
}
