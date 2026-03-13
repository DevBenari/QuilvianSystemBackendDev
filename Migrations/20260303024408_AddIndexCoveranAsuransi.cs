using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddIndexCoveranAsuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================
            // KamarAsuransi (AsuransiId + KamarId) - active (not deleted)
            // =========================
            migrationBuilder.CreateIndex(
                name: "ix_kamar_asuransi_asuransi_kamar_active",
                table: "KamarAsuransi",
                columns: new[] { "AsuransiId", "KamarId" },
                filter: "\"IsDelete\" IS NULL OR \"IsDelete\" = FALSE"
            );

            // =========================
            // MstTindakanAsuransi (AsuransiId + TindakanId) - active
            // =========================
            migrationBuilder.CreateIndex(
                name: "ix_mst_tindakan_asuransi_asuransi_tindakan_active",
                table: "MstTindakanAsuransi",
                columns: new[] { "AsuransiId", "TindakanId" },
                filter: "\"IsDelete\" IS NULL OR \"IsDelete\" = FALSE"
            );

            migrationBuilder.CreateIndex(
                name: "ix_mst_pemeriksaan_asuransi_asuransi_pemeriksaan_active",
                table: "MstPemeriksaanAsuransi",
                columns: new[] { "AsuransiId", "PemeriksaanLabId" },
                filter: "\"IsDelete\" IS NULL OR \"IsDelete\" = FALSE"
            );

            migrationBuilder.CreateIndex(
                name: "ix_obat_asuransi_asuransi_obat_active",
                table: "MstObatAsuransi",
                columns: new[] { "AsuransiId", "ObatId" },
                filter: "\"IsDelete\" IS NULL OR \"IsDelete\" = FALSE"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop kebalikan dari Up()
            migrationBuilder.DropIndex(
                name: "ix_kamar_asuransi_asuransi_kamar_active",
                table: "KamarAsuransi"
            );

            migrationBuilder.DropIndex(
                name: "ix_mst_tindakan_asuransi_asuransi_tindakan_active",
                table: "MstTindakanAsuransi"
            );

            migrationBuilder.DropIndex(
                name: "ix_mst_pemeriksaan_asuransi_asuransi_pemeriksaan_active",
                table: "MstPemeriksaanAsuransi"
            );

            migrationBuilder.DropIndex(
                name: "ix_obat_asuransi_asuransi_obat_active",
                table: "MstObatAsuransi"
            );
        }
    }
}
