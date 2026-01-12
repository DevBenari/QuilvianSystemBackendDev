using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class DeleteKolomObservasiCairanIdDiTransferPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndikatorPengkajianId",
                table: "TransferPasiens");

            migrationBuilder.DropColumn(
                name: "ObservasiCairanId",
                table: "TransferPasiens");

            migrationBuilder.DropColumn(
                name: "PemberianObatId",
                table: "TransferPasiens");

            migrationBuilder.DropColumn(
                name: "PengawasanHarianId",
                table: "TransferPasiens");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IndikatorPengkajianId",
                table: "TransferPasiens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObservasiCairanId",
                table: "TransferPasiens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PemberianObatId",
                table: "TransferPasiens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PengawasanHarianId",
                table: "TransferPasiens",
                type: "uuid",
                nullable: true);
        }
    }
}
