#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the ZCHARTDTNGPU (Chart Destination GPU) table.
    /// </summary>
    [Table("zchartdtngpu")]
    [PrimaryKey(nameof(ChartStatusNumber), nameof(DestinationCountryCode), nameof(DestinationPoliticalDivision2))]
    public class ChartDestinationGpu
    {
        [Column("zch_sts_nr")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        public string DestinationCountryCode { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("dtn_pol_div_2_na", TypeName = "varchar(50)")]
        public string DestinationPoliticalDivision2 { get; set; } = string.Empty;

        [StringLength(5)]
        [Column("dtn_pol_div_1_cd", TypeName = "varchar(5)")]
        public string? DestinationPoliticalDivision1 { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
