#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the ZCHARTORGGPU_STG (Chart Origin GPU Staging) table.
    /// </summary>
    [Table("zchartorggpu_stg")]
    [PrimaryKey(nameof(ChartStatusNumber), nameof(OriginCountryCode), nameof(OriginPoliticalDivision2))]
    public class ChartOriginGpuStaging
    {
        [Column("zch_sts_nr")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        public string OriginCountryCode { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("org_pol_div_2_na", TypeName = "varchar(50)")]
        public string OriginPoliticalDivision2 { get; set; } = string.Empty;

        [StringLength(5)]
        [Column("org_pol_div_1_cd", TypeName = "varchar(5)")]
        public string? OriginPoliticalDivision1 { get; set; }

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
