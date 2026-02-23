#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTORGDTNPST_STG (International Zone Chart Origin Destination Postal Staging) table.
    /// </summary>
    [Table("izchartorgdtnpst_stg")]
    [PrimaryKey(
        nameof(ChartStatusNumber),
        nameof(OriginGpuNumber),
        nameof(OriginRangeLowPostalCode),
        nameof(OriginRangeHighPostalCode),
        nameof(OriginPoliticalDivision2Name),
        nameof(DestinationGpuNumber),
        nameof(DestinationRangeLowPostalCode),
        nameof(DestinationRangeHighPostalCode),
        nameof(DestinationPoliticalDivision2Name),
        nameof(DeliveryZoneNumber)
    )]
    public class InternationalZoneChartOriginDestinationPostalStaging
    {
        [Required]
        [Column("zch_sts_nr", TypeName = "integer")]
        public int ChartStatusNumber { get; set; }

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

        [Required, StringLength(50)]
        [Column("org_pol_div_2_na", TypeName = "varchar(50)")]
        public string OriginPoliticalDivision2Name { get; set; } = string.Empty;

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

        [Required, StringLength(50)]
        [Column("dtn_pol_div_2_na", TypeName = "varchar(50)")]
        public string DestinationPoliticalDivision2Name { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("del_zn_nr", TypeName = "varchar(6)")]
        public string DeliveryZoneNumber { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
