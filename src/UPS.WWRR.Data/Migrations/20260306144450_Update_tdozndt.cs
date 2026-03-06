using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_tdozndt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "tdozndt_new_stg",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "tdozndt",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "izchartdtl_stg",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "izchartdtl",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldMaxLength: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "tdozndt_new_stg",
                type: "char(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "tdozndt",
                type: "char(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "izchartdtl_stg",
                type: "char(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "zn_ncv_typ_cd",
                table: "izchartdtl",
                type: "char(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(2)",
                oldMaxLength: 2);
        }
    }
}
