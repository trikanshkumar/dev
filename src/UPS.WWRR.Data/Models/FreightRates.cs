#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TRASTD (Freight Rates) main table.
    /// </summary>
    [Table("trastd")]
    [PrimaryKey(
        nameof(ChartNumber),
        nameof(ChartEffectiveDate),
        nameof(ChartStatusCode),
        nameof(CompanyClassCode),
        nameof(CalculationMethodTypeCode),
        nameof(DeliveryZoneNumber),
        nameof(WeightMeasureUnitTypeCode),
        nameof(WeightCategoryMinWeight)
    )]
    public class FreightRates
    {
        [Required, StringLength(6)]
        [Column("svc_ra_cht_nr", TypeName = "char")]
        public string ChartNumber { get; set; } = string.Empty;

        [Required]
        [Column("svc_ra_cht_eff_dt", TypeName = "date")]
        public DateTime ChartEffectiveDate { get; set; }

        [Required, StringLength(2)]
        [Column("svc_ra_cht_sts_cd", TypeName = "char")]
        public string ChartStatusCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("cmy_cls_cd", TypeName = "char")]
        public string CompanyClassCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("ccl_mth_typ_cd", TypeName = "char")]
        public string CalculationMethodTypeCode { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("del_zn_nr", TypeName = "char")]
        public string DeliveryZoneNumber { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("wgt_ms_unt_typ_cd", TypeName = "char")]
        public string WeightMeasureUnitTypeCode { get; set; } = string.Empty;

        [Required]
        [Column("wgt_cgy_min_wgt_qy", TypeName = "float8")]
        public double WeightCategoryMinWeight { get; set; }

        [Required]
        [Column("wgt_cgy_max_wgt_qy", TypeName = "float8")]
        public double WeightCategoryMaxWeight { get; set; }

        [Required]
        [Column("ac_spl_bil_ter_pr", TypeName = "numeric(17,4)")]
        public decimal AccountSplitBillTermPrice { get; set; }

        [Required]
        [Column("cns_spl_bil_ter_pr", TypeName = "numeric(17,4)")]
        public decimal ConsigneeSplitBillTermPrice { get; set; }

        [Required]
        [Column("svc_ra_cht_end_dt", TypeName = "date")]
        public DateTime ChartEndDate { get; set; }

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
