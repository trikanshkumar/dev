#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

/// <summary>
/// Represents the CHARTSTS table
/// </summary>
[Table("chartsts")]
public class RateChartLookup
{
    [Key]
    [Column("zch_sts_nr")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ChartId { get; set; }

    [Required, StringLength(6)]
    [Column("svc_ra_cht_nr", TypeName = "char")]
    public string ChartNumber { get; set; } = string.Empty;

    [Required]
    [Column("svc_ra_cht_eff_dt", TypeName = "date")]
    public DateTime ChartEffectiveDate { get; set; }

    [Required]
    [Column("svc_ra_cht_end_dt", TypeName = "date")]
    public DateTime ChartEndDate { get; set; }

    [Required, StringLength(2)]
    [Column("svc_ra_cht_sts_cd", TypeName = "char")]
    public string ChartStatus { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
