#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TVORGBT_STG (Valid Origin Bill Term Staging) table.
    /// </summary>
    [Table("tvorgbt_stg")]
    [PrimaryKey(
        nameof(ExportCountryCode),
        nameof(BillingTermTypeCode),
        nameof(ApprovalStatusCode),
        nameof(RecordEffectiveStartDate)
    )]
    public class ValidOriginBillTermStaging
    {
        [Required, StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        public string ExportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("bil_ter_typ_cd", TypeName = "char")]
        public string BillingTermTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "date")]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Required]
        [Column("rec_eff_end_dt", TypeName = "date")]
        public DateTime RecordEffectiveEndDate { get; set; }

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
