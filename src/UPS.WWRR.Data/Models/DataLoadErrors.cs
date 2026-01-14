#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("data_load_errors")]
    public class DataLoadError
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
        [Column("err_cd")]
        public int ErrorCode { get; set; }

        [StringLength(100)]
        [Column("err_sp_na", TypeName = "varchar(100)")]
        public string? ErrorStoredProcedureName { get; set; }

        [StringLength(500)]
        [Column("err_msg_te", TypeName = "varchar(500)")]
        public string? ErrorMessage { get; set; }

        [Required]
        [Column("create_udt_ts", TypeName = "timestamptz")]
        public DateTime CreatedOn { get; set; }
    }
}
