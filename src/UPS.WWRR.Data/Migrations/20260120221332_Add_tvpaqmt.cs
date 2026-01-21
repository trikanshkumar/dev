using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_tvpaqmt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tvpaqmt",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apl_ra_typ_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    tbl_row_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    tbl_row_exp_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvpaqmt", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.svc_typ_cd, x.pkg_acq_mth_typ_cd, x.apl_ra_typ_cd, x.tbl_row_eff_dt });
                });

            migrationBuilder.CreateTable(
                name: "tvpaqmt_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_acq_mth_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apl_ra_typ_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    tbl_row_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    tbl_row_exp_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvpaqmt_stg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.svc_typ_cd, x.pkg_acq_mth_typ_cd, x.apl_ra_typ_cd, x.tbl_row_eff_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tvpaqmt");

            migrationBuilder.DropTable(
                name: "tvpaqmt_stg");
        }
    }
}
