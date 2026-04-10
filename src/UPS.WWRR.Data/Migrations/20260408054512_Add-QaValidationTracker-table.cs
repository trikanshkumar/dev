using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQaValidationTrackertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qa_validation_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    load_ver = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: false),
                    load_table_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    validation_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    validation_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    validation_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    expected_value = table.Column<string>(type: "text", nullable: true),
                    actual_value = table.Column<string>(type: "text", nullable: true),
                    difference_count = table.Column<int>(type: "integer", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qa_validation_results", x => x.id);
                    table.CheckConstraint("chk_results_validation_status", "validation_status IN ('PASS','FAIL')");
                });

            migrationBuilder.CreateTable(
                name: "qa_validation_tracker",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    load_ver = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: false),
                    load_table_na = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    validation_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    run_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    start_ts = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    end_ts = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_sec = table.Column<int>(type: "integer", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qa_validation_tracker", x => x.id);
                    table.CheckConstraint("chk_tracker_validation_status", "validation_status IN ('IN_PROGRESS','COMPLETED','FAILED')");
                });

            migrationBuilder.CreateIndex(
                name: "uq_exec",
                table: "qa_validation_tracker",
                columns: new[] { "load_ver", "load_table_na" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qa_validation_results");

            migrationBuilder.DropTable(
                name: "qa_validation_tracker");
        }
    }
}
