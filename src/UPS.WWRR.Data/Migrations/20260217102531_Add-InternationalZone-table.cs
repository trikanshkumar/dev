using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInternationalZonetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "izchartdtl",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    zn_ncv_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartdtl", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "izchartdtl_stg",
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
                    table.PrimaryKey("PK_izchartdtl_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "izchartdtnpoldiv",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartdtnpoldiv", x => new { x.zch_sts_nr, x.dtn_cny_cd });
                });

            migrationBuilder.CreateTable(
                name: "izchartdtnpoldiv_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartdtnpoldiv_stg", x => new { x.zch_sts_nr, x.dtn_cny_cd });
                });

            migrationBuilder.CreateTable(
                name: "izcharthd",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izcharthd", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "izcharthd_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izcharthd_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "izchartlkup",
                columns: table => new
                {
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    zch_sht_dsc_te = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartlkup", x => x.zch_nr);
                });

            migrationBuilder.CreateTable(
                name: "izchartlkup_stg",
                columns: table => new
                {
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    zch_sht_dsc_te = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartlkup_stg", x => x.zch_nr);
                });

            migrationBuilder.CreateTable(
                name: "izchartorgdtnpst",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    del_zn_nr = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartorgdtnpst", x => new { x.zch_sts_nr, x.org_gpu_nr, x.org_rng_lo_psl_cd, x.org_rng_hi_psl_cd, x.org_pol_div_2_na, x.dtn_gpu_nr, x.dtn_rng_lo_psl_cd, x.dtn_rng_hi_psl_cd, x.dtn_pol_div_2_na, x.del_zn_nr });
                });

            migrationBuilder.CreateTable(
                name: "izchartorgdtnpst_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    del_zn_nr = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartorgdtnpst_stg", x => new { x.zch_sts_nr, x.org_gpu_nr, x.org_rng_lo_psl_cd, x.org_rng_hi_psl_cd, x.org_pol_div_2_na, x.dtn_gpu_nr, x.dtn_rng_lo_psl_cd, x.dtn_rng_hi_psl_cd, x.dtn_pol_div_2_na, x.del_zn_nr });
                });

            migrationBuilder.CreateTable(
                name: "izchartorgpoldiv",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartorgpoldiv", x => new { x.zch_sts_nr, x.org_cny_cd });
                });

            migrationBuilder.CreateTable(
                name: "izchartorgpoldiv_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartorgpoldiv_stg", x => new { x.zch_sts_nr, x.org_cny_cd });
                });

            migrationBuilder.CreateTable(
                name: "izchartsts",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    inl_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    inl_zn_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartsts", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "izchartsts_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    inl_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    inl_zn_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izchartsts_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tinzndt_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    inl_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "char(50)", maxLength: 50, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "char(50)", maxLength: 50, nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    inl_zn_dtl_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    zn_ncv_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    org_geo_ara_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_geo_ara_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinzndt_stg", x => new { x.org_cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.dtn_cny_cd, x.zch_nr, x.inl_zn_hdr_stt_dt, x.org_rng_lo_psl_cd, x.org_rng_hi_psl_cd, x.org_pol_div_2_na, x.dtn_rng_lo_psl_cd, x.dtn_rng_hi_psl_cd, x.dtn_pol_div_2_na, x.bus_eny_acs_sts_cd, x.inl_zn_dtl_end_dt, x.zn_ncv_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "tinznhd_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    inl_zn_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    inl_zn_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    zch_sht_dsc_te = table.Column<string>(type: "char(35)", maxLength: 35, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinznhd_stg", x => new { x.org_cny_cd, x.org_gpu_nr, x.svc_typ_cd, x.pkg_cha_typ_cd, x.dtn_cny_cd, x.dtn_gpu_nr, x.zch_nr, x.inl_zn_hdr_stt_dt, x.inl_zn_hdr_end_dt, x.bus_eny_acs_sts_cd });
                });

            migrationBuilder.CreateIndex(
                name: "IX_izchartsts_zch_nr_inl_zn_hdr_stt_dt_inl_zn_hdr_end_dt",
                table: "izchartsts",
                columns: new[] { "zch_nr", "inl_zn_hdr_stt_dt", "inl_zn_hdr_end_dt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_izchartsts_stg_zch_nr_inl_zn_hdr_stt_dt_inl_zn_hdr_end_dt",
                table: "izchartsts_stg",
                columns: new[] { "zch_nr", "inl_zn_hdr_stt_dt", "inl_zn_hdr_end_dt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "izchartdtl");

            migrationBuilder.DropTable(
                name: "izchartdtl_stg");

            migrationBuilder.DropTable(
                name: "izchartdtnpoldiv");

            migrationBuilder.DropTable(
                name: "izchartdtnpoldiv_stg");

            migrationBuilder.DropTable(
                name: "izcharthd");

            migrationBuilder.DropTable(
                name: "izcharthd_stg");

            migrationBuilder.DropTable(
                name: "izchartlkup");

            migrationBuilder.DropTable(
                name: "izchartlkup_stg");

            migrationBuilder.DropTable(
                name: "izchartorgdtnpst");

            migrationBuilder.DropTable(
                name: "izchartorgdtnpst_stg");

            migrationBuilder.DropTable(
                name: "izchartorgpoldiv");

            migrationBuilder.DropTable(
                name: "izchartorgpoldiv_stg");

            migrationBuilder.DropIndex(
                name: "IX_izchartsts_zch_nr_inl_zn_hdr_stt_dt_inl_zn_hdr_end_dt",
                table: "izchartsts");

            migrationBuilder.DropIndex(
                name: "IX_izchartsts_stg_zch_nr_inl_zn_hdr_stt_dt_inl_zn_hdr_end_dt",
                table: "izchartsts_stg");

            migrationBuilder.DropTable(
                name: "izchartsts");

            migrationBuilder.DropTable(
                name: "izchartsts_stg");

            migrationBuilder.DropTable(
                name: "tinzndt_stg");

            migrationBuilder.DropTable(
                name: "tinznhd_stg");
        }
    }
}
