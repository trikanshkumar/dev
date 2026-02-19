#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPS.WWRR.Data.Models
{
    /// <summary>
    /// Represents the TDOZNHD_STG (Domestic Zone Header Staging) table.
    /// </summary>
    [Table("tdoznhd_stg")]
    [PrimaryKey(
        nameof(OriginCountryCode),
        nameof(OriginGpuNumber),
        nameof(ServiceTypeCode),
        nameof(DestinationCountryCode),
        nameof(DestinationGpuNumber),
        nameof(ZoneChartNumber),
        nameof(DomesticZoneHeaderStartDate),
        nameof(DomesticZoneHeaderEndDate),
        nameof(BusinessEntityAccessStatusCode)
    )]
    [Index(nameof(LoadReference), Name = "idx_tdoznhd_stg_load_ref_te")]
    [Index(nameof(IsCompletedIndicator), Name = "idx_tdoznhd_stg_is_completed_ir")]
    public class DomesticZoneHeaderStaging
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
        [Column("dom_zn_hdr_stt_dt", TypeName = "date")]
        public DateTime DomesticZoneHeaderStartDate { get; set; }

        [Required]
        [Column("dom_zn_hdr_end_dt", TypeName = "date")]
        public DateTime DomesticZoneHeaderEndDate { get; set; }

        [Required, StringLength(2)]
        [Column("bus_eny_acs_sts_cd", TypeName = "char")]
        public string BusinessEntityAccessStatusCode { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Column("zch_lg_dsc_te", TypeName = "char")]
        public string ZoneChartLongDescriptionText { get; set; } = string.Empty;

        [Required, StringLength(35)]
        [Column("zch_sht_dsc_te", TypeName = "char")]
        public string ZoneChartShortDescriptionText { get; set; } = string.Empty;

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
