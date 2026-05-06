using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableDetailDiskon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TglFinishedKasir",
                schema: "public",
                table: "MstKunjungan",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodeDiskon",
                schema: "public",
                table: "Diskon",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusBiayaLainnya",
                schema: "public",
                table: "Billing",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubBiayaLainnya",
                schema: "public",
                table: "Billing",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiskonDetails",
                columns: table => new
                {
                    DetailDiskonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiskonId = table.Column<Guid>(type: "uuid", nullable: true),
                    LayananId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodeLayanan = table.Column<string>(type: "text", nullable: true),
                    KategoriLayanan = table.Column<string>(type: "text", nullable: true),
                    MaxQty = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxHarga = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_DiskonDetails", x => x.DetailDiskonId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiskonDetails");

            migrationBuilder.DropColumn(
                name: "TglFinishedKasir",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "KodeDiskon",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "StatusBiayaLainnya",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "SubBiayaLainnya",
                schema: "public",
                table: "Billing");
        }
    }
}
