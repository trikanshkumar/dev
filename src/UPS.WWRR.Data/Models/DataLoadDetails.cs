#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("data_load_details")]
    public class DataLoadDetail
    {
        [Key]
        [Column("seq_nr")]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(DataLoad))]
        [Column("data_load_seq_nr")]
        public long DataLoadId { get; set; }
        public DataLoad? DataLoad { get; set; }

        [Required]
        [StringLength(3)]
        [Column("data_load_typ", TypeName = "varchar(3)")]
        public string DataLoadType { get; set; } = string.Empty;

        [Required]
        [Column("err_ir", TypeName = "smallint")]
        public short ErrorIndicator { get; set; }

        [Required]
        [Column("tm_prc_val")]
        public int TimeProcessValue { get; set; }

        [Required]
        [StringLength(10)]
        [Column("tm_prd_typ_cd", TypeName = "varchar(10)")]
        public string TimePeriodTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_ins_nr")]
        public int RecordsInserted { get; set; }

        [Required]
        [Column("rec_upd_nr")]
        public int RecordsUpdated { get; set; }

        [Required]
        [Column("rec_del_nr")]
        public int RecordsDeleted { get; set; }

        [Required]
        [Column("batch_nr")]
        public int BatchNumber { get; set; }

        // Navigation collections
        public ICollection<DataLoadError> Errors { get; set; } = new List<DataLoadError>();
        public ICollection<DataLoadException> Exceptions { get; set; } = new List<DataLoadException>();
    }
}
