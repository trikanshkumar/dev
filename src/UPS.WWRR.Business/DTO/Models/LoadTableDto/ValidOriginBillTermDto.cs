#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TVORGBT (Valid Origin Bill Term) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class ValidOriginBillTermDto
    {
        [Required, StringLength(4)]
        public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string BIL_TER_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }
    }
}
