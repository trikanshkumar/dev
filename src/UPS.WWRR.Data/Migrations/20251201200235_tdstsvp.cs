using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class tdstsvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tdstsvp",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    prc_pgm_prm_vlu_te = table.Column<decimal>(type: "numeric(16,0)", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdstsvp", x => new { x.cny_cd, x.dtn_psl_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tdstsvp_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    prc_pgm_prm_vlu_te = table.Column<decimal>(type: "numeric(16,0)", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdstsvp_stg", x => new { x.cny_cd, x.dtn_psl_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tdstsvp");

            migrationBuilder.DropTable(
                name: "tdstsvp_stg");
        }
    }
}
