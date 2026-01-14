using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTSIARAVtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tsiarav",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    vol_rng_min_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    vol_rng_max_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    ms_unt_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dw_min_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    dw_max_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    pbh_max_wgt_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsiarav", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.pkg_cha_typ_cd, x.bil_ter_typ_cd, x.svc_fea_typ_cd, x.svc_typ_cd, x.mvm_drc_cd, x.cus_csf_typ_cd, x.wgt_ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });

            migrationBuilder.CreateTable(
                name: "tsiarav_stg",
                columns: table => new
                {
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    bil_ter_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    mvm_drc_cd = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    vol_rng_min_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    vol_rng_max_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    ms_unt_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dw_min_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    dw_max_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    pbh_max_wgt_qy = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tsiarav_stg", x => new { x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.pkg_cha_typ_cd, x.bil_ter_typ_cd, x.svc_fea_typ_cd, x.svc_typ_cd, x.mvm_drc_cd, x.cus_csf_typ_cd, x.wgt_ms_unt_typ_cd, x.apv_sts_cd, x.rec_eff_stt_dt });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tsiarav");

            migrationBuilder.DropTable(
                name: "tsiarav_stg");
        }
    }
}
