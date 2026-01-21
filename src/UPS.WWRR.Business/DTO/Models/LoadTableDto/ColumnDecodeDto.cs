#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TCOLDEC (Column Decodes) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class ColumnDecodeDto
    {
        [Required, StringLength(15)]
        public string TBL_CLU_NA { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string TBL_CLU_VLU_TE { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string SCR_CD_VLU_TE { get; set; } = string.Empty;
    }
}
