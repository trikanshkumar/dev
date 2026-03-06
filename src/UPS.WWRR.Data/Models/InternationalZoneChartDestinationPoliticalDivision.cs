#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTDTNPOLDIV (International Zone Chart Destination Political Division) table.
    /// </summary>
    [Table("izchartdtnpoldiv")]
    [PrimaryKey(
        nameof(ChartStatusNumber),
        nameof(DestinationCountryCode)
    )]
    public class InternationalZoneChartDestinationPoliticalDivision
    {
        [Required]
        [Column("zch_sts_nr", TypeName = "integer")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        public string DestinationCountryCode { get; set; } = string.Empty;

        [Required, StringLength(5)]
        [Column("dtn_pol_div_1_cd", TypeName = "char(5)")]
        public string DestinationPoliticalDivision1Code { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
