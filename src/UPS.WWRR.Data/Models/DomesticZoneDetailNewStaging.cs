#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TDOZNDT_STG (Domestic Zone Detail New Staging) table.
    /// </summary>
    [Table("tdozndt_new_stg")]
    [PrimaryKey(nameof(ZoneChartStatusNumber), nameof(ServiceTypeCode))]
    [Index(nameof(LoadReference), Name = "idx_tdozndt_stg_load_ref_te")]
    [Index(nameof(IsCompletedIndicator), Name = "idx_tdozndt_stg_is_completed_ir")]
    public class DomesticZoneDetailNewStaging
    {
        [Column("zch_sts_nr")]
        public int ZoneChartStatusNumber { get; set; }

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
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
