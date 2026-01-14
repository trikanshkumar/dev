using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addtpslbur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tpslbur",
                columns: table => new
                {
                    brl_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    rng_low_psl_cd = table.Column<string>(type: "char(12)", maxLength: 12, nullable: false),
                    rng_hi_psl_cd = table.Column<string>(type: "char(12)", maxLength: 12, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    rec_ins_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tpslbur", x => new { x.brl_cd, x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.rng_low_psl_cd, x.rng_hi_psl_cd, x.rec_eff_stt_dt, x.apv_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tpslbur_stg",
                columns: table => new
                {
                    brl_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    gpn_xpt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    gpn_ipt_cny_cd = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    rng_low_psl_cd = table.Column<string>(type: "char(12)", maxLength: 12, nullable: false),
                    rng_hi_psl_cd = table.Column<string>(type: "char(12)", maxLength: 12, nullable: false),
                    rec_eff_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    apv_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_eff_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    pol_div_2_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    rec_ins_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tpslbur_stg", x => new { x.brl_cd, x.gpn_xpt_cny_cd, x.gpn_ipt_cny_cd, x.rng_low_psl_cd, x.rng_hi_psl_cd, x.rec_eff_stt_dt, x.apv_sts_cd });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tpslbur");

            migrationBuilder.DropTable(
                name: "tpslbur_stg");
        }
    }
}
