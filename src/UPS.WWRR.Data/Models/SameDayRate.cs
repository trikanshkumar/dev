#nullable enable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tsdrwsf")]
[PrimaryKey(
    nameof(OriginCountry),
    nameof(DestinationCountry),
    nameof(ServiceType),
    nameof(DeliveryZoneNumber),
    nameof(Currency),
    nameof(PackageCharacteristicType),
    nameof(CustomerClassificationType),
    nameof(ServiceFeatureType),
    nameof(MovementDirection),
    nameof(WeightMeasurementUnitType),
    nameof(MinimumWeightRange),
    nameof(StatusCode),
    nameof(EffectiveDate)
    )]
public class SameDayRate
{
    [Required, StringLength(4)]
    [Column("org_gpn_cd", TypeName = "char")]
    public string OriginCountry { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("dtn_gpn_cd", TypeName = "char")]
    public string DestinationCountry { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceType { get; set; } = string.Empty;

    [Required, StringLength(6)]
    [Column("del_zn_nr", TypeName = "char")]
    public string DeliveryZoneNumber { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    public string Currency { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("pkg_cha_typ_cd", TypeName = "char")]
    public string PackageCharacteristicType { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("cus_csf_typ_cd", TypeName = "char")]
    public string CustomerClassificationType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    public string ServiceFeatureType { get; set; } = string.Empty;

    [Required, StringLength(1)]
    [Column("cny_ra_sei_rl_cd", TypeName = "char")]
    public string MovementDirection { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("wgt_ms_unt_typ_cd", TypeName = "char")]
    public string WeightMeasurementUnitType { get; set; } = string.Empty;

    [Required]
    [Column("wgt_cgy_max_wgt_qy", TypeName = "decimal(9,2)")]
    public decimal MaximumWeightRange { get; set; }

    [Required]
    [Column("wgt_cgy_min_wgt_qy", TypeName = "decimal(9,2)")]
    public decimal MinimumWeightRange { get; set; }

    [Required, StringLength(2)]
    [Column("svc_ra_cht_sts_cd", TypeName = "char")]
    public string StatusCode { get; set; } = string.Empty;

    [Required]
    [Column("svc_ra_cht_eff_dt", TypeName = "Date")]
    public DateTime EffectiveDate { get; set; }

    [Required]
    [Column("svc_ra_cht_end_dt", TypeName = "Date")]
    public DateTime EndDate { get; set; }

    [Required, StringLength(2)]
    [Column("ccl_mth_typ_cd", TypeName = "char")]
    public string CalculationMethod { get; set; } = string.Empty;

    [Required]
    [Column("dtr_cri_ra_a", TypeName = "decimal(17,4)")]
    public decimal CriteriaRange { get; set; }

    [Required, StringLength(7)]
    [Column("svc_ra_cht_nr", TypeName = "char")]
    public string RateChartNumber { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
