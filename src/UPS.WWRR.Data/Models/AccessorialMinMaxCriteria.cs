#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tasytrh")]
[PrimaryKey(nameof(Country), nameof(AccessorialCode), nameof(Currency), nameof(AccessorialThresholdType), nameof(EffectiveDate), nameof(StatusCode))]
public class AccessorialMinMaxCriteria
{
    [StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    [Required]
    public string Country { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    [Required]
    public string Currency { get; set; } = string.Empty;

    [Column("asy_trh_a", TypeName = "decimal(17,4)")]
    [Required]
    public decimal AccessorialThreshold { get; set; }

    [StringLength(2)]
    [Column("asy_trh_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialThresholdType { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("ccl_mth_typ_cd", TypeName = "char")]
    [Required]
    public string CalculationMethod { get; set; } = string.Empty;

    [Column("rec_eff_stt_dt", TypeName = "date")]
    [Required]
    public DateTime EffectiveDate { get; set; }

    [Column("rec_eff_end_dt", TypeName = "date")]
    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(2)]
    [Column("svc_ra_cht_sts_cd", TypeName = "char")]
    [Required]
    public string StatusCode { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }

}
