#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

/// <summary>
/// Represents the TFSCMAP_STG (Fuel Surcharge Category Map Staging) table.
/// </summary>
[Table("tfscmap_stg")]
[PrimaryKey(
    nameof(GeopoliticalExportCountryCode),
    nameof(GeopoliticalImportCountryCode),
    nameof(MovementDirectionCode),
    nameof(ServiceTypeCode),
    nameof(CustomerClassificationTypeCode),
    nameof(CurrencyCode),
    nameof(ApprovalStatusCode),
    nameof(RecordEffectiveStartDate)
)]
[Index(nameof(LoadReference), Name = "idx_tfscmap_stg_load_ref_te")]
[Index(nameof(IsCompletedIndicator), Name = "idx_tfscmap_stg_is_completed_ir")]
public class FuelSurchargeCategoryMapStaging
{
    [Required, StringLength(4)]
    [Column("gpn_xpt_cny_cd", TypeName = "char")]
    public string GeopoliticalExportCountryCode { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("gpn_ipt_cny_cd", TypeName = "char")]
    public string GeopoliticalImportCountryCode { get; set; } = string.Empty;

    [Required, StringLength(1)]
    [Column("mvm_drc_cd", TypeName = "char")]
    public string MovementDirectionCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceTypeCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("cus_csf_typ_cd", TypeName = "char")]
    public string CustomerClassificationTypeCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [Required]
    [Column("rec_eff_stt_dt", TypeName = "date")]
    public DateTime RecordEffectiveStartDate { get; set; }

    [Required]
    [Column("rec_eff_end_dt", TypeName = "date")]
    public DateTime RecordEffectiveEndDate { get; set; }

    [Required, StringLength(2)]
    [Column("pse_idx_fu_cgy_cd", TypeName = "char")]
    public string IndexFuelCategoryCode { get; set; } = string.Empty;

    [Required]
    [Column("rec_ins_ts", TypeName = "timestamp(6)")]
    public DateTime RecordInsertTimestamp { get; set; }

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
