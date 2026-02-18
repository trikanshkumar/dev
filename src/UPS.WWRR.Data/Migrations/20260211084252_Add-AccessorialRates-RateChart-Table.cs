using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessorialRatesRateChartTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accrate",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accrate", x => new { x.zch_sts_nr, x.asy_svc_ra, x.del_zn_nr, x.ra_chg_csf_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "accrate_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accrate_stg", x => new { x.zch_sts_nr, x.asy_svc_ra, x.del_zn_nr, x.ra_chg_csf_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "accratecrit",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    dtr_cri_vlu_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_lo_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_hi_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accratecrit", x => new { x.zch_sts_nr, x.asy_svc_ra, x.dtr_cri_vlu_typ_cd, x.dtr_cri_lo_rng_te, x.ccl_mth_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "accratecrit_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    dtr_cri_vlu_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_lo_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_hi_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accratecrit_stg", x => new { x.zch_sts_nr, x.asy_svc_ra, x.dtr_cri_vlu_typ_cd, x.dtr_cri_lo_rng_te, x.ccl_mth_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "chartacccd",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartacccd", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "chartacccd_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartacccd_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "chartorggeo",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    gpu_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpu_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_cls_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    xpt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ipt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartorggeo", x => new { x.zch_sts_nr, x.gpu_xpt_cny_cd, x.gpu_ipt_cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.cus_cls_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "chartorggeo_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    gpu_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpu_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_cls_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    xpt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ipt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartorggeo_stg", x => new { x.zch_sts_nr, x.gpu_xpt_cny_cd, x.gpu_ipt_cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.cus_cls_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "chartsts",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_ra_cht_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartsts", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "chartsts_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    svc_ra_cht_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartsts_stg", x => x.zch_sts_nr);
                });

            migrationBuilder.CreateTable(
                name: "chartsvcpkg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    na_nrs_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_ra_cht_seq_nr = table.Column<decimal>(type: "numeric(6,0)", nullable: false),
                    pkg_acq_mth_csf_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartsvcpkg", x => new { x.zch_sts_nr, x.svc_typ_cd, x.pkg_cha_typ_cd, x.svc_fea_typ_cd, x.pkg_acq_mth_typ_cd, x.na_nrs_cd });
                });

            migrationBuilder.CreateTable(
                name: "chartsvcpkg_stg",
                columns: table => new
                {
                    zch_sts_nr = table.Column<int>(type: "integer", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    na_nrs_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_ra_cht_seq_nr = table.Column<decimal>(type: "numeric(6,0)", nullable: false),
                    pkg_acq_mth_csf_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chartsvcpkg_stg", x => new { x.zch_sts_nr, x.svc_typ_cd, x.pkg_cha_typ_cd, x.svc_fea_typ_cd, x.pkg_acq_mth_typ_cd, x.na_nrs_cd });
                });

            migrationBuilder.CreateTable(
                name: "tasyra_stg",
                columns: table => new
                {
                    svc_ra_cht_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    asy_svc_ra_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_vlu_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_lo_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    asy_svc_ra_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_hi_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasyra_stg", x => new { x.svc_ra_cht_nr, x.ccl_mth_typ_cd, x.del_zn_nr, x.asy_svc_ra_eff_dt, x.dtr_cri_vlu_typ_cd, x.dtr_cri_lo_rng_te, x.ra_chg_csf_typ_cd, x.svc_ra_cht_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tchart_stg",
                columns: table => new
                {
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_cls_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    na_nrs_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    gpu_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpu_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    pkg_acq_mth_csf_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    svc_ra_cht_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    xpt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ipt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    svc_ra_cht_seq_nr = table.Column<decimal>(type: "numeric(6,0)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tchart_stg", x => new { x.pkg_cha_typ_cd, x.svc_ra_cht_sts_cd, x.svc_typ_cd, x.pkg_acq_mth_typ_cd, x.svc_fea_typ_cd, x.bil_ter_typ_cd, x.asy_svc_typ_cd, x.cus_cls_typ_cd, x.ccy_cd, x.svc_ra_cht_eff_dt, x.na_nrs_cd, x.gpu_xpt_cny_cd, x.gpu_ipt_cny_cd, x.pkg_acq_mth_csf_cd, x.mvm_drc_cd });
                });

            migrationBuilder.CreateIndex(
                name: "IX_chartsts_svc_ra_cht_nr_svc_ra_cht_eff_dt_svc_ra_cht_end_dt",
                table: "chartsts",
                columns: new[] { "svc_ra_cht_nr", "svc_ra_cht_eff_dt", "svc_ra_cht_end_dt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accrate");

            migrationBuilder.DropTable(
                name: "accrate_stg");

            migrationBuilder.DropTable(
                name: "accratecrit");

            migrationBuilder.DropTable(
                name: "accratecrit_stg");

            migrationBuilder.DropTable(
                name: "chartacccd");

            migrationBuilder.DropTable(
                name: "chartacccd_stg");

            migrationBuilder.DropTable(
                name: "chartorggeo");

            migrationBuilder.DropTable(
                name: "chartorggeo_stg");

            migrationBuilder.DropTable(
                name: "chartsts");

            migrationBuilder.DropTable(
                name: "chartsts_stg");

            migrationBuilder.DropTable(
                name: "chartsvcpkg");

            migrationBuilder.DropTable(
                name: "chartsvcpkg_stg");

            migrationBuilder.DropTable(
                name: "tasyra_stg");

            migrationBuilder.DropTable(
                name: "tchart_stg");
        }
    }
}
