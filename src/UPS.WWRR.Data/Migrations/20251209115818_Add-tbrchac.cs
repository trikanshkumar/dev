using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtbrchac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbrchac",
                columns: table => new
                {
                    brl_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ms_unt_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cha_vlu_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbrchac", x => new { x.brl_typ_cd, x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.pkg_cha_typ_cd, x.bil_ter_typ_cd, x.svc_fea_typ_cd, x.svc_typ_cd, x.mvm_drc_cd, x.cus_csf_typ_cd, x.dtr_cha_typ_cd, x.ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tbrchac_stg",
                columns: table => new
                {
                    brl_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ms_unt_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cha_vlu_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbrchac_stg", x => new { x.brl_typ_cd, x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.pkg_cha_typ_cd, x.bil_ter_typ_cd, x.svc_fea_typ_cd, x.svc_typ_cd, x.mvm_drc_cd, x.cus_csf_typ_cd, x.dtr_cha_typ_cd, x.ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbrchac");

            migrationBuilder.DropTable(
                name: "tbrchac_stg");
        }
    }
}
