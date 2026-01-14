using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtinftrhtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tinftrh",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    tm_prd_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_vlu_qy = table.Column<int>(type: "integer", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinftrh", x => new { x.cny_cd, x.asy_svc_typ_cd, x.tm_prd_typ_cd, x.dtr_cri_eff_dt, x.cus_csf_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "tinftrh_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    tm_prd_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_vlu_qy = table.Column<int>(type: "integer", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinftrh_stg", x => new { x.cny_cd, x.asy_svc_typ_cd, x.tm_prd_typ_cd, x.dtr_cri_eff_dt, x.cus_csf_typ_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tinftrh");

            migrationBuilder.DropTable(
                name: "tinftrh_stg");
        }
    }
}
