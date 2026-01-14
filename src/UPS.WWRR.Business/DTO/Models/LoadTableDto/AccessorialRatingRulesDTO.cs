#nullable enable
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TCNYASY_STG (Accessorial Rating Rules) with property names matching column names.
    /// Used for CSV validation and batch loading.
    /// </summary>
    public class AccessorialRatingRulesDTO
    {
        [Required, StringLength(2)]
        public string CNY_CD { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string MVM_DRC_TYP_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(2)]
        public string CUS_CSF_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string ASY_SVC_TYP_CD { get; set; } = string.Empty;

        [Required]
        public DateTime ASY_SVC_CHG_EFF_DT { get; set; }

        [Required]
        public DateTime ASY_SVC_CHG_END_DT { get; set; }

        [Required, StringLength(2)]
        public string ASY_SVC_CHG_TYP_CD { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string CTL_VLU_1_TE { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(6)]
        public string CTL_VLU_2_TE { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string CTL_VLU_3_TE { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(6)]
        public string CTL_VLU_4_TE { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(6)]
        public string CTL_VLU_5_TE { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(6)]
        public string CTL_VLU_6_TE { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(35)]
        public string CTL_VLU_DSC_TE { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string APV_STS_CD { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true), StringLength(20)]
        public string RA_TYP_CD_ARY_TE { get; set; } = string.Empty;


    }
}
