#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("tsvcacp_stg")]
[PrimaryKey(
    nameof(AccessoryServiceTypeCode),
    nameof(ServiceTypeCode),
    nameof(ApprovalStatusCode),
    nameof(RecordEffectiveStartDate))]
public class ServiceDowngradeValidAccessorialRulesStaging
{
    [Required, StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    public string AccessoryServiceTypeCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceTypeCode { get; set; } = string.Empty;

    [Required]
    [Column("prc_pgm_prm_vlu_te", TypeName = "decimal(16,0)")]
    public decimal PricingProgramPremiumValue { get; set; }

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
