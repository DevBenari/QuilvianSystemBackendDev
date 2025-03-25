using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editjp2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.AlterColumn<Guid>(
                name: "AsuransiId",
                schema: "public",
                table: "MstDokterAsuransi",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PoliId",
                table: "DokterPolis",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "");


            migrationBuilder.AlterColumn<Guid>(
                name: "PoliId",
                table: "DokterPolis",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

        }
    }
}
