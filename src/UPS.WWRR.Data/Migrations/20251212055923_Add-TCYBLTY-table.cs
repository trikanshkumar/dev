using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTCYBLTYtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tcyblty",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cny_bil_ter_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    cny_bil_ter_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcyblty", x => new { x.cny_cd, x.mvm_drc_cd, x.bil_ter_typ_cd, x.cny_bil_ter_stt_dt, x.cny_bil_ter_end_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tcyblty_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cny_bil_ter_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    cny_bil_ter_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcyblty_stg", x => new { x.cny_cd, x.mvm_drc_cd, x.bil_ter_typ_cd, x.cny_bil_ter_stt_dt, x.cny_bil_ter_end_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tcyblty");

            migrationBuilder.DropTable(
                name: "tcyblty_stg");
        }
    }
}
