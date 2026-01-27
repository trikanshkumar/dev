#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tsubchg_stg")]
[PrimaryKey(
    nameof(ExportCountry),
    nameof(ImportCountry),
    nameof(Accessorial),
    nameof(MovementDirection),
    nameof(ServiceType),
    nameof(ServiceFeatureType),
    nameof(PackageCharacteristicType),
    nameof(PackageAcquisitionMethod),
    nameof(Currency),
    nameof(BilingTermType),
    nameof(EffectveDate),
    nameof(StatusCode),
    nameof(CustomerClassificationType)
)]
public class FuelSurchargeStaging
{
    [Required, StringLength(4)]
    [Column("gpn_xpt_cny_cd", TypeName = "char")]
    public string ExportCountry { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("gpn_ipt_cny_cd", TypeName = "char")]
    public string ImportCountry { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    public string Accessorial { get; set; } = string.Empty;

    [Required, StringLength(1)]
    [Column("mvm_drc_cd", TypeName = "char")]
    public string MovementDirection { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    public string ServiceFeatureType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("pkg_cha_typ_cd", TypeName = "char")]
    public string PackageCharacteristicType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("pkg_acq_mth_typ_cd", TypeName = "char")]
    public string PackageAcquisitionMethod { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    public string Currency { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("bil_ter_typ_cd", TypeName = "char")]
    public string BilingTermType { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("ccl_mth_typ_cd", TypeName = "char")]
    public string CalculationMethod { get; set; } = string.Empty;

    [Required]
    [Column("asy_svc_ra_eff_dt", TypeName = "Date")]
    public DateTime EffectveDate { get; set; }

    [Required]
    [Column("asy_svc_ra_end_dt", TypeName = "Date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Column("asy_svc_ra", TypeName = "decimal(17,4)")]
    public decimal Rate { get; set; }

    [Required]
    [Column("asy_svc_min_amt", TypeName = "decimal(17,4)")]
    public decimal MinimumRate { get; set; }

    [Required, StringLength(2)]
    [Column("svc_ra_cht_sts_cd", TypeName = "char")]
    public string StatusCode { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("cus_csf_typ_cd", TypeName = "char")]
    public string CustomerClassificationType { get; set; } = string.Empty;

    [Column("is_completed_ir", TypeName = "smallint")]
    [Required]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
