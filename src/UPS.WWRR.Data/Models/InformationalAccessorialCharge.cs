#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tinfchg")]
[PrimaryKey(
    nameof(Country),
    nameof(AccessorialCode),
    nameof(TimePeriodType),
    nameof(Currency),
    nameof(EffectiveDate),
    nameof(CustomerClassificationType)
)]
public class InformationalAccessorialCharge
{
    [StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    [Required]
    public string Country { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialCode { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("tm_prd_typ_cd", TypeName = "char")]
    [Required]
    public string TimePeriodType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    [Required]
    public string Currency { get; set; } = string.Empty;

    [Column("dtr_cri_eff_dt", TypeName = "Date")]
    [Required]
    public DateTime EffectiveDate { get; set; }

    [Column("dtr_cri_end_dt", TypeName = "Date")]
    [Required]
    public DateTime EndDate { get; set; }

    [Column("dtr_cri_vlu_a", TypeName = "decimal(15,2)")]
    [Required]
    public decimal DeterminingCriteriaValue { get; set; }

    [StringLength(2)]
    [Column("cus_csf_typ_cd", TypeName = "char")]
    [Required]
    public string CustomerClassificationType { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
