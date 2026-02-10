#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TARCLHD (Area Classification Header) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class AreaClassificationHeaderDto
    {
        [Required, StringLength(2)]
        public string ORG_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string ORG_GPU_NR { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DTN_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string DTN_GPU_NR { get; set; } = string.Empty;

        [Required]
        public int ZCH_NR { get; set; }

        [Required]
        public DateTime ARA_CSF_HDR_STT_DT { get; set; }

        [Required]
        public DateTime ARA_CSF_HDR_END_DT { get; set; }

        [Required, StringLength(2)]
        public string BUS_ENY_ACS_STS_CD { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string ZCH_LG_DSC_TE { get; set; } = string.Empty;

        [Required, StringLength(35)]
        public string ZCH_SHT_DSC_TE { get; set; } = string.Empty;
    }
}
