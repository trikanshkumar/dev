#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TVASYLN (Valid Accessorial Lane) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class ValidAccessorialLaneDto
    {
        [Required, StringLength(2)]
        public string ORG_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string DTN_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(1)]
        public string MVM_DRC_CD { get; set; } = string.Empty;

        [Required, StringLength(1)]
        public string GPN_UNT_PIR_CSF_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required, StringLength(3)]
        public string ASY_SVC_ALT_NMC_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string SVC_TYP_ALT_NMC_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }

        [Required, StringLength(4)]
        public string ORG_GPN_MNM_TE { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string DTN_GPN_MNM_TE { get; set; } = string.Empty;
    }
}
