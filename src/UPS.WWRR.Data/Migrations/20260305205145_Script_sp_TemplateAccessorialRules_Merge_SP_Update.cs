using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_sp_TemplateAccessorialRules_Merge_SP_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_TemplateAccessorialRules_Merge_Proc.sql"));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
