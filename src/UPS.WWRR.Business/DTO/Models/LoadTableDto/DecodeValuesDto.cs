#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TDECODE (Decode Values) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class DecodeValuesDto
    {
        [Required, StringLength(30)]
        public string FLD_NA { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(10)]
        public string TYP_CD_FLD_VLU_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(100)]
        public string TYP_CD_FLD_DSC_TE { get; set; } = string.Empty;

        [Required]
        public DateTime REC_EFF_STT_DT { get; set; }

        [Required]
        public DateTime REC_EFF_END_DT { get; set; }
    }
}
