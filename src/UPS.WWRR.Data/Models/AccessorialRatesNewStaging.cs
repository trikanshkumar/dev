#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("accrate_stg")]
[PrimaryKey(
    nameof(ChartId),
    nameof(Rate),
    nameof(ZoneNumber),
    nameof(ChargeClassification)
)]
public class AccessorialRatesNewStaging
{
    [Column("zch_sts_nr")]
    [Required]
    public int ChartId { get; set; }

    [Column("asy_svc_ra", TypeName = "decimal(17,4)")]
    [Required]
    public decimal Rate { get; set; }

    [StringLength(6)]
    [Column("del_zn_nr", TypeName = "char")]
    [Required]
    public string ZoneNumber { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("ra_chg_csf_typ_cd", TypeName = "char")]
    [Required]
    public string ChargeClassification { get; set; } = string.Empty;

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    [DefaultValue((short)0)]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
