#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the DOMZCHARTDTNGEO (Domestic Zone Chart Destination Geo) table.
    /// </summary>
    [Table("domzchartdtngeo")]
    [PrimaryKey(nameof(ZoneChartStatusNumber), nameof(DestinationGpuNumber), nameof(DestinationRangeLowPostalCode))]
    public class DomesticZoneChartDestinationGeo
    {
        [Column("zch_sts_nr")]
        public int ZoneChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        public string DestinationCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("dtn_gpu_nr", TypeName = "char")]
        public string DestinationGpuNumber { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("dtn_rng_lo_psl_cd", TypeName = "varchar(9)")]
        public string DestinationRangeLowPostalCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("dtn_rng_hi_psl_cd", TypeName = "varchar(9)")]
        public string DestinationRangeHighPostalCode { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("del_zn_nr", TypeName = "char")]
        public string DeliveryZoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
