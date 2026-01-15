#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TVLNSVC (Valid Lane Service) main table.
    /// </summary>
    [Table("tvlnsvc")]
    [PrimaryKey(
        nameof(ExportCountryCode),
        nameof(ImportCountryCode),
        nameof(ServiceTypeCode),
        nameof(TableRowEffectiveDate),
        nameof(TableRowEndDate)
    )]
    public class ValidLaneService
    {
        [Required, StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        public string ExportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        public string ImportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("tbl_row_eff_dt", TypeName = "date")]
        public DateTime TableRowEffectiveDate { get; set; }

        [Required]
        [Column("tbl_row_end_dt", TypeName = "date")]
        public DateTime TableRowEndDate { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
