using Microsoft.EntityFrameworkCore;

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
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AreaClassificationHeader>(e =>
                e.HasKey(t => new
                {
                    t.OriginCountry,
                    t.ServiceType,
                    t.AccessorialCode,
                    t.DestinationCountry,
                    t.ChartNumber,
                    t.ChartEffectiveDate,
                    t.ChartEndDate,
                    t.StatusCode
                })
            );

            modelBuilder.Entity<AreaClassificationDetail>(e =>
                e.HasKey(t => new
                {
                    t.ChartNumber,
                    t.ChartEffectiveDate,
                    t.ChartEndDate,
                    t.OriginCountry,
                    t.DestinationCountry,
                    t.ServiceType,
                    t.AreaClassificationRule,
                    t.OriginPolticialDivision2,
                    t.DestinationPoliticalDivision2,
                    t.OriginLowPostal,
                    t.OriginHighPostal,
                    t.DestinationLowPostal,
                    t.DestinationHighPostal,
                    t.StatusCode
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
