#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("chartsvcpkg")]
[PrimaryKey(
    nameof(ChartId),
    nameof(ServiceType),
    nameof(PackageType),
    nameof(ServiceFeatureType),
    nameof(PackageAcquisitionMethod),
    nameof(InterIntraStateCode)
    )]
public class ChartServicePackage
{
    [Column("zch_sts_nr")]
    [Required]
    public int ChartId { get; set; }

    [StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("pkg_cha_typ_cd", TypeName = "char")]
    [Required]
    public string PackageType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceFeatureType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("pkg_acq_mth_typ_cd", TypeName = "char")]
    [Required]
    public string PackageAcquisitionMethod { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("na_nrs_cd", TypeName = "char")]
    [Required]
    public string InterIntraStateCode { get; set; } = string.Empty;

    [Column("svc_ra_cht_seq_nr", TypeName = "decimal(6,0)")]
    [Required]
    public decimal AcquisitionSequenceNumber { get; set; }

    [StringLength(3)]
    [Column("pkg_acq_mth_csf_cd", TypeName = "char")]
    [Required]
    public string PackageAcquisitionClassification { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
