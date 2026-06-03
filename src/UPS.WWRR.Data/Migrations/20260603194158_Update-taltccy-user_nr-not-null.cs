using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updatetaltccyuser_nrnotnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "usr_nr",
                schema: "a886aa_ao",
                table: "taltccy_stg",
                type: "char(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(8)",
                oldMaxLength: 8,
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "usr_nr",
                schema: "a886aa_ao",
                table: "taltccy",
                type: "char(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(8)",
                oldMaxLength: 8,
                oldNullable: true,
                oldDefaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "usr_nr",
                schema: "a886aa_ao",
                table: "taltccy_stg",
                type: "char(8)",
                maxLength: 8,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(8)",
                oldMaxLength: 8,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "usr_nr",
                schema: "a886aa_ao",
                table: "taltccy",
                type: "char(8)",
                maxLength: 8,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(8)",
                oldMaxLength: 8,
                oldDefaultValue: "");
        }
    }
}
