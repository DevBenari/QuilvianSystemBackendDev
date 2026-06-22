using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class pembayaranap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fin_PembayaranAP",
                schema: "public",
                columns: table => new
                {
                    PembayaranAPId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodePembayaranAP = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NoReferensi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalTagihan = table.Column<decimal>(type: "numeric", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    TglPembayaranAP = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BankId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipePembayaran = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatusPembayaran = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Potongan = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_Fin_PembayaranAP", x => x.PembayaranAPId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_DetailPembayaranAP",
                schema: "public",
                columns: table => new
                {
                    DetailPembayaranAPId = table.Column<Guid>(type: "uuid", nullable: false),
                    PembayaranAPId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchasingInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SisaTagihan = table.Column<decimal>(type: "numeric", nullable: true),
                    PembayaranTagihan = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_Fin_DetailPembayaranAP", x => x.DetailPembayaranAPId);
                    table.ForeignKey(
                        name: "FK_Fin_DetailPembayaranAP_Fin_PembayaranAP_PembayaranAPId",
                        column: x => x.PembayaranAPId,
                        principalSchema: "public",
                        principalTable: "Fin_PembayaranAP",
                        principalColumn: "PembayaranAPId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fin_DetailPembayaranAP_PembayaranAPId",
                schema: "public",
                table: "Fin_DetailPembayaranAP",
                column: "PembayaranAPId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_DetailPembayaranAP",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_PembayaranAP",
                schema: "public");
        }
    }
}
