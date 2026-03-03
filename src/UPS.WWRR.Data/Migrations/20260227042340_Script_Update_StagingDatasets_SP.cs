using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_Update_StagingDatasets_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AreaClassification_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InternationalZone_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_DomesticZone_StagingDataset_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_UpdateLoadRef_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_UpdateLoadRef_Proc_V2.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_RateChart_AccessorialRates_StagingDataset_Proc.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
