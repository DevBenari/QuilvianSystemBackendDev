using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class newpagesretusr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_TukarFaktur",
                schema: "public",
                table: "FIN_TukarFaktur");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_MataUang",
                schema: "public",
                table: "FIN_MataUang");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_ExchangeRate",
                schema: "public",
                table: "FIN_ExchangeRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FIN_DetailTukarFaktur",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.RenameTable(
                name: "FIN_TukarFaktur",
                schema: "public",
                newName: "Fin_TukarFaktur",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FIN_MataUang",
                schema: "public",
                newName: "Fin_MataUang",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FIN_ExchangeRate",
                schema: "public",
                newName: "Fin_ExchangeRate",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FIN_DetailTukarFaktur",
                schema: "public",
                newName: "Fin_DetailTukarFaktur",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_TukarFaktur",
                schema: "public",
                table: "Fin_TukarFaktur",
                column: "TukarFakturId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_MataUang",
                schema: "public",
                table: "Fin_MataUang",
                column: "MataUangId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_ExchangeRate",
                schema: "public",
                table: "Fin_ExchangeRate",
                column: "ExchangeRateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fin_DetailTukarFaktur",
                schema: "public",
                table: "Fin_DetailTukarFaktur",
                column: "DetailTukarFakturId");

            migrationBuilder.CreateTable(
                name: "Fin_DepositRetur",
                schema: "public",
                columns: table => new
                {
                    DepositReturId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiveOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeaderReturId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglInsertDeposit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusDeposit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    JumlahDeposit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_Fin_DepositRetur", x => x.DepositReturId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_HeaderRetur",
                schema: "public",
                columns: table => new
                {
                    HeaderReturId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    GudangId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeRetur = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StatusRetur = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsTerkonfirmasi = table.Column<bool>(type: "boolean", nullable: false),
                    TglRetur = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Fin_HeaderRetur", x => x.HeaderReturId);
                });

            migrationBuilder.CreateTable(
                name: "Fin_ItemRetur",
                schema: "public",
                columns: table => new
                {
                    ItemReturId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdukId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeaderReturId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusRetur = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsTerkonfirmasi = table.Column<bool>(type: "boolean", nullable: false),
                    TglRetur = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoBatch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NoFakturInvoice = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NoPO = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    POId = table.Column<Guid>(type: "uuid", nullable: false),
                    QtyDiterima = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    QtyTelahDiretur = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReceiveOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    QtyRetur = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Satuan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HargaSatuan = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SubtotalHarga = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TglPenerimaanPO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglTukarFaktur = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Fin_ItemRetur", x => x.ItemReturId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fin_DepositRetur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_HeaderRetur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fin_ItemRetur",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_TukarFaktur",
                schema: "public",
                table: "Fin_TukarFaktur");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_MataUang",
                schema: "public",
                table: "Fin_MataUang");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_ExchangeRate",
                schema: "public",
                table: "Fin_ExchangeRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fin_DetailTukarFaktur",
                schema: "public",
                table: "Fin_DetailTukarFaktur");

            migrationBuilder.RenameTable(
                name: "Fin_TukarFaktur",
                schema: "public",
                newName: "FIN_TukarFaktur",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_MataUang",
                schema: "public",
                newName: "FIN_MataUang",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_ExchangeRate",
                schema: "public",
                newName: "FIN_ExchangeRate",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Fin_DetailTukarFaktur",
                schema: "public",
                newName: "FIN_DetailTukarFaktur",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_TukarFaktur",
                schema: "public",
                table: "FIN_TukarFaktur",
                column: "TukarFakturId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_MataUang",
                schema: "public",
                table: "FIN_MataUang",
                column: "MataUangId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_ExchangeRate",
                schema: "public",
                table: "FIN_ExchangeRate",
                column: "ExchangeRateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FIN_DetailTukarFaktur",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                column: "DetailTukarFakturId");
        }
    }
}
