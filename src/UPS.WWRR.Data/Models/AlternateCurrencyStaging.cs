#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("taltccy_stg")]
    public class AlternateCurrencyStaging
    {
        [StringLength(2)]
        [Column("xpt_cny_cd", TypeName = "char")]
        [Required]
        public string ExportCountry { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("cnv_fr_ccy_cd", TypeName = "char")]
        [Required]
        public string CurrencyFrom { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("cnv_to_ccy_cd", TypeName = "char")]
        [Required]
        public string CurrencyTo { get; set; } = string.Empty;

        [Column("alt_ccy_xch_stt_dt", TypeName = "Date")]
        [Required]
        public DateTime EffectiveDate { get; set; }

        [Column("alt_ccy_xch_end_dt", TypeName = "Date")]
        [Required]
        public DateTime EndDate { get; set; }

        [Column("alt_ccy_xch_ra_qy", TypeName = "decimal(15,9)")]
        [Required]
        public decimal ExchangeRate { get; set; }

        [Column("alt_ccy_xch_or_qy", TypeName = "decimal(15,9)")]
        [Required]
        public decimal OriginalQuantity { get; set; }

        [Column("alt_ccy_dmc_ccl_qy", TypeName = "decimal(9,0)")]
        [Required]
        public decimal DomesticCalculationQuantity { get; set; }

        [Column("alt_ccy_rou_dmc_qy", TypeName = "decimal(9,0)")]
        [Required]
        public decimal RoundedDomesticQuantity { get; set; }

        [StringLength(8)]
        [Column("usr_nr", TypeName = "char")]
        public string? UserNumber { get; set; }

        [Column("is_completed_ir", TypeName = "smallint")]
        [Required]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
