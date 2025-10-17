using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddCatatanDiet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserActiveId",
                table: "ResumePulangDetails");

            migrationBuilder.CreateTable(
                name: "CatatanDiets",
                columns: table => new
                {
                    CatatanDietId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    Diet = table.Column<string>(type: "text", nullable: true),
                    StatusDiet = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    TglCatatanDiet = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatatanDiets", x => x.CatatanDietId);
                });

            migrationBuilder.CreateTable(
                name: "CatatanDietDetail",
                columns: table => new
                {
                    CatatanDietDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatatanDietId = table.Column<Guid>(type: "uuid", nullable: false),
                    Icd10Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatatanDietDetail", x => x.CatatanDietDetailId);
                    table.ForeignKey(
                        name: "FK_CatatanDietDetail_CatatanDiets_CatatanDietId",
                        column: x => x.CatatanDietId,
                        principalTable: "CatatanDiets",
                        principalColumn: "CatatanDietId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatatanDietDetail_CatatanDietId",
                table: "CatatanDietDetail",
                column: "CatatanDietId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatatanDietDetail");

            migrationBuilder.DropTable(
                name: "CatatanDiets");

            migrationBuilder.AddColumn<Guid>(
                name: "UserActiveId",
                table: "ResumePulangDetails",
                type: "uuid",
                nullable: true);
        }
    }
}
