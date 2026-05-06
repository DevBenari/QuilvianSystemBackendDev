using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class IndexingPaketLayanan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================
            // PaketLayananAsuransis
            // =========================================
            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananAsuransis_PaketLayananId",
                table: "PaketLayananAsuransis",
                column: "PaketLayananId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananAsuransis_AsuransiId",
                table: "PaketLayananAsuransis",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananAsuransis_CorporateId",
                table: "PaketLayananAsuransis",
                column: "CorporateId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananAsuransis_PaketLayananId_AsuransiId",
                table: "PaketLayananAsuransis",
                columns: new[] { "PaketLayananId", "AsuransiId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananAsuransis_PaketLayananId_IsDelete",
                table: "PaketLayananAsuransis",
                columns: new[] { "PaketLayananId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananAsuransis_AsuransiId_IsDelete",
                table: "PaketLayananAsuransis",
                columns: new[] { "AsuransiId", "IsDelete" });

            // =========================================
            // PaketLayananDetails
            // =========================================
            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDetails_DetailPaketId",
                table: "PaketLayananDetails",
                column: "DetailPaketId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDetails_LayananId",
                table: "PaketLayananDetails",
                column: "LayananId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDetails_DetailPaketId_LayananId",
                table: "PaketLayananDetails",
                columns: new[] { "DetailPaketId", "LayananId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDetails_DetailPaketId_IsDelete",
                table: "PaketLayananDetails",
                columns: new[] { "DetailPaketId", "IsDelete" });

            // =========================================
            // PaketLayananDiskons
            // =========================================
            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDiskons_PaketLayananId",
                table: "PaketLayananDiskons",
                column: "PaketLayananId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDiskons_PaketLayananAsuransiId",
                table: "PaketLayananDiskons",
                column: "PaketLayananAsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDiskons_DiskonPercentageId",
                table: "PaketLayananDiskons",
                column: "DiskonPercentageId");

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDiskons_PaketLayananId_PaketLayananAsuransiId",
                table: "PaketLayananDiskons",
                columns: new[] { "PaketLayananId", "PaketLayananAsuransiId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDiskons_PaketLayananAsuransiId_IsDelete",
                table: "PaketLayananDiskons",
                columns: new[] { "PaketLayananAsuransiId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PaketLayananDiskons_PaketLayananId_IsDelete",
                table: "PaketLayananDiskons",
                columns: new[] { "PaketLayananId", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =========================================
            // PaketLayananDiskons
            // =========================================
            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDiskons_PaketLayananId_IsDelete",
                table: "PaketLayananDiskons");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDiskons_PaketLayananAsuransiId_IsDelete",
                table: "PaketLayananDiskons");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDiskons_PaketLayananId_PaketLayananAsuransiId",
                table: "PaketLayananDiskons");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDiskons_DiskonPercentageId",
                table: "PaketLayananDiskons");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDiskons_PaketLayananAsuransiId",
                table: "PaketLayananDiskons");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDiskons_PaketLayananId",
                table: "PaketLayananDiskons");

            // =========================================
            // PaketLayananDetails
            // =========================================
            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDetails_DetailPaketId_IsDelete",
                table: "PaketLayananDetails");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDetails_DetailPaketId_LayananId",
                table: "PaketLayananDetails");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDetails_LayananId",
                table: "PaketLayananDetails");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananDetails_DetailPaketId",
                table: "PaketLayananDetails");

            // =========================================
            // PaketLayananAsuransis
            // =========================================
            migrationBuilder.DropIndex(
                name: "IX_PaketLayananAsuransis_AsuransiId_IsDelete",
                table: "PaketLayananAsuransis");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananAsuransis_PaketLayananId_IsDelete",
                table: "PaketLayananAsuransis");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananAsuransis_PaketLayananId_AsuransiId",
                table: "PaketLayananAsuransis");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananAsuransis_CorporateId",
                table: "PaketLayananAsuransis");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananAsuransis_AsuransiId",
                table: "PaketLayananAsuransis");

            migrationBuilder.DropIndex(
                name: "IX_PaketLayananAsuransis_PaketLayananId",
                table: "PaketLayananAsuransis");
        }
    }
}
