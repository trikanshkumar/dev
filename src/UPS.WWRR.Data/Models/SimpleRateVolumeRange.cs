#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tsiarav")]
    [PrimaryKey(
        nameof(OriginGpuExportCountryCode),
        nameof(DestinationGpuImportCountryCode),
        nameof(PackageTypeCode),
        nameof(BillingTermTypeCode),
        nameof(ServiceFeatureTypeCode),
        nameof(ServiceTypeCode),
        nameof(MovementDirectionCode),
        nameof(CustomerRateTypeCode),
        nameof(WeightMeasurementUnitTypeCode),
        nameof(ApprovalStatusCode),
        nameof(RecordEffectiveStartDate)
    )]
    [Index(nameof(LoadReference), Name = "idx_tsiarav_load_ref_te")]
    public class SimpleRateVolumeRange
    {
        [Required, StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        public string OriginGpuExportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        public string DestinationGpuImportCountryCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        public string PackageTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("bil_ter_typ_cd", TypeName = "char")]
        public string BillingTermTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_fea_typ_cd", TypeName = "char")]
        public string ServiceFeatureTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("cus_csf_typ_cd", TypeName = "char")]
        public string CustomerRateTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("wgt_ms_unt_typ_cd", TypeName = "char")]
        public string WeightMeasurementUnitTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "Date")]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Required]
        [Column("rec_eff_end_dt", TypeName = "Date")]
        public DateTime RecordEffectiveEndDate { get; set; }

        [Required]
        [Column("vol_rng_min_qy", TypeName = "decimal(13,2)")]
        public decimal VolumeRangeMinimumQuantity { get; set; }

        [Required]
        [Column("vol_rng_max_qy", TypeName = "decimal(13,2)")]
        public decimal VolumeRangeMaximumQuantity { get; set; }

        [Required, StringLength(3)]
        [Column("ms_unt_typ_cd", TypeName = "char")]
        public string MeasurementUnitTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("dw_min_qy", TypeName = "decimal(13,2)")]
        public decimal DimensionalWeightMinimumQuantity { get; set; }

        [Required]
        [Column("dw_max_qy", TypeName = "decimal(13,2)")]
        public decimal DimensionalWeightMaximumQuantity { get; set; }

        [Required]
        [Column("pbh_max_wgt_qy", TypeName = "decimal(13,2)")]
        public decimal PublishedMaximumWeightQuantity { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
