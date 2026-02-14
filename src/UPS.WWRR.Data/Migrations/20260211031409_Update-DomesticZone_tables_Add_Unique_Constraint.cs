using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDomesticZone_tables_Add_Unique_Constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_domzchartsts_stg_zch_nr_dom_zn_hdr_stt_dt_dom_zn_hdr_end_dt",
                schema: "a886aa_ao",
                table: "domzchartsts_stg",
                columns: new[] { "zch_nr", "dom_zn_hdr_stt_dt", "dom_zn_hdr_end_dt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_domzchartsts_zch_nr_dom_zn_hdr_stt_dt_dom_zn_hdr_end_dt",
                schema: "a886aa_ao",
                table: "domzchartsts",
                columns: new[] { "zch_nr", "dom_zn_hdr_stt_dt", "dom_zn_hdr_end_dt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_domzchartsts_stg_zch_nr_dom_zn_hdr_stt_dt_dom_zn_hdr_end_dt",
                schema: "a886aa_ao",
                table: "domzchartsts_stg");

            migrationBuilder.DropIndex(
                name: "IX_domzchartsts_zch_nr_dom_zn_hdr_stt_dt_dom_zn_hdr_end_dt",
                schema: "a886aa_ao",
                table: "domzchartsts");
        }
    }
}
