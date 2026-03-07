#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTLKUP (International Zone Chart Lookup) table.
    /// </summary>
    [Table("izchartlkup")]
    public class InternationalZoneChartLookup
    {
        [Key]
        [Column("zch_nr", TypeName = "integer")]
        public int ZoneChartNumber { get; set; }

        [Required, StringLength(35)]
        [Column("zch_sht_dsc_te", TypeName = "char(35)")]
        public string ZoneChartShortDescriptionText { get; set; } = string.Empty;

        [Required, StringLength(135)]
        [Column("zch_lg_dsc_te", TypeName = "char(135)")]
        public string ZoneChartLongDescriptionText { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
