using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class dedomination : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasterDenominasi",
                schema: "public",
                columns: table => new
                {
                    DenominasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeDenominasi = table.Column<string>(type: "text", nullable: false),
                    MataUang = table.Column<decimal>(type: "numeric", nullable: false),
                    NominalPecahan = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterDenominasi", x => x.DenominasiId);
                });

            migrationBuilder.CreateTable(
                name: "PergantianShift",
                schema: "public",
                columns: table => new
                {
                    PergantianShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeRegistrasi = table.Column<string>(type: "text", nullable: false),
                    LayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KasirId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftPergantian = table.Column<string>(type: "text", nullable: false),
                    WaktuMulai = table.Column<TimeSpan>(type: "interval", nullable: false),
                    WaktuAkhir = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TanggalPergantian = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SaldoAwal = table.Column<decimal>(type: "numeric", nullable: false),
                    PendapatanTunai = table.Column<decimal>(type: "numeric", nullable: false),
                    KasFisik = table.Column<decimal>(type: "numeric", nullable: false),
                    SelisihPendapatanTunai = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPendapatan = table.Column<decimal>(type: "numeric", nullable: false),
                    PendapatanNonTunai = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PergantianShift", x => x.PergantianShiftId);
                });

            migrationBuilder.CreateTable(
                name: "ShiftDenominasi",
                schema: "public",
                columns: table => new
                {
                    ShiftDenominasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeShiftDenominasi = table.Column<string>(type: "text", nullable: false),
                    LayananId = table.Column<Guid>(type: "uuid", nullable: false),
                    KasirId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipePerhitungan = table.Column<string>(type: "text", nullable: false),
                    DenominasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    LembarKoin = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalDenominasi = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftDenominasi", x => x.ShiftDenominasiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterDenominasi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PergantianShift",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ShiftDenominasi",
                schema: "public");
        }
    }
}
