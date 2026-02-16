#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

/// <summary>
/// Represents the TFSCIDX_STG (Fuel Surcharge Index Staging) table.
/// </summary>
[Table("tfscidx_stg")]
[PrimaryKey(
    nameof(IndexFuelCategoryCode),
    nameof(RecordEffectiveStartDate),
    nameof(ApprovalStatusCode)
)]
public class FuelSurchargeIndexStaging
{
    [Required, StringLength(2)]
    [Column("pse_idx_fu_cgy_cd", TypeName = "char")]
    public string IndexFuelCategoryCode { get; set; } = string.Empty;

    [Required]
    [Column("rec_eff_stt_dt", TypeName = "date")]
    public DateTime RecordEffectiveStartDate { get; set; }

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [Required]
    [Column("rec_eff_end_dt", TypeName = "date")]
    public DateTime RecordEffectiveEndDate { get; set; }

    [Required]
    [Column("pse_idx_fu_ra_pr", TypeName = "numeric(18,4)")]
    public decimal IndexFuelRate { get; set; }

    [Required, StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("fu_ms_unt_typ_cd", TypeName = "char")]
    public string FuelMeasurementUnitTypeCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Column("pse_idx_fu_cgy_te", TypeName = "char")]
    public string IndexFuelCategoryDescription { get; set; } = string.Empty;

    [Required]
    [Column("fu_sur_pbh_rt_a", TypeName = "numeric(17,4)")]
    public decimal SurchargePublishedRate { get; set; }

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
