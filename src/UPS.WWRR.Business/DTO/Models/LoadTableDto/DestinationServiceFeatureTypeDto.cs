#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TVDSVCF (Destination Service Feature Types) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class DestinationServiceFeatureTypeDto
    {
        [Required, StringLength(4)]
        public string GPN_IPT_CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string SVC_FEA_TYP_CD { get; set; } = string.Empty;

        [Required]
        public DateTime TBL_ROW_EFF_DT { get; set; }

        [Required]
        public DateTime TBL_ROW_EXP_DT { get; set; }

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;
    }
}
