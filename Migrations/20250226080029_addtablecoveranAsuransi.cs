using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtablecoveranAsuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Foto",
                schema: "dbo",
                table: "PdfPasienBaru",
                newName: "FotoName");

            migrationBuilder.AddColumn<string>(
                name: "FotoBase64",
                schema: "dbo",
                table: "PdfPasienBaru",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoBase64",
                schema: "dbo",
                table: "MstDokter",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPKS",
                schema: "dbo",
                table: "MstAsuransi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MstCoveranAsuransi",
                schema: "dbo",
                columns: table => new
                {
                    CoveranAsuransiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeCoveranAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaAsuransi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceDesc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceCodeClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSurgery = table.Column<bool>(type: "bit", nullable: false),
                    Tarif = table.Column<int>(type: "int", nullable: false),
                    TglBerlaku = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TglBerakhir = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPKS = table.Column<bool>(type: "bit", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MstCoveranAsuransi", x => x.CoveranAsuransiId);
                    table.ForeignKey(
                        name: "FK_MstCoveranAsuransi_MstAsuransi_AsuransiId",
                        column: x => x.AsuransiId,
                        principalSchema: "dbo",
                        principalTable: "MstAsuransi",
                        principalColumn: "AsuransiId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstCoveranAsuransi_AsuransiId",
                schema: "dbo",
                table: "MstCoveranAsuransi",
                column: "AsuransiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstCoveranAsuransi",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "FotoBase64",
                schema: "dbo",
                table: "PdfPasienBaru");

            migrationBuilder.DropColumn(
                name: "FotoBase64",
                schema: "dbo",
                table: "MstDokter");

            migrationBuilder.DropColumn(
                name: "IsPKS",
                schema: "dbo",
                table: "MstAsuransi");

            migrationBuilder.RenameColumn(
                name: "FotoName",
                schema: "dbo",
                table: "PdfPasienBaru",
                newName: "Foto");
        }
    }
}
