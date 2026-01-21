#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tvpaqmt_stg")]
[PrimaryKey(
    nameof(ExportCountry),
    nameof(ImportCountry),
    nameof(ServiceType),
    nameof(PackageAcquisitionMethodType),
    nameof(MovementDirection),
    nameof(EffectiveDate)
)]
public class ValidAcquisitionMethodStaging
{
    [Required, StringLength(4)]
    [Column("gpn_xpt_cny_cd", TypeName = "char")]
    public string ExportCountry { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("gpn_ipt_cny_cd", TypeName = "char")]
    public string ImportCountry { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("pkg_acq_mth_typ_cd", TypeName = "char")]
    public string PackageAcquisitionMethodType { get; set; } = string.Empty;

    [Required, StringLength(1)]
    [Column("apl_ra_typ_cd", TypeName = "char")]
    public string MovementDirection { get; set; } = string.Empty;

    [Required]
    [Column("tbl_row_eff_dt", TypeName = "date")]
    public DateTime EffectiveDate { get; set; }

    [Required]
    [Column("tbl_row_exp_dt", TypeName = "date")]
    public DateTime EndDate { get; set; }

    [Required, StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    public string StatusCode { get; set; } = string.Empty;

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
