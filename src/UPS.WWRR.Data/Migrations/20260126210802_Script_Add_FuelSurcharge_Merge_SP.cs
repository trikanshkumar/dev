using Microsoft.EntityFrameworkCore.Migrations;
using UPS.WWRR.Data.Extensions;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Script_Add_FuelSurcharge_Merge_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReflectionEx.ReadAllTextFromMyAssembly("sp_FuelSurcharge_Merge_Proc.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
