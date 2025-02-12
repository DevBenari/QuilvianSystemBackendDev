using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class Jabatans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstJabatan",
                schema: "dbo",
                columns: table => new
                {
                    JabatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JabatanKode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JenisJabatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_MstJabatan", x => x.JabatanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstJabatan",
                schema: "dbo");
        }
    }
}
