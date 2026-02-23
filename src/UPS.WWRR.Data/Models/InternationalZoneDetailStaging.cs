#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TINZNDT_STG (International Zone Detail Staging) table.
    /// </summary>
    [Table("tinzndt_stg")]
    [PrimaryKey(
        nameof(OriginCountryCode),
        nameof(ServiceTypeCode),
        nameof(PackageChargeTypeCode),
        nameof(DestinationCountryCode),
        nameof(ZoneChartNumber),
        nameof(InternationalZoneHeaderStartDate),
        nameof(OriginRangeLowPostalCode),
        nameof(OriginRangeHighPostalCode),
        nameof(OriginPoliticalDivision2Name),
        nameof(DestinationRangeLowPostalCode),
        nameof(DestinationRangeHighPostalCode),
        nameof(DestinationPoliticalDivision2Name),
        nameof(BusinessEntityAccessStatusCode),
        nameof(InternationalZoneDetailEndDate),
        nameof(ZoneIncentiveTypeCode)
    )]
    public class InternationalZoneDetailStaging
    {
        [Required, StringLength(2)]
        [Column("org_cny_cd", TypeName = "char")]
        public string OriginCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("org_gpu_nr", TypeName = "char")]
        public string OriginGpuNumber { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("svc_typ_cd", TypeName = "char")]
        public string ServiceTypeCode { get; set; } = string.Empty;

        [Required, StringLength(3)]
        [Column("pkg_cha_typ_cd", TypeName = "char")]
        public string PackageChargeTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("dtn_cny_cd", TypeName = "char")]
        public string DestinationCountryCode { get; set; } = string.Empty;

        [Required, StringLength(4)]
        [Column("dtn_gpu_nr", TypeName = "char")]
        public string DestinationGpuNumber { get; set; } = string.Empty;

        [Required]
        [Column("zch_nr", TypeName = "integer")]
        public int ZoneChartNumber { get; set; }

        [Required]
        [Column("inl_zn_hdr_stt_dt", TypeName = "date")]
        public DateTime InternationalZoneHeaderStartDate { get; set; }

        [Required, StringLength(9)]
        [Column("org_rng_lo_psl_cd", TypeName = "char")]
        public string OriginRangeLowPostalCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("org_rng_hi_psl_cd", TypeName = "char")]
        public string OriginRangeHighPostalCode { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("org_pol_div_2_na", TypeName = "char")]
        public string OriginPoliticalDivision2Name { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("dtn_rng_lo_psl_cd", TypeName = "char")]
        public string DestinationRangeLowPostalCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Column("dtn_rng_hi_psl_cd", TypeName = "char")]
        public string DestinationRangeHighPostalCode { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("dtn_pol_div_2_na", TypeName = "char")]
        public string DestinationPoliticalDivision2Name { get; set; } = string.Empty;

        [Required, StringLength(6)]
        [Column("del_zn_nr", TypeName = "char")]
        public string DeliveryZoneNumber { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("org_geo_ara_typ_cd", TypeName = "char")]
        public string OriginGeographicAreaTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("dtn_geo_ara_typ_cd", TypeName = "char")]
        public string DestinationGeographicAreaTypeCode { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        public string BusinessEntityAccessStatusCode { get; set; } = string.Empty;

        [Required]
        [Column("inl_zn_dtl_end_dt", TypeName = "date")]
        public DateTime InternationalZoneDetailEndDate { get; set; }

        [Required, StringLength(5)]
        [Column("org_pol_div_1_cd", TypeName = "char")]
        public string OriginPoliticalDivision1Code { get; set; } = string.Empty;

        [Required, StringLength(5)]
        [Column("dtn_pol_div_1_cd", TypeName = "char")]
        public string DestinationPoliticalDivision1Code { get; set; } = string.Empty;

        [Required, StringLength(2)]
        [Column("zn_ncv_typ_cd", TypeName = "char")]
        public string ZoneIncentiveTypeCode { get; set; } = string.Empty;

        [Required, StringLength(1)]
        [Column("mvm_drc_cd", TypeName = "char")]
        public string MovementDirectionCode { get; set; } = string.Empty;

        [Required]
        [Column("is_completed_ir", TypeName = "smallint")]
        public short IsCompletedIndicator { get; set; } = 0;

        [StringLength(100)]
        [Column("load_ref_te", TypeName = "varchar(100)")]
        public string? LoadReference { get; set; }
    }
}
