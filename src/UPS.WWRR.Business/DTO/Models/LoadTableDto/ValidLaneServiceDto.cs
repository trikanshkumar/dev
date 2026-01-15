#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TVLNSVC (Valid Lane Service) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class ValidLaneServiceDto
    {
        [Required, StringLength(4)]
        public string GPN_XPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(4)]
        public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required]
        public DateTime TBL_ROW_EFF_DT { get; set; }

        [Required]
        public DateTime TBL_ROW_END_DT { get; set; }
    }
}
