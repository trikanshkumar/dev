using Microsoft.EntityFrameworkCore;
using UPS.WWRR.Data.Common;

namespace UPS.WWRR.Data.Models
{
    public partial class DataContext : DbContext
    {
        // DI-only constructor
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        #region DbSets
        public virtual DbSet<AreaClassificationHeader> AreaClassificationHeader { get; set; }
        public virtual DbSet<AreaClassificationDetail> AreaClassificationDetail { get; set; }
        public virtual DbSet<DataLoad> DataLoads { get; set; }
        public virtual DbSet<DataLoadDetail> DataLoadDetails { get; set; }
        public virtual DbSet<DataLoadError> DataLoadErrors { get; set; }
        public virtual DbSet<DataLoadException> DataLoadExceptions { get; set; }
        public virtual DbSet<AlternateCurrency> AlternateCurrencies { get; set; }
        public virtual DbSet<AlternateCurrencyStaging> AlternateCurrenciesStaging { get; set; }
        public virtual DbSet<AccessorialException> AccessorialExceptions { get; set; }
        public virtual DbSet<AccessorialExceptionStaging> AccessorialExceptionsStaging { get; set; }
        public virtual DbSet<AccessorialThreshold> AccessorialThresholds { get; set; }
        public virtual DbSet<AccessorialThresholdStaging> AccessorialThresholdsStaging { get; set; }
        public virtual DbSet<DestinationZipSvcAsyValidation> DestinationZipSvcAsyValidation { get; set; }
        public virtual DbSet<DestinationZipSvcAsyValidationStaging> DestinationZipSvcAsyValidationStaging { get; set; }
        public virtual DbSet<DeficitWeightThreshold> DeficitWeightThreshold { get; set; }
        public virtual DbSet<DeficitWeightThresholdStaging> DeficitWeightThresholdStaging { get; set; }
        public virtual DbSet<AccessorialMinMaxCriteria> AccessorialMinMaxCriteria { get; set; }
        public virtual DbSet<AccessorialMinMaxCriteriaStaging> AccessorialMinMaxCriteriaStaging { get; set; }
        public virtual DbSet<BmaCapAmount> BmaCapAmount { get; set; }
        public virtual DbSet<BmaCapAmountStaging> BmaCapAmountStaging { get; set; }
        public virtual DbSet<ThresholdSimpleRates> ThresholdSimpleRates { get; set; }
        public virtual DbSet<ThresholdSimpleRatesStaging> ThresholdSimpleRatesStaging { get; set; }
        public virtual DbSet<SimpleRateVolumeRange> SimpleRateVolumeRanges { get; set; }
        public virtual DbSet<SimpleRateVolumeRangeStaging> SimpleRateVolumeRangesStaging { get; set; }
        public virtual DbSet<CzmSystemRules> CzmSystemRules { get; set; }
        public virtual DbSet<CzmSystemRulesStaging> CzmSystemRulesStaging { get; set; }
        public virtual DbSet<AuditHistory> AuditHistory { get; set; }
        public virtual DbSet<AuditHistoryStaging> AuditHistoryStaging { get; set; }
        public virtual DbSet<AccessorialRatingRules> AccessorialRatingRules { get; set; }
        public virtual DbSet<AccessorialRatingRulesStaging> AccessorialRatingRulesStaging { get; set; }
        public virtual DbSet<CountryBillType> CountryBillType { get; set; }
        public virtual DbSet<CountryBillTypeStaging> CountryBillTypeStaging { get; set; }
        public virtual DbSet<ImportServiceValidation> ImportServiceValidations { get; set; }
        public virtual DbSet<ImportServiceValidationStaging> ImportServiceValidationStaging { get; set; }
        public virtual DbSet<InformationalAccessorialThreshold> InformationalAccessorialThresholds { get; set; }
        public virtual DbSet<InformationalAccessorialThresholdStaging> InformationalAccessorialThresholdsStaging { get; set; }
        public virtual DbSet<PostalException> PostalExceptions { get; set; }
        public virtual DbSet<PostalExceptionStaging> PostalExceptionsStaging { get; set; }
        public virtual DbSet<InformationalAccessorialCharge> InformationalAccessorialCharges { get; set; }
        public virtual DbSet<InformationalAccessorialChargeStaging> InformationalAccessorialChargeStaging { get; set; }
        public virtual DbSet<InsuranceCriteria> InsuranceCriteria { get; set; }
        public virtual DbSet<InsuranceCriteriaStaging> InsuranceCriteriaStaging { get; set; }
        public virtual DbSet<InternationalRatingCurrency> InternationalRatingCurrencies { get; set; }
        public virtual DbSet<InternationalRatingCurrencyStaging> InternationalRatingCurrenciesStaging { get; set; }
        public virtual DbSet<LimitValuesBasedOnCriteria> LimitValuesBasedOnCriteria { get; set; }
        public virtual DbSet<LimitValuesBasedOnCriteriaStaging> LimitValuesBasedOnCriteriaStaging { get; set; }
        public virtual DbSet<ServiceDowngradeValidAccessorialRules> ServiceDowngradeValidAccessorialRules { get; set; }
        public virtual DbSet<ServiceDowngradeValidAccessorialRulesStaging> ServiceDowngradeValidAccessorialRulesStaging { get; set; }
        public virtual DbSet<ServiceDowngradeRules> ServiceDowngradeRules { get; set; }
        public virtual DbSet<ServiceDowngradeRulesStaging> ServiceDowngradeRulesStaging { get; set; }
        public virtual DbSet<ServiceDefaultRules> ServiceDefaultRules { get; set; }
        public virtual DbSet<ServiceDefaultRulesStaging> ServiceDefaultRulesStaging { get; set; }
        public virtual DbSet<MinimumCriteria> MinimumCriteria { get; set; }
        public virtual DbSet<MinimumCriteriaStaging> MinimumCriteriaStaging { get; set; }
        public virtual DbSet<InformationalAccessorialRate> InformationalAccessorialRates { get; set; }
        public virtual DbSet<InformationalAccessorialRateStaging> InformationalAccessorialRateStaging { get; set; }
        public virtual DbSet<ValidDestinationBillTerm> ValidDestinationBillTerms { get; set; }
        public virtual DbSet<ValidDestinationBillTermStaging> ValidDestinationBillTermsStaging { get; set; }
        public virtual DbSet<FreightRatingRules> FreightRatingRules { get; set; }
        public virtual DbSet<FreightRatingRulesStaging> FreightRatingRulesStaging { get; set; }
        public virtual DbSet<DestinationServiceFeatureType> DestinationServiceFeatureTypes { get; set; }
        public virtual DbSet<DestinationServiceFeatureTypeStaging> DestinationServiceFeatureTypesStaging { get; set; }
        public virtual DbSet<SameDayRate> SameDayRates { get; set; }
        public virtual DbSet<SameDayRateStaging> SameDayRateStaging { get; set; }
        public virtual DbSet<TemplateAccessorialRules> TemplateAccessorialRules { get; set; }
        public virtual DbSet<TemplateAccessorialRulesStaging> TemplateAccessorialRulesStaging { get; set; }
        public virtual DbSet<ValidOriginBillTerm> ValidOriginBillTerms { get; set; }
        public virtual DbSet<ValidOriginBillTermStaging> ValidOriginBillTermsStaging { get; set; }
        public virtual DbSet<ValidLaneService> ValidLaneServices { get; set; }
        public virtual DbSet<ValidLaneServiceStaging> ValidLaneServicesStaging { get; set; }
        public virtual DbSet<OriginServiceFeatureTypes> OriginServiceFeatureTypes { get; set; }
        public virtual DbSet<OriginServiceFeatureTypesStaging> OriginServiceFeatureTypesStaging { get; set; }
        public virtual DbSet<PublishedLetterThreshold> PublishedLetterThresholds { get; set; }
        public virtual DbSet<PublishedLetterThresholdStaging> PublishedLetterThresholdsStaging { get; set; }
        public virtual DbSet<ValidAcquisitionMethod> ValidAcquisitionMethods { get; set; }
        public virtual DbSet<ValidAcquisitionMethodStaging> ValidAcquisitionMethodsStaging { get; set; }
        public virtual DbSet<ColumnDecode> ColumnDecodes { get; set; }
        public virtual DbSet<ColumnDecodeStaging> ColumnDecodesStaging { get; set; }
        public virtual DbSet<ValidOriginServicePackage> ValidOriginServicePackages { get; set; }
        public virtual DbSet<ValidOriginServicePackageStaging> ValidOriginServicePackagesStaging { get; set; }
        public virtual DbSet<DecodeValues> DecodeValues { get; set; }
        public virtual DbSet<DecodeValuesStaging> DecodeValuesStaging { get; set; }
        public virtual DbSet<FuelSurcharge> FuelSurcharge { get; set; }
        public virtual DbSet<FuelSurchargeStaging> FuelSurchargeStating { get; set; }
        public virtual DbSet<ValidAccessorialLane> ValidAccessorialLanes { get; set; }
        public virtual DbSet<ValidAccessorialLaneStaging> ValidAccessorialLanesStaging { get; set; }
        public virtual DbSet<FreightRates> FreightRates { get; set; }
        public virtual DbSet<FreightRatesStaging> FreightRatesStaging { get; set; }
        public virtual DbSet<AreaClassificationHeaderStaging> AreaClassificationHeaderStaging { get; set; }
        public virtual DbSet<AreaClassificationDetailStaging> AreaClassificationDetailStaging { get; set; }
        public virtual DbSet<AreaClassificationHeaderNewStaging> AreaClassificationHeaderNewStaging { get; set; }
        public virtual DbSet<AreaClassificationDetailNewStaging> AreaClassificationDetailNewStaging { get; set; }
        public virtual DbSet<ChartLookup> ChartLookups { get; set; }
        public virtual DbSet<ChartLookupStaging> ChartLookupsStaging { get; set; }
        public virtual DbSet<ChartServiceType> ChartServiceTypes { get; set; }
        public virtual DbSet<ChartServiceTypeStaging> ChartServiceTypesStaging { get; set; }
        public virtual DbSet<ChartStatus> ChartStatuses { get; set; }
        public virtual DbSet<ChartStatusStaging> ChartStatusesStaging { get; set; }
        public virtual DbSet<ChartOriginGeo> ChartOriginGeos { get; set; }
        public virtual DbSet<ChartOriginGeoStaging> ChartOriginGeosStaging { get; set; }
        public virtual DbSet<ChartOriginGpu> ChartOriginGpus { get; set; }
        public virtual DbSet<ChartOriginGpuStaging> ChartOriginGpusStaging { get; set; }
        public virtual DbSet<ChartDestinationGeo> ChartDestinationGeos { get; set; }
        public virtual DbSet<ChartDestinationGeoStaging> ChartDestinationGeosStaging { get; set; }
        public virtual DbSet<ChartDestinationGpu> ChartDestinationGpus { get; set; }
        public virtual DbSet<ChartDestinationGpuStaging> ChartDestinationGpusStaging { get; set; }
       
        public virtual DbSet<DomesticZoneDetailStaging> DomesticZoneDetailsStaging { get; set; }
        public virtual DbSet<DomesticZoneHeaderStaging> DomesticZoneHeadersStaging { get; set; }
        public virtual DbSet<DomesticZoneChartLookup> DomesticZoneChartLookups { get; set; }
        public virtual DbSet<DomesticZoneChartLookupStaging> DomesticZoneChartLookupsStaging { get; set; }
        public virtual DbSet<DomesticZoneHeader> DomesticZoneHeaderNew { get; set; }
        public virtual DbSet<DomesticZoneHeaderNewStaging> DomesticZoneHeaderNewStaging { get; set; }
        public virtual DbSet<DomesticZoneDetail> DomesticZoneDetailNew { get; set; }
        public virtual DbSet<DomesticZoneDetailNewStaging> DomesticZoneDetailNewStaging { get; set; }
        public virtual DbSet<DomesticZoneChartStatus> DomesticZoneChartStatuses { get; set; }
        public virtual DbSet<DomesticZoneChartStatusStaging> DomesticZoneChartStatusesStaging { get; set; }
        public virtual DbSet<DomesticZoneChartDestinationGeo> DomesticZoneChartDestinationGeos { get; set; }
        public virtual DbSet<DomesticZoneChartDestinationGeoStaging> DomesticZoneChartDestinationGeosStaging { get; set; }
        public virtual DbSet<DomesticZoneChartOriginGeo> DomesticZoneChartOriginGeos { get; set; }
        public virtual DbSet<DomesticZoneChartOriginGeoStaging> DomesticZoneChartOriginGeosStaging { get; set; }
        public virtual DbSet<FuelSurchargeIndex> FuelSurchargeIndex { get; set; }
        public virtual DbSet<FuelSurchargeIndexStaging> FuelSurchargeIndexStaging { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DataConstants.defaultSchema);

            modelBuilder.Entity<AreaClassificationHeader>(e =>
                e.HasKey(t => t.ChartStatusNumber)
            );

            modelBuilder.Entity<AreaClassificationDetail>(e =>
                e.HasKey(t => new
                {
                    t.ChartStatusNumber,
                    t.ServiceTypeCode,
                    t.RateChargeClassificationTypeCode
                })
            );

            modelBuilder.Entity<AlternateCurrency>(e =>
            {
                e.HasNoKey();
                e.ToTable("taltccy");
                e.Property(p => p.UserNumber).HasDefaultValue(string.Empty);
            });

            modelBuilder.Entity<AlternateCurrencyStaging>(e =>
            {
                e.HasNoKey();
                e.ToTable("taltccy_stg");
                e.Property(p => p.UserNumber).HasDefaultValue(string.Empty);
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AccessorialException>(e =>
                e.HasKey(t => new
                {
                    t.BusinessRuleTypeCode,
                    t.OriginGpuExportCountry,
                    t.DestinationGpuImportCountry,
                    t.PackageType,
                    t.BillTerm,
                    t.ServiceFeatureTypeCode,
                    t.ServiceType,
                    t.MovementDirectionCode,
                    t.CustomerRateType,
                    t.AccessorialCode,
                    t.ApprovalStatusCode,
                    t.RecordEffectiveStartDate
                })
             );

            modelBuilder.Entity<AccessorialExceptionStaging>(e =>
            {
                e.HasKey(t => new
                {
                    t.BusinessRuleTypeCode,
                    t.OriginGpuExportCountry,
                    t.DestinationGpuImportCountry,
                    t.PackageType,
                    t.BillTerm,
                    t.ServiceFeatureTypeCode,
                    t.ServiceType,
                    t.MovementDirectionCode,
                    t.CustomerRateType,
                    t.AccessorialCode,
                    t.ApprovalStatusCode,
                    t.RecordEffectiveStartDate
                });
                e.ToTable("tasybrl_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AccessorialThreshold>(e =>
                e.HasKey(t => new
                {
                    t.CountryCode,
                    t.AccessorialServiceTypeCode,
                    t.ApprovalStatusCode,
                    t.RecordEffectiveStartDate
                })
            );

            modelBuilder.Entity<AccessorialThresholdStaging>(e =>
            {
                e.HasKey(t => new
                {
                    t.CountryCode,
                    t.AccessorialServiceTypeCode,
                    t.ApprovalStatusCode,
                    t.RecordEffectiveStartDate
                });
                e.ToTable("tfputrh_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DestinationZipSvcAsyValidation>(e =>
            {
                e.HasKey(t => new
                {
                    t.Country,
                    t.DestinationPostalCode,
                    t.EffectiveDate
                });
                e.ToTable("tdstsvp");
            });

            modelBuilder.Entity<DestinationZipSvcAsyValidationStaging>(e =>
            {
                e.HasKey(t => new
                {
                    t.Country,
                    t.DestinationPostalCode,
                    t.EffectiveDate
                });
                e.ToTable("tdstsvp_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for DeficitWeightThresholdStaging.IsCompletedIndicator
            modelBuilder.Entity<DeficitWeightThresholdStaging>(e =>
            {
                e.ToTable("tdfwthr_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for BmaCapAmountStaging.IsCompletedIndicator
            modelBuilder.Entity<BmaCapAmountStaging>(e =>
            {
                e.ToTable("tbmavcs_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for ThresholdSimpleRatesStaging.IsCompletedIndicator
            modelBuilder.Entity<ThresholdSimpleRatesStaging>(e =>
            {
                e.ToTable("tbrchac_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<SimpleRateVolumeRangeStaging>(e =>
            {
                e.ToTable("tsiarav_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for ThresholdSimpleRatesStaging.IsCompletedIndicator
            modelBuilder.Entity<AccessorialRatingRulesStaging>(e =>
            {
                e.ToTable("tcnyasy_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for InformationalAccessorialThresholdStaging.IsCompletedIndicator
            modelBuilder.Entity<InformationalAccessorialThresholdStaging>(e =>
            {
                e.ToTable("tinftrh_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for PostalExceptionStaging.IsCompletedIndicator
            modelBuilder.Entity<PostalExceptionStaging>(e =>
            {
                e.ToTable("tpslbur_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add default for InsuranceCriteriaStaging.IsCompletedIndicator
            modelBuilder.Entity<InsuranceCriteriaStaging>(e =>
            {
                e.ToTable("tinscri_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<InternationalRatingCurrencyStaging>(e =>
            {
                e.ToTable("tiraccy_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<LimitValuesBasedOnCriteriaStaging>(e =>
            {
                e.ToTable("tlmtvlu_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ServiceDowngradeValidAccessorialRulesStaging>(e =>
            {
                e.ToTable("tsvcacp_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ServiceDowngradeRulesStaging>(e =>
            {
                e.ToTable("tsvcdgr_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ServiceDefaultRulesStaging>(e =>
            {
                e.ToTable("tsvcdfl_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<MinimumCriteriaStaging>(e =>
            {
                e.ToTable("tmincri_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ValidDestinationBillTermStaging>(e =>
            {
                e.ToTable("tvdstbt_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ValidOriginBillTermStaging>(e =>
            {
                e.ToTable("tvorgbt_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DestinationServiceFeatureTypeStaging>(e =>
            {
                e.ToTable("tvdsvcf_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ValidLaneServiceStaging>(e =>
            {
                e.ToTable("tvlnsvc_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<PublishedLetterThresholdStaging>(e =>
            {
                e.ToTable("twgttrh_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ColumnDecodeStaging>(e =>
            {
                e.ToTable("tcoldec_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ValidOriginServicePackageStaging>(e =>
            {
                e.ToTable("tvsvcpk_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DecodeValuesStaging>(e =>
            {
                e.ToTable("tdecode_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AuditHistoryStaging>(e =>
            {
                e.ToTable("tauhist_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AccessorialMinMaxCriteriaStaging>(e =>
            {
                e.ToTable("tasytrh_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ImportServiceValidationStaging>(e =>
            {
                e.ToTable("timpsvc_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<InformationalAccessorialChargeStaging>(e =>
            {
                e.ToTable("tinfchg_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<InformationalAccessorialRateStaging>(e =>
            {
                e.ToTable("tinfrat_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<FreightRatingRulesStaging>(e =>
            {
                e.ToTable("tratrul_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<SameDayRateStaging>(e =>
            {
                e.ToTable("tsdrwsf_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<TemplateAccessorialRulesStaging>(e =>
            {
                e.ToTable("tspmycd_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<OriginServiceFeatureTypesStaging>(e =>
            {
                e.ToTable("tvosvcf_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ValidAcquisitionMethodStaging>(e =>
            {
                e.ToTable("tvpaqmt_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<FuelSurchargeStaging>(e =>
            {
                e.ToTable("tsubchg_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ValidAccessorialLaneStaging>(e =>
            {
                e.ToTable("tvasyln_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<FreightRatesStaging>(e =>
            {
                e.ToTable("trastd_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AreaClassificationHeaderStaging>(e =>
            {
                e.ToTable("tarclhd_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AreaClassificationDetailStaging>(e =>
            {
                e.ToTable("tarcldt_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AreaClassificationHeaderNewStaging>(e =>
            {
                e.ToTable("tarclhd_new_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<AreaClassificationDetailNewStaging>(e =>
            {
                e.ToTable("tarcldt_new_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ChartLookupStaging>(e =>
            {
                e.ToTable("zchartlkup_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ChartServiceTypeStaging>(e =>
            {
                e.ToTable("zchartsvctyp_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ChartStatusStaging>(e =>
            {
                e.ToTable("zchartsts_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add unique constraint for ChartStatus (matches CONSTRAINT uq_ZCHARTSTS in DB schema)
            modelBuilder.Entity<ChartStatus>(e =>
            {
                e.HasIndex(t => new
                {
                    t.ChartNumber,
                    t.ChartEffectiveStartDate,
                    t.ChartEffectiveEndDate
                }).IsUnique();
            });

            modelBuilder.Entity<ChartOriginGeoStaging>(e =>
            {
                e.ToTable("zchartorggeo_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ChartOriginGpuStaging>(e =>
            {
                e.ToTable("zchartorggpu_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ChartDestinationGeoStaging>(e =>
            {
                e.ToTable("zchartdtngeo_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<ChartDestinationGpuStaging>(e =>
            {
                e.ToTable("zchartdtngpu_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DomesticZoneDetailStaging>(e =>
            {
                e.ToTable("tdozndt_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DomesticZoneHeaderStaging>(e =>
            {
                e.ToTable("tdoznhd_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DomesticZoneChartLookupStaging>(e =>
            {
                e.ToTable("domzchartlkup_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DomesticZoneHeaderNewStaging>(e =>
            {
                e.ToTable("tdoznhd_new_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DomesticZoneDetailNewStaging>(e =>
            {
                e.ToTable("tdozndt_new_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Add unique constraint for DomesticZoneChartStatus (matches CONSTRAINT uq_domzchartsts in DB schema)
            modelBuilder.Entity<DomesticZoneChartStatus>(e =>
            {
                e.HasIndex(t => new
                {
                    t.ZoneChartNumber,
                    t.DomesticZoneHeaderStartDate,
                    t.DomesticZoneHeaderEndDate
                }).IsUnique();
            });

            modelBuilder.Entity<DomesticZoneChartStatusStaging>(e =>
            {
                e.ToTable("domzchartsts_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
                // Add unique constraint for DomesticZoneChartStatusStaging (matches CONSTRAINT uq_domzchartsts_stg in DB schema)
                e.HasIndex(t => new
                {
                    t.ZoneChartNumber,
                    t.DomesticZoneHeaderStartDate,
                    t.DomesticZoneHeaderEndDate
                }).IsUnique();
            });

            modelBuilder.Entity<DomesticZoneChartDestinationGeoStaging>(e =>
            {
                e.ToTable("domzchartdtngeo_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<DomesticZoneChartOriginGeoStaging>(e =>
            {
                e.ToTable("domzchartorggeo_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            modelBuilder.Entity<FuelSurchargeIndexStaging>(e =>
            {
                e.ToTable("tfscidx_stg");
                e.Property(p => p.IsCompletedIndicator).HasDefaultValue((short)0);
            });

            // Relationships
            modelBuilder.Entity<DataLoadDetail>()
                .HasMany(d => d.Errors)
                .WithOne(e => e.DataLoadDetail!)
                .HasForeignKey(e => e.DataLoadDetailId);

            modelBuilder.Entity<DataLoadDetail>()
                .HasMany(d => d.Exceptions)
                .WithOne(x => x.DataLoadDetail!)
                .HasForeignKey(x => x.DataLoadDetailId);

            modelBuilder.Entity<DataLoad>()
                .HasMany(l => l.Details)
                .WithOne(d => d.DataLoad!)
                .HasForeignKey(d => d.DataLoadId);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
