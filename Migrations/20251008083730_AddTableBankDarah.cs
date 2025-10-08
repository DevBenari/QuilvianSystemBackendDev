using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableBankDarah : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DarahPermintaans",
                columns: table => new
                {
                    BankDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    KomponenDarahId = table.Column<Guid>(type: "uuid", nullable: true),
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: true),
                    JumlahKantong = table.Column<decimal>(type: "numeric", nullable: true),
                    Rhesus = table.Column<bool>(type: "boolean", nullable: true),
                    TglPemesanan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WaktuPemesanan = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TglDiperlukan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DokterPerujukId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterBDRSId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_DarahPermintaans", x => x.BankDarahId);
                });

            migrationBuilder.CreateTable(
                name: "MstDarah",
                schema: "public",
                columns: table => new
                {
                    KomponenDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKomponenDarah = table.Column<string>(type: "text", nullable: true),
                    KodeKomponenDarah = table.Column<string>(type: "text", nullable: true),
                    TipeKomponenDarah = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstDarah", x => x.KomponenDarahId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DarahPermintaans");

            migrationBuilder.DropTable(
                name: "MstDarah",
                schema: "public");
        }
    }
}
