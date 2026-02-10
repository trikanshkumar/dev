#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tarclhd")]
    public class AreaClassificationHeader
    {
        [Key]
        [Column("zch_sts_nr")]
        [Required]
        public int ChartStatusNumber { get; set; }

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceTypeCode { get; set; } = string.Empty; 

        [StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        [Required]
        public string AccessorialServiceTypeCode { get; set; } = string.Empty;  

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
