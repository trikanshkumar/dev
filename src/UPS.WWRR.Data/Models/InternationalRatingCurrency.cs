#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UPS.WWRR.Data.Models;

[Table("tiraccy")]
[PrimaryKey(
    nameof(CountryCode),
    nameof(RatingCurrencyCode),
    nameof(RatingCurrencyStartDate),
    nameof(ApprovalStatusCode))]
public class InternationalRatingCurrency
{
    [StringLength(2)]
    [Column("cny_cd", TypeName = "char")]
    [Required]
    public string CountryCode { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("rtg_ccy_cd", TypeName = "char")]
    [Required]
    public string RatingCurrencyCode { get; set; } = string.Empty;

    [Column("rtg_ccy_stt_dt", TypeName = "date")]
    [Required]
    public DateTime RatingCurrencyStartDate { get; set; }

    [Column("rtg_ccy_end_dt", TypeName = "date")]
    [Required]
    public DateTime RatingCurrencyEndDate { get; set; }

    [StringLength(2)]
    [Column("apv_sts_cd", TypeName = "char")]
    [Required]
    public string ApprovalStatusCode { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
