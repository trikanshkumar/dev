#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tczmsys_stg")]
    [PrimaryKey(nameof(CommandTableTypeCode), nameof(CommandTableCode), nameof(TableStartDate), nameof(TableEndDate), nameof(ApprovalStatusCode))]
    public class CzmSystemRulesStaging
    {
        [StringLength(3)]
        [Column("cd_tbl_typ_cd", TypeName = "char")]
        [Required]
        public string CommandTableTypeCode { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("cd_tbl_cd", TypeName = "char")]
        [Required]
        public string CommandTableCode { get; set; } = string.Empty;

        [StringLength(80)]
        [Column("dco_cd_dsc_te", TypeName = "char")]
        [Required]
        public string DecodeCodeDescription { get; set; } = string.Empty;

        [Column("cd_tbl_stt_dt", TypeName = "Date")]
        [Required]
        public DateTime TableStartDate { get; set; }

        [Column("cd_tbl_end_dt", TypeName = "Date")]
        [Required]
        public DateTime TableEndDate { get; set; }

        [StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        [Required]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Column("is_completed_ir", TypeName = "smallint")]
        [Required]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
