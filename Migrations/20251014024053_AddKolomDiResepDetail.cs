using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiResepDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ObatMalamDiambil",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ObatPagiDiambil",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ObatSiangDiambil",
                schema: "public",
                table: "MstResepDetail",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RuangBedahBookings",
                columns: table => new
                {
                    BookingRuanganBedahId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglOperasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuOperasi = table.Column<TimeSpan>(type: "interval", nullable: true),
                    RuangTindakan = table.Column<string>(type: "text", nullable: true),
                    DiagnosaDokter1 = table.Column<string>(type: "text", nullable: true),
                    DiagnosaDokter2 = table.Column<string>(type: "text", nullable: true),
                    DiagnosaDokter3 = table.Column<string>(type: "text", nullable: true),
                    DiagnosaDokter4 = table.Column<string>(type: "text", nullable: true),
                    DiagnosaDokter5 = table.Column<string>(type: "text", nullable: true),
                    BeratBadan = table.Column<decimal>(type: "numeric", nullable: true),
                    DokterOperator1 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterOperator2 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterOperator3 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterOperator4 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterOperator5 = table.Column<Guid>(type: "uuid", nullable: true),
                    RencanaTindakanOperasi = table.Column<string>(type: "text", nullable: true),
                    JenisAnastesi = table.Column<string>(type: "text", nullable: true),
                    TypeOK = table.Column<string>(type: "text", nullable: true),
                    PenandaanLokasiOperasi = table.Column<string>(type: "text", nullable: true),
                    isSuratIzinOperasi = table.Column<bool>(type: "boolean", nullable: true),
                    isBedahBersalin = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_RuangBedahBookings", x => x.BookingRuanganBedahId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuangBedahBookings");

            migrationBuilder.DropColumn(
                name: "ObatMalamDiambil",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "ObatPagiDiambil",
                schema: "public",
                table: "MstResepDetail");

            migrationBuilder.DropColumn(
                name: "ObatSiangDiambil",
                schema: "public",
                table: "MstResepDetail");
        }
    }
}
