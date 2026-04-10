#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("qa_validation_tracker")]
    public class QaValidationTracker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(35)]
        [Column("load_ver", TypeName = "varchar(35)")]
        public string LoadVersion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("load_table_na", TypeName = "varchar(50)")]
        public string LoadTableName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("validation_status", TypeName = "varchar(20)")]
        public string ValidationStatus { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("run_id", TypeName = "varchar(100)")]
        public string? RunId { get; set; }

        [Column("start_ts")]
        public DateTime? StartTs { get; set; }

        [Column("end_ts")]
        public DateTime? EndTs { get; set; }

        [Column("duration_sec")]
        public int? DurationSec { get; set; }

        [Column("retry_count")]
        public int? RetryCount { get; set; }

        [Column("error_message", TypeName = "text")]
        public string? ErrorMessage { get; set; }
    }
}
