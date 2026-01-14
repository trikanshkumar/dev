#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tcyblty")]
    [PrimaryKey(nameof(CountryCode), nameof(MovementDirectionCode), nameof(BillingTermTypeCode), nameof(CountryBillingTermStartDate), nameof(CountryBillingTermEndDate), nameof(ApprovalStatusCode))]
    public class CountryBillType
    {
        [StringLength(2)]
        [Column("cny_cd", TypeName = "char")]
        [Required]
        public string CountryCode { get; set; } = string.Empty;

        [StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        [Required]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("bil_ter_typ_cd", TypeName = "char")]
        [Required]
        public string BillingTermTypeCode { get; set; } = string.Empty;

        [Column("cny_bil_ter_stt_dt", TypeName = "Date")]
        [Required]
        public DateTime CountryBillingTermStartDate { get; set; }

        [Column("cny_bil_ter_end_dt", TypeName = "Date")]
        [Required]
        public DateTime CountryBillingTermEndDate { get; set; }

        [StringLength(2)]
        [Column("apv_sts_cd", TypeName = "char")]
        [Required]
        public string ApprovalStatusCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
