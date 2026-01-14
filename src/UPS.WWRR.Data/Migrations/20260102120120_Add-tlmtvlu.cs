using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtlmtvlu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tlmtvlu",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cri_grp_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cri_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dcl_vlu_max_a = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tlmtvlu", x => new { x.cny_cd, x.cri_grp_cd, x.cri_typ_cd, x.apv_sts_cd, x.ccy_cd, x.dtr_cri_eff_dt });
                });

            migrationBuilder.CreateTable(
                name: "tlmtvlu_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cri_grp_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cri_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dcl_vlu_max_a = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tlmtvlu_stg", x => new { x.cny_cd, x.cri_grp_cd, x.cri_typ_cd, x.apv_sts_cd, x.ccy_cd, x.dtr_cri_eff_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tlmtvlu");

            migrationBuilder.DropTable(
                name: "tlmtvlu_stg");
        }
    }
}
