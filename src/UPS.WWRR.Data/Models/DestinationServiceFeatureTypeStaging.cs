#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TVDSVCF_STG (Destination Service Feature Types Staging) table.
    /// </summary>
    [Table("tvdsvcf_stg")]
    [PrimaryKey(
        nameof(ImportCountryCode),
        nameof(ServiceTypeCode),
        nameof(ServiceFeatureTypeCode),
        nameof(TableRowEffectiveDate),
        nameof(ApprovalStatusCode)
    )]
    public class DestinationServiceFeatureTypeStaging
    {
        [Required, StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        public string ImportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_fea_typ_cd", TypeName = "char")]
        public string ServiceFeatureTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("tbl_row_eff_dt", TypeName = "date")]
        public DateTime TableRowEffectiveDate { get; set; }

        [Required]
        [Column("tbl_row_exp_dt", TypeName = "date")]
        public DateTime TableRowExpirationDate { get; set; }

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
