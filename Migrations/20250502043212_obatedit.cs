using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class obatedit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MstObat",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Barcode",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "BuyPrice",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Cogs",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "DosageForm",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "DosageStrength",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "DosageVolume",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "Fungsi",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "IsSupplierUtama",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "KategoryObatId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "MeasurementId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "MeasurementName",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "NamaKategoriObat",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ProductExtCode",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ProductName",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "RackNumber",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "WarehouseLocationId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "WarehouseLocationName",
                schema: "public",
                table: "MstObat");

            migrationBuilder.RenameColumn(
                name: "ZatAktif",
                schema: "public",
                table: "MstObat",
                newName: "ObatCode");

            migrationBuilder.RenameColumn(
                name: "StorageLocation",
                schema: "public",
                table: "MstObat",
                newName: "ObatName");

            migrationBuilder.RenameColumn(
                name: "RetailPrice",
                schema: "public",
                table: "MstObat",
                newName: "HargaJual");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "public",
                table: "MstObat",
                newName: "BentukObat");

            migrationBuilder.AddColumn<Guid>(
                name: "ObatId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "HargaAwal",
                schema: "public",
                table: "MstObat",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MstObat",
                schema: "public",
                table: "MstObat",
                column: "ObatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MstObat",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ObatId",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "HargaAwal",
                schema: "public",
                table: "MstObat");

            migrationBuilder.RenameColumn(
                name: "ObatName",
                schema: "public",
                table: "MstObat",
                newName: "StorageLocation");

            migrationBuilder.RenameColumn(
                name: "ObatCode",
                schema: "public",
                table: "MstObat",
                newName: "ZatAktif");

            migrationBuilder.RenameColumn(
                name: "HargaJual",
                schema: "public",
                table: "MstObat",
                newName: "RetailPrice");

            migrationBuilder.RenameColumn(
                name: "BentukObat",
                schema: "public",
                table: "MstObat",
                newName: "ProductId");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BuyPrice",
                schema: "public",
                table: "MstObat",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cogs",
                schema: "public",
                table: "MstObat",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DiscountId",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountValue",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageForm",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageStrength",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageVolume",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiredDate",
                schema: "public",
                table: "MstObat",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fungsi",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSupplierUtama",
                schema: "public",
                table: "MstObat",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KategoryObatId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MeasurementId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeasurementName",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaKategoriObat",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductExtCode",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RackNumber",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseLocationId",
                schema: "public",
                table: "MstObat",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseLocationName",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MstObat",
                schema: "public",
                table: "MstObat",
                column: "ProductId");
        }
    }
}
