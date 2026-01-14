using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tarcldt")]
    public class AreaClassificationDetail
    {
        [StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        [Required]
        public string OriginCountry { get; set; }

        [StringLength(4)]
        [Column("org_gpu_nr", TypeName = "char")]
        [Required]
        public string GeopoliticalOriginCountry { get; set; }

        [StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        [Required]
        public string ServiceType { get; set; }

        [StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        [Required]
        public string DestinationCountry { get; set; }

        [StringLength(4)]
        [Column("dtn_gpu_nr", TypeName = "char")]
        [Required]
        public string GeopoliticalDestinationCountry { get; set; }

        [Column("zch_nr")]
        public int ChartNumber { get; set; }

        [Column("ara_csf_hdr_stt_dt", TypeName = "Date")]
        public DateTime ChartEffectiveDate { get; set; }

        [StringLength(9)]
        [Column("org_rng_lo_psl_cd", TypeName = "char")]
        [Required]
        public string OriginLowPostal { get; set; }

        [StringLength(9)]
        [Column("org_rng_hi_psl_cd", TypeName = "char")]
        [Required]
        public string OriginHighPostal { get; set; }

        [StringLength(50)]
        [Column("org_pol_div_2_na", TypeName = "char")]
        [Required]
        public string OriginPolticialDivision2 { get; set; }

        [StringLength(9)]
        [Column("dtn_rng_lo_psl_cd", TypeName = "char")]
        [Required]
        public string DestinationLowPostal { get; set; }

        [StringLength(9)]
        [Column("dtn_rng_hi_psl_cd", TypeName = "char")]
        [Required]
        public string DestinationHighPostal { get; set; }

        [StringLength(50)]
        [Column("dtn_pol_div_2_na", TypeName = "char")]
        [Required]
        public string DestinationPoliticalDivision2 { get; set; }

        [StringLength(2)]
        [Column("org_geo_ara_typ_cd", TypeName = "char")]
        [Required]
        public string OriginGeographicArea { get; set; }

        [StringLength(2)]
        [Column("dtn_geo_ara_typ_cd", TypeName = "char")]
        [Required]
        public string DestinationGeographicArea { get; set; }

        [StringLength(2)]
        [Column("ara_csf_dtl_rul_cd", TypeName = "char")]
        [Required]
        public string AreaClassificationRule { get; set; }

        [StringLength(2)]
        [Column("ara_csf_dtl_mnt_cd", TypeName = "char")]
        [Required]
        public string AreaClassificationMnt { get; set; }

        [StringLength(3)]
        [Column("ra_chg_csf_typ_cd", TypeName = "char")]
        [Required]
        public string ChargeClassification { get; set; }

        [StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        [Required]
        public string StatusCode { get; set; }

        [Column("ara_csf_dtl_end_dt", TypeName = "Date")]
        public DateTime ChartEndDate { get; set; }

        [StringLength(5)]
        [Column("org_pol_div_1_cd", TypeName = "char")]
        [Required]
        public string OriginPolticialDivision1 { get; set; }

        [StringLength(5)]
        [Column("dtn_pol_div_1_cd", TypeName = "char")]
        [Required]
        public string DestinationPoliticalDivision1 { get; set; }
    }
}
