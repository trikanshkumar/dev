#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace UPS.WWRR.Data.Models
{
    [Table("tarcldt")]
    [Index(nameof(LoadReference), Name = "idx_tarcldt_load_ref_te")]
    public class AreaClassificationDetail
    {
        [Column("zch_sts_nr")]
        [Required]
        public int ChartStatusNumber { get; set; }

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("ra_chg_csf_typ_cd", TypeName = "char")]
        [Required]
        public string RateChargeClassificationTypeCode { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("ara_csf_dtl_rul_cd", TypeName = "char")]
        [Required]
        public string AreaClassificationDetailRuleCode { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("ara_csf_dtl_mnt_cd", TypeName = "char")]
        public string AreaClassificationDetailMntCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
