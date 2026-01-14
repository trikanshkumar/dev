#nullable enable

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tspmycd_stg")]
[PrimaryKey(
    nameof(ExportCountry),
    nameof(ImportCountry),
    nameof(MovementDirection),
    nameof(ServiceType),
    nameof(Accessorial),
    nameof(DeterminingCriteriaStatus),
    nameof(EffectiveDate)
)]
public class TemplateAccessorialRulesStaging
{
    [Required, StringLength(4)]
    [Column("gpn_xpt_cny_cd", TypeName = "char")]
    public string ExportCountry { get; set; } = string.Empty;

    [Required, StringLength(4)]
    [Column("gpn_ipt_cny_cd", TypeName = "char")]
    public string ImportCountry { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("cny_ra_sei_rl_cd", TypeName = "char")]
    public string MovementDirection { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    public string ServiceType { get; set; } = string.Empty;

    [Required, StringLength(3)]
    [Column("spm_lin_cd", TypeName = "char")]
    public string Accessorial { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("dtr_cri_sts_cd", TypeName = "char")]
    public string DeterminingCriteriaStatus { get; set; } = string.Empty;

    [Required]
    [Column("dtr_cri_eff_dt", TypeName = "Date")]
    public DateTime EffectiveDate { get; set; }

    [Required]
    [Column("dtr_cri_end_dt", TypeName = "Date")]
    public DateTime EndDate { get; set; }

    [Required, StringLength(2)]
    [Column("chg_ccl_rul_cd", TypeName = "char")]
    public string ChargeCalculationRule { get; set; } = string.Empty;

    [Required, StringLength(1)]
    [Column("spm_chg_rfd_elg_ir", TypeName = "char")]
    public string SpmChgRfdElgIr { get; set; } = string.Empty;

    [Required, StringLength(2)]
    [Column("spm_typ_cd", TypeName = "char")]
    public string SpmTypCd { get; set; } = string.Empty;

    [Required, StringLength(1)]
    [Column("inf_xmp_ir", TypeName = "char")]
    public string InfXmpIr { get; set; } = string.Empty;

    [Column("is_completed_ir", TypeName = "smallint")]
    [Required]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }

}
