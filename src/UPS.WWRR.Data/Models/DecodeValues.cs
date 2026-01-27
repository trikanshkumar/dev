#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TDECODE (Decode Values) main table.
    /// </summary>
    [Table("tdecode")]
    [PrimaryKey(
        nameof(FieldName),
        nameof(TypeCodeFieldValueCode),
        nameof(RecordEffectiveStartDate)
    )]
    public class DecodeValues
    {
        [Required, StringLength(30)]
        [Column("fld_na", TypeName = "varchar(30)")]
        public string FieldName { get; set; } = string.Empty;

        [Required, StringLength(10)]
        [Column("typ_cd_fld_vlu_cd", TypeName = "char")]
        public string TypeCodeFieldValueCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Column("typ_cd_fld_dsc_te", TypeName = "varchar(100)")]
        public string TypeCodeFieldDescription { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "date")]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Required]
        [Column("rec_eff_end_dt", TypeName = "date")]
        public DateTime RecordEffectiveEndDate { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
