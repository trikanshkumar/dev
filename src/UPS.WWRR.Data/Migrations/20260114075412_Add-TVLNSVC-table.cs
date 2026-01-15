using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTVLNSVCtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tvlnsvc",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    tbl_row_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    tbl_row_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvlnsvc", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.svc_typ_cd, x.tbl_row_eff_dt, x.tbl_row_end_dt });
                });

            migrationBuilder.CreateTable(
                name: "tvlnsvc_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    tbl_row_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    tbl_row_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvlnsvc_stg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.svc_typ_cd, x.tbl_row_eff_dt, x.tbl_row_end_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tvlnsvc");

            migrationBuilder.DropTable(
                name: "tvlnsvc_stg");
        }
    }
}
