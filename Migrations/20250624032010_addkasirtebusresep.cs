using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addkasirtebusresep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamaPenebus",
                schema: "public",
                table: "ResepTebus",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KasirTebusResep",
                schema: "public",
                columns: table => new
                {
                    KasirTebusResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepTebusId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoRegistrasi = table.Column<string>(type: "text", nullable: true),
                    NoAntrian = table.Column<decimal>(type: "numeric", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaMetode = table.Column<string>(type: "text", nullable: true),
                    StatusPembayaran = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TanggalBayar = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_KasirTebusResep", x => x.KasirTebusResepId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KasirTebusResep",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "NamaPenebus",
                schema: "public",
                table: "ResepTebus");
        }
    }
}
