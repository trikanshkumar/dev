using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    [Table("tarclhd")]
    public class AreaClassificationHeader
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

        [StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        [Required]
        public string AccessorialCode { get; set; }

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

        [Column("ara_csf_hdr_end_dt", TypeName = "Date")]
        public DateTime ChartEndDate { get; set; }

        [StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        [Required]
        public string StatusCode { get; set; }

        [StringLength(100)]
        [Column("zch_lg_dsc_te", TypeName = "char")]
        [Required]
        public string LongDescription { get; set; }

        [StringLength(35)]
        [Column("zch_sht_dsc_te", TypeName = "char")]
        [Required]
        public string ShortDescription { get; set; }

    }
}
