#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TARCLDT_STG (Area Classification Detail Staging) table.
    /// </summary>
    [Table("tarcldt_stg")]
    [PrimaryKey(
        nameof(OriginCountry),
        nameof(ServiceType),
        nameof(DestinationCountry),
        nameof(ChartNumber),
        nameof(ChartEffectiveDate),
        nameof(OriginLowPostal),
        nameof(OriginHighPostal),
        nameof(OriginPolticialDivision2),
        nameof(DestinationLowPostal),
        nameof(DestinationHighPostal),
        nameof(DestinationPoliticalDivision2),
        nameof(AreaClassificationRule),
        nameof(StatusCode),
        nameof(ChartEndDate)
    )]
    public class AreaClassificationDetailStaging
    {
        [StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        [Required]
        public string OriginCountry { get; set; } = string.Empty;

        [StringLength(4)]
        [Column("org_gpu_nr", TypeName = "char")]
        [Required]
        public string GeopoliticalOriginCountry { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        [Required]
        public string DestinationCountry { get; set; } = string.Empty;

        [StringLength(4)]
        [Column("dtn_gpu_nr", TypeName = "char")]
        [Required]
        public string GeopoliticalDestinationCountry { get; set; } = string.Empty;

        [Column("zch_nr")]
        public int ChartNumber { get; set; }

        [Column("ara_csf_hdr_stt_dt", TypeName = "date")]
        public DateTime ChartEffectiveDate { get; set; }

        [StringLength(9)]
        [Column("org_rng_lo_psl_cd", TypeName = "char")]
        [Required]
        public string OriginLowPostal { get; set; } = string.Empty;

        [StringLength(9)]
        [Column("org_rng_hi_psl_cd", TypeName = "char")]
        [Required]
        public string OriginHighPostal { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("org_pol_div_2_na", TypeName = "char")]
        [Required]
        public string OriginPolticialDivision2 { get; set; } = string.Empty;

        [StringLength(9)]
        [Column("dtn_rng_lo_psl_cd", TypeName = "char")]
        [Required]
        public string DestinationLowPostal { get; set; } = string.Empty;

        [StringLength(9)]
        [Column("dtn_rng_hi_psl_cd", TypeName = "char")]
        [Required]
        public string DestinationHighPostal { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("dtn_pol_div_2_na", TypeName = "char")]
        [Required]
        public string DestinationPoliticalDivision2 { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("org_geo_ara_typ_cd", TypeName = "char")]
        [Required]
        public string OriginGeographicArea { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("dtn_geo_ara_typ_cd", TypeName = "char")]
        [Required]
        public string DestinationGeographicArea { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("ara_csf_dtl_rul_cd", TypeName = "char")]
        [Required]
        public string AreaClassificationRule { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("ara_csf_dtl_mnt_cd", TypeName = "char")]
        [Required]
        public string AreaClassificationMnt { get; set; } = string.Empty;

        [StringLength(3)]
        [Column("ra_chg_csf_typ_cd", TypeName = "char")]
        [Required]
        public string ChargeClassification { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        [Required]
        public string StatusCode { get; set; } = string.Empty;

        [Column("ara_csf_dtl_end_dt", TypeName = "date")]
        public DateTime ChartEndDate { get; set; }

        [StringLength(5)]
        [Column("org_pol_div_1_cd", TypeName = "char")]
        [Required]
        public string OriginPolticialDivision1 { get; set; } = string.Empty;

        [StringLength(5)]
        [Column("dtn_pol_div_1_cd", TypeName = "char")]
        [Required]
        public string DestinationPoliticalDivision1 { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
