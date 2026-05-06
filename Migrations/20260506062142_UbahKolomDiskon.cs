using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomDiskon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billing_MstKunjungan_KunjunganId",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "MaxHarga",
                table: "DiskonDetails");

            migrationBuilder.DropColumn(
                name: "TipeDiskonDokter",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "ValueDiskonDokter",
                schema: "public",
                table: "Diskon");

            migrationBuilder.RenameColumn(
                name: "MaxQty",
                table: "DiskonDetails",
                newName: "HargaItem");

            migrationBuilder.AddColumn<Guid>(
                name: "KelasId",
                table: "DiskonDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_MstKunjungan_KunjunganId",
                schema: "public",
                table: "Billing",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billing_MstKunjungan_KunjunganId",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "KelasId",
                table: "DiskonDetails");

            migrationBuilder.RenameColumn(
                name: "HargaItem",
                table: "DiskonDetails",
                newName: "MaxQty");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxHarga",
                table: "DiskonDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeDiskonDokter",
                schema: "public",
                table: "Diskon",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValueDiskonDokter",
                schema: "public",
                table: "Diskon",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Billing_MstKunjungan_KunjunganId",
                schema: "public",
                table: "Billing",
                column: "KunjunganId",
                principalSchema: "public",
                principalTable: "MstKunjungan",
                principalColumn: "KunjunganID");
        }
    }
}
