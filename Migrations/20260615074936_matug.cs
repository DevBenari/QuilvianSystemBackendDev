using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class matug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.AlterColumn<string>(
                name: "Keterangan",
                schema: "public",
                table: "FIN_TukarFaktur",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "NoTukarFaktur",
                schema: "public",
                table: "FIN_TukarFaktur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalInvoiceAP",
                schema: "public",
                table: "FIN_TukarFaktur",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalInvoiceGRN",
                schema: "public",
                table: "FIN_TukarFaktur",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "NomorPO",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NoInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Keterangan",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "KodePurchasingInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "NilaiPurchasingInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "POId",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "StatusInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "TglPembuatanInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "FIN_ExchangeRate",
                schema: "public",
                columns: table => new
                {
                    ExchangeRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MataUangId = table.Column<Guid>(type: "uuid", nullable: false),
                    RateToIDR = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    RateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_FIN_ExchangeRate", x => x.ExchangeRateId);
                });

            migrationBuilder.CreateTable(
                name: "FIN_MataUang",
                schema: "public",
                columns: table => new
                {
                    MataUangId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeMataUang = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NamaMataUang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SimbolMataUang = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsBaseCurrency = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_FIN_MataUang", x => x.MataUangId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIN_ExchangeRate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FIN_MataUang",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "NoTukarFaktur",
                schema: "public",
                table: "FIN_TukarFaktur");

            migrationBuilder.DropColumn(
                name: "TotalInvoiceAP",
                schema: "public",
                table: "FIN_TukarFaktur");

            migrationBuilder.DropColumn(
                name: "TotalInvoiceGRN",
                schema: "public",
                table: "FIN_TukarFaktur");

            migrationBuilder.DropColumn(
                name: "KodePurchasingInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.DropColumn(
                name: "NilaiPurchasingInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.DropColumn(
                name: "POId",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.DropColumn(
                name: "StatusInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.DropColumn(
                name: "TglPembuatanInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur");

            migrationBuilder.AlterColumn<string>(
                name: "Keterangan",
                schema: "public",
                table: "FIN_TukarFaktur",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NomorPO",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NoInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Keterangan",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalInvoice",
                schema: "public",
                table: "FIN_DetailTukarFaktur",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
