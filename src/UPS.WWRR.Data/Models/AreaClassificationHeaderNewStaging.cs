#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TARCLHD_NEW_STG (Area Classification Header New Staging) table.
    /// </summary>
    [Table("tarclhd_new_stg")]
    [PrimaryKey(nameof(ChartStatusNumber))]
    public class AreaClassificationHeaderNewStaging
    {
        [Key]
        [Column("zch_sts_nr")]
        [Required]
        public int ChartStatusNumber { get; set; }

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        [Required]
        public string AccessorialServiceTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
