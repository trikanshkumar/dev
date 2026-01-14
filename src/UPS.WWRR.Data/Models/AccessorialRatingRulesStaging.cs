#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tcnyasy_stg")]
    [PrimaryKey(
        nameof(CountryCode),
        nameof(MovementDirectionTypeCode),
        nameof(CustomerClassificationTypeCode),
        nameof(AccessorialServiceTypeCode),
        nameof(AccessorialServiceChargeEffectiveDate),
        nameof(AccessorialServiceChargeEndDate),
        nameof(ApprovalStatusCode)
    )]
    public class AccessorialRatingRulesStaging
    {
        [Required, StringLength(2)]
        [Column("cny_cd", TypeName = "char")]
        public string CountryCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("mvm_drc_typ_cd", TypeName = "char")]
        public string MovementDirectionTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("cus_csf_typ_cd", TypeName = "char")]
        public string CustomerClassificationTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        public string AccessorialServiceTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("asy_svc_chg_eff_dt", TypeName = "Date")]
        public DateTime AccessorialServiceChargeEffectiveDate { get; set; }

        [Required]
        [Column("asy_svc_chg_end_dt", TypeName = "Date")]
        public DateTime AccessorialServiceChargeEndDate { get; set; }

        [Required, StringLength(2)]
        [Column("asy_svc_chg_typ_cd", TypeName = "char")]
        public string AccessorialServiceChargeTypeCode { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("ctl_vlu_1_te", TypeName = "char")]
        public string ControlValueCountry { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("ctl_vlu_2_te", TypeName = "char")]
        public string ControlValueService { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("ctl_vlu_3_te", TypeName = "char")]
        public string ControlValueFeature { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("ctl_vlu_4_te", TypeName = "char")]
        public string ControlValuePackage { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("ctl_vlu_5_te", TypeName = "char")]
        public string ControlValueBillTerm { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("ctl_vlu_6_te", TypeName = "char")]
        public string ControlValueOpen { get; set; } = string.Empty;

        [Required, StringLength(35)]
        [Column("ctl_vlu_dsc_te", TypeName = "char")]
        public string ControlValueDescription { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Column("ra_typ_cd_ary_te", TypeName = "char")]
        public string RateTypeCodeArray { get; set; } = string.Empty;

        [Column("is_completed_ir", TypeName = "smallint")]
        [Required]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
