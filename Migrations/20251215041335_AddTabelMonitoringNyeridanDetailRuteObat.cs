using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTabelMonitoringNyeridanDetailRuteObat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoringNyeris",
                columns: table => new
                {
                    MonitoringNyeriId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaktuMonitoringNyeri = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SkorNyeri = table.Column<decimal>(type: "numeric", nullable: true),
                    SkorSedasi = table.Column<decimal>(type: "numeric", nullable: true),
                    Sistolik = table.Column<decimal>(type: "numeric", nullable: true),
                    Diastolic = table.Column<decimal>(type: "numeric", nullable: true),
                    Nadi = table.Column<decimal>(type: "numeric", nullable: true),
                    Respirasi = table.Column<decimal>(type: "numeric", nullable: true),
                    Suhu = table.Column<decimal>(type: "numeric", nullable: true),
                    PerawatMonitoringId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParafPerawatMonitoring = table.Column<string>(type: "text", nullable: true),
                    WaktuIntervensi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dosis = table.Column<string>(type: "text", nullable: true),
                    Rute = table.Column<string>(type: "text", nullable: true),
                    IntervensiNonFarmakologi = table.Column<string>(type: "text", nullable: true),
                    PerawatIntervensiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParafPerawatIntervensi = table.Column<string>(type: "text", nullable: true),
                    WaktuKajianUlang = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_MonitoringNyeris", x => x.MonitoringNyeriId);
                });

            migrationBuilder.CreateTable(
                name: "ObatRuteDetails",
                columns: table => new
                {
                    DetailRuteObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuteObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSingkat = table.Column<string>(type: "text", nullable: true),
                    Kepanjangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ObatRuteDetails", x => x.DetailRuteObatId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringNyeris");

            migrationBuilder.DropTable(
                name: "ObatRuteDetails");
        }
    }
}
