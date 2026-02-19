#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TDOZNHD (Domestic Zone Header New) table.
    /// </summary>
    [Table("tdoznhd")]
    [Index(nameof(LoadReference), Name = "idx_tdoznhd_load_ref_te")]
    public class DomesticZoneHeader
    {
        [Key]
        [Column("zch_sts_nr")]
        public int ZoneChartStatusNumber { get; set; }

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
