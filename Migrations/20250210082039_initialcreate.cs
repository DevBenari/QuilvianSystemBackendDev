using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class initialcreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKecamatan_MstKabupaten_KabupatenId",
                schema: "dbo",
                table: "MstKecamatan");

            migrationBuilder.DropTable(
                name: "MstKabupaten",
                schema: "dbo");

            migrationBuilder.RenameColumn(
                name: "KabupatenId",
                schema: "dbo",
                table: "MstKecamatan",
                newName: "KabupatenKotaId");

            migrationBuilder.RenameIndex(
                name: "IX_MstKecamatan_KabupatenId",
                schema: "dbo",
                table: "MstKecamatan",
                newName: "IX_MstKecamatan_KabupatenKotaId");

            migrationBuilder.AddColumn<Guid>(
                name: "NegaraId",
                schema: "dbo",
                table: "MstProvinsi",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MstKabupatenKota",
                schema: "dbo",
                columns: table => new
                {
                    KabupatenKotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KabupatenKotaCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenKotaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKabupatenKota", x => x.KabupatenKotaId);
                    table.ForeignKey(
                        name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                        column: x => x.ProvinsiId,
                        principalSchema: "dbo",
                        principalTable: "MstProvinsi",
                        principalColumn: "ProvinsiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProvinsi_NegaraId",
                schema: "dbo",
                table: "MstProvinsi",
                column: "NegaraId");

            migrationBuilder.CreateIndex(
                name: "IX_MstKabupatenKota_ProvinsiId",
                schema: "dbo",
                table: "MstKabupatenKota",
                column: "ProvinsiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenKotaId",
                principalSchema: "dbo",
                principalTable: "MstKabupatenKota",
                principalColumn: "KabupatenKotaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstProvinsi_MstNegara_NegaraId",
                schema: "dbo",
                table: "MstProvinsi",
                column: "NegaraId",
                principalSchema: "dbo",
                principalTable: "MstNegara",
                principalColumn: "NegaraId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstProvinsi_MstNegara_NegaraId",
                schema: "dbo",
                table: "MstProvinsi");

            migrationBuilder.DropTable(
                name: "MstKabupatenKota",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_MstProvinsi_NegaraId",
                schema: "dbo",
                table: "MstProvinsi");

            migrationBuilder.DropColumn(
                name: "NegaraId",
                schema: "dbo",
                table: "MstProvinsi");

            migrationBuilder.RenameColumn(
                name: "KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan",
                newName: "KabupatenId");

            migrationBuilder.RenameIndex(
                name: "IX_MstKecamatan_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan",
                newName: "IX_MstKecamatan_KabupatenId");

            migrationBuilder.CreateTable(
                name: "MstKabupaten",
                schema: "dbo",
                columns: table => new
                {
                    KabupatenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KabupatenCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KabupatenName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKabupaten", x => x.KabupatenId);
                    table.ForeignKey(
                        name: "FK_MstKabupaten_MstProvinsi_ProvinsiId",
                        column: x => x.ProvinsiId,
                        principalSchema: "dbo",
                        principalTable: "MstProvinsi",
                        principalColumn: "ProvinsiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstKabupaten_ProvinsiId",
                schema: "dbo",
                table: "MstKabupaten",
                column: "ProvinsiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKecamatan_MstKabupaten_KabupatenId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenId",
                principalSchema: "dbo",
                principalTable: "MstKabupaten",
                principalColumn: "KabupatenId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
