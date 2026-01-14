#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tratrul_stg")]
[PrimaryKey(
    nameof(RatingCountry),
    nameof(MovementDirection),
    nameof(CustomerClassificationType),
    nameof(ServiceType),
    nameof(MultipieceIndicator),
    nameof(EffectiveDate),
    nameof(EndDate),
    nameof(StatusCode)
)]
public class FreightRatingRulesStaging
{
    [StringLength(2)]
    [Column("rtg_cny_cd", TypeName = "char")]
    [Required]
    public string RatingCountry { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("cny_ra_sei_rl_cd", TypeName = "char")]
    [Required]
    public string MovementDirection { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("cus_csf_typ_cd", TypeName = "char")]
    [Required]
    public string CustomerClassificationType { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("mps_pkg_ir", TypeName = "char")]
    [Required]
    public string MultipieceIndicator { get; set; } = string.Empty;

    [Column("tbl_row_eff_dt", TypeName = "Date")]
    [Required]
    public DateTime EffectiveDate { get; set; }

    [Column("tbl_row_end_dt", TypeName = "Date")]
    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(6)]
    [Column("ctl_vlu_1_te", TypeName = "char")]
    [Required]
    public string ControlValveCountry { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("ctl_vlu_2_te", TypeName = "char")]
    [Required]
    public string ControlValveService { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("ctl_vlu_3_te", TypeName = "char")]
    [Required]
    public string ControlValveFeature { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("ctl_vlu_4_te", TypeName = "char")]
    [Required]
    public string ControlValvePackage { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("ctl_vlu_5_te", TypeName = "char")]
    [Required]
    public string ControlValveBillTerm { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("ctl_vlu_6_te", TypeName = "char")]
    [Required]
    public string ControlValveOpen { get; set; } = string.Empty;

    [StringLength(35)]
    [Column("ctl_vlu_dsc_te", TypeName = "char")]
    [Required]
    public string ControlValveDescription { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    [Required]
    public string StatusCode { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("ra_typ_cd_ary_te", TypeName = "char")]
    [Required]
    public string RateTypeCodeArray { get; set; } = string.Empty;

    [Column("is_completed_ir", TypeName = "smallint")]
    [Required]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
