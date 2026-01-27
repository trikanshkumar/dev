using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_tsubchg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tsubchg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_ra_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_ra_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    asy_svc_min_amt = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsubchg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.asy_svc_typ_cd, x.mvm_drc_cd, x.svc_typ_cd, x.svc_fea_typ_cd, x.pkg_cha_typ_cd, x.pkg_acq_mth_typ_cd, x.ccy_cd, x.bil_ter_typ_cd, x.asy_svc_ra_eff_dt, x.svc_ra_cht_sts_cd, x.cus_csf_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "tsubchg_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_ra_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_ra_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    asy_svc_ra = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    asy_svc_min_amt = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsubchg_stg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.asy_svc_typ_cd, x.mvm_drc_cd, x.svc_typ_cd, x.svc_fea_typ_cd, x.pkg_cha_typ_cd, x.pkg_acq_mth_typ_cd, x.ccy_cd, x.bil_ter_typ_cd, x.asy_svc_ra_eff_dt, x.svc_ra_cht_sts_cd, x.cus_csf_typ_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tsubchg");

            migrationBuilder.DropTable(
                name: "tsubchg_stg");
        }
    }
}
