#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("tsvcdgr")]
[PrimaryKey(
    nameof(ServiceTypeCode),
    nameof(ApprovalStatusCode),
    nameof(RecordEffectiveStartDate))]
[Index(nameof(LoadReference), Name = "idx_tsvcdgr_load_ref_te")]
public class ServiceDowngradeRules
{
    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceTypeCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("nxt_lwr_svc_typ_cd", TypeName = "char")]
    public string NextLowerServiceTypeCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [Required]
    [Column("rec_eff_stt_dt", TypeName = "date")]
    public DateTime RecordEffectiveStartDate { get; set; }

    [Required]
    [Column("rec_eff_end_dt", TypeName = "date")]
    public DateTime RecordEffectiveEndDate { get; set; }

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
