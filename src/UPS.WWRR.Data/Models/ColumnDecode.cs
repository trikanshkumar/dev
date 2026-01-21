#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TCOLDEC (Column Decodes) table.
    /// </summary>
    [Table("tcoldec")]
    [PrimaryKey(
        nameof(TableColumnName),
        nameof(TableColumnValue),
        nameof(ScreenCodeValue)
    )]
    public class ColumnDecode
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

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
