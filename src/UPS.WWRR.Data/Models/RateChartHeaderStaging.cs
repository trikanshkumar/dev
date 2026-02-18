#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tchart_stg")]
[PrimaryKey(
    nameof(PackageType),
    nameof(StatusCode),
    nameof(ServiceType),
    nameof(PackageAcquisitionMethod),
    nameof(ServiceFeatureType),
    nameof(BillingTerms),
    nameof(AccessorialCode),
    nameof(CustomerCode),
    nameof(Currency),
    nameof(ChartEffectiveDate),
    nameof(InterIntraStateCode),
    nameof(GeopoliticalUnitOriginCountry),
    nameof(GeopoliticalUnitDestinationCountry),
    nameof(PackageAcquisitionClassification),
    nameof(MovementDirection))]
public class RateChartHeaderStaging
{
    [StringLength(6)]
    [Column("svc_ra_cht_nr", TypeName = "char")]
    [Required]
    public string ChartNumber { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("pkg_cha_typ_cd", TypeName = "char")]
    [Required]
    public string PackageType { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("svc_ra_cht_sts_cd", TypeName = "char")]
    [Required]
    public string StatusCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("pkg_acq_mth_typ_cd", TypeName = "char")]
    [Required]
    public string PackageAcquisitionMethod { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("xpt_cny_cd", TypeName = "char")]
    [Required]
    public string OriginCountry { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceFeatureType { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("ipt_cny_cd", TypeName = "char")]
    [Required]
    public string DestinationCountry { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("bil_ter_typ_cd", TypeName = "char")]
    [Required]
    public string BillingTerms { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("asy_svc_typ_cd", TypeName = "char")]
    [Required]
    public string AccessorialCode { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("cus_cls_typ_cd", TypeName = "char")]
    [Required]
    public string CustomerCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("ccy_cd", TypeName = "char")]
    [Required]
    public string Currency { get; set; } = string.Empty;

    [Column("svc_ra_cht_eff_dt", TypeName = "Date")]
    [Required]
    public DateTime ChartEffectiveDate { get; set; }

    [StringLength(2)]
    [Column("na_nrs_cd", TypeName = "char")]
    [Required]
    public string InterIntraStateCode { get; set; } = string.Empty;

    [Column("svc_ra_cht_end_dt", TypeName = "Date")]
    [Required]
    public DateTime ChartEndDate { get; set; }

    [StringLength(4)]
    [Column("gpu_xpt_cny_cd", TypeName = "char")]
    [Required]
    public string GeopoliticalUnitOriginCountry { get; set; } = string.Empty;

    [StringLength(4)]
    [Column("gpu_ipt_cny_cd", TypeName = "char")]
    [Required]
    public string GeopoliticalUnitDestinationCountry { get; set; } = string.Empty;

    [Column("svc_ra_cht_seq_nr", TypeName = "decimal(6,0)")]
    [Required]
    public decimal AcquisitionSequenceNumber { get; set; }

    [StringLength(3)]
    [Column("pkg_acq_mth_csf_cd", TypeName = "char")]
    [Required]
    public string PackageAcquisitionClassification { get; set; } = string.Empty;

    [StringLength(1)]
    [Column("mvm_drc_cd", TypeName = "char")]
    [Required]
    public string MovementDirection { get; set; } = string.Empty;

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    [DefaultValue((short)0)]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
