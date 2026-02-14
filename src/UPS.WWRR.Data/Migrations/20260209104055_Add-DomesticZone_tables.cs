using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDomesticZone_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "domzchartdtngeo",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartdtngeo", x => new { x.zch_sts_nr, x.dtn_gpu_nr, x.dtn_rng_lo_psl_cd });
                });

            migrationBuilder.CreateTable(
                name: "domzchartdtngeo_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartdtngeo_stg", x => new { x.zch_sts_nr, x.dtn_gpu_nr, x.dtn_rng_lo_psl_cd });
                });

            migrationBuilder.CreateTable(
                name: "domzchartlkup",
                columns: table => new
                {
                    zch_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_sht_dsc_te = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartlkup", x => x.zch_nr);
                });

            migrationBuilder.CreateTable(
                name: "domzchartlkup_stg",
                columns: table => new
                {
                    zch_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_sht_dsc_te = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartlkup_stg", x => x.zch_nr);
                });

            migrationBuilder.CreateTable(
                name: "domzchartorggeo",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartorggeo", x => new { x.zch_sts_nr, x.org_gpu_nr, x.org_rng_lo_psl_cd });
                });

            migrationBuilder.CreateTable(
                name: "domzchartorggeo_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartorggeo_stg", x => new { x.zch_sts_nr, x.org_gpu_nr, x.org_rng_lo_psl_cd });
                });

            migrationBuilder.CreateTable(
                name: "domzchartsts",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    dom_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dom_zn_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartsts", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "domzchartsts_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    dom_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dom_zn_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domzchartsts_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tdozndt_new",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    zn_ncv_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdozndt_new", x => new { x.zch_sts_nr, x.svc_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "tdozndt_new_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    zn_ncv_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdozndt_new_stg", x => new { x.zch_sts_nr, x.svc_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "tdozndt_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    dom_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zn_ncv_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dom_zn_dtl_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdozndt_stg", x => new { x.org_cny_cd, x.svc_typ_cd, x.dtn_cny_cd, x.zch_nr, x.dom_zn_hdr_stt_dt, x.org_rng_lo_psl_cd, x.org_rng_hi_psl_cd, x.dtn_rng_lo_psl_cd, x.dtn_rng_hi_psl_cd, x.bus_eny_acs_sts_cd, x.zn_ncv_typ_cd, x.dom_zn_dtl_end_dt });
                });

            migrationBuilder.CreateTable(
                name: "tdoznhd_new",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdoznhd_new", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tdoznhd_new_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdoznhd_new_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tdoznhd_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    dom_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dom_zn_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    zch_sht_dsc_te = table.Column<string>(type: "char(35)", maxLength: 35, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdoznhd_stg", x => new { x.org_cny_cd, x.org_gpu_nr, x.svc_typ_cd, x.dtn_cny_cd, x.dtn_gpu_nr, x.zch_nr, x.dom_zn_hdr_stt_dt, x.dom_zn_hdr_end_dt, x.bus_eny_acs_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domzchartdtngeo");

            migrationBuilder.DropTable(
                name: "domzchartdtngeo_stg");

            migrationBuilder.DropTable(
                name: "domzchartlkup");

            migrationBuilder.DropTable(
                name: "domzchartlkup_stg");

            migrationBuilder.DropTable(
                name: "domzchartorggeo");

            migrationBuilder.DropTable(
                name: "domzchartorggeo_stg");

            migrationBuilder.DropTable(
                name: "domzchartsts");

            migrationBuilder.DropTable(
                name: "domzchartsts_stg");

            migrationBuilder.DropTable(
                name: "tdozndt_new");

            migrationBuilder.DropTable(
                name: "tdozndt_new_stg");

            migrationBuilder.DropTable(
                name: "tdozndt_stg");

            migrationBuilder.DropTable(
                name: "tdoznhd_new");

            migrationBuilder.DropTable(
                name: "tdoznhd_new_stg");

            migrationBuilder.DropTable(
                name: "tdoznhd_stg");
        }
    }
}
