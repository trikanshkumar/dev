#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("tsvcdfl_stg")]
[PrimaryKey(
    nameof(CountryCode),
    nameof(ServiceTypeCode),
    nameof(ServiceDefaultUnitTypeCode),
    nameof(ServiceDefaultTypeCode),
    nameof(MovementDirectionCode),
    nameof(RecordEffectiveStartDate),
    nameof(DeterminingChartTypeCode),
    nameof(ApprovalStatusCode),
    nameof(ControlValueRangeLowQuantity))]
public class ServiceDefaultRulesStaging
{
    [Required, StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    public string CountryCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceTypeCode { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("svc_dfl_unt_typ_cd", TypeName = "char")]
    public string ServiceDefaultUnitTypeCode { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("svc_dfl_typ_cd", TypeName = "char")]
    public string ServiceDefaultTypeCode { get; set; } = string.Empty;

    [Required, StringLength(6)]
    [Column("svc_dfl_vlu_te", TypeName = "char")]
    public string ServiceDefaultValue { get; set; } = string.Empty;

    [Required]
    [Column("udt_ts", TypeName = "timestamp")]
    public DateTime UpdateTimestamp { get; set; }

    [Required, StringLength(1)]
    [Column("mvm_drc_cd", TypeName = "char")]
    public string MovementDirectionCode { get; set; } = string.Empty;

    [Required]
    [Column("rec_eff_stt_dt", TypeName = "date")]
    public DateTime RecordEffectiveStartDate { get; set; }

    [Required]
    [Column("rec_eff_end_dt", TypeName = "date")]
    public DateTime RecordEffectiveEndDate { get; set; }

    [Required, StringLength(3)]
    [Column("dtr_cha_typ_cd", TypeName = "char")]
    public string DeterminingChartTypeCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [Required]
    [Column("cri_vlu_rng_lo_qy", TypeName = "decimal(18,4)")]
    public decimal ControlValueRangeLowQuantity { get; set; }

    [Required]
    [Column("cri_vlu_rng_hi_qy", TypeName = "decimal(18,4)")]
    public decimal ControlValueRangeHighQuantity { get; set; }

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
