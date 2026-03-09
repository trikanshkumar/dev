using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_Update_Merge_Logic_SPs_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AccessorialThreshold_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ColumnDecode_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_CountryBillType_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_RateChart_AccessorialRates_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_ValidLaneService_MergeProc.sql"));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
