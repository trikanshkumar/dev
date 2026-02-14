#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the DOMZCHARTLKUP (Domestic Zone Chart Lookup) table.
    /// </summary>
    [Table("domzchartlkup")]
    public class DomesticZoneChartLookup
    {
        [Key]
        [Column("zch_nr")]
        public int ChartNumber { get; set; }

        [Required, StringLength(50)]
        [Column("zch_sht_dsc_te", TypeName = "varchar(50)")]
        public string ShortDescription { get; set; } = string.Empty;

        [Required, StringLength(150)]
        [Column("zch_lg_dsc_te", TypeName = "varchar(150)")]
        public string LongDescription { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
