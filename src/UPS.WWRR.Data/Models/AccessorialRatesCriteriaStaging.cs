#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("accratecrit_stg")]
[PrimaryKey(
    nameof(ChartId),
    nameof(Rate),
    nameof(DeterminationCriteria),
    nameof(DeterminingCriteriaLowValue),
    nameof(CalculationMethod)
)]
public class AccessorialRatesCriteriaStaging
{
    [Column("zch_sts_nr")]
    [Required]
    public int ChartId { get; set; }

    [Column("asy_svc_ra", TypeName = "decimal(17,4)")]
    [Required]
    public decimal Rate { get; set; }

    [StringLength(2)]
    [Column("dtr_cri_vlu_typ_cd", TypeName = "char")]
    [Required]
    public string DeterminationCriteria { get; set; } = string.Empty;

    [Column("dtr_cri_lo_rng_te", TypeName = "decimal(13,2)")]
    [Required]
    public decimal DeterminingCriteriaLowValue { get; set; }

    [Column("dtr_cri_hi_rng_te", TypeName = "decimal(13,2)")]
    [Required]
    public decimal DeterminingCriteriaHighValue { get; set; }

    [StringLength(2)]
    [Column("ccl_mth_typ_cd", TypeName = "char")]
    [Required]
    public string CalculationMethod { get; set; } = string.Empty;

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    [DefaultValue((short)0)]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
