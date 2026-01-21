using System.ComponentModel;

namespace UPS.WWRR.Business.Common.Constants
{
    /// <summary>
    /// Enum containing all database table names with description.
    /// </summary>
    public enum TableName
    {
        [Description("Area Classification Detail")]
        TARCLDT,

        [Description("Area Classification Header")]
        TARCLHD,

        [Description("Alternate Currency")]
        TALTCCY,

        [Description("Accessorial Exception")]
        TASYBRL,

        [Description("Accessorial Threshold")]
        TFPUTRH,

        [Description("Destination Zip/Svc/Asy Validation")]
        TDSTSVP,

        [Description("Deficit Weight Threshold")]
        TDFWTHR,

        [Description("Accessorial Mix/Max Criteria")]
        TASYTRH,

        [Description("BMA CAP Amount")]
        TBMAVCS,

        [Description("Threshold Table For Simple Rates Tolerance")]
        TBRCHAC,

        [Description("CZM System Rules")]
        TCZMSYS,

        [Description("Audit History")]
        TAUHIST,

        [Description("Country Bill Type")]
        TCYBLTY,

        [Description("Import Service Validation")]
        TIMPSVC,

        [Description("Accessorial Rating Rules")]
        TCNYASY,

        [Description("Informational Accessorial Threshold")]
        TINFTRH,


        [Description("Postal Exception")]
        TPSLBUR,

        [Description("Informational Accessorial Charge")]
        TINFCHG,

        [Description("Insurance Criteria")]
        TINSCRI,

        [Description("Informational Accessorial Rate")]
        TINFRAT,

        [Description("International Rating Currency")]
        TIRACCY,

        [Description("Limit Values Based On Criteria")]
        TLMTVLU,

        [Description("Service Default Rules")]
        TSVCDFL,

        [Description("Service Downgrade Valid Accessorial Rules")]
        TSVCACP,

        [Description("Service Downgrade Rules")]
        TSVCDGR,

        [Description("Minimum Criteria")]
        TMINCRI,

        [Description("Simple Rate Volume Range")]
        TSIARAV,

        [Description("Valid Destination Bill Term")]
        TVDSTBT,

        [Description("Freight Rating Rules")]
        TRATRUL,

        [Description("Destination Service Feature Types")]
        TVDSVCF,

        [Description("Same Day Rate")]
        TSDRWSF,

        [Description("Valid Origin Bill Term")]
        TVORGBT,

        [Description("Template Accessorial Rules")]
        TSPMYCD,

        [Description("Valid Lane Service")]
        TVLNSVC,

        [Description("Origin Service Feature Type")]
        TVOSVCF,

        [Description("Published Letter Thresholds and Scan Tolerances")]
        TWGTTRH,

        [Description("Valid Acquisition Method")]
        TVPAQMT,

        [Description("Column Decodes")]
        TCOLDEC,
    }
}
