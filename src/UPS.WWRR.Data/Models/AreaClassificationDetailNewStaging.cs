#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TARCLDT_NEW_STG (Area Classification Detail New Staging) table.
    /// </summary>
    [Table("tarcldt_new_stg")]
    [PrimaryKey(
        nameof(ChartStatusNumber),
        nameof(ServiceTypeCode),
        nameof(RateChargeClassificationTypeCode)
    )]
    public class AreaClassificationDetailNewStaging
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
        public string? AreaClassificationDetailMntCode { get; set; }

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
