using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_Update_BmaCapAmount_MergeProc_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_BmaCapAmount_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_CountryBillType_MergeProc.sql"));
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_CzmSystemRules_MergeProc.sql"));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
