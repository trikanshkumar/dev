using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtbmavcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbmavcs",
                columns: table => new
                {
                    gpn_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    max_ncv_pr = table.Column<decimal>(type: "numeric(7,4)", nullable: false),
                    ups_ofr_pgm_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbmavcs", x => new { x.gpn_cd, x.svc_typ_cd, x.rec_eff_stt_dt, x.svc_ra_cht_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tbmavcs_stg",
                columns: table => new
                {
                    gpn_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    svc_ra_cht_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    max_ncv_pr = table.Column<decimal>(type: "numeric(7,4)", nullable: false),
                    ups_ofr_pgm_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbmavcs_stg", x => new { x.gpn_cd, x.svc_typ_cd, x.rec_eff_stt_dt, x.svc_ra_cht_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbmavcs");

            migrationBuilder.DropTable(
                name: "tbmavcs_stg");
        }
    }
}
