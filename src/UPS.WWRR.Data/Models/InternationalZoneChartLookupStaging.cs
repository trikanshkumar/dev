#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTLKUP_STG (International Zone Chart Lookup Staging) table.
    /// </summary>
    [Table("izchartlkup_stg")]
    [PrimaryKey(nameof(ZoneChartNumber))]
    public class InternationalZoneChartLookupStaging
    {
        [Required]
        [Column("zch_nr", TypeName = "integer")]
        public int ZoneChartNumber { get; set; }

        [Required, StringLength(35)]
        [Column("zch_sht_dsc_te", TypeName = "char(35)")]
        public string ZoneChartShortDescriptionText { get; set; } = string.Empty;

        [Required, StringLength(135)]
        [Column("zch_lg_dsc_te", TypeName = "char(135)")]
        public string ZoneChartLongDescriptionText { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
