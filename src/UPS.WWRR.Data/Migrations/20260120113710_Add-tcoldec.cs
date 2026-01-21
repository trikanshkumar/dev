using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtcoldec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tcoldec",
                columns: table => new
                {
                    tbl_clu_na = table.Column<string>(type: "char(15)", maxLength: 15, nullable: false),
                    tbl_clu_vlu_te = table.Column<string>(type: "char(15)", maxLength: 15, nullable: false),
                    scr_cd_vlu_te = table.Column<string>(type: "char(15)", maxLength: 15, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcoldec", x => new { x.tbl_clu_na, x.tbl_clu_vlu_te, x.scr_cd_vlu_te });
                });

            migrationBuilder.CreateTable(
                name: "tcoldec_stg",
                columns: table => new
                {
                    tbl_clu_na = table.Column<string>(type: "char(15)", maxLength: 15, nullable: false),
                    tbl_clu_vlu_te = table.Column<string>(type: "char(15)", maxLength: 15, nullable: false),
                    scr_cd_vlu_te = table.Column<string>(type: "char(15)", maxLength: 15, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcoldec_stg", x => new { x.tbl_clu_na, x.tbl_clu_vlu_te, x.scr_cd_vlu_te });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tcoldec");

            migrationBuilder.DropTable(
                name: "tcoldec_stg");
        }
    }
}
