using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableHemodialisa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HemodialisaHasils",
                columns: table => new
                {
                    HasilHemodialisaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    NoMesin = table.Column<int>(type: "integer", nullable: true),
                    HemodialisaKe = table.Column<int>(type: "integer", nullable: true),
                    TipeDializer = table.Column<string>(type: "text", nullable: true),
                    JamMulai = table.Column<TimeSpan>(type: "interval", nullable: true),
                    JamAkhir = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AksesVaskuler = table.Column<string>(type: "text", nullable: true),
                    JenisHemodialisa = table.Column<string>(type: "text", nullable: true),
                    Dialisat = table.Column<string>(type: "text", nullable: true),
                    SirkulasiHeparin = table.Column<decimal>(type: "numeric", nullable: true),
                    HeparinAwal = table.Column<decimal>(type: "numeric", nullable: true),
                    HeparinMaintenance = table.Column<decimal>(type: "numeric", nullable: true),
                    HeparinContinue = table.Column<decimal>(type: "numeric", nullable: true),
                    HeparinIntermitten = table.Column<decimal>(type: "numeric", nullable: true),
                    PenyulitHD = table.Column<string>(type: "text", nullable: true),
                    TTDAksesVaskuler = table.Column<string>(type: "text", nullable: true),
                    TTDPPJA = table.Column<string>(type: "text", nullable: true),
                    AksesVaskulerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DPPIAId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifikatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScoreTotalGizi = table.Column<decimal>(type: "numeric", nullable: true),
                    StatusGizi = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    UF = table.Column<string>(type: "jsonb", nullable: true),
                    LaporanNaCl = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_HemodialisaHasils", x => x.HasilHemodialisaId);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringHDs",
                columns: table => new
                {
                    MonitoringHDId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasilHemodialisaId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoDx = table.Column<decimal>(type: "numeric", nullable: true),
                    JamMonitoring = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Tensi = table.Column<string>(type: "text", nullable: true),
                    Nadi = table.Column<decimal>(type: "numeric", nullable: true),
                    TD = table.Column<decimal>(type: "numeric", nullable: true),
                    VP = table.Column<decimal>(type: "numeric", nullable: true),
                    AP = table.Column<decimal>(type: "numeric", nullable: true),
                    QB = table.Column<decimal>(type: "numeric", nullable: true),
                    QD = table.Column<decimal>(type: "numeric", nullable: true),
                    TMP = table.Column<decimal>(type: "numeric", nullable: true),
                    DP = table.Column<decimal>(type: "numeric", nullable: true),
                    UF = table.Column<decimal>(type: "numeric", nullable: true),
                    Keluhan = table.Column<string>(type: "text", nullable: true),
                    Terapi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MonitoringHDs", x => x.MonitoringHDId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HemodialisaHasils");

            migrationBuilder.DropTable(
                name: "MonitoringHDs");
        }
    }
}
