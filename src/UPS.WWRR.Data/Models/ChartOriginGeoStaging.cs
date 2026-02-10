#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the ZCHARTORGGEO_STG (Chart Origin Geo Staging) table.
    /// </summary>
    [Table("zchartorggeo_stg")]
    [PrimaryKey(nameof(ChartStatusNumber), nameof(OriginGeopoliticalNumber), nameof(OriginLowPostalCode), nameof(OriginHighPostalCode))]
    public class ChartOriginGeoStaging
    {
        [Column("zch_sts_nr")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        public string OriginCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("org_gpu_nr", TypeName = "char")]
        public string OriginGeopoliticalNumber { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("org_rng_lo_psl_cd", TypeName = "varchar(9)")]
        public string OriginLowPostalCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("org_rng_hi_psl_cd", TypeName = "varchar(9)")]
        public string OriginHighPostalCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
