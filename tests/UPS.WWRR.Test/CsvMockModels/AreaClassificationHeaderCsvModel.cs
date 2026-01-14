using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace UPS.WWRR.UnitTests.CsvMockModels;

public class AreaClassificationHeaderCsvModel
{
    [StringLength(2)]
    [Name("ORG_CNY_CD")]
    [Required]
    public string OriginCountry { get; set; }

    [StringLength(4)]
    [Name("ORG_GPU_NR")]
    [Required]
    public string GeopoliticalOriginCountry { get; set; }

    [StringLength(3)]
    [Name("SVC_TYP_CD")]
    [Required]
    public string ServiceType { get; set; }

    [StringLength(3)]
    [Name("ASY_SVC_TYP_CD")]
    [Required]
    public string AccessorialCode { get; set; }

    [StringLength(2)]
    [Name("DTN_CNY_CD")]
    [Required]
    public string DestinationCountry { get; set; }

    [StringLength(4)]
    [Name("DTN_GPU_NR")]
    [Required]
    public string GeopoliticalDestinationCountry { get; set; }

    [Name("ZCH_NR")]
    public int ChartNumber { get; set; }

    [Name("ARA_CSF_HDR_STT_DT")]
    public DateTime ChartEffectiveDate { get; set; }

    [Name("ARA_CSF_HDR_END_DT")]
    public DateTime ChartEndDate { get; set; }

    [StringLength(2)]
    [Name("BUS_ENY_ACS_STS_CD")]
    [Required]
    public string StatusCode { get; set; }

    [StringLength(100)]
    [Name("ZCH_LG_DSC_TE")]
    [Required]
    public string LongDescription { get; set; }

    [StringLength(35)]
    [Name("ZCH_SHT_DSC_TE")]
    [Required]
    public string ShortDescription { get; set; }

}
