using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newClickup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiskonProduk",
                schema: "public",
                table: "Fin_ReceiveOrderItem",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaSatuan",
                schema: "public",
                table: "Fin_ReceiveOrderItem",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaTotal",
                schema: "public",
                table: "Fin_ReceiveOrderItem",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaTotalPO",
                schema: "public",
                table: "Fin_ReceiveOrder",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NominalPPN",
                schema: "public",
                table: "Fin_ReceiveOrder",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiskon",
                schema: "public",
                table: "Fin_ReceiveOrder",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DPPo",
                schema: "public",
                table: "Fin_PembayaranAP",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiveOrderNumber",
                schema: "public",
                table: "Fin_PembayaranAP",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HargaTotal",
                schema: "public",
                table: "Fin_ItemRetur",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DPPo",
                schema: "public",
                table: "Fin_DetailPembayaranAP",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReceiveOrderNumber",
                schema: "public",
                table: "Fin_DetailPembayaranAP",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Fin_CostCenter",
                schema: "public",
                columns: table => new
                {
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeCostCenter = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LokasiCostCenter = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_CostCenter", x => x.CostCenterId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_PembayaranManual",
                schema: "public",
                columns: table => new
                {
                    PembayaranManualId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePembayaranManual = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TglDokumen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglPembayaranManual = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MataUangId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExchangeRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglJatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    PajakId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersenanPajak = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NominalPajak = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NomorFakturPajak = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TglFakturPajak = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoReferensiManual = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatusPembayaranManual = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_PembayaranManual", x => x.PembayaranManualId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_RekapAP",
                schema: "public",
                columns: table => new
                {
                    RekapAPId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    RekapVariasiHarga = table.Column<decimal>(type: "numeric", nullable: true),
                    RekapOther = table.Column<decimal>(type: "numeric", nullable: true),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_RekapAP", x => x.RekapAPId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_DetailPembayaranManual",
                schema: "public",
                columns: table => new
                {
                    DetailPembayaranManualId = table.Column<Guid>(type: "uuid", nullable: false),
                    PembayaranManualId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeskripsiPembayaran = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    NominalPembayaran = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Keterangan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetailPembayaranManual", x => x.DetailPembayaranManualId);
                    table.ForeignKey(
                        name: "FK_Fin_DetailPembayaranManual_Fin_PembayaranManual_PembayaranM~",
                        column: x => x.PembayaranManualId,
                        principalSchema: "public",
                        principalTable: "Fin_PembayaranManual",
                        principalColumn: "PembayaranManualId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fin_DetailPembayaranManual_PembayaranManualId",
                schema: "public",
                table: "Fin_DetailPembayaranManual",
                column: "PembayaranManualId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_CostCenter",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_DetailPembayaranManual",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_RekapAP",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_PembayaranManual",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "DiskonProduk",
                schema: "public",
                table: "Fin_ReceiveOrderItem");

            migrationBuilder.DropColumn(
                name: "HargaSatuan",
                schema: "public",
                table: "Fin_ReceiveOrderItem");

            migrationBuilder.DropColumn(
                name: "HargaTotal",
                schema: "public",
                table: "Fin_ReceiveOrderItem");

            migrationBuilder.DropColumn(
                name: "HargaTotalPO",
                schema: "public",
                table: "Fin_ReceiveOrder");

            migrationBuilder.DropColumn(
                name: "NominalPPN",
                schema: "public",
                table: "Fin_ReceiveOrder");

            migrationBuilder.DropColumn(
                name: "TotalDiskon",
                schema: "public",
                table: "Fin_ReceiveOrder");

            migrationBuilder.DropColumn(
                name: "DPPo",
                schema: "public",
                table: "Fin_PembayaranAP");

            migrationBuilder.DropColumn(
                name: "ReceiveOrderNumber",
                schema: "public",
                table: "Fin_PembayaranAP");

            migrationBuilder.DropColumn(
                name: "HargaTotal",
                schema: "public",
                table: "Fin_ItemRetur");

            migrationBuilder.DropColumn(
                name: "DPPo",
                schema: "public",
                table: "Fin_DetailPembayaranAP");

            migrationBuilder.DropColumn(
                name: "ReceiveOrderNumber",
                schema: "public",
                table: "Fin_DetailPembayaranAP");
        }
    }
}
