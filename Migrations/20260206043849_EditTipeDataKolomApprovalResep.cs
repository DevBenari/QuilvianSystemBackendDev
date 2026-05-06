using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditTipeDataKolomApprovalResep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TTDPetugasFarmasiId",
                schema: "public",
                table: "MstResep",
                newName: "PathTTDPetugasFarmasi");

            migrationBuilder.AlterColumn<string>(
                name: "PathTTDDokter",
                schema: "public",
                table: "MstResep",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PathTTDPetugasFarmasi",
                schema: "public",
                table: "MstResep",
                newName: "TTDPetugasFarmasiId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PathTTDDokter",
                schema: "public",
                table: "MstResep",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
