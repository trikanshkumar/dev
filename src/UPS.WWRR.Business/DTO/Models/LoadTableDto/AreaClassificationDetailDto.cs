#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TARCLDT (Area Classification Detail) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class AreaClassificationDetailDto
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
        public DateTime ARA_CSF_HDR_STT_DT { get; set; }

        [Required(AllowEmptyStrings = true), StringLength(9)]
        public string ORG_RNG_LO_PSL_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(9)]
        public string ORG_RNG_HI_PSL_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(50)]
        public string ORG_POL_DIV_2_NA { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(9)]
        public string DTN_RNG_LO_PSL_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(9)]
        public string DTN_RNG_HI_PSL_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(50)]
        public string DTN_POL_DIV_2_NA { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string ORG_GEO_ARA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DTN_GEO_ARA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string ARA_CSF_DTL_RUL_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string ARA_CSF_DTL_MNT_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(3)]
        public string RA_CHG_CSF_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string BUS_ENY_ACS_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime ARA_CSF_DTL_END_DT { get; set; }

        [Required(AllowEmptyStrings = true), StringLength(5)]
        public string ORG_POL_DIV_1_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(5)]
        public string DTN_POL_DIV_1_CD { get; set; } = string.Empty;
    }
}
