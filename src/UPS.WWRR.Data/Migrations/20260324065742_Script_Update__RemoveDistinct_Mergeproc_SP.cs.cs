using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_Update__RemoveDistinct_Mergeproc_SPcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidAccessorialLane_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidLaneService_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_FuelSurcharge_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_SameDayRate_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ImportServiceValidation_Merge_Proc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AreaClassification_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_InternationalZone_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_RateChart_AccessorialRates_MergeProc.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
