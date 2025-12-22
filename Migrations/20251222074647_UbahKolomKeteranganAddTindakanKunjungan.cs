using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomKeteranganAddTindakanKunjungan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartementId",
                table: "TindakanKunjungans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DokterPemeriksaId",
                table: "TindakanKunjungans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KelasId",
                table: "TindakanKunjungans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Keterangan",
                table: "AlatPemakaianDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartementId",
                table: "TindakanKunjungans");

            migrationBuilder.DropColumn(
                name: "DokterPemeriksaId",
                table: "TindakanKunjungans");

            migrationBuilder.DropColumn(
                name: "KelasId",
                table: "TindakanKunjungans");

            migrationBuilder.AlterColumn<decimal>(
                name: "Keterangan",
                table: "AlatPemakaianDetails",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
