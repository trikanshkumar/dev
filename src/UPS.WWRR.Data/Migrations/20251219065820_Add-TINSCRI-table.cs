using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTINSCRItable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tinscri",
                columns: table => new
                {
                    cus_cls_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ins_cri_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ins_bss_a = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    min_ins_avail = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ins_min_dcl_vlu = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ins_min_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    ins_max_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    ins_cri_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    isn_max_dcl_vlu_a = table.Column<decimal>(type: "numeric(17,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinscri", x => new { x.cus_cls_typ_cd, x.cny_cd, x.asy_svc_typ_cd, x.svc_fea_typ_cd, x.wgt_ms_unt_typ_cd, x.ccy_cd, x.ins_cri_eff_stt_dt, x.svc_typ_cd, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tinscri_stg",
                columns: table => new
                {
                    cus_cls_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    wgt_ms_unt_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    ins_cri_eff_stt_dt = table.Column<DateTime>(type: "date", nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ins_bss_a = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    min_ins_avail = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ins_min_dcl_vlu = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ins_min_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    ins_max_wgt_qy = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    ins_cri_eff_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    isn_max_dcl_vlu_a = table.Column<decimal>(type: "numeric(17,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinscri_stg", x => new { x.cus_cls_typ_cd, x.cny_cd, x.asy_svc_typ_cd, x.svc_fea_typ_cd, x.wgt_ms_unt_typ_cd, x.ccy_cd, x.ins_cri_eff_stt_dt, x.svc_typ_cd, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tinscri");

            migrationBuilder.DropTable(
                name: "tinscri_stg");
        }
    }
}
