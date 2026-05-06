using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahNamaBeberapaKolomTTD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaKepalaRuangan",
                table: "Nosokomials");

            migrationBuilder.DropColumn(
                name: "NamaPerawat",
                table: "Nosokomials");

            migrationBuilder.RenameColumn(
                name: "TTDPenerimaId",
                table: "TransferPasiens",
                newName: "PetugasPenerimaId");

            migrationBuilder.RenameColumn(
                name: "TTDMenyerahkanId",
                table: "TransferPasiens",
                newName: "PetugasMenyerahkanId");

            migrationBuilder.RenameColumn(
                name: "TTDMengetahuiId",
                table: "TransferPasiens",
                newName: "PetugasMengetahuiId");

            migrationBuilder.RenameColumn(
                name: "DPPIAId",
                table: "HemodialisaHasils",
                newName: "DPPJAId");

            migrationBuilder.AddColumn<Guid>(
                name: "KepalaRuanganId",
                table: "Nosokomials",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerawatId",
                table: "Nosokomials",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                "ALTER TABLE \"IGDAssessmentAwals\" " +
                "ALTER COLUMN \"TTDPerawatId\" TYPE uuid USING (NULLIF(\"TTDPerawatId\", '')::uuid);"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KepalaRuanganId",
                table: "Nosokomials");

            migrationBuilder.DropColumn(
                name: "PerawatId",
                table: "Nosokomials");

            migrationBuilder.RenameColumn(
                name: "PetugasPenerimaId",
                table: "TransferPasiens",
                newName: "TTDPenerimaId");

            migrationBuilder.RenameColumn(
                name: "PetugasMenyerahkanId",
                table: "TransferPasiens",
                newName: "TTDMenyerahkanId");

            migrationBuilder.RenameColumn(
                name: "PetugasMengetahuiId",
                table: "TransferPasiens",
                newName: "TTDMengetahuiId");

            migrationBuilder.RenameColumn(
                name: "DPPJAId",
                table: "HemodialisaHasils",
                newName: "DPPIAId");

            migrationBuilder.AddColumn<string>(
                name: "NamaKepalaRuangan",
                table: "Nosokomials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaPerawat",
                table: "Nosokomials",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TTDPerawatId",
                table: "IGDAssessmentAwals",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
