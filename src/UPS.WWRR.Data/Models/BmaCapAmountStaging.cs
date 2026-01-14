#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tbmavcs_stg")]
    [PrimaryKey(nameof(CountryCode), nameof(ServiceType), nameof(EffectiveDate), nameof(StatusCode))]
    public class BmaCapAmountStaging
    {
        [Required]
        [StringLength(4)]
        [Column("gpn_cd", TypeName = "char")]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_stt_dt", TypeName = "Date")]
        public DateTime EffectiveDate { get; set; }

        [Required]
        [StringLength(2)]
        [Column("svc_ra_cht_sts_cd", TypeName = "char")]
        public string StatusCode { get; set; } = string.Empty;

        [Required]
        [Column("max_ncv_pr", TypeName = "decimal(7,4)")]
        public decimal MaximumIncentive { get; set; }

        [Required]
        [StringLength(2)]
        [Column("ups_ofr_pgm_cd", TypeName = "char")]
        public string UpsOfferProgramCode { get; set; } = string.Empty;

        [Required]
        [Column("rec_eff_end_dt", TypeName = "Date")]
        public DateTime EndDate { get; set; }

        [Column("is_completed_ir", TypeName = "smallint")]
        [Required]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
