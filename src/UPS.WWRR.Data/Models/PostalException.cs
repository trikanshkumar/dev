#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tpslbur")]
    [PrimaryKey(
        nameof(BusinessRuleCode),
        nameof(ExportCountryCode),
        nameof(ImportCountryCode),
        nameof(PostalCodeLowRange),
        nameof(PostalCodeHighRange),
        nameof(RecordEffectiveStartDate),
        nameof(ApprovalStatusCode)
    )]
    public class PostalException
    {
        [Required, StringLength(2)]
        [Column("brl_cd", TypeName = "char")]
        public string BusinessRuleCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        public string ExportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        public string ImportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(12)]
        [Column("rng_low_psl_cd", TypeName = "char")]
        public string PostalCodeLowRange { get; set; } = string.Empty;

        [Required, StringLength(12)]
        [Column("rng_hi_psl_cd", TypeName = "char")]
        public string PostalCodeHighRange { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "Date")]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_end_dt", TypeName = "Date")]
        public DateTime RecordEffectiveEndDate { get; set; }

        [Required]
        [StringLength(50)]
        [Column("pol_div_2_na", TypeName = "varchar(50)")]
        public string CityName { get; set; } = string.Empty;

        [Required]
        [Column("rec_ins_ts", TypeName = "timestamp(6)")]
        public DateTime RecordInsertTimestamp { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
