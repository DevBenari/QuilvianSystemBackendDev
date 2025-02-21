using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addDepartementandPositions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDepartement",
                schema: "dbo",
                columns: table => new
                {
                    DepartementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeDepartement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaDepartement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KepalaDepartement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lokasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telepon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamBuka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JamTutup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Layanan = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_MstDepartement", x => x.DepartementId);
                });

            migrationBuilder.CreateTable(
                name: "MstPosition",
                schema: "dbo",
                columns: table => new
                {
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MstPosition", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_MstPosition_MstDepartement_DepartementId",
                        column: x => x.DepartementId,
                        principalSchema: "dbo",
                        principalTable: "MstDepartement",
                        principalColumn: "DepartementId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPosition_DepartementId",
                schema: "dbo",
                table: "MstPosition",
                column: "DepartementId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPosition",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstDepartement",
                schema: "dbo");
        }
    }
}
