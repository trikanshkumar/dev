using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_timpsvc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "timpsvc",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timpsvc", x => new { x.org_cny_cd, x.dtn_cny_cd, x.svc_typ_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "timpsvc_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timpsvc_stg", x => new { x.org_cny_cd, x.dtn_cny_cd, x.svc_typ_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "timpsvc");

            migrationBuilder.DropTable(
                name: "timpsvc_stg");
        }
    }
}
