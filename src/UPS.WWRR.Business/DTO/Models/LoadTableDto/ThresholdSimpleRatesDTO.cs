#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TBRCHAC (Threshold Table - Simple Rates) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class ThresholdSimpleRatesDTO
    {
        [Required, StringLength(3)]
        public string BRL_TYP_CD { get; set; } = string.Empty;

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

        [Required, StringLength(3)]
        public string DTR_CHA_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string MS_UNT_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }

        [Required]
        public decimal DTR_CHA_VLU_QY { get; set; }
    }
}
