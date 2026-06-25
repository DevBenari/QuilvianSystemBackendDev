using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomTTgRujukanDiKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MataUangId",
                schema: "public",
                table: "MstSupplier",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "DokterPerujuk",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RSPerujuk",
                schema: "public",
                table: "MstKunjungan",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DokterPerujuk",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.DropColumn(
                name: "RSPerujuk",
                schema: "public",
                table: "MstKunjungan");

            migrationBuilder.AlterColumn<Guid>(
                name: "MataUangId",
                schema: "public",
                table: "MstSupplier",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
