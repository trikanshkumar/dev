using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtmincri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tmincri",
                columns: table => new
                {
                    gpu_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svm_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_typ_cd = table.Column<short>(type: "smallint", nullable: false),
                    dtr_cri_unt_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cmy_cls_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dtr_cri_vlu_te = table.Column<decimal>(type: "numeric(11,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tmincri", x => new { x.gpu_xpt_cny_cd, x.svc_fea_typ_cd, x.pkg_cha_typ_cd, x.svm_typ_cd, x.dtr_cri_typ_cd, x.dtr_cri_unt_typ_cd, x.cmy_cls_cd, x.del_zn_nr, x.dtr_cri_eff_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tmincri_stg",
                columns: table => new
                {
                    gpu_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    svc_fea_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    pkg_cha_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    svm_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_typ_cd = table.Column<short>(type: "smallint", nullable: false),
                    dtr_cri_unt_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cmy_cls_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    del_zn_nr = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "date", nullable: false),
                    dtr_cri_vlu_te = table.Column<decimal>(type: "numeric(11,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tmincri_stg", x => new { x.gpu_xpt_cny_cd, x.svc_fea_typ_cd, x.pkg_cha_typ_cd, x.svm_typ_cd, x.dtr_cri_typ_cd, x.dtr_cri_unt_typ_cd, x.cmy_cls_cd, x.del_zn_nr, x.dtr_cri_eff_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tmincri");

            migrationBuilder.DropTable(
                name: "tmincri_stg");
        }
    }
}
