#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("chartacccd_stg")]
[PrimaryKey(nameof(ChartId))]
public class ChartAccessorialCodeStaging
{
    [Column("zch_sts_nr")]
    [Required]
    public int ChartId { get; set; }

    [StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("bil_ter_typ_cd", TypeName = "char")]
    [Required]
    public string BillingTerms { get; set; } = string.Empty;

    [StringLength(1)]
    [Column("mvm_drc_cd", TypeName = "char")]
    [Required]
    public string MovementDirection { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    [DefaultValue((short)0)]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
