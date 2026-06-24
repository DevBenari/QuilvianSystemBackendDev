using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomDiBeberapaTabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MataUangId",
                schema: "public",
                table: "MstSupplier",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "StatusTagihan",
                schema: "public",
                table: "FIN_TukarFaktur",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LayananId",
                schema: "public",
                table: "Fin_PurchaseOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProdukId",
                schema: "public",
                table: "Fin_PurchaseOrder",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MataUangId",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.DropColumn(
                name: "StatusTagihan",
                schema: "public",
                table: "FIN_TukarFaktur");

            migrationBuilder.DropColumn(
                name: "LayananId",
                schema: "public",
                table: "Fin_PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "ProdukId",
                schema: "public",
                table: "Fin_PurchaseOrder");
        }
    }
}
