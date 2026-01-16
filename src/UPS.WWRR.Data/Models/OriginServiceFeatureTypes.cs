#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;


[Table("tvosvcf")]
[PrimaryKey(
    nameof(ExportCountryCode),
    nameof(ServiceType),
    nameof(ServiceFeatureType),
    nameof(EffectiveDate),
    nameof(Status)
    )]
public class OriginServiceFeatureTypes
{
    [Required, StringLength(4)]
    [Column("gpn_xpt_cny_cd", TypeName = "char")]
    public string ExportCountryCode { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_fea_typ_cd", TypeName = "char")]
    public string ServiceFeatureType { get; set; } = string.Empty;

    [Required]
    [Column("tbl_row_eff_dt", TypeName = "date")]
    public DateTime EffectiveDate { get; set; }

    [Required]
    [Column("tbl_row_exp_dt", TypeName = "date")]
    public DateTime EndDate { get; set; }

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string Status { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
