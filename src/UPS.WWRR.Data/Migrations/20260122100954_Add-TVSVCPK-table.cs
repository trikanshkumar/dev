using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTVSVCPKtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tvsvcpk",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvsvcpk", x => new { x.gpn_xpt_cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tvsvcpk_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvsvcpk_stg", x => new { x.gpn_xpt_cny_cd, x.svc_typ_cd, x.pkg_cha_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tvsvcpk");

            migrationBuilder.DropTable(
                name: "tvsvcpk_stg");
        }
    }
}
