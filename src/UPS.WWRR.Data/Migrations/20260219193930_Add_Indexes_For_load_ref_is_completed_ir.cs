using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPS.WWRR.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indexes_For_load_ref_is_completed_ir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_tvdstbt_stg_is_completed_ir",
                table: "tvdstbt_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tvdstbt_stg_load_ref_te",
                table: "tvdstbt_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tvdstbt_load_ref_te",
                table: "tvdstbt",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tvasyln_stg_is_completed_ir",
                table: "tvasyln_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tvasyln_stg_load_ref_te",
                table: "tvasyln_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tvasyln_load_ref_te",
                table: "tvasyln",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcdgr_stg_is_completed_ir",
                table: "tsvcdgr_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcdgr_stg_load_ref_te",
                table: "tsvcdgr_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcdgr_load_ref_te",
                table: "tsvcdgr",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcdfl_stg_is_completed_ir",
                table: "tsvcdfl_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcdfl_stg_load_ref_te",
                table: "tsvcdfl_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcdfl_load_ref_te",
                table: "tsvcdfl",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcacp_stg_is_completed_ir",
                table: "tsvcacp_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcacp_stg_load_ref_te",
                table: "tsvcacp_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsvcacp_load_ref_te",
                
                table: "tsvcacp",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsubchg_stg_is_completed_ir",
                
                table: "tsubchg_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tsubchg_stg_load_ref_te",
                
                table: "tsubchg_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsubchg_load_ref_te",
                
                table: "tsubchg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsiarav_stg_is_completed_ir",
                
                table: "tsiarav_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tsiarav_stg_load_ref_te",
                
                table: "tsiarav_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsiarav_load_ref_te",
                
                table: "tsiarav",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsdrwsf_stg_is_completed_ir",
                
                table: "tsdrwsf_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tsdrwsf_stg_load_ref_te",
                
                table: "tsdrwsf_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tsdrwsf_load_ref_te",
                
                table: "tsdrwsf",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tratrul_stg_is_completed_ir",
                
                table: "tratrul_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tratrul_stg_load_ref_te",
                
                table: "tratrul_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tratrul_load_ref_te",
                
                table: "tratrul",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tfscidx_stg_is_completed_ir",
                
                table: "tfscidx_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tfscidx_stg_load_ref_te",
                
                table: "tfscidx_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tfscidx_load_ref_te",
                
                table: "tfscidx",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tdoznhd_stg_is_completed_ir1",
                
                table: "tdoznhd_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tdoznhd_stg_load_ref_te1",
                
                table: "tdoznhd_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tdoznhd_stg_is_completed_ir",
                
                table: "tdoznhd_new_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tdoznhd_stg_load_ref_te",
                
                table: "tdoznhd_new_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tdoznhd_load_ref_te",
                
                table: "tdoznhd",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tdozndt_stg_is_completed_ir1",
                
                table: "tdozndt_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tdozndt_stg_load_ref_te1",
                
                table: "tdozndt_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tdozndt_stg_is_completed_ir",
                
                table: "tdozndt_new_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tdozndt_stg_load_ref_te",
                
                table: "tdozndt_new_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tdozndt_load_ref_te",
                
                table: "tdozndt",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tarcldt_stg_is_completed_ir1",
                
                table: "tarcldt_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tarcldt_stg_load_ref_te1",
                
                table: "tarcldt_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tarcldt_stg_is_completed_ir",
                
                table: "tarcldt_new_stg",
                column: "is_completed_ir");

            migrationBuilder.CreateIndex(
                name: "idx_tarcldt_stg_load_ref_te",
                
                table: "tarcldt_new_stg",
                column: "load_ref_te");

            migrationBuilder.CreateIndex(
                name: "idx_tarcldt_load_ref_te",
                
                table: "tarcldt",
                column: "load_ref_te");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_tvdstbt_stg_is_completed_ir",
                
                table: "tvdstbt_stg");

            migrationBuilder.DropIndex(
                name: "idx_tvdstbt_stg_load_ref_te",
                
                table: "tvdstbt_stg");

            migrationBuilder.DropIndex(
                name: "idx_tvdstbt_load_ref_te",
                
                table: "tvdstbt");

            migrationBuilder.DropIndex(
                name: "idx_tvasyln_stg_is_completed_ir",
                
                table: "tvasyln_stg");

            migrationBuilder.DropIndex(
                name: "idx_tvasyln_stg_load_ref_te",
                
                table: "tvasyln_stg");

            migrationBuilder.DropIndex(
                name: "idx_tvasyln_load_ref_te",
                
                table: "tvasyln");

            migrationBuilder.DropIndex(
                name: "idx_tsvcdgr_stg_is_completed_ir",
                
                table: "tsvcdgr_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsvcdgr_stg_load_ref_te",
                
                table: "tsvcdgr_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsvcdgr_load_ref_te",
                
                table: "tsvcdgr");

            migrationBuilder.DropIndex(
                name: "idx_tsvcdfl_stg_is_completed_ir",
                
                table: "tsvcdfl_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsvcdfl_stg_load_ref_te",
                
                table: "tsvcdfl_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsvcdfl_load_ref_te",
                
                table: "tsvcdfl");

            migrationBuilder.DropIndex(
                name: "idx_tsvcacp_stg_is_completed_ir",
                
                table: "tsvcacp_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsvcacp_stg_load_ref_te",
                
                table: "tsvcacp_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsvcacp_load_ref_te",
                
                table: "tsvcacp");

            migrationBuilder.DropIndex(
                name: "idx_tsubchg_stg_is_completed_ir",
                
                table: "tsubchg_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsubchg_stg_load_ref_te",
                
                table: "tsubchg_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsubchg_load_ref_te",
                
                table: "tsubchg");

            migrationBuilder.DropIndex(
                name: "idx_tsiarav_stg_is_completed_ir",
                
                table: "tsiarav_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsiarav_stg_load_ref_te",
                
                table: "tsiarav_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsiarav_load_ref_te",
                
                table: "tsiarav");

            migrationBuilder.DropIndex(
                name: "idx_tsdrwsf_stg_is_completed_ir",
                
                table: "tsdrwsf_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsdrwsf_stg_load_ref_te",
                
                table: "tsdrwsf_stg");

            migrationBuilder.DropIndex(
                name: "idx_tsdrwsf_load_ref_te",
                
                table: "tsdrwsf");

            migrationBuilder.DropIndex(
                name: "idx_tratrul_stg_is_completed_ir",
                
                table: "tratrul_stg");

            migrationBuilder.DropIndex(
                name: "idx_tratrul_stg_load_ref_te",
                
                table: "tratrul_stg");

            migrationBuilder.DropIndex(
                name: "idx_tratrul_load_ref_te",
                
                table: "tratrul");

            migrationBuilder.DropIndex(
                name: "idx_tfscidx_stg_is_completed_ir",
                
                table: "tfscidx_stg");

            migrationBuilder.DropIndex(
                name: "idx_tfscidx_stg_load_ref_te",
                
                table: "tfscidx_stg");

            migrationBuilder.DropIndex(
                name: "idx_tfscidx_load_ref_te",
                
                table: "tfscidx");

            migrationBuilder.DropIndex(
                name: "idx_tdoznhd_stg_is_completed_ir1",
                
                table: "tdoznhd_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdoznhd_stg_load_ref_te1",
                
                table: "tdoznhd_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdoznhd_stg_is_completed_ir",
                
                table: "tdoznhd_new_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdoznhd_stg_load_ref_te",
                
                table: "tdoznhd_new_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdoznhd_load_ref_te",
                
                table: "tdoznhd");

            migrationBuilder.DropIndex(
                name: "idx_tdozndt_stg_is_completed_ir1",
                
                table: "tdozndt_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdozndt_stg_load_ref_te1",
                
                table: "tdozndt_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdozndt_stg_is_completed_ir",
                
                table: "tdozndt_new_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdozndt_stg_load_ref_te",
                
                table: "tdozndt_new_stg");

            migrationBuilder.DropIndex(
                name: "idx_tdozndt_load_ref_te",
                
                table: "tdozndt");

            migrationBuilder.DropIndex(
                name: "idx_tarcldt_stg_is_completed_ir1",
                
                table: "tarcldt_stg");

            migrationBuilder.DropIndex(
                name: "idx_tarcldt_stg_load_ref_te1",
                
                table: "tarcldt_stg");

            migrationBuilder.DropIndex(
                name: "idx_tarcldt_stg_is_completed_ir",
                
                table: "tarcldt_new_stg");

            migrationBuilder.DropIndex(
                name: "idx_tarcldt_stg_load_ref_te",
                
                table: "tarcldt_new_stg");

            migrationBuilder.DropIndex(
                name: "idx_tarcldt_load_ref_te",
                
                table: "tarcldt");
        }
    }
}
