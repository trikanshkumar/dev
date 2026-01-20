using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtwgttrh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "twgttrh",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    pce_max_alw_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    trh_max_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    sn_tln_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twgttrh", x => new { x.cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.wgt_ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "twgttrh_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    pce_max_alw_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    trh_max_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    sn_tln_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twgttrh_stg", x => new { x.cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.wgt_ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twgttrh");

            migrationBuilder.DropTable(
                name: "twgttrh_stg");
        }
    }
}
