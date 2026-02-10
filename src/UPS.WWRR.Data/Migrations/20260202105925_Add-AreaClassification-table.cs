using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaClassificationtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tarcldt_new",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ara_csf_dtl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_mnt_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarcldt_new", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tarcldt_new_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ara_csf_dtl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_mnt_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarcldt_new_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tarcldt_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    ara_csf_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "char(50)", maxLength: 50, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "char(50)", maxLength: 50, nullable: false),
                    ara_csf_dtl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_geo_ara_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_geo_ara_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_mnt_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarcldt_stg", x => new { x.org_cny_cd, x.svc_typ_cd, x.dtn_cny_cd, x.zch_nr, x.ara_csf_hdr_stt_dt, x.org_rng_lo_psl_cd, x.org_rng_hi_psl_cd, x.org_pol_div_2_na, x.dtn_rng_lo_psl_cd, x.dtn_rng_hi_psl_cd, x.dtn_pol_div_2_na, x.ara_csf_dtl_rul_cd, x.bus_eny_acs_sts_cd, x.ara_csf_dtl_end_dt });
                });

            migrationBuilder.CreateTable(
                name: "tarclhd_new",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarclhd_new", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tarclhd_new_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarclhd_new_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "tarclhd_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    ara_csf_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    ara_csf_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    zch_sht_dsc_te = table.Column<string>(type: "char(35)", maxLength: 35, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarclhd_stg", x => new { x.org_cny_cd, x.svc_typ_cd, x.asy_svc_typ_cd, x.dtn_cny_cd, x.zch_nr, x.ara_csf_hdr_stt_dt, x.ara_csf_hdr_end_dt, x.bus_eny_acs_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "zchartdtngeo",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartdtngeo", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartdtngeo_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartdtngeo_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartdtngpu",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartdtngpu", x => new { x.zch_sts_nr, x.dtn_cny_cd, x.dtn_pol_div_2_na });
                });

            migrationBuilder.CreateTable(
                name: "zchartdtngpu_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartdtngpu_stg", x => new { x.zch_sts_nr, x.dtn_cny_cd, x.dtn_pol_div_2_na });
                });

            migrationBuilder.CreateTable(
                name: "zchartlkup",
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
                    table.PrimaryKey("PK_zchartlkup", x => x.zch_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartlkup_stg",
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
                    table.PrimaryKey("PK_zchartlkup_stg", x => x.zch_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartorggeo",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartorggeo", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartorggeo_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartorggeo_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartorggpu",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartorggpu", x => new { x.zch_sts_nr, x.org_cny_cd, x.org_pol_div_2_na, x.org_pol_div_1_cd });
                });

            migrationBuilder.CreateTable(
                name: "zchartorggpu_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartorggpu_stg", x => new { x.zch_sts_nr, x.org_cny_cd, x.org_pol_div_2_na, x.org_pol_div_1_cd });
                });

            migrationBuilder.CreateTable(
                name: "zchartsts",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    ara_csf_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    ara_csf_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartsts", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartsts_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    ara_csf_hdr_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    ara_csf_hdr_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartsts_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "zchartsvctyp",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartsvctyp", x => new { x.cny_cd, x.gpu_nr, x.svc_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "zchartsvctyp_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zchartsvctyp_stg", x => new { x.cny_cd, x.gpu_nr, x.svc_typ_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tarcldt_new");

            migrationBuilder.DropTable(
                name: "tarcldt_new_stg");

            migrationBuilder.DropTable(
                name: "tarcldt_stg");

            migrationBuilder.DropTable(
                name: "tarclhd_new");

            migrationBuilder.DropTable(
                name: "tarclhd_new_stg");

            migrationBuilder.DropTable(
                name: "tarclhd_stg");

            migrationBuilder.DropTable(
                name: "zchartdtngeo");

            migrationBuilder.DropTable(
                name: "zchartdtngeo_stg");

            migrationBuilder.DropTable(
                name: "zchartdtngpu");

            migrationBuilder.DropTable(
                name: "zchartdtngpu_stg");

            migrationBuilder.DropTable(
                name: "zchartlkup");

            migrationBuilder.DropTable(
                name: "zchartlkup_stg");

            migrationBuilder.DropTable(
                name: "zchartorggeo");

            migrationBuilder.DropTable(
                name: "zchartorggeo_stg");

            migrationBuilder.DropTable(
                name: "zchartorggpu");

            migrationBuilder.DropTable(
                name: "zchartorggpu_stg");

            migrationBuilder.DropTable(
                name: "zchartsts");

            migrationBuilder.DropTable(
                name: "zchartsts_stg");

            migrationBuilder.DropTable(
                name: "zchartsvctyp");

            migrationBuilder.DropTable(
                name: "zchartsvctyp_stg");
        }
    }
}
