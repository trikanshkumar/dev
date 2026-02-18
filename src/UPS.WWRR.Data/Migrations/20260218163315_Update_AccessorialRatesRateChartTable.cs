using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_AccessorialRatesRateChartTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_chartsts_stg_svc_ra_cht_nr_svc_ra_cht_eff_dt_svc_ra_cht_end~",
                table: "chartsts_stg",
                columns: new[] { "svc_ra_cht_nr", "svc_ra_cht_eff_dt", "svc_ra_cht_end_dt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_chartsts_stg_svc_ra_cht_nr_svc_ra_cht_eff_dt_svc_ra_cht_end~",
                table: "chartsts_stg");
        }
    }
}
