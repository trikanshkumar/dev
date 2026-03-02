using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtfscmap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tfscmap",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    pse_idx_fu_cgy_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_ins_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tfscmap", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.mvm_drc_cd, x.svc_typ_cd, x.cus_csf_typ_cd, x.ccy_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tfscmap_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    pse_idx_fu_cgy_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_ins_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tfscmap_stg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.mvm_drc_cd, x.svc_typ_cd, x.cus_csf_typ_cd, x.ccy_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateIndex(
                name: "idx_tfscmap_load_ref_te",
                table: "tfscmap",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tfscmap_stg_is_completed_ir",
                table: "tfscmap_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tfscmap_stg_load_ref_te",
                table: "tfscmap_stg",
                column: "load_ref_te");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tfscmap");

            migrationBuilder.DropTable(
                name: "tfscmap_stg");
        }
    }
}
