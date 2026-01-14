#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TBMAVCS_STG (BMA CAP Amount staging) table with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class BmaCapAmountDTO
    {
        [Required]
        [StringLength(4)]
        public string GPN_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string SVC_TYP_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required]
        [StringLength(2)]
        public string SVC_RA_CHT_STS_CD { get; set; } = string.Empty;

        [Required]
        public decimal MAX_NCV_PR { get; set; }

        [Required]
        [StringLength(2)]
        public string UPS_OFR_PGM_CD { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }

    }
}
