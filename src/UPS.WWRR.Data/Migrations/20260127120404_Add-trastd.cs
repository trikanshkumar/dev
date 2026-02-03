using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtrastd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trastd",
                columns: table => new
                {
                    svc_ra_cht_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cmy_cls_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_cgy_min_wgt_qy = table.Column<double>(type: "float8", nullable: false),
                    wgt_cgy_max_wgt_qy = table.Column<double>(type: "float8", nullable: false),
                    ac_spl_bil_ter_pr = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    cns_spl_bil_ter_pr = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trastd", x => new { x.svc_ra_cht_nr, x.svc_ra_cht_eff_dt, x.svc_ra_cht_sts_cd, x.cmy_cls_cd, x.ccl_mth_typ_cd, x.del_zn_nr, x.wgt_ms_unt_typ_cd, x.wgt_cgy_min_wgt_qy });
                });

            migrationBuilder.CreateTable(
                name: "trastd_stg",
                columns: table => new
                {
                    svc_ra_cht_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cmy_cls_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_cgy_min_wgt_qy = table.Column<double>(type: "float8", nullable: false),
                    wgt_cgy_max_wgt_qy = table.Column<double>(type: "float8", nullable: false),
                    ac_spl_bil_ter_pr = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    cns_spl_bil_ter_pr = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trastd_stg", x => new { x.svc_ra_cht_nr, x.svc_ra_cht_eff_dt, x.svc_ra_cht_sts_cd, x.cmy_cls_cd, x.ccl_mth_typ_cd, x.del_zn_nr, x.wgt_ms_unt_typ_cd, x.wgt_cgy_min_wgt_qy });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trastd");

            migrationBuilder.DropTable(
                name: "trastd_stg");
        }
    }
}
