#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tdfwthr")]
    [PrimaryKey(nameof(Country), nameof(WeightMeasurementUnitType), nameof(ApprovalStatusCode), nameof(RecordEffectiveStartDate))]
    public class DeficitWeightThreshold
    {
        [StringLength(2)]
        [Column("cny_cd", TypeName = "char")]
        [Required]
        public string Country { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("wgt_ms_unt_typ_cd", TypeName = "char")]
        [Required]
        public string WeightMeasurementUnitType { get; set; } = string.Empty;

        [Column("dfw_rtg_min_wgt_qy", TypeName = "decimal(9,2)")]
        [Required]
        public decimal DeficitRatingMinimumWeight { get; set; }

        [StringLength(1)]
        [Column("wgt_dat_ppn_ir", TypeName = "char")]
        [Required]
        public string WeightDataPpnIndicator { get; set; } = string.Empty;

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

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
