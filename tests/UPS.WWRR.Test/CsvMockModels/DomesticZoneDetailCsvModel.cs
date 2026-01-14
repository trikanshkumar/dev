using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.UnitTests.CsvMockModels;

public class DomesticZoneDetailCsvModel
{
    [Name("ORG_CNY_CD")]
    [Required]
    [StringLength(2)]
    public string OriginCountry { get; set; }

    [Name("ORG_GPU_NR")]
    [Required]
    [StringLength(4)]
    public string GeopoliticalUnitOriginCountry { get; set; }

    [Name("SVC_TYP_CD")]
    [Required]
    [StringLength(3)]
    public string ServiceType { get; set; }

    [Name("DTN_CNY_CD")]
    [Required]
    [StringLength(2)]
    public string DestinationCountry { get; set; }

    [Name("DTN_GPU_NR")]
    [Required]
    [StringLength(4)]
    public string GeopoliticalUnitDestinationCountry { get; set; }

    [Name("ZCH_NR")]
    [Required]
    public int ZoneChartNumber { get; set; }

    [Name("DOM_ZN_HDR_STT_DT")]
    [Required]
    public DateTime ChartEffectiveDate { get; set; }

    [Name("ORG_RNG_LO_PSL_CD")]
    [Required]
    [StringLength(9)]
    public string OriginLowPostal { get; set; }

    [Name("ORG_RNG_HI_PSL_CD")]
    [Required]
    [StringLength(9)]
    public string OriginHighPostal { get; set; }

    [Name("DTN_RNG_LO_PSL_CD")]
    [Required]
    [StringLength(9)]
    public string DestinationLowPostal { get; set; }

    [Name("DTN_RNG_HI_PSL_CD")]
    [Required]
    [StringLength(9)]
    public string DestinationHighPostal { get; set; }

    [Name("BUS_ENY_ACS_STS_CD")]
    [Required]
    [StringLength(2)]
    public string StatusCode { get; set; }

    [Name("ZN_NCV_TYP_CD")]
    [StringLength(2)]
    public string ZoneIncentiveType { get; set; }

    [Name("DEL_ZN_NR")]
    [Required]
    [StringLength(6)]
    public string ZoneNumber { get; set; }

    [Name("DOM_ZN_DTL_END_DT")]
    [Required]
    public DateTime ChartEndDate { get; set; }

    [Name("MVM_DRC_CD")]
    [Required]
    [StringLength(1)]
    public string MovementDirection { get; set; }
}
