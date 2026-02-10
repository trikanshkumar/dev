using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAreaClassificationtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tarcldt_new");

            migrationBuilder.DropTable(
                name: "tarclhd_new");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggpu_stg",
                table: "zchartorggpu_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggpu",
                table: "zchartorggpu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggeo_stg",
                table: "zchartorggeo_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggeo",
                table: "zchartorggeo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartdtngeo_stg",
                table: "zchartdtngeo_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartdtngeo",
                table: "zchartdtngeo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarclhd",
                table: "tarclhd");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarcldt_new_stg",
                table: "tarcldt_new_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarcldt",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_cny_cd",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "dtn_cny_cd",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "ara_csf_hdr_stt_dt",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "ara_csf_hdr_end_dt",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "bus_eny_acs_sts_cd",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "dtn_gpu_nr",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "org_gpu_nr",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "zch_lg_dsc_te",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "zch_sht_dsc_te",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "ara_csf_hdr_stt_dt",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "ara_csf_dtl_end_dt",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_cny_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_cny_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_pol_div_2_na",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_pol_div_2_na",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_rng_lo_psl_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_rng_hi_psl_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_rng_lo_psl_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_rng_hi_psl_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "bus_eny_acs_sts_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_geo_ara_typ_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_gpu_nr",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "dtn_pol_div_1_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_geo_ara_typ_cd",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_gpu_nr",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "org_pol_div_1_cd",
                table: "tarcldt");

            migrationBuilder.RenameColumn(
                name: "zch_nr",
                table: "tarclhd",
                newName: "zch_sts_nr");

            migrationBuilder.RenameColumn(
                name: "zch_nr",
                table: "tarcldt",
                newName: "zch_sts_nr");

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "zchartorggpu_stg",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "zchartorggpu",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartorggeo_stg",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartorggeo",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartdtngeo_stg",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartdtngeo",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "tarclhd",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "load_ref_te",
                table: "tarclhd",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ara_csf_dtl_mnt_cd",
                table: "tarcldt_new_stg",
                type: "char(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "tarcldt_new_stg",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "load_ref_te",
                table: "tarcldt",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggpu_stg",
                table: "zchartorggpu_stg",
                columns: new[] { "zch_sts_nr", "org_cny_cd", "org_pol_div_2_na" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggpu",
                table: "zchartorggpu",
                columns: new[] { "zch_sts_nr", "org_cny_cd", "org_pol_div_2_na" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggeo_stg",
                table: "zchartorggeo_stg",
                columns: new[] { "zch_sts_nr", "org_gpu_nr", "org_rng_lo_psl_cd", "org_rng_hi_psl_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggeo",
                table: "zchartorggeo",
                columns: new[] { "zch_sts_nr", "org_gpu_nr", "org_rng_lo_psl_cd", "org_rng_hi_psl_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartdtngeo_stg",
                table: "zchartdtngeo_stg",
                columns: new[] { "zch_sts_nr", "dtn_gpu_nr", "dtn_rng_lo_psl_cd", "dtn_rng_hi_psl_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartdtngeo",
                table: "zchartdtngeo",
                columns: new[] { "zch_sts_nr", "dtn_gpu_nr", "dtn_rng_lo_psl_cd", "dtn_rng_hi_psl_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarclhd",
                table: "tarclhd",
                column: "zch_sts_nr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarcldt_new_stg",
                table: "tarcldt_new_stg",
                columns: new[] { "zch_sts_nr", "svc_typ_cd", "ra_chg_csf_typ_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarcldt",
                table: "tarcldt",
                columns: new[] { "zch_sts_nr", "svc_typ_cd", "ra_chg_csf_typ_cd" });

            migrationBuilder.AddUniqueConstraint(
                name: "uq_ZCHARTSTS",
                table: "zchartsts",
                columns: new[] { "zch_nr", "ara_csf_hdr_stt_dt", "ara_csf_hdr_end_dt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "uq_ZCHARTSTS",
                table: "zchartsts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggpu_stg",
                table: "zchartorggpu_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggpu",
                table: "zchartorggpu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggeo_stg",
                table: "zchartorggeo_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartorggeo",
                table: "zchartorggeo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartdtngeo_stg",
                table: "zchartdtngeo_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_zchartdtngeo",
                table: "zchartdtngeo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarclhd",
                table: "tarclhd");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarcldt_new_stg",
                table: "tarcldt_new_stg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarcldt",
                table: "tarcldt");

            migrationBuilder.DropColumn(
                name: "load_ref_te",
                table: "tarclhd");

            migrationBuilder.DropColumn(
                name: "load_ref_te",
                table: "tarcldt");

            migrationBuilder.RenameColumn(
                name: "zch_sts_nr",
                table: "tarclhd",
                newName: "zch_nr");

            migrationBuilder.RenameColumn(
                name: "zch_sts_nr",
                table: "tarcldt",
                newName: "zch_nr");

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "zchartorggpu_stg",
                type: "varchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "org_pol_div_1_cd",
                table: "zchartorggpu",
                type: "varchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartorggeo_stg",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartorggeo",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartdtngeo_stg",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "zchartdtngeo",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "zch_nr",
                table: "tarclhd",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "org_cny_cd",
                table: "tarclhd",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_cny_cd",
                table: "tarclhd",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ara_csf_hdr_stt_dt",
                table: "tarclhd",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ara_csf_hdr_end_dt",
                table: "tarclhd",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "bus_eny_acs_sts_cd",
                table: "tarclhd",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_gpu_nr",
                table: "tarclhd",
                type: "char(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_gpu_nr",
                table: "tarclhd",
                type: "char(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "zch_lg_dsc_te",
                table: "tarclhd",
                type: "char(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "zch_sht_dsc_te",
                table: "tarclhd",
                type: "char(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ara_csf_dtl_mnt_cd",
                table: "tarcldt_new_stg",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "zch_sts_nr",
                table: "tarcldt_new_stg",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "ara_csf_hdr_stt_dt",
                table: "tarcldt",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ara_csf_dtl_end_dt",
                table: "tarcldt",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "org_cny_cd",
                table: "tarcldt",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_cny_cd",
                table: "tarcldt",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_pol_div_2_na",
                table: "tarcldt",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_pol_div_2_na",
                table: "tarcldt",
                type: "char(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_rng_lo_psl_cd",
                table: "tarcldt",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_rng_hi_psl_cd",
                table: "tarcldt",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_rng_lo_psl_cd",
                table: "tarcldt",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_rng_hi_psl_cd",
                table: "tarcldt",
                type: "char(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "bus_eny_acs_sts_cd",
                table: "tarcldt",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_geo_ara_typ_cd",
                table: "tarcldt",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_gpu_nr",
                table: "tarcldt",
                type: "char(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dtn_pol_div_1_cd",
                table: "tarcldt",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_geo_ara_typ_cd",
                table: "tarcldt",
                type: "char(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_gpu_nr",
                table: "tarcldt",
                type: "char(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "org_pol_div_1_cd",
                table: "tarcldt",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggpu_stg",
                table: "zchartorggpu_stg",
                columns: new[] { "zch_sts_nr", "org_cny_cd", "org_pol_div_2_na", "org_pol_div_1_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggpu",
                table: "zchartorggpu",
                columns: new[] { "zch_sts_nr", "org_cny_cd", "org_pol_div_2_na", "org_pol_div_1_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggeo_stg",
                table: "zchartorggeo_stg",
                column: "zch_sts_nr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartorggeo",
                table: "zchartorggeo",
                column: "zch_sts_nr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartdtngeo_stg",
                table: "zchartdtngeo_stg",
                column: "zch_sts_nr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_zchartdtngeo",
                table: "zchartdtngeo",
                column: "zch_sts_nr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarclhd",
                table: "tarclhd",
                columns: new[] { "org_cny_cd", "svc_typ_cd", "asy_svc_typ_cd", "dtn_cny_cd", "zch_nr", "ara_csf_hdr_stt_dt", "ara_csf_hdr_end_dt", "bus_eny_acs_sts_cd" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarcldt_new_stg",
                table: "tarcldt_new_stg",
                column: "zch_sts_nr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarcldt",
                table: "tarcldt",
                columns: new[] { "zch_nr", "ara_csf_hdr_stt_dt", "ara_csf_dtl_end_dt", "org_cny_cd", "dtn_cny_cd", "svc_typ_cd", "ara_csf_dtl_rul_cd", "org_pol_div_2_na", "dtn_pol_div_2_na", "org_rng_lo_psl_cd", "org_rng_hi_psl_cd", "dtn_rng_lo_psl_cd", "dtn_rng_hi_psl_cd", "bus_eny_acs_sts_cd" });

            migrationBuilder.CreateTable(
                name: "tarcldt_new",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ara_csf_dtl_mnt_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarcldt_new", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tarclhd_new",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarclhd_new", x => x.zch_sts_nr);
                });
        }
    }
}
