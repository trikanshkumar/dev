#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TCYBLTY table with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class CountryBillTypeDto
    {
        [Required]
        [StringLength(2)]
        public string CNY_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(1)]
        public string MVM_DRC_CD { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string BIL_TER_TYP_CD { get; set; } = string.Empty;

        [Required]
        public DateTime CNY_BIL_TER_STT_DT { get; set; }

        [Required]
        public DateTime CNY_BIL_TER_END_DT { get; set; }

        [Required]
        [StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;
    }
}
