using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_tinfrat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tinfrat",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    tm_prd_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_low_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_hi_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_ra_a = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinfrat", x => new { x.cny_cd, x.asy_svc_typ_cd, x.tm_prd_typ_cd, x.ccy_cd, x.dtr_cri_eff_dt, x.dtr_cri_low_rng_te, x.cus_csf_typ_cd });
                });

            migrationBuilder.CreateTable(
                name: "tinfrat_stg",
                columns: table => new
                {
                    cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    tm_prd_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtr_cri_eff_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    dtr_cri_low_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    cus_csf_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_hi_rng_te = table.Column<decimal>(type: "numeric(13,2)", nullable: false),
                    dtr_cri_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    ccl_mth_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtr_cri_ra_a = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tinfrat_stg", x => new { x.cny_cd, x.asy_svc_typ_cd, x.tm_prd_typ_cd, x.ccy_cd, x.dtr_cri_eff_dt, x.dtr_cri_low_rng_te, x.cus_csf_typ_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tinfrat");

            migrationBuilder.DropTable(
                name: "tinfrat_stg");
        }
    }
}
