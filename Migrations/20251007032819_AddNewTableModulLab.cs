using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddNewTableModulLab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabBookingDetails",
                columns: table => new
                {
                    DetailBookingLabId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
                    KategoriPatologiAnatomi = table.Column<string>(type: "text", nullable: true),
                    JenisSpecimen = table.Column<string>(type: "text", nullable: true),
                    LokasiSpecimen = table.Column<string>(type: "text", nullable: true),
                    KeteranganKlinik = table.Column<string>(type: "text", nullable: true),
                    PerkiraanPenyakit = table.Column<string>(type: "text", nullable: true),
                    PenyakitSebelumnya = table.Column<string>(type: "text", nullable: true),
                    PenggunaanFiksasi = table.Column<string>(type: "text", nullable: true),
                    JenisPemeriksaanGC = table.Column<string>(type: "text", nullable: true),
                    JenisGC = table.Column<string>(type: "text", nullable: true),
                    BahanNonGC = table.Column<string>(type: "text", nullable: true),
                    BahanMicrobiologi = table.Column<string>(type: "text", nullable: true),
                    MasaHaidTerakhir = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LabBookingDetails", x => x.DetailBookingLabId);
                });

            migrationBuilder.CreateTable(
                name: "LabBookings",
                columns: table => new
                {
                    BookingLabId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPenyerahanSampling = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglBooking = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailIcdId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsCito = table.Column<bool>(type: "boolean", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabBookings", x => x.BookingLabId);
                });

            migrationBuilder.CreateTable(
                name: "LabKategoriPemeriksaans",
                columns: table => new
                {
                    KategoriPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKategori = table.Column<string>(type: "text", nullable: true),
                    KodeKategori = table.Column<string>(type: "text", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabKategoriPemeriksaans", x => x.KategoriPemeriksaanId);
                });

            migrationBuilder.CreateTable(
                name: "LabPemeriksaans",
                columns: table => new
                {
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaPemeriksaan = table.Column<string>(type: "text", nullable: true),
                    KodePemeriksaan = table.Column<string>(type: "text", nullable: true),
                    HargaPemeriksaan = table.Column<decimal>(type: "numeric", nullable: true),
                    KategoriPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabPemeriksaans", x => x.PemeriksaanLabId);
                });

            migrationBuilder.CreateTable(
                name: "MstLab",
                schema: "public",
                columns: table => new
                {
                    LabId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaLab = table.Column<string>(type: "text", nullable: true),
                    KodeKategori = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstLab", x => x.LabId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabBookingDetails");

            migrationBuilder.DropTable(
                name: "LabBookings");

            migrationBuilder.DropTable(
                name: "LabKategoriPemeriksaans");

            migrationBuilder.DropTable(
                name: "LabPemeriksaans");

            migrationBuilder.DropTable(
                name: "MstLab",
                schema: "public");
        }
    }
}
