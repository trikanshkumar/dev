using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtiraccy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tiraccy",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rtg_ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    rtg_ccy_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rtg_ccy_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tiraccy", x => new { x.cny_cd, x.rtg_ccy_cd, x.rtg_ccy_stt_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tiraccy_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rtg_ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    rtg_ccy_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rtg_ccy_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tiraccy_stg", x => new { x.cny_cd, x.rtg_ccy_cd, x.rtg_ccy_stt_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tiraccy");

            migrationBuilder.DropTable(
                name: "tiraccy_stg");
        }
    }
}
