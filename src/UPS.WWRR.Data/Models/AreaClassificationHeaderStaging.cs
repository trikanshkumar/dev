#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TARCLHD_STG (Area Classification Header Staging) table.
    /// </summary>
    [Table("tarclhd_stg")]
    [PrimaryKey(
        nameof(OriginCountry),
        nameof(ServiceType),
        nameof(AccessorialCode),
        nameof(DestinationCountry),
        nameof(ChartNumber),
        nameof(ChartEffectiveDate),
        nameof(ChartEndDate),
        nameof(StatusCode)
    )]
    public class AreaClassificationHeaderStaging
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

        [StringLength(3)]
        [Column("asy_svc_typ_cd", TypeName = "char")]
        [Required]
        public string AccessorialCode { get; set; } = string.Empty;

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

        [Column("ara_csf_hdr_end_dt", TypeName = "date")]
        public DateTime ChartEndDate { get; set; }

        [StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        [Required]
        public string StatusCode { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("zch_lg_dsc_te", TypeName = "char")]
        [Required]
        public string LongDescription { get; set; } = string.Empty;

        [StringLength(35)]
        [Column("zch_sht_dsc_te", TypeName = "char")]
        [Required]
        public string ShortDescription { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
