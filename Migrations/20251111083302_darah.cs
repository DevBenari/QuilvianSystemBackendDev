using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class darah : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PenerimaanDarahs",
                columns: table => new
                {
                    PenerimaanDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rhesus = table.Column<string>(type: "text", nullable: true),
                    JumlahKantong = table.Column<decimal>(type: "numeric", nullable: true),
                    Sumber = table.Column<string>(type: "text", nullable: true),
                    TglMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglExpired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenerimaanDarahs", x => x.PenerimaanDarahId);
                });

            migrationBuilder.CreateTable(
                name: "StockDarahs",
                columns: table => new
                {
                    StockDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    GolonganDarahId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipeKomponenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rhesus = table.Column<string>(type: "text", nullable: true),
                    Golongan = table.Column<string>(type: "text", nullable: true),
                    Wacc = table.Column<decimal>(type: "numeric", nullable: true),
                    JumlahKantong = table.Column<decimal>(type: "numeric", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    JumlahExpired = table.Column<decimal>(type: "numeric", nullable: true),
                    TglExpired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SisaStock = table.Column<decimal>(type: "numeric", nullable: true),
                    MinStock = table.Column<decimal>(type: "numeric", nullable: true),
                    StatusStock = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDarahs", x => x.StockDarahId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PenerimaanDarahs");

            migrationBuilder.DropTable(
                name: "StockDarahs");
        }
    }
}
