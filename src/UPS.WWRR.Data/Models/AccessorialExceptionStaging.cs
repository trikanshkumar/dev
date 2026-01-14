#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Staging table model for Accessorial Exception (tasybrl_stg).
    /// </summary>
    [Table("tasybrl_stg")]
    public class AccessorialExceptionStaging
    {
        [StringLength(3)]
        [Column("brl_typ_cd", TypeName = "char")]
        [Required]
        public string BusinessRuleTypeCode { get; set; } = string.Empty;

        [StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        [Required]
        public string OriginGpuExportCountry { get; set; } = string.Empty;

        [StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        [Required]
        public string DestinationGpuImportCountry { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        [Required]
        public string PackageType { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("bil_ter_typ_cd", TypeName = "char")]
        [Required]
        public string BillTerm { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("svc_fea_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceFeatureTypeCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        [Required]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("cus_csf_typ_cd", TypeName = "char")]
        [Required]
        public string CustomerRateType { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        [Required]
        public string AccessorialCode { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        [Required]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Column("rec_eff_stt_dt", TypeName = "Date")]
        [Required]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Column("rec_eff_end_dt", TypeName = "Date")]
        [Required]
        public DateTime RecordEffectiveEndDate { get; set; }

        [Column("is_completed_ir", TypeName = "smallint")]
        [Required]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
