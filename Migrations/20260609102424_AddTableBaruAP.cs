using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableBaruAP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLunas",
                schema: "public",
                table: "FIN_ARHeader",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SisaPembayaran",
                schema: "public",
                table: "FIN_ARHeader",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FIN_DetailTukarFaktur",
                schema: "public",
                columns: table => new
                {
                    DetailTukarFakturId = table.Column<Guid>(type: "uuid", nullable: false),
                    TukarFakturId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomorPO = table.Column<string>(type: "text", nullable: false),
                    NoInvoice = table.Column<string>(type: "text", nullable: false),
                    TotalInvoice = table.Column<decimal>(type: "numeric", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_DetailTukarFaktur", x => x.DetailTukarFakturId);
                });

            migrationBuilder.CreateTable(
                name: "FIN_TukarFaktur",
                schema: "public",
                columns: table => new
                {
                    TukarFakturId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglRegistrasi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TglTerimaFaktur = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglJatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_FIN_TukarFaktur", x => x.TukarFakturId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FIN_DetailTukarFaktur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FIN_TukarFaktur",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "IsLunas",
                schema: "public",
                table: "FIN_ARHeader");

            migrationBuilder.DropColumn(
                name: "SisaPembayaran",
                schema: "public",
                table: "FIN_ARHeader");
        }
    }
}
