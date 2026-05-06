using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addKolomItemDetailDiskondanShiftkasir : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KodeDiskon",
                schema: "public",
                table: "Diskon",
                newName: "KodeVoucher");

            migrationBuilder.AddColumn<string>(
                name: "StatusShift",
                schema: "public",
                table: "PergantianShift",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "DiskonDetails",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusShift",
                schema: "public",
                table: "PergantianShift");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "DiskonDetails");

            migrationBuilder.RenameColumn(
                name: "KodeVoucher",
                schema: "public",
                table: "Diskon",
                newName: "KodeDiskon");
        }
    }
}
