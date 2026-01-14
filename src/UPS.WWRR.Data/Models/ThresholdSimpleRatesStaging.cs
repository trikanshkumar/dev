#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tbrchac_stg")]
    [PrimaryKey(
        nameof(BusinessRuleTypeCode),
        nameof(OriginGpuExportCountry),
        nameof(DestinationGpuImportCountry),
        nameof(PackageTypeCode),
        nameof(BillTermTypeCode),
        nameof(ServiceFeatureTypeCode),
        nameof(ServiceTypeCode),
        nameof(MovementDirectionCode),
        nameof(CustomerRateTypeCode),
        nameof(DeterminingCriteriaTypeCode),
        nameof(MeasurementUnitTypeCode),
        nameof(ApprovalStatusCode),
        nameof(RecordEffectiveStartDate)
    )]
    public class ThresholdSimpleRatesStaging
    {
        [Required, StringLength(3)]
        [Column("brl_typ_cd", TypeName = "char")]
        public string BusinessRuleTypeCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpn_xpt_cny_cd", TypeName = "char")]
        public string OriginGpuExportCountry { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("gpn_ipt_cny_cd", TypeName = "char")]
        public string DestinationGpuImportCountry { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        public string PackageTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("bil_ter_typ_cd", TypeName = "char")]
        public string BillTermTypeCode { get; set; } = string.Empty;

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

        [Required, StringLength(3)]
        [Column("dtr_cha_typ_cd", TypeName = "char")]
        public string DeterminingCriteriaTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("ms_unt_typ_cd", TypeName = "char")]
        public string MeasurementUnitTypeCode { get; set; } = string.Empty;

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
        [Column("dtr_cha_vlu_qy", TypeName = "decimal(13,2)")]
        public decimal DeterminingCharacteristicValueQuantity { get; set; }

        [Column("is_completed_ir", TypeName = "smallint")]
        [Required]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
