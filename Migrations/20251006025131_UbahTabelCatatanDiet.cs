using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahTabelCatatanDiet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatatanDietDetails");

            migrationBuilder.AddColumn<string>(
                name: "Diagnosa",
                table: "CatatanDiets",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diagnosa",
                table: "CatatanDiets");

            migrationBuilder.CreateTable(
                name: "CatatanDietDetails",
                columns: table => new
                {
                    CatatanDietDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatatanDietId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Icd10Id = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatatanDietDetails", x => x.CatatanDietDetailId);
                    table.ForeignKey(
                        name: "FK_CatatanDietDetails_CatatanDiets_CatatanDietId",
                        column: x => x.CatatanDietId,
                        principalTable: "CatatanDiets",
                        principalColumn: "CatatanDietId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatatanDietDetails_CatatanDietId",
                table: "CatatanDietDetails",
                column: "CatatanDietId");
        }
    }
}
