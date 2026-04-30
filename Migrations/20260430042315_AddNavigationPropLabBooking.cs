using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNavigationPropLabBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstDokter_DokterId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LabBookings",
                table: "LabBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LabBookingDetails",
                table: "LabBookingDetails");

            migrationBuilder.RenameTable(
                name: "LabBookings",
                newName: "LabBooking",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "LabBookingDetails",
                newName: "LabBookingDetail",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LabBooking",
                schema: "public",
                table: "LabBooking",
                column: "BookingLabId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LabBookingDetail",
                schema: "public",
                table: "LabBookingDetail",
                column: "DetailBookingLabId");

            migrationBuilder.CreateTable(
                name: "LabBookingDetailSpecimenJenis",
                schema: "public",
                columns: table => new
                {
                    LabBookingDetailSpecimenJenisId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailBookingLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpecimenJenisId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabBookingDetailSpecimenJenis", x => x.LabBookingDetailSpecimenJenisId);
                });

            migrationBuilder.CreateTable(
                name: "LabBookingDetailSpecimenMethod",
                schema: "public",
                columns: table => new
                {
                    LabBookingDetailSpecimenMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailBookingLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpecimenMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabBookingDetailSpecimenMethod", x => x.LabBookingDetailSpecimenMethodId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_AsuransiId",
                schema: "public",
                table: "LabBooking",
                column: "AsuransiId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_DokterId",
                schema: "public",
                table: "LabBooking",
                column: "DokterId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_DokterId_CreateDateTime",
                schema: "public",
                table: "LabBooking",
                columns: new[] { "DokterId", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_DokterKonsulenId",
                schema: "public",
                table: "LabBooking",
                column: "DokterKonsulenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_IsDelete_CreateDateTime",
                schema: "public",
                table: "LabBooking",
                columns: new[] { "IsDelete", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_KelasId",
                schema: "public",
                table: "LabBooking",
                column: "KelasId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_KunjunganId",
                schema: "public",
                table: "LabBooking",
                column: "KunjunganId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_KunjunganId_CreateDateTime",
                schema: "public",
                table: "LabBooking",
                columns: new[] { "KunjunganId", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_NoLab",
                schema: "public",
                table: "LabBooking",
                column: "NoLab");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_NoOrder",
                schema: "public",
                table: "LabBooking",
                column: "NoOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_NoPA",
                schema: "public",
                table: "LabBooking",
                column: "NoPA");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_PasienId",
                schema: "public",
                table: "LabBooking",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_PasienId_CreateDateTime",
                schema: "public",
                table: "LabBooking",
                columns: new[] { "PasienId", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_StatusPembayaran",
                schema: "public",
                table: "LabBooking",
                column: "StatusPembayaran");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_StatusPemeriksaan",
                schema: "public",
                table: "LabBooking",
                column: "StatusPemeriksaan");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_TerapisId",
                schema: "public",
                table: "LabBooking",
                column: "TerapisId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_TglBooking",
                schema: "public",
                table: "LabBooking",
                column: "TglBooking");

            migrationBuilder.CreateIndex(
                name: "IX_LabBooking_TglPemeriksaan",
                schema: "public",
                table: "LabBooking",
                column: "TglPemeriksaan");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_AsalSpecimenId",
                schema: "public",
                table: "LabBookingDetail",
                column: "AsalSpecimenId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_BookingLabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "BookingLabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_BookingLabId_CreateDateTime",
                schema: "public",
                table: "LabBookingDetail",
                columns: new[] { "BookingLabId", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_IsDelete_CreateDateTime",
                schema: "public",
                table: "LabBookingDetail",
                columns: new[] { "IsDelete", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_LabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_NoOrder",
                schema: "public",
                table: "LabBookingDetail",
                column: "NoOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_PasienId",
                schema: "public",
                table: "LabBookingDetail",
                column: "PasienId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_PemeriksaanLabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "PemeriksaanLabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_StatusPemeriksaan",
                schema: "public",
                table: "LabBookingDetail",
                column: "StatusPemeriksaan");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_StatusVerifikasi",
                schema: "public",
                table: "LabBookingDetail",
                column: "StatusVerifikasi");

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_TanggalSelesai",
                schema: "public",
                table: "LabBookingDetail",
                column: "TanggalSelesai");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstAsuransi_AsuransiId",
                schema: "public",
                table: "LabBooking",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstDokter_DokterId",
                schema: "public",
                table: "LabBooking",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstDokter_DokterKonsulenId",
                schema: "public",
                table: "LabBooking",
                column: "DokterKonsulenId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstKelas_KelasId",
                schema: "public",
                table: "LabBooking",
                column: "KelasId",
                principalSchema: "public",
                principalTable: "MstKelas",
                principalColumn: "KelasId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstKunjungan_KunjunganId",
                schema: "public",
                table: "LabBooking",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBooking_MstPasien_PasienId",
                schema: "public",
                table: "LabBooking",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_LabBooking_BookingLabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "BookingLabId",
                principalSchema: "public",
                principalTable: "LabBooking",
                principalColumn: "BookingLabId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstAsalSpecimen_AsalSpecimenId",
                schema: "public",
                table: "LabBookingDetail",
                column: "AsalSpecimenId",
                principalSchema: "public",
                principalTable: "MstSpecimenAsal",
                principalColumn: "SpecimenAsalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstLab_LabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstPasien_PasienId",
                schema: "public",
                table: "LabBookingDetail",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstPemeriksaanLab_PemeriksaanLabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "PemeriksaanLabId",
                principalTable: "LabPemeriksaans",
                principalColumn: "PemeriksaanLabId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiExcessId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienExcessId",
                principalSchema: "public",
                principalTable: "MstAsuransiPasien",
                principalColumn: "AsuransiPasienId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienId",
                principalSchema: "public",
                principalTable: "MstAsuransiPasien",
                principalColumn: "AsuransiPasienId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstDokter_DokterId",
                schema: "public",
                table: "MstKunjungan",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstPendaftaranPasienBaru_PasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstKunjungan",
                column: "PoliklinikId",
                principalSchema: "public",
                principalTable: "MstPoliklinik",
                principalColumn: "PoliklinikId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstAsuransi_AsuransiId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstDokter_DokterId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstDokter_DokterKonsulenId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstKelas_KelasId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstKunjungan_KunjunganId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBooking_MstPasien_PasienId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_LabBooking_BookingLabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstAsalSpecimen_AsalSpecimenId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstLab_LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstPasien_PasienId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstPemeriksaanLab_PemeriksaanLabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstDokter_DokterId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstPendaftaranPasienBaru_PasienId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKunjungan_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropTable(
                name: "LabBookingDetailSpecimenJenis",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabBookingDetailSpecimenMethod",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LabBookingDetail",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_AsalSpecimenId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_BookingLabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_BookingLabId_CreateDateTime",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_IsDelete_CreateDateTime",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_NoOrder",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_PasienId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_PemeriksaanLabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_StatusPemeriksaan",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_StatusVerifikasi",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_TanggalSelesai",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LabBooking",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_AsuransiId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_DokterId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_DokterId_CreateDateTime",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_DokterKonsulenId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_IsDelete_CreateDateTime",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_KelasId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_KunjunganId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_KunjunganId_CreateDateTime",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_NoLab",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_NoOrder",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_NoPA",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_PasienId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_PasienId_CreateDateTime",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_StatusPembayaran",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_StatusPemeriksaan",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_TerapisId",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_TglBooking",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.DropIndex(
                name: "IX_LabBooking_TglPemeriksaan",
                schema: "public",
                table: "LabBooking");

            migrationBuilder.RenameTable(
                name: "LabBookingDetail",
                schema: "public",
                newName: "LabBookingDetails");

            migrationBuilder.RenameTable(
                name: "LabBooking",
                schema: "public",
                newName: "LabBookings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LabBookingDetails",
                table: "LabBookingDetails",
                column: "DetailBookingLabId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LabBookings",
                table: "LabBookings",
                column: "BookingLabId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiExcessId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransi_AsuransiId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiId",
                principalSchema: "public",
                principalTable: "MstAsuransi",
                principalColumn: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienExcessId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienExcessId",
                principalSchema: "public",
                principalTable: "MstAsuransiPasien",
                principalColumn: "AsuransiPasienId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstAsuransiPasien_AsuransiPasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "AsuransiPasienId",
                principalSchema: "public",
                principalTable: "MstAsuransiPasien",
                principalColumn: "AsuransiPasienId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstDokter_DokterId",
                schema: "public",
                table: "MstKunjungan",
                column: "DokterId",
                principalSchema: "public",
                principalTable: "MstDokter",
                principalColumn: "DokterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_MstPoliklinik_PoliklinikId",
                schema: "public",
                table: "MstKunjungan",
                column: "PoliklinikId",
                principalSchema: "public",
                principalTable: "MstPoliklinik",
                principalColumn: "PoliklinikId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKunjungan_PdfPasienBaru_PasienId",
                schema: "public",
                table: "MstKunjungan",
                column: "PasienId",
                principalSchema: "public",
                principalTable: "PdfPasienBaru",
                principalColumn: "PendaftaranPasienBaruId");
        }
    }
}
