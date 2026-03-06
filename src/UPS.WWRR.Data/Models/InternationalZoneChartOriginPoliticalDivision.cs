#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTORGPOLDIV (International Zone Chart Origin Political Division) table.
    /// </summary>
    [Table("izchartorgpoldiv")]
    [PrimaryKey(
        nameof(ChartStatusNumber),
        nameof(OriginCountryCode)
    )]
    public class InternationalZoneChartOriginPoliticalDivision
    {
        [Required]
        [Column("zch_sts_nr", TypeName = "integer")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        public string OriginCountryCode { get; set; } = string.Empty;

        [Required, StringLength(5)]
        [Column("org_pol_div_1_cd", TypeName = "char(5)")]
        public string OriginPoliticalDivision1Code { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
