using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTCZMSYStable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tczmsys",
                columns: table => new
                {
                    cd_tbl_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cd_tbl_cd = table.Column<string>(type: "char(20)", maxLength: 20, nullable: false),
                    cd_tbl_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    cd_tbl_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dco_cd_dsc_te = table.Column<string>(type: "char(80)", maxLength: 80, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tczmsys", x => new { x.cd_tbl_typ_cd, x.cd_tbl_cd, x.cd_tbl_stt_dt, x.cd_tbl_end_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tczmsys_stg",
                columns: table => new
                {
                    cd_tbl_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cd_tbl_cd = table.Column<string>(type: "char(20)", maxLength: 20, nullable: false),
                    cd_tbl_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    cd_tbl_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dco_cd_dsc_te = table.Column<string>(type: "char(80)", maxLength: 80, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tczmsys_stg", x => new { x.cd_tbl_typ_cd, x.cd_tbl_cd, x.cd_tbl_stt_dt, x.cd_tbl_end_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tczmsys");

            migrationBuilder.DropTable(
                name: "tczmsys_stg");
        }
    }
}
