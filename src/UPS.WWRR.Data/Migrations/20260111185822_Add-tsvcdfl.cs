using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtsvcdfl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tsvcdfl",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_dfl_unt_typ_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_dfl_typ_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dtr_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cri_vlu_rng_lo_qy = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    svc_dfl_vlu_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    udt_ts = table.Column<DateTime>(type: "timestamp", nullable: true),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    cri_vlu_rng_hi_qy = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsvcdfl", x => new { x.cny_cd, x.svc_typ_cd, x.svc_dfl_unt_typ_cd, x.svc_dfl_typ_cd, x.mvm_drc_cd, x.rec_eff_stt_dt, x.dtr_cha_typ_cd, x.apv_sts_cd, x.cri_vlu_rng_lo_qy });
                });

            migrationBuilder.CreateTable(
                name: "tsvcdfl_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_dfl_unt_typ_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_dfl_typ_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dtr_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cri_vlu_rng_lo_qy = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    svc_dfl_vlu_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    udt_ts = table.Column<DateTime>(type: "timestamp", nullable: true),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    cri_vlu_rng_hi_qy = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsvcdfl_stg", x => new { x.cny_cd, x.svc_typ_cd, x.svc_dfl_unt_typ_cd, x.svc_dfl_typ_cd, x.mvm_drc_cd, x.rec_eff_stt_dt, x.dtr_cha_typ_cd, x.apv_sts_cd, x.cri_vlu_rng_lo_qy });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tsvcdfl");

            migrationBuilder.DropTable(
                name: "tsvcdfl_stg");
        }
    }
}
