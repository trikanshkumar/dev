#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TCOLDEC_STG (Column Decodes Staging) table.
    /// </summary>
    [Table("tcoldec_stg")]
    [PrimaryKey(
        nameof(TableColumnName),
        nameof(TableColumnValue),
        nameof(ScreenCodeValue)
    )]
    public class ColumnDecodeStaging
    {
        [Required, StringLength(15)]
        [Column("tbl_clu_na", TypeName = "char")]
        public string TableColumnName { get; set; } = string.Empty;

        [Required, StringLength(15)]
        [Column("tbl_clu_vlu_te", TypeName = "char")]
        public string TableColumnValue { get; set; } = string.Empty;

        [Required, StringLength(15)]
        [Column("scr_cd_vlu_te", TypeName = "char")]
        public string ScreenCodeValue { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
