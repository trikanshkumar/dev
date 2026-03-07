using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updateizchartlkup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                schema: "a886aa_ao",
                table: "izchartlkup_stg",
                type: "char(135)",
                maxLength: 135,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                schema: "a886aa_ao",
                table: "izchartlkup",
                type: "char(135)",
                maxLength: 135,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                schema: "a886aa_ao",
                table: "izchartlkup_stg",
                type: "char(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(135)",
                oldMaxLength: 135);

            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                schema: "a886aa_ao",
                table: "izchartlkup",
                type: "char(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(135)",
                oldMaxLength: 135);
        }
    }
}
