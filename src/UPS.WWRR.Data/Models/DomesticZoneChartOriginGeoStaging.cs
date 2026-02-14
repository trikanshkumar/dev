#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the DOMZCHARTORGGEO_STG (Domestic Zone Chart Origin Geo Staging) table.
    /// </summary>
    [Table("domzchartorggeo_stg")]
    [PrimaryKey(nameof(ZoneChartStatusNumber), nameof(OriginGpuNumber), nameof(OriginRangeLowPostalCode))]
    public class DomesticZoneChartOriginGeoStaging
    {
        [Column("zch_sts_nr")]
        public int ZoneChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        public string OriginCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("org_gpu_nr", TypeName = "char")]
        public string OriginGpuNumber { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("org_rng_lo_psl_cd", TypeName = "varchar(9)")]
        public string OriginRangeLowPostalCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("org_rng_hi_psl_cd", TypeName = "varchar(9)")]
        public string OriginRangeHighPostalCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
