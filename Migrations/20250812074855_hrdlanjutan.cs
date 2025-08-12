using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class hrdlanjutan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hrd_MstGradePay",
                schema: "public",
                columns: table => new
                {
                    GradePayId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeGrade = table.Column<string>(type: "text", nullable: false),
                    MinSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hrd_MstGradePay", x => x.GradePayId);
                });

            migrationBuilder.CreateTable(
                name: "Hrd_MstLevel",
                schema: "public",
                columns: table => new
                {
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeLevel = table.Column<string>(type: "text", nullable: false),
                    MinSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hrd_MstLevel", x => x.LevelId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_MstGradePay",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Hrd_MstLevel",
                schema: "public");
        }
    }
}
