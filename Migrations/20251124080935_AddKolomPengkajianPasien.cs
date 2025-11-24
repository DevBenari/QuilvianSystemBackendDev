using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomPengkajianPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ObatId",
                table: "StockBatchs",
                newName: "ItemId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TipeKomponenId",
                table: "StockDarahs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "DarahDetailId",
                table: "StockDarahs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "MensPertama",
                table: "PengkajianPerawats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MensTerakhir",
                table: "PengkajianPerawats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Minum",
                table: "PengkajianPerawats",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalImunisasiLanjutan",
                table: "PengkajianPerawats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeImunisasi",
                table: "PengkajianPerawats",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MensPertama",
                table: "PengkajianPerawats");

            migrationBuilder.DropColumn(
                name: "MensTerakhir",
                table: "PengkajianPerawats");

            migrationBuilder.DropColumn(
                name: "Minum",
                table: "PengkajianPerawats");

            migrationBuilder.DropColumn(
                name: "TanggalImunisasiLanjutan",
                table: "PengkajianPerawats");

            migrationBuilder.DropColumn(
                name: "TipeImunisasi",
                table: "PengkajianPerawats");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "StockBatchs",
                newName: "ObatId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TipeKomponenId",
                table: "StockDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DarahDetailId",
                table: "StockDarahs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
