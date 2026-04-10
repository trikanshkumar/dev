#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("qa_validation_results")]
    public class QaValidationResult
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
        [StringLength(50)]
        [Column("validation_type", TypeName = "varchar(50)")]
        public string ValidationType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("validation_name", TypeName = "varchar(100)")]
        public string ValidationName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("validation_status", TypeName = "varchar(20)")]
        public string ValidationStatus { get; set; } = string.Empty;

        [Column("expected_value", TypeName = "text")]
        public string? ExpectedValue { get; set; }

        [Column("actual_value", TypeName = "text")]
        public string? ActualValue { get; set; }

        [Column("difference_count")]
        public int? DifferenceCount { get; set; }

        [Column("error_message", TypeName = "text")]
        public string? ErrorMessage { get; set; }
    }
}
