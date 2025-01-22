using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystem.Migrations
{
    public partial class datanewlasr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "MstDokterPraktek",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MasterDokter",
                schema: "dbo",
                newName: "MstDokter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MasterAsuransi",
                schema: "dbo",
                newName: "MstAsuransi",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_MasterDokterPraktek_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek",
                newName: "IX_MstDokterPraktek_DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MstDokterPraktek",
                schema: "dbo",
                table: "MstDokterPraktek",
                column: "DokterPraktekId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MstDokter",
                schema: "dbo",
                table: "MstDokter",
                column: "DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MstAsuransi",
                schema: "dbo",
                table: "MstAsuransi",
                column: "AsuransiId");

            migrationBuilder.CreateTable(
                name: "MstKeangotaan",
                schema: "dbo",
                columns: table => new
                {
                    KeangotaanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeangotaanKode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisKeangotaan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisPromo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstKeangotaan", x => x.KeangotaanId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterPraktek_MstDokter_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek",
                column: "DokterId",
                principalSchema: "dbo",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterPraktek_MstDokter_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek");

            migrationBuilder.DropTable(
                name: "MstKeangotaan",
                schema: "dbo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MstDokterPraktek",
                schema: "dbo",
                table: "MstDokterPraktek");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MstDokter",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MstAsuransi",
                schema: "dbo",
                table: "MstAsuransi");

            migrationBuilder.RenameTable(
                name: "MstDokterPraktek",
                schema: "dbo",
                newName: "MasterDokterPraktek",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MstDokter",
                schema: "dbo",
                newName: "MasterDokter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MstAsuransi",
                schema: "dbo",
                newName: "MasterAsuransi",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_MstDokterPraktek_DokterId",
                schema: "dbo",
                table: "MasterDokterPraktek",
                newName: "IX_MasterDokterPraktek_DokterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterDokterPraktek",
                schema: "dbo",
                table: "MasterDokterPraktek",
                column: "DokterPraktekId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MasterDokter",
                schema: "dbo",
                table: "MasterDokter",
                column: "DokterId");

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
    }
}
