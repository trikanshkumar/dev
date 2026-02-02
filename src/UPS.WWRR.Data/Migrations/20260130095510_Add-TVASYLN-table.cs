using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTVASYLNtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tvasyln",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    gpn_unt_pir_csf_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    asy_svc_alt_nmc_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    org_gpn_mnm_te = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpn_mnm_te = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_alt_nmc_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvasyln", x => new { x.org_cny_cd, x.dtn_cny_cd, x.asy_svc_typ_cd, x.svc_typ_cd, x.mvm_drc_cd, x.gpn_unt_pir_csf_cd, x.apv_sts_cd, x.rec_eff_stt_dt, x.asy_svc_alt_nmc_cd, x.org_gpn_mnm_te, x.dtn_gpn_mnm_te });
                });

            migrationBuilder.CreateTable(
                name: "tvasyln_stg",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    gpn_unt_pir_csf_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    asy_svc_alt_nmc_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    org_gpn_mnm_te = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpn_mnm_te = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_typ_alt_nmc_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tvasyln_stg", x => new { x.org_cny_cd, x.dtn_cny_cd, x.asy_svc_typ_cd, x.svc_typ_cd, x.mvm_drc_cd, x.gpn_unt_pir_csf_cd, x.apv_sts_cd, x.rec_eff_stt_dt, x.asy_svc_alt_nmc_cd, x.org_gpn_mnm_te, x.dtn_gpn_mnm_te });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tvasyln");

            migrationBuilder.DropTable(
                name: "tvasyln_stg");
        }
    }
}
