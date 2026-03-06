using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_izchartorgdtnpst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "izchartorgpoldiv_stg",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "izchartorgpoldiv",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "del_zn_nr",
                table: "izchartorgdtnpst_stg",
                type: "char(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_hi_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_lo_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_2_na",
                table: "izchartorgdtnpst_stg",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_hi_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_lo_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "del_zn_nr",
                table: "izchartorgdtnpst",
                type: "char(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_hi_psl_cd",
                table: "izchartorgdtnpst",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_lo_psl_cd",
                table: "izchartorgdtnpst",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_2_na",
                table: "izchartorgdtnpst",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_hi_psl_cd",
                table: "izchartorgdtnpst",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_lo_psl_cd",
                table: "izchartorgdtnpst",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "zch_sht_dsc_te",
                table: "izchartlkup_stg",
                type: "char(35)",
                maxLength: 35,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                table: "izchartlkup_stg",
                type: "char(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "zch_sht_dsc_te",
                table: "izchartlkup",
                type: "char(35)",
                maxLength: 35,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                table: "izchartlkup",
                type: "char(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_pol_div_1_cd",
                table: "izchartdtnpoldiv_stg",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_pol_div_1_cd",
                table: "izchartdtnpoldiv",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "izchartorgpoldiv_stg",
                type: "varchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "izchartorgpoldiv",
                type: "varchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "del_zn_nr",
                table: "izchartorgdtnpst_stg",
                type: "varchar(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_hi_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_lo_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_2_na",
                table: "izchartorgdtnpst_stg",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_hi_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_lo_psl_cd",
                table: "izchartorgdtnpst_stg",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "del_zn_nr",
                table: "izchartorgdtnpst",
                type: "varchar(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_hi_psl_cd",
                table: "izchartorgdtnpst",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_rng_lo_psl_cd",
                table: "izchartorgdtnpst",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_2_na",
                table: "izchartorgdtnpst",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_hi_psl_cd",
                table: "izchartorgdtnpst",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "org_rng_lo_psl_cd",
                table: "izchartorgdtnpst",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "zch_sht_dsc_te",
                table: "izchartlkup_stg",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(35)",
                oldMaxLength: 35);

            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                table: "izchartlkup_stg",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "zch_sht_dsc_te",
                table: "izchartlkup",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(35)",
                oldMaxLength: 35);

            migrationBuilder.AlterColumn<string>(
                name: "zch_lg_dsc_te",
                table: "izchartlkup",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_pol_div_1_cd",
                table: "izchartdtnpoldiv_stg",
                type: "varchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "dtn_pol_div_1_cd",
                table: "izchartdtnpoldiv",
                type: "varchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(5)",
                oldMaxLength: 5);
        }
    }
}
