#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Main table model for Accessorial Threshold (Pick Ups)
    /// </summary>
    [Table("tfputrh")]
    public class AccessorialThreshold
    {
        [StringLength(2)]
        [Column("cny_cd", TypeName = "char")]
        [Required]
        public string CountryCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        [Required]
        public string AccessorialServiceTypeCode { get; set; } = string.Empty;

        [Column("asy_dly_trh_qy")]
        [Required]
        public int DailyThresholdQuantity { get; set; }

        [Column("asy_wky_trh_qy")]
        [Required]
        public int WeeklyThresholdQuantity { get; set; }

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
