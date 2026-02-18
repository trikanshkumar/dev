#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("chartorggeo")]
[PrimaryKey(
    nameof(ChartId),
    nameof(OriginGeopoliticalUnitCountry),
    nameof(DestinationGeopoliticalUnitCountry),
    nameof(ServiceType),
    nameof(PackageType),
    nameof(CustomerCode)
    )]
public class ChartGeo
{
    [Column("zch_sts_nr")]
    [Required]
    public int ChartId { get; set; }

    [StringLength(2)]
    [Column("xpt_cny_cd", TypeName = "char")]
    [Required]
    public string OriginCountry { get; set; } = string.Empty;

    [StringLength(4)]
    [Column("gpu_xpt_cny_cd", TypeName = "char")]
    [Required]
    public string OriginGeopoliticalUnitCountry { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("ipt_cny_cd", TypeName = "char")]
    [Required]
    public string DestinationCountry { get; set; } = string.Empty;

    [StringLength(4)]
    [Column("gpu_ipt_cny_cd", TypeName = "char")]
    [Required]
    public string DestinationGeopoliticalUnitCountry { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("pkg_cha_typ_cd", TypeName = "char")]
    [Required]
    public string PackageType { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("cus_cls_typ_cd", TypeName = "char")]
    [Required]
    public string CustomerCode { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
