using System.Collections.Immutable;

namespace UPS.WWRR.Business.Common.Constants
{
    public static class StoredProcConstant
    {
        public const string AlternateCurrencyMerge = "sp_AlternateCurrency_Merge_Proc";
        public const string AccessorialExceptionMerge = "sp_accessorialexception_merge_proc";
        public const string AccessorialMinMaxCriteriaMerge = "sp_accessorialminmaxcriteria_merge_proc";
        public const string AccessorialThresholdMerge = "sp_accessorialthreshold_merge_proc";
        public const string DestinationZipSvcAsyValidationMerge = "sp_destinationzipsvcasyvalidation_merge_proc";
        public const string DeficitWeightThresholdMerge = "sp_deficitweightthreshold_merge_proc";
        public const string BmaCapAmountMerge = "sp_bmacapamount_merge_proc";
        public const string ThresholdSimpleRatesMerge = "sp_thresholdsimplerates_merge_proc";
        public const string CzmSystemRulesMerge = "sp_czmsystemrules_merge_proc";
        public const string AuditHistoryMerge = "sp_audithistory_merge_proc";
        public const string AccessorialRatingRulesMerge = "sp_accessorialratingrules_merge_proc";
        public const string CountryBillTypeMerge = "sp_countrybilltype_merge_proc";
        public const string ServiceDowngradeValidAccessorialRulesMerge = "sp_servicedowngradevalidaccessorialrules_merge_proc";
        public const string ServiceDowngradeRulesMerge = "sp_servicedowngraderules_merge_proc";
        public const string ServiceDefaultRulesMerge = "sp_servicedefaultrules_merge_proc";
        public const string ImportServiceValidationMerge = "sp_importservicevalidation_merge_proc";
        public const string InformationalAccessorialThresholdMerge = "sp_informationalaccessorialthreshold_merge_proc";
        public const string PostalExceptionMerge = "sp_postalexception_merge_proc";
        public const string InformationalAccessorialChargeMerge = "sp_informationalaccessorialcharge_merge_proc";
        public const string InsuranceCriteriaMerge = "sp_insurancecriteria_merge_proc";
        public const string InformationalAccessorialRateMerge = "sp_informationalaccessorialrate_merge_proc";
        public const string MinimumCriteriaMerge = "sp_minimumcriteria_merge_proc";
        public const string InternationalRatingCurrencyMerge = "sp_internationalratingcurrency_merge_proc";
        public const string LimitValuesBasedOnCriteriaMerge = "sp_limitvaluesbasedoncriteria_merge_proc";
        public const string SimpleRateVolumeRangeMerge = "sp_simpleratevolumerange_merge_proc";
        public const string ValidDestinationBillTermMerge = "sp_validdestinationbillterm_merge_proc";
        public const string FreightRatingRulesMerge = "sp_freightratingrules_merge_proc";
        public const string DestinationServiceFeatureTypeMerge = "sp_destinationservicefeaturetype_merge_proc";
        public const string SameDayRateMerge = "sp_samedayrate_merge_proc";
        public const string TemplateAccessorialRulesMerge = "sp_templateaccessorialrules_merge_proc";
        public const string ValidOriginBillTermMerge = "sp_validoriginbillterm_merge_proc";
        public const string ValidLaneServiceMerge = "sp_validlaneservice_merge_proc";
        public const string OriginServiceFeatureTypeMerge = "sp_originservicefeaturetypes_merge_proc";
        public const string PublishedLetterThresholdMerge = "sp_publishedletterthreshold_merge_proc";
        public const string ValidAcquisitionMethodMerge = "sp_validacquisitionmethod_merge_proc";
        public const string ColumnDecodeMerge = "sp_columndecode_merge_proc";
        public const string ValidOriginServicePackageMerge = "sp_validoriginservicepackage_merge_proc";
        public const string DecodeValuesMerge = "sp_decodevalues_merge_proc";
        public const string FuelSurchargeMerge = "sp_fuelsurcharge_merge_proc";
        public const string FuelSurchargeBatchMerge = "sp_fuelsurcharge_batch_merge_proc";
        public const string ValidAccessorialLaneMerge = "sp_validaccessoriallane_merge_proc";
        public const string ValidAccessorialLaneBatchMerge = "sp_validaccessoriallane_batch_merge_proc";
        public const string FreightRatesMerge = "sp_freightrates_merge_proc";
        public const string FreightRatesBatchMarge = "sp_freightrates_batch_merge_proc";
        public const string AreaClassificationHeaderNormalizeStaging = "sp_areaclassification_stagingdataset_proc";
        public const string AreaClassificationHeaderMerge = "sp_areaclassification_merge_proc";
        public const string DomesticZoneNormalizeStaging = "sp_domesticzone_stagingdataset_proc";
        public const string DomesticZoneMerge = "sp_domesticzone_merge_proc";
        public const string FuelSurchargeIndexMerge = "sp_fuelsurchargeindex_merge_proc";
        public const string FuelSurchargeCategoryMapMerge = "sp_fuelsurchargecategorymap_merge_proc";
        public const string RateChartAccessorialRatesNormalizeStaging = "sp_ratechart_accessorialrates_stagingdataset_proc";
        public const string RateChartAccessorialRatesMerge = "sp_ratechart_accessorialrates_merge_proc";
        public const string InternationalZoneNormalizeStaging = "sp_internationalzone_stagingdataset_proc";
        public const string InternationalZoneMerge = "sp_internationalzone_merge_proc";

        public static bool IsValidProcedureName(string procedureName)
        {
            return AllowedProcedures.Contains(procedureName);
        }

        private static readonly ImmutableHashSet<string> AllowedProcedures = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        [
            AlternateCurrencyMerge, AccessorialExceptionMerge, AccessorialMinMaxCriteriaMerge,
            AccessorialThresholdMerge, DestinationZipSvcAsyValidationMerge, DeficitWeightThresholdMerge,
            BmaCapAmountMerge, ThresholdSimpleRatesMerge, CzmSystemRulesMerge, AuditHistoryMerge,
            AccessorialRatingRulesMerge, CountryBillTypeMerge, ServiceDowngradeValidAccessorialRulesMerge,
            ServiceDowngradeRulesMerge, ServiceDefaultRulesMerge, ImportServiceValidationMerge,
            InformationalAccessorialThresholdMerge, PostalExceptionMerge, InformationalAccessorialChargeMerge,
            InsuranceCriteriaMerge, InformationalAccessorialRateMerge, MinimumCriteriaMerge,
            InternationalRatingCurrencyMerge, LimitValuesBasedOnCriteriaMerge, SimpleRateVolumeRangeMerge,
            ValidDestinationBillTermMerge, FreightRatingRulesMerge, DestinationServiceFeatureTypeMerge,
            SameDayRateMerge, TemplateAccessorialRulesMerge, ValidOriginBillTermMerge, ValidLaneServiceMerge,
            OriginServiceFeatureTypeMerge, PublishedLetterThresholdMerge, ValidAcquisitionMethodMerge,
            ColumnDecodeMerge, ValidOriginServicePackageMerge, DecodeValuesMerge, FuelSurchargeMerge,
            FuelSurchargeBatchMerge, ValidAccessorialLaneMerge, ValidAccessorialLaneBatchMerge,
            FreightRatesMerge, FreightRatesBatchMarge, AreaClassificationHeaderNormalizeStaging,
            AreaClassificationHeaderMerge, DomesticZoneNormalizeStaging, DomesticZoneMerge,
            FuelSurchargeIndexMerge, FuelSurchargeCategoryMapMerge, RateChartAccessorialRatesNormalizeStaging,
            RateChartAccessorialRatesMerge, InternationalZoneNormalizeStaging, InternationalZoneMerge
        ]);
    }
}
