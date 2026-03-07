using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_Update_Merge_Logic_All_SPs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InformationalAccessorialRate_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InformationalAccessorialThreshold_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InsuranceCriteria_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InternationalRatingCurrency_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InternationalZone_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InternationalZone_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_LimitValuesBasedOnCriteria_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_MinimumCriteria_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_PostalException_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_PublishedLetterThreshold_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_RateChart_AccessorialRates_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_RateChart_AccessorialRates_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ServiceDefaultRules_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ServiceDowngradeRules_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ServiceDowngradeValidAccessorialRules_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_SimpleRateVolumeRange_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_TemplateAccessorialRules_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ThresholdSimpleRates_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidAccessorialLane_BatchMergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidAccessorialLane_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidAcquisitionMethod_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidDestinationBillTerm_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidOriginBillTerm_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidOriginServicePackage_MergeProc.sql"));

            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AccessorialException_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AccessorialMinMaxCriteria_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AccessorialRatingRules_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AlternateCurrency_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AreaClassification_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AreaClassification_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_BmaCapAmount_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_CzmSystemRules_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DecodeValues_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DeficitWeightThreshold_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DestinationServiceFeatureType_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DestinationZipSvcAsyValidation_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DomesticZone_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DomesticZone_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_FreightRates_BatchMergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_FreightRates_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_FuelSurcharge_Batch_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_FuelSurchargeCategoryMap_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ImportServiceValidation_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InformationalAccessorialCharge_Merge_Proc.sql"));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
