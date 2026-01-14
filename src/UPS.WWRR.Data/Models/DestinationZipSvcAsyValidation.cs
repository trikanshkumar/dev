#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("tdstsvp")]
public class DestinationZipSvcAsyValidation
{
    [StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    [Required]
    public string Country { get; set; } = string.Empty;

    [StringLength(9)]
    [Column("dtn_psl_cd", TypeName = "char")]
    [Required]
    public string DestinationPostalCode { get; set; } = string.Empty;

    [Column("prc_pgm_prm_vlu_te", TypeName = "decimal(16,0)")]
    [Required]
    public decimal PercentProgramPremiumValue { get; set; }

    [Column("rec_eff_stt_dt", TypeName = "Date")]
    [Required]
    public DateTime EffectiveDate { get; set; }

    [Column("rec_eff_end_dt", TypeName = "Date")]
    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
