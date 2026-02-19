#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TVDSTBT_STG (Valid Destination Bill Term Staging) table.
    /// </summary>
    [Table("tvdstbt_stg")]
    [PrimaryKey(
        nameof(ImportCountryCode),
        nameof(BillingTermTypeCode),
        nameof(ApprovalStatusCode),
        nameof(RecordEffectiveStartDate)
    )]
    [Index(nameof(LoadReference), Name = "idx_tvdstbt_stg_load_ref_te")]
    [Index(nameof(IsCompletedIndicator), Name = "idx_tvdstbt_stg_is_completed_ir")]
    public class ValidDestinationBillTermStaging
    {
        [Required, StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        public string ImportCountryCode { get; set; } = string.Empty;

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
