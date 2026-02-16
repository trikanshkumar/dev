using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtfscidx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tfscidx",
                columns: table => new
                {
                    pse_idx_fu_cgy_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    pse_idx_fu_ra_pr = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    fu_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    pse_idx_fu_cgy_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    fu_sur_pbh_rt_a = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tfscidx", x => new { x.pse_idx_fu_cgy_cd, x.rec_eff_stt_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tfscidx_stg",
                columns: table => new
                {
                    pse_idx_fu_cgy_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    pse_idx_fu_ra_pr = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    fu_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    pse_idx_fu_cgy_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    fu_sur_pbh_rt_a = table.Column<decimal>(type: "numeric(17,4)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tfscidx_stg", x => new { x.pse_idx_fu_cgy_cd, x.rec_eff_stt_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tfscidx");

            migrationBuilder.DropTable(
                name: "tfscidx_stg");
        }
    }
}
