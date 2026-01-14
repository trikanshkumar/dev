#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tinftrh")]
[PrimaryKey(nameof(CountryCode), nameof(AccessorialServiceTypeCode), nameof(TimePeriodTypeCode), nameof(DeterminingCriteriaEffectiveDate), nameof(CustomerClassificationTypeCode))]
public class InformationalAccessorialThreshold
{
    [StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    [Required]
    public string CountryCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialServiceTypeCode { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("tm_prd_typ_cd", TypeName = "char")]
    [Required]
    public string TimePeriodTypeCode { get; set; } = string.Empty;

    [Column("dtr_cri_eff_dt", TypeName = "Date")]
    [Required]
    public DateTime DeterminingCriteriaEffectiveDate { get; set; }

    [Column("dtr_cri_end_dt", TypeName = "Date")]
    [Required]
    public DateTime DeterminingCriteriaEndDate { get; set; }

    [Column("dtr_cri_vlu_qy", TypeName = "integer")]
    [Required]
    public int DeterminingCriteriaValueQuantity { get; set; }

    [StringLength(2)]
    [Column("cus_csf_typ_cd", TypeName = "char")]
    [Required]
    public string CustomerClassificationTypeCode { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
