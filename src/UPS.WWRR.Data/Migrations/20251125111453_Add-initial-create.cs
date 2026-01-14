using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addinitialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_loads",
                columns: table => new
                {
                    seq_nr = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    load_table_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    load_ver_nr = table.Column<long>(type: "bigint", nullable: false),
                    load_sts_cd = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false),
                    file_location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    data_source = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false),
                    create_udt_ts = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    processed_udt_ts = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    log_file_location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    total_batch_nr = table.Column<int>(type: "integer", nullable: false),
                    batch_size = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_loads", x => x.seq_nr);
                });

            migrationBuilder.CreateTable(
                name: "taltccy",
                columns: table => new
                {
                    xpt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cnv_fr_ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cnv_to_ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    alt_ccy_xch_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    alt_ccy_xch_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    alt_ccy_xch_ra_qy = table.Column<decimal>(type: "numeric(15,9)", nullable: false),
                    alt_ccy_xch_or_qy = table.Column<decimal>(type: "numeric(15,9)", nullable: false),
                    alt_ccy_dmc_ccl_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    alt_ccy_rou_dmc_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    usr_nr = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "taltccy_stg",
                columns: table => new
                {
                    xpt_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    cnv_fr_ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    cnv_to_ccy_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    alt_ccy_xch_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    alt_ccy_xch_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    alt_ccy_xch_ra_qy = table.Column<decimal>(type: "numeric(15,9)", nullable: false),
                    alt_ccy_xch_or_qy = table.Column<decimal>(type: "numeric(15,9)", nullable: false),
                    alt_ccy_dmc_ccl_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    alt_ccy_rou_dmc_qy = table.Column<decimal>(type: "numeric(9,0)", nullable: false),
                    usr_nr = table.Column<string>(type: "char(8)", maxLength: 8, nullable: true, defaultValue: ""),
                    is_completed_ir = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    load_ref_te = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tarcldt",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    ara_csf_hdr_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    org_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    org_pol_div_2_na = table.Column<string>(type: "char(50)", maxLength: 50, nullable: false),
                    dtn_rng_lo_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_rng_hi_psl_cd = table.Column<string>(type: "char(9)", maxLength: 9, nullable: false),
                    dtn_pol_div_2_na = table.Column<string>(type: "char(50)", maxLength: 50, nullable: false),
                    ara_csf_dtl_rul_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    org_geo_ara_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    dtn_geo_ara_typ_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ara_csf_dtl_mnt_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    ra_chg_csf_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    org_pol_div_1_cd = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    dtn_pol_div_1_cd = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarcldt", x => new { x.zch_nr, x.ara_csf_hdr_stt_dt, x.ara_csf_dtl_end_dt, x.org_cny_cd, x.dtn_cny_cd, x.svc_typ_cd, x.ara_csf_dtl_rul_cd, x.org_pol_div_2_na, x.dtn_pol_div_2_na, x.org_rng_lo_psl_cd, x.org_rng_hi_psl_cd, x.dtn_rng_lo_psl_cd, x.dtn_rng_hi_psl_cd, x.bus_eny_acs_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "tarclhd",
                columns: table => new
                {
                    org_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    asy_svc_typ_cd = table.Column<string>(type: "char(3)", maxLength: 3, nullable: false),
                    dtn_cny_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    zch_nr = table.Column<int>(type: "integer", nullable: false),
                    ara_csf_hdr_stt_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    ara_csf_hdr_end_dt = table.Column<DateTime>(type: "Date", nullable: false),
                    bus_eny_acs_sts_cd = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    org_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    dtn_gpu_nr = table.Column<string>(type: "char(4)", maxLength: 4, nullable: false),
                    zch_lg_dsc_te = table.Column<string>(type: "char(100)", maxLength: 100, nullable: false),
                    zch_sht_dsc_te = table.Column<string>(type: "char(35)", maxLength: 35, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarclhd", x => new { x.org_cny_cd, x.svc_typ_cd, x.asy_svc_typ_cd, x.dtn_cny_cd, x.zch_nr, x.ara_csf_hdr_stt_dt, x.ara_csf_hdr_end_dt, x.bus_eny_acs_sts_cd });
                });

            migrationBuilder.CreateTable(
                name: "data_load_details",
                columns: table => new
                {
                    seq_nr = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_load_seq_nr = table.Column<long>(type: "bigint", nullable: false),
                    data_load_typ = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    err_ir = table.Column<short>(type: "smallint", nullable: false),
                    tm_prc_val = table.Column<int>(type: "integer", nullable: false),
                    tm_prd_typ_cd = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    rec_ins_nr = table.Column<int>(type: "integer", nullable: false),
                    rec_upd_nr = table.Column<int>(type: "integer", nullable: false),
                    rec_del_nr = table.Column<int>(type: "integer", nullable: false),
                    batch_nr = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_load_details", x => x.seq_nr);
                    table.ForeignKey(
                        name: "FK_data_load_details_data_loads_data_load_seq_nr",
                        column: x => x.data_load_seq_nr,
                        principalTable: "data_loads",
                        principalColumn: "seq_nr",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_load_errors",
                columns: table => new
                {
                    seq_nr = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_load_detail_seq_nr = table.Column<long>(type: "bigint", nullable: false),
                    err_cd = table.Column<int>(type: "integer", nullable: false),
                    err_sp_na = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    err_msg_te = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    create_udt_ts = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_load_errors", x => x.seq_nr);
                    table.ForeignKey(
                        name: "FK_data_load_errors_data_load_details_data_load_detail_seq_nr",
                        column: x => x.data_load_detail_seq_nr,
                        principalTable: "data_load_details",
                        principalColumn: "seq_nr",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_load_exceptions",
                columns: table => new
                {
                    seq_nr = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_load_detail_seq_nr = table.Column<long>(type: "bigint", nullable: false),
                    table_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    table_key_str = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    err_fld_str = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    err_fld_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    create_udt_ts = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_load_exceptions", x => x.seq_nr);
                    table.ForeignKey(
                        name: "FK_data_load_exceptions_data_load_details_data_load_detail_seq~",
                        column: x => x.data_load_detail_seq_nr,
                        principalTable: "data_load_details",
                        principalColumn: "seq_nr",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_load_details_data_load_seq_nr",
                table: "data_load_details",
                column: "data_load_seq_nr");

            migrationBuilder.CreateIndex(
                name: "IX_data_load_errors_data_load_detail_seq_nr",
                table: "data_load_errors",
                column: "data_load_detail_seq_nr");

            migrationBuilder.CreateIndex(
                name: "IX_data_load_exceptions_data_load_detail_seq_nr",
                table: "data_load_exceptions",
                column: "data_load_detail_seq_nr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_load_errors");

            migrationBuilder.DropTable(
                name: "data_load_exceptions");

            migrationBuilder.DropTable(
                name: "taltccy");

            migrationBuilder.DropTable(
                name: "taltccy_stg");

            migrationBuilder.DropTable(
                name: "tarcldt");

            migrationBuilder.DropTable(
                name: "tarclhd");

            migrationBuilder.DropTable(
                name: "data_load_details");

            migrationBuilder.DropTable(
                name: "data_loads");
        }
    }
}
