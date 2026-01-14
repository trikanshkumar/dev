using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_tspmycd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tspmycd",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    cny_ra_sei_rl_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    spm_lin_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    chg_ccl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    spm_chg_rfd_elg_ir = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    spm_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    inf_xmp_ir = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tspmycd", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.cny_ra_sei_rl_cd, x.svc_typ_cd, x.spm_lin_cd, x.dtr_cri_sts_cd, x.dtr_cri_eff_dt });
                });

            migrationBuilder.CreateTable(
                name: "tspmycd_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    cny_ra_sei_rl_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    spm_lin_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    chg_ccl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    spm_chg_rfd_elg_ir = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    spm_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    inf_xmp_ir = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tspmycd_stg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.cny_ra_sei_rl_cd, x.svc_typ_cd, x.spm_lin_cd, x.dtr_cri_sts_cd, x.dtr_cri_eff_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tspmycd");

            migrationBuilder.DropTable(
                name: "tspmycd_stg");
        }
    }
}
