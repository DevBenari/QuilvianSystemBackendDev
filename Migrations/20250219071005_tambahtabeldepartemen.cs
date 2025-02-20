using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class tambahtabeldepartemen : Migration
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
                    JamBuka = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamTutup = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Layanan = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDepartement",
                schema: "dbo");
        }
    }
}
