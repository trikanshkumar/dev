#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the IZCHARTDTL_STG (International Zone Chart Detail Staging) table.
    /// </summary>
    [Table("izchartdtl_stg")]
    [PrimaryKey(nameof(ChartStatusNumber))]
    public class InternationalZoneChartDetailStaging
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("zch_sts_nr", TypeName = "integer")]
        public int ChartStatusNumber { get; set; }

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("zn_ncv_typ_cd", TypeName = "char")]
        public string ZoneIncentiveTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
