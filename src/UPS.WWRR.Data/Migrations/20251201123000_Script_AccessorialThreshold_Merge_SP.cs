using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_AccessorialThreshold_Merge_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deploy stored procedure for Accessorial Threshold merge
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_AccessorialThreshold_MergeProc.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optionally drop the procedure (left blank to avoid accidental loss)
            // migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_accessorialthreshold_merge_proc();");
        }
    }
}
