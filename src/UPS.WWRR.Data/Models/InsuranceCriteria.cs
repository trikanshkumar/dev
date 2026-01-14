#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tinscri")]
[PrimaryKey(
    nameof(CustomerClassificationTypeCode),
    nameof(CountryCode),
    nameof(AccessorialServiceTypeCode),
    nameof(ServiceFeatureTypeCode),
    nameof(WeightMeasurementUnitTypeCode),
    nameof(CurrencyCode),
    nameof(InsuranceCriteriaEffectiveStartDate),
    nameof(ServiceTypeCode),
    nameof(ApprovalStatusCode))]
public class InsuranceCriteria
{
    [StringLength(2)]
    [Column("cus_cls_typ_cd", TypeName = "char")]
    [Required]
    public string CustomerClassificationTypeCode { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    [Required]
    public string CountryCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialServiceTypeCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceFeatureTypeCode { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("wgt_ms_unt_typ_cd", TypeName = "char")]
    [Required]
    public string WeightMeasurementUnitTypeCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    [Required]
    public string CurrencyCode { get; set; } = string.Empty;

    [Column("ins_bss_a", TypeName = "decimal(15,2)")]
    [Required]
    public decimal InsuranceBasisAmount { get; set; }

    [Column("min_ins_avail", TypeName = "decimal(15,2)")]
    [Required]
    public decimal MinimumInsuranceAvailable { get; set; }

    [Column("ins_min_dcl_vlu", TypeName = "decimal(15,2)")]
    [Required]
    public decimal InsuranceMinimumDeclaredValue { get; set; }

    [Column("ins_min_wgt_qy", TypeName = "decimal(9,2)")]
    [Required]
    public decimal InsuranceMinimumWeightQuantity { get; set; }

    [Column("ins_max_wgt_qy", TypeName = "decimal(9,2)")]
    [Required]
    public decimal InsuranceMaximumWeightQuantity { get; set; }

    [Column("ins_cri_eff_stt_dt", TypeName = "date")]
    [Required]
    public DateTime InsuranceCriteriaEffectiveStartDate { get; set; }

    [Column("ins_cri_eff_end_dt", TypeName = "date")]
    [Required]
    public DateTime InsuranceCriteriaEffectiveEndDate { get; set; }

    [StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceTypeCode { get; set; } = string.Empty;

    [Column("isn_max_dcl_vlu_a", TypeName = "decimal(17,2)")]
    [Required]
    public decimal InsuranceMaximumDeclaredValue { get; set; }

    [StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    [Required]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
