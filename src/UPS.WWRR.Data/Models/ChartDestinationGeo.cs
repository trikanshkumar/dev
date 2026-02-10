#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the ZCHARTDTNGEO (Chart Destination Geo) table.
    /// </summary>
    [Table("zchartdtngeo")]
    [PrimaryKey(nameof(ChartStatusNumber), nameof(DestinationGeopoliticalNumber), nameof(DestinationLowPostalCode), nameof(DestinationHighPostalCode))]
    public class ChartDestinationGeo
    {
        [Column("zch_sts_nr")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        public string DestinationCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("dtn_gpu_nr", TypeName = "char")]
        public string DestinationGeopoliticalNumber { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("dtn_rng_lo_psl_cd", TypeName = "varchar(9)")]
        public string DestinationLowPostalCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("dtn_rng_hi_psl_cd", TypeName = "varchar(9)")]
        public string DestinationHighPostalCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
