#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TDECODE_STG (Decode Values Staging) table.
    /// </summary>
    [Table("tdecode_stg")]
    [PrimaryKey(
        nameof(FieldName),
        nameof(TypeCodeFieldValueCode),
        nameof(RecordEffectiveStartDate)
    )]
    public class DecodeValuesStaging
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
        public DateTime RecordEffectiveStartDate { get; set; } = new DateTime(2007, 12, 31);

        [Required]
        [Column("rec_eff_end_dt", TypeName = "date")]
        public DateTime RecordEffectiveEndDate { get; set; } = new DateTime(9999, 12, 31);

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
