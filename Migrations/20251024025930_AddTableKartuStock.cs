using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableKartuStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockBatchs",
                columns: table => new
                {
                    StockBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBatch = table.Column<string>(type: "text", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiredDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_StockBatchs", x => x.StockBatchId);
                });

            migrationBuilder.CreateTable(
                name: "StockKartus",
                columns: table => new
                {
                    KartuStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitAsalId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitTujuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    SatuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KonversiSatuanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<decimal>(type: "numeric", nullable: true),
                    QtyKonversi = table.Column<decimal>(type: "numeric", nullable: true),
                    JenisTransaksi = table.Column<string>(type: "text", nullable: true),
                    IO = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_StockKartus", x => x.KartuStockId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockBatchs");

            migrationBuilder.DropTable(
                name: "StockKartus");
        }
    }
}
