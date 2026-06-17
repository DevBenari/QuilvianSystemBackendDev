using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newPI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fin_PurchasingInvoice",
                schema: "public",
                columns: table => new
                {
                    PurchasingInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    POId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoPO = table.Column<string>(type: "text", nullable: true),
                    TglPO = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    POAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaSupplier = table.Column<string>(type: "text", nullable: true),
                    DiskonSupplier = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SupplierTermPayment = table.Column<int>(type: "integer", nullable: true),
                    TglPembuatanInvoice = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglJatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipePembayaran = table.Column<string>(type: "text", nullable: true),
                    ReceiveOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiveOrderNumber = table.Column<string>(type: "text", nullable: true),
                    NoInvoice = table.Column<string>(type: "text", nullable: true),
                    DownPayment = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiskonPersen = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiskonNominal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PPNPersen = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PPNNominal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OngkosKirim = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Materai = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Pembulatan = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Potongan = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Retur = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OutstandingDP = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    COAId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoFakturPajak = table.Column<string>(type: "text", nullable: true),
                    TglFaktur = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MataUangId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaMataUang = table.Column<string>(type: "text", nullable: true),
                    RateToIdr = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    HasilKonversi = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fin_PurchasingInvoice", x => x.PurchasingInvoiceId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_ItemPurchasingInvoice",
                schema: "public",
                columns: table => new
                {
                    ItemPurchasingInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasingInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    POId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemPOId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodeProduk = table.Column<string>(type: "text", nullable: true),
                    NamaProduk = table.Column<string>(type: "text", nullable: true),
                    QtyProduk = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SatuanProduk = table.Column<string>(type: "text", nullable: true),
                    HargaNormal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TipeTax = table.Column<string>(type: "text", nullable: true),
                    PajakPersen = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PajakNominal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    HargaAkhir = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    HargaTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fin_ItemPurchasingInvoice", x => x.ItemPurchasingInvoiceId);
                    table.ForeignKey(
                        name: "FK_Fin_ItemPurchasingInvoice_Fin_PurchasingInvoice_PurchasingI~",
                        column: x => x.PurchasingInvoiceId,
                        principalSchema: "public",
                        principalTable: "Fin_PurchasingInvoice",
                        principalColumn: "PurchasingInvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fin_ItemPurchasingInvoice_PurchasingInvoiceId",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice",
                column: "PurchasingInvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_ItemPurchasingInvoice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_PurchasingInvoice",
                schema: "public");
        }
    }
}
