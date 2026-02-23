#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTHD_STG (International Zone Chart Header Staging) table.
    /// </summary>
    [Table("izcharthd_stg")]
    [PrimaryKey(nameof(ChartStatusNumber))]
    public class InternationalZoneChartHeaderStaging
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("zch_sts_nr", TypeName = "integer")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        public string PackageChargeTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
