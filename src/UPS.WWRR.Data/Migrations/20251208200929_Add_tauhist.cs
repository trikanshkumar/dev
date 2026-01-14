using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_tauhist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tauhist",
                columns: table => new
                {
                    aud_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    aud_acn_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    aud_trs_nr = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    aud_trs_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_crt_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    rec_crt_usr_nr = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    dat_tms_snd_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    aud_rmk_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    aud_hdr_udt_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    aud_dtl_udt_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    aud_rpt_rsl_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tauhist", x => new { x.aud_typ_cd, x.aud_acn_cd, x.aud_trs_nr, x.aud_trs_typ_cd, x.rec_crt_ts });
                });

            migrationBuilder.CreateTable(
                name: "tauhist_stg",
                columns: table => new
                {
                    aud_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    aud_acn_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    aud_trs_nr = table.Column<string>(type: "char(10)", maxLength: 10, nullable: false),
                    aud_trs_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    rec_crt_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    rec_crt_usr_nr = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    dat_tms_snd_ts = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: false),
                    aud_rmk_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    aud_hdr_udt_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    aud_dtl_udt_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    aud_rpt_rsl_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tauhist_stg", x => new { x.aud_typ_cd, x.aud_acn_cd, x.aud_trs_nr, x.aud_trs_typ_cd, x.rec_crt_ts });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tauhist");

            migrationBuilder.DropTable(
                name: "tauhist_stg");
        }
    }
}
