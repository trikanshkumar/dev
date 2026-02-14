#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the DOMZCHARTSTS_STG (Domestic Zone Chart Status Staging) table.
    /// </summary>
    [Table("domzchartsts_stg")]
    [PrimaryKey(nameof(ZoneChartStatusNumber))]
    public class DomesticZoneChartStatusStaging
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("zch_sts_nr")]
        public int ZoneChartStatusNumber { get; set; }

        [Required]
        [Column("zch_nr")]
        public int ZoneChartNumber { get; set; }

        [Required]
        [Column("dom_zn_hdr_stt_dt", TypeName = "date")]
        public DateTime DomesticZoneHeaderStartDate { get; set; }

        [Required]
        [Column("dom_zn_hdr_end_dt", TypeName = "date")]
        public DateTime DomesticZoneHeaderEndDate { get; set; }

        [Required, StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        public string BusinessEntityAccessStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
