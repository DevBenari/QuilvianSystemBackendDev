using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystem.Migrations
{
    public partial class datanewl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DokterPrakteks_Dokters_DokterId",
                table: "DokterPrakteks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dokters",
                table: "Dokters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DokterPrakteks",
                table: "DokterPrakteks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Asuransis",
                table: "Asuransis");

            migrationBuilder.RenameTable(
                name: "Dokters",
                newName: "MasterDokter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DokterPrakteks",
                newName: "MasterDokterPraktek",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Asuransis",
                newName: "MasterAsuransi",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_DokterPrakteks_DokterId",
                schema: "dbo",
                table: "MasterDokterPraktek",
                newName: "IX_MasterDokterPraktek_DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterDokter",
                schema: "dbo",
                table: "MasterDokter",
                column: "DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterDokterPraktek",
                schema: "dbo",
                table: "MasterDokterPraktek",
                column: "DokterPraktekId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterAsuransi",
                schema: "dbo",
                table: "MasterAsuransi",
                column: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterDokterPraktek_MasterDokter_DokterId",
                schema: "dbo",
                table: "MasterDokterPraktek",
                column: "DokterId",
                principalSchema: "dbo",
                principalTable: "MasterDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MasterDokterPraktek_MasterDokter_DokterId",
                schema: "dbo",
                table: "MasterDokterPraktek");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MasterDokterPraktek",
                schema: "dbo",
                table: "MasterDokterPraktek");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MasterDokter",
                schema: "dbo",
                table: "MasterDokter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MasterAsuransi",
                schema: "dbo",
                table: "MasterAsuransi");

            migrationBuilder.RenameTable(
                name: "MasterDokterPraktek",
                schema: "dbo",
                newName: "DokterPrakteks");

            migrationBuilder.RenameTable(
                name: "MasterDokter",
                schema: "dbo",
                newName: "Dokters");

            migrationBuilder.RenameTable(
                name: "MasterAsuransi",
                schema: "dbo",
                newName: "Asuransis");

            migrationBuilder.RenameIndex(
                name: "IX_MasterDokterPraktek_DokterId",
                table: "DokterPrakteks",
                newName: "IX_DokterPrakteks_DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DokterPrakteks",
                table: "DokterPrakteks",
                column: "DokterPraktekId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dokters",
                table: "Dokters",
                column: "DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Asuransis",
                table: "Asuransis",
                column: "AsuransiId");

            migrationBuilder.AddForeignKey(
                name: "FK_DokterPrakteks_Dokters_DokterId",
                table: "DokterPrakteks",
                column: "DokterId",
                principalTable: "Dokters",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
