using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtcnyasy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tcnyasy",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    mvm_drc_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_chg_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    asy_svc_chg_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_chg_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ctl_vlu_1_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_2_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_3_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_4_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_5_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_6_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_dsc_te = table.Column<string>(type: "char(35)", maxLength: 35, nullable: false),
                    ra_typ_cd_ary_te = table.Column<string>(type: "char(20)", maxLength: 20, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcnyasy", x => new { x.cny_cd, x.mvm_drc_typ_cd, x.cus_csf_typ_cd, x.asy_svc_typ_cd, x.asy_svc_chg_eff_dt, x.asy_svc_chg_end_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tcnyasy_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    mvm_drc_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_chg_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    asy_svc_chg_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_chg_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ctl_vlu_1_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_2_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_3_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_4_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_5_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_6_te = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    ctl_vlu_dsc_te = table.Column<string>(type: "char(35)", maxLength: 35, nullable: false),
                    ra_typ_cd_ary_te = table.Column<string>(type: "char(20)", maxLength: 20, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcnyasy_stg", x => new { x.cny_cd, x.mvm_drc_typ_cd, x.cus_csf_typ_cd, x.asy_svc_typ_cd, x.asy_svc_chg_eff_dt, x.asy_svc_chg_end_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tcnyasy");

            migrationBuilder.DropTable(
                name: "tcnyasy_stg");
        }
    }
}
