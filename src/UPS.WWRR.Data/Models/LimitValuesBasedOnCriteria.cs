#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("tlmtvlu")]
[PrimaryKey(
    nameof(CountryCode),
    nameof(CriteriaGroupCode),
    nameof(CriteriaTypeCode),
    nameof(ApprovalStatusCode),
    nameof(CurrencyCode),
    nameof(DeterminingCriteriaEffectiveDate))]
public class LimitValuesBasedOnCriteria
{
    [Required, StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    public string CountryCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("cri_grp_cd", TypeName = "char")]
    public string CriteriaGroupCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("cri_typ_cd", TypeName = "char")]
    public string CriteriaTypeCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required]
    [Column("dtr_cri_eff_dt", TypeName = "date")]
    public DateTime DeterminingCriteriaEffectiveDate { get; set; }

    [Required]
    [Column("dtr_cri_end_dt", TypeName = "date")]
    public DateTime DeterminingCriteriaEndDate { get; set; }

    [Required]
    [Column("dcl_vlu_max_a", TypeName = "decimal(18,2)")]
    public decimal DeclaredValueMaximumAmount { get; set; }

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
