using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtdfwthr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tdfwthr",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dfw_rtg_min_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    wgt_dat_ppn_ir = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdfwthr", x => new { x.cny_cd, x.wgt_ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tdfwthr_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dfw_rtg_min_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    wgt_dat_ppn_ir = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdfwthr_stg", x => new { x.cny_cd, x.wgt_ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tdfwthr");

            migrationBuilder.DropTable(
                name: "tdfwthr_stg");
        }
    }
}
