#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tauhist_stg")]
[PrimaryKey(
    nameof(AuditTypeCode),
    nameof(AuditActionCode),
    nameof(AuditTransactionNumber),
    nameof(AuditTransactionTypeCode),
    nameof(RecordCreationTimeStamp))]
public class AuditHistoryStaging
{
    [StringLength(3)]
    [Column("aud_typ_cd", TypeName = "char")]
    [Required]
    public string AuditTypeCode { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("aud_acn_cd", TypeName = "char")]
    [Required]
    public string AuditActionCode { get; set; } = string.Empty;

    [StringLength(10)]
    [Column("aud_trs_nr", TypeName = "char")]
    [Required]
    public string AuditTransactionNumber { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("aud_trs_typ_cd", TypeName = "char")]
    [Required]
    public string AuditTransactionTypeCode { get; set; } = string.Empty;

    [Column("rec_crt_ts", TypeName = "timestamp(6)")]
    [Required]
    public DateTime RecordCreationTimeStamp { get; set; }

    [StringLength(8)]
    [Column("rec_crt_usr_nr", TypeName = "char")]
    [Required]
    public string RecordCreationUser { get; set; } = string.Empty;

    [Column("dat_tms_snd_ts", TypeName = "timestamp(6)")]
    [Required]
    public DateTime Date { get; set; }

    [StringLength(100)]
    [Column("aud_rmk_te", TypeName = "char")]
    [Required]
    public string AuditRemarkText { get; set; } = string.Empty;

    [Column("aud_hdr_udt_qy", TypeName = "decimal(9,0)")]
    [Required]
    public decimal AuditHeaderUpdateQuantity { get; set; }

    [Column("aud_dtl_udt_qy", TypeName = "decimal(9,0)")]
    [Required]
    public decimal AuditDetailUpdateQuantity { get; set; }

    [StringLength(2)]
    [Column("aud_rpt_rsl_cd", TypeName = "char")]
    [Required]
    public string AuditReportResolutionCode { get; set; } = string.Empty;

    [Column("is_completed_ir", TypeName = "smallint")]
    [Required]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
