#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("data_load_exceptions")]
    public class DataLoadException
    {
        [Key]
        [Column("seq_nr")]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(DataLoadDetail))]
        [Column("data_load_detail_seq_nr")]
        public long DataLoadDetailId { get; set; }
        public DataLoadDetail? DataLoadDetail { get; set; }

        [Required]
        [StringLength(50)]
        [Column("table_na", TypeName = "varchar(50)")]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("table_key_str", TypeName = "varchar(100)")]
        public string TableKey { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("err_fld_str", TypeName = "varchar(100)")]
        public string? ErrorFieldValue { get; set; }

        [Required]
        [StringLength(50)]
        [Column("err_fld_na", TypeName = "varchar(50)")]
        public string ErrorFieldName { get; set; } = string.Empty;

        [Required]
        [Column("create_udt_ts", TypeName = "timestamptz")]
        public DateTime CreatedOn { get; set; }
    }
}
