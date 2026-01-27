using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTDECODEtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tdecode",
                columns: table => new
                {
                    fld_na = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    typ_cd_fld_vlu_cd = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    typ_cd_fld_dsc_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdecode", x => new { x.fld_na, x.typ_cd_fld_vlu_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tdecode_stg",
                columns: table => new
                {
                    fld_na = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    typ_cd_fld_vlu_cd = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    typ_cd_fld_dsc_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tdecode_stg", x => new { x.fld_na, x.typ_cd_fld_vlu_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tdecode");

            migrationBuilder.DropTable(
                name: "tdecode_stg");
        }
    }
}
