#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tasyra_stg")]
[PrimaryKey(
    nameof(ChartNumber),
    nameof(CalculationMethod),
    nameof(ZoneNumber),
    nameof(ChartEffectiveDate),
    nameof(DeterminationCriteria),
    nameof(DeterminingCriteriaLowValue),
    nameof(ChargeClassification),
    nameof(StatusCode))]
public class AccessorialRatesStaging
{
    [StringLength(6)]
    [Column("svc_ra_cht_nr", TypeName = "char")]
    [Required]
    public string ChartNumber { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("ccl_mth_typ_cd", TypeName = "char")]
    [Required]
    public string CalculationMethod { get; set; } = string.Empty;

    [StringLength(6)]
    [Column("del_zn_nr", TypeName = "char")]
    [Required]
    public string ZoneNumber { get; set; } = string.Empty;

    [Column("asy_svc_ra", TypeName = "decimal(17,4)")]
    [Required]
    public decimal Rate { get; set; }

    [Column("asy_svc_ra_eff_dt", TypeName = "Date")]
    [Required]
    public DateTime ChartEffectiveDate { get; set; }

    [Column("asy_svc_ra_end_dt", TypeName = "Date")]
    [Required]
    public DateTime ChartEndDate { get; set; }

    [StringLength(2)]
    [Column("dtr_cri_vlu_typ_cd", TypeName = "char")]
    [Required]
    public string DeterminationCriteria { get; set; } = string.Empty;

    [Column("dtr_cri_hi_rng_te", TypeName = "decimal(13,2)")]
    [Required]
    public decimal DeterminingCriteriaHighValue { get; set; }

    [Column("dtr_cri_lo_rng_te", TypeName = "decimal(13,2)")]
    [Required]
    public decimal DeterminingCriteriaLowValue { get; set; }

    [StringLength(3)]
    [Column("ra_chg_csf_typ_cd", TypeName = "char")]
    [Required]
    public string ChargeClassification { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("svc_ra_cht_sts_cd", TypeName = "char")]
    [Required]
    public string StatusCode { get; set; } = string.Empty;

    [Required]
    [Column("is_completed_ir", TypeName = "smallint")]
    [DefaultValue((short)0)]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
