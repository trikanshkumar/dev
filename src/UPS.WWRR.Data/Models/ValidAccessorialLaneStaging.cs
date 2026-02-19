#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TVASYLN_STG (Valid Accessorial Lane Staging) table.
    /// </summary>
    [Table("tvasyln_stg")]
    [PrimaryKey(
        nameof(OriginCountryCode),
        nameof(DestinationCountryCode),
        nameof(AccessorialServiceTypeCode),
        nameof(ServiceTypeCode),
        nameof(MovementDirectionCode),
        nameof(LaneClassTypeCode),
        nameof(ApprovalStatusCode),
        nameof(RecordEffectiveStartDate),
        nameof(AccessorialAlternateNumericCode),
        nameof(OriginGeopoliticalCountry),
        nameof(DestinationGeopoliticalCountry)
    )]
    [Index(nameof(LoadReference), Name = "idx_tvasyln_stg_load_ref_te")]
    [Index(nameof(IsCompletedIndicator), Name = "idx_tvasyln_stg_is_completed_ir")]
    public class ValidAccessorialLaneStaging
    {
        [Required, StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        public string OriginCountryCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        public string DestinationCountryCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        public string AccessorialServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [Required, StringLength(1)]
        [Column("gpn_unt_pir_csf_cd", TypeName = "char")]
        public string LaneClassTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "date")]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Required, StringLength(3)]
        [Column("asy_svc_alt_nmc_cd", TypeName = "char")]
        public string AccessorialAlternateNumericCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("svc_typ_alt_nmc_cd", TypeName = "char")]
        public string ServiceTypeAlternateNumericCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_end_dt", TypeName = "date")]
        public DateTime RecordEffectiveEndDate { get; set; }

        [Required, StringLength(4)]
        [Column("org_gpn_mnm_te", TypeName = "char")]
        public string OriginGeopoliticalCountry { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("dtn_gpn_mnm_te", TypeName = "char")]
        public string DestinationGeopoliticalCountry { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
