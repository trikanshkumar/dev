#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the ZCHARTSTS (Chart Status) table.
    /// </summary>
    [Table("zchartsts")]
    public class ChartStatus
    {
        [Key]
        [Column("zch_sts_nr")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChartStatusNumber { get; set; }

        [Column("zch_nr")]
        public int ChartNumber { get; set; }

        [Column("ara_csf_hdr_stt_dt", TypeName = "date")]
        public DateTime ChartEffectiveStartDate { get; set; }

        [Column("ara_csf_hdr_end_dt", TypeName = "date")]
        public DateTime ChartEffectiveEndDate { get; set; }

        [Required, StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        public string StatusCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
