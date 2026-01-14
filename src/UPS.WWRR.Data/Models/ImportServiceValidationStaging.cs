#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models;

[Table("timpsvc_stg")]
[PrimaryKey(nameof(OriginCountry), nameof(DestinationCountry), nameof(ServiceType), nameof(EffectiveDate))]
public class ImportServiceValidationStaging
{
    [StringLength(2)]
    [Column("org_cny_cd", TypeName = "char")]
    [Required]
    public string OriginCountry { get; set; } = string.Empty;

    [StringLength(2)]
    [Column("dtn_cny_cd", TypeName = "char")]
    [Required]
    public string DestinationCountry { get; set; } = string.Empty;

    [StringLength(3)]
    [Column("svc_typ_cd", TypeName = "char")]
    [Required]
    public string ServiceType { get; set; } = string.Empty;

    [Column("rec_eff_stt_dt", TypeName = "Date")]
    [Required]
    public DateTime EffectiveDate { get; set; }

    [Column("rec_eff_end_dt", TypeName = "Date")]
    [Required]
    public DateTime EndDate { get; set; }

    [Column("is_completed_ir", TypeName = "smallint")]
    [Required]
    public short IsCompletedIndicator { get; set; } = 0;

    [StringLength(100)]
    [Column("load_ref_te", TypeName = "varchar(100)")]
    public string? LoadReference { get; set; }
}
