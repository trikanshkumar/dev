using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_tsdrwsf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tsdrwsf",
                columns: table => new
                {
                    org_gpn_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpn_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cny_ra_sei_rl_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_cgy_min_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    wgt_cgy_max_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_ra_a = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    svc_ra_cht_nr = table.Column<string>(type: "char(7)", maxLength: 7, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsdrwsf", x => new { x.org_gpn_cd, x.dtn_gpn_cd, x.svc_typ_cd, x.del_zn_nr, x.ccy_cd, x.pkg_cha_typ_cd, x.cus_csf_typ_cd, x.svc_fea_typ_cd, x.cny_ra_sei_rl_cd, x.wgt_ms_unt_typ_cd, x.wgt_cgy_min_wgt_qy, x.svc_ra_cht_sts_cd, x.svc_ra_cht_eff_dt });
                });

            migrationBuilder.CreateTable(
                name: "tsdrwsf_stg",
                columns: table => new
                {
                    org_gpn_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpn_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cny_ra_sei_rl_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_cgy_min_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_ra_cht_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    wgt_cgy_max_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    svc_ra_cht_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_ra_a = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    svc_ra_cht_nr = table.Column<string>(type: "char(7)", maxLength: 7, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsdrwsf_stg", x => new { x.org_gpn_cd, x.dtn_gpn_cd, x.svc_typ_cd, x.del_zn_nr, x.ccy_cd, x.pkg_cha_typ_cd, x.cus_csf_typ_cd, x.svc_fea_typ_cd, x.cny_ra_sei_rl_cd, x.wgt_ms_unt_typ_cd, x.wgt_cgy_min_wgt_qy, x.svc_ra_cht_sts_cd, x.svc_ra_cht_eff_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tsdrwsf");

            migrationBuilder.DropTable(
                name: "tsdrwsf_stg");
        }
    }
}
