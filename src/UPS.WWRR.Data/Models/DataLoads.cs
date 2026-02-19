#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("data_loads")]
    public class DataLoad
    {
        [Key]
        [Column("seq_nr")]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("load_table_na", TypeName = "varchar(50)")]
        public string LoadTableName { get; set; } = string.Empty;

        [Required]
        [Column("load_ver_nr")]
        public long LoadVersionNumber { get; set; }

        [Required]
        [StringLength(35)]
        [Column("load_ver", TypeName = "varchar(35)")]
        public string LoadVersion { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        [Column("load_sts_cd", TypeName = "varchar(25)")]
        public string LoadStatusCode { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("file_location", TypeName = "varchar(255)")]
        public string FileLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        [Column("data_source", TypeName = "varchar(25)")]
        public string DataSource { get; set; } = string.Empty;

        [Required]
        [Column("create_udt_ts", TypeName = "timestamptz")]
        public DateTime CreatedOn { get; set; }

        [Column("processed_udt_ts", TypeName = "timestamptz")]
        public DateTime? ProcessedOn { get; set; }

        [Required]
        [StringLength(255)]
        [Column("log_file_location", TypeName = "varchar(255)")]
        public string LogFileLocation { get; set; } = string.Empty;

        [Required]
        [Column("total_batch_nr")]
        public int TotalBatchNumber { get; set; }

        [Required]
        [Column("batch_size")]
        public int BatchSize { get; set; }

        // Navigation
        public ICollection<DataLoadDetail> Details { get; set; } = new List<DataLoadDetail>();
    }
}
