using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKajianPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PekerjaanOrangTua",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "PekerjaanWali");

            migrationBuilder.RenameColumn(
                name: "NoIdentitasDarurat",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NamaKontakDarurat");

            migrationBuilder.AddColumn<string>(
                name: "HubunganPasien",
                schema: "public",
                table: "PdfPasienBaru",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KajianPasien",
                schema: "public",
                columns: table => new
                {
                    KajianPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    KeadaanUmum = table.Column<string>(type: "text", nullable: true),
                    KeadaanKulit = table.Column<string>(type: "text", nullable: true),
                    KeadaanKepalaLeher = table.Column<string>(type: "text", nullable: true),
                    KeadaanDada = table.Column<string>(type: "text", nullable: true),
                    KeadaanJantung = table.Column<string>(type: "text", nullable: true),
                    KeadaanParuParu = table.Column<string>(type: "text", nullable: true),
                    KeadaanAbdomen = table.Column<string>(type: "text", nullable: true),
                    KeadaanGenitalia = table.Column<string>(type: "text", nullable: true),
                    KeadaanAnggotaGerak = table.Column<string>(type: "text", nullable: true),
                    KeadaanLainnya = table.Column<string>(type: "text", nullable: true),
                    StatusLokalis = table.Column<string>(type: "text", nullable: true),
                    PemeriksaanPenunjang = table.Column<string>(type: "text", nullable: true),
                    DiagnosaSaatIni = table.Column<string>(type: "text", nullable: true),
                    DiagnosaBanding = table.Column<string>(type: "text", nullable: true),
                    DaftarMasalah = table.Column<string>(type: "text", nullable: true),
                    Program = table.Column<string>(type: "text", nullable: true),
                    Terapi = table.Column<string>(type: "text", nullable: true),
                    Edukasi = table.Column<bool>(type: "boolean", nullable: true),
                    EdukasiKepada = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglKajian = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_KajianPasien", x => x.KajianPasienId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KajianPasien",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "HubunganPasien",
                schema: "public",
                table: "PdfPasienBaru");

            migrationBuilder.RenameColumn(
                name: "PekerjaanWali",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "PekerjaanOrangTua");

            migrationBuilder.RenameColumn(
                name: "NamaKontakDarurat",
                schema: "public",
                table: "PdfPasienBaru",
                newName: "NoIdentitasDarurat");
        }
    }
}
