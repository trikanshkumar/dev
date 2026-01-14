#nullable enable
namespace UPS.WWRR.Business.DTO.Models.LoadTableDto
{
    /// <summary>
    /// DTO representing TALTCCY table with property names matching column names.
    /// </summary>
    public class AlternateCurrencyDto
    {
        public string XPT_CNY_CD { get; set; } = string.Empty;
        public string CNV_FR_CCY_CD { get; set; } = string.Empty;
        public string CNV_TO_CCY_CD { get; set; } = string.Empty;
        public DateTime ALT_CCY_XCH_STT_DT { get; set; }
        public DateTime ALT_CCY_XCH_END_DT { get; set; }
        public decimal ALT_CCY_XCH_RA_QY { get; set; }
        public decimal ALT_CCY_XCH_OR_QY { get; set; }
        public decimal ALT_CCY_DMC_CCL_QY { get; set; }
        public decimal ALT_CCY_ROU_DMC_QY { get; set; }
        public string USR_NR { get; set; } = string.Empty;
    }
}