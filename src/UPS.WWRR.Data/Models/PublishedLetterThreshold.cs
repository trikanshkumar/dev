#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Main table model for Published Letter Thresholds and Scan Tolerances
    /// </summary>
    [Table("twgttrh")]
    [PrimaryKey(nameof(CountryCode), nameof(ServiceTypeCode), nameof(PackageCharacteristicTypeCode), nameof(WeightMeasurementUnitTypeCode), nameof(ApprovalStatusCode), nameof(RecordEffectiveStartDate))]
    public class PublishedLetterThreshold
    {
        [StringLength(2)]
        [Column("cny_cd", TypeName = "char")]
        [Required]
        public string CountryCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        [Required]
        public string PackageCharacteristicTypeCode { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("wgt_ms_unt_typ_cd", TypeName = "char")]
        [Required]
        public string WeightMeasurementUnitTypeCode { get; set; } = string.Empty;

        [Column("pce_max_alw_wgt_qy", TypeName = "numeric(9,2)")]
        [Required]
        public decimal PieceMaxAllowableWeightQuantity { get; set; }

        [Column("trh_max_wgt_qy", TypeName = "numeric(9,2)")]
        [Required]
        public decimal ThresholdMaxWeightQuantity { get; set; }

        [Column("sn_tln_wgt_qy", TypeName = "numeric(9,2)")]
        [Required]
        public decimal ScanToleranceWeightQuantity { get; set; }

        [StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        [Required]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [Column("rec_eff_stt_dt", TypeName = "date")]
        [Required]
        public DateTime RecordEffectiveStartDate { get; set; }

        [Column("rec_eff_end_dt", TypeName = "date")]
        [Required]
        public DateTime RecordEffectiveEndDate { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
