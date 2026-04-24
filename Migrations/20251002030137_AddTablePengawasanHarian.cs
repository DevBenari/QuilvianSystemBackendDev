using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePengawasanHarian : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PengawasanHarians",
                columns: table => new
                {
                    PengawasanHarianId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    VitalSignId = table.Column<Guid>(type: "uuid", nullable: true),
                    PainAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPengawasanHarian = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuPengawasan = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    IsRelaksasi = table.Column<bool>(type: "boolean", nullable: true),
                    IsKompres = table.Column<bool>(type: "boolean", nullable: true),
                    IsDetailKompres = table.Column<bool>(type: "boolean", nullable: true),
                    IsPijatan = table.Column<bool>(type: "boolean", nullable: true),
                    IsTens = table.Column<bool>(type: "boolean", nullable: true),
                    IsIstirahat = table.Column<bool>(type: "boolean", nullable: true),
                    IsMusik = table.Column<bool>(type: "boolean", nullable: true),
                    IsTeraphyAktivitas = table.Column<bool>(type: "boolean", nullable: true),
                    IsLatihanOtot = table.Column<bool>(type: "boolean", nullable: true),
                    IntakeInfuse = table.Column<decimal>(type: "numeric", nullable: true),
                    IntakeOral = table.Column<decimal>(type: "numeric", nullable: true),
                    IntakeNGT = table.Column<decimal>(type: "numeric", nullable: true),
                    IntakeDarah = table.Column<decimal>(type: "numeric", nullable: true),
                    IntakeObat = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalIntake = table.Column<decimal>(type: "numeric", nullable: true),
                    OutputUrin = table.Column<decimal>(type: "numeric", nullable: true),
                    OutputFeses = table.Column<decimal>(type: "numeric", nullable: true),
                    OutputNGT = table.Column<decimal>(type: "numeric", nullable: true),
                    OutputWL = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalOutput = table.Column<decimal>(type: "numeric", nullable: true),
                    BalanceShift = table.Column<decimal>(type: "numeric", nullable: true),
                    Balance24H = table.Column<decimal>(type: "numeric", nullable: true),
                    GulaDarah = table.Column<decimal>(type: "numeric", nullable: true),
                    AsupanMakanan = table.Column<string>(type: "text", nullable: true),
                    Diet = table.Column<string>(type: "text", nullable: true),
                    LingkarPerut = table.Column<decimal>(type: "numeric", nullable: true),
                    MobilisasiPasien = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PengawasanHarians", x => x.PengawasanHarianId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PengawasanHarians");
        }
    }
}
