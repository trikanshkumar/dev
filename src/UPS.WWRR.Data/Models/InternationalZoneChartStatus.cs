#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTSTS (International Zone Chart Status) table.
    /// </summary>
    [Table("izchartsts")]
    public class InternationalZoneChartStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("zch_sts_nr", TypeName = "integer")]
        public int ChartStatusNumber { get; set; }

        [Required]
        [Column("zch_nr", TypeName = "integer")]
        public int ZoneChartNumber { get; set; }

        [Required]
        [Column("inl_zn_hdr_stt_dt", TypeName = "date")]
        public DateTime InternationalZoneHeaderStartDate { get; set; }

        [Required]
        [Column("inl_zn_hdr_end_dt", TypeName = "date")]
        public DateTime InternationalZoneHeaderEndDate { get; set; }

        [Required, StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        public string BusinessEntityAccessStatusCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
