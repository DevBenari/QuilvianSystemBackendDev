using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtabelkunjunganranap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingBedRanap",
                schema: "public",
                columns: table => new
                {
                    BookingBedRanapId = table.Column<Guid>(type: "uuid", nullable: false),
                    RanapId = table.Column<Guid>(type: "uuid", nullable: true),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
                    BedId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusBed = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_BookingBedRanap", x => x.BookingBedRanapId);
                });

            migrationBuilder.CreateTable(
                name: "KunjunganRanap",
                schema: "public",
                columns: table => new
                {
                    KunjunganRanapId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterDPJPId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipePembayaran = table.Column<string>(type: "text", nullable: true),
                    StatusRanap = table.Column<bool>(type: "boolean", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuratPengantarId = table.Column<Guid>(type: "uuid", nullable: true),
                    BedId = table.Column<Guid>(type: "uuid", nullable: true),
                    KeteranganSelesaiRanap = table.Column<string>(type: "text", nullable: true),
                    IsPrioritas = table.Column<bool>(type: "boolean", nullable: true),
                    IsCito = table.Column<bool>(type: "boolean", nullable: true),
                    TglAdministrasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KodeKunjungan = table.Column<string>(type: "text", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_KunjunganRanap", x => x.KunjunganRanapId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingBedRanap",
                schema: "public");

            migrationBuilder.DropTable(
                name: "KunjunganRanap",
                schema: "public");
        }
    }
}
