using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class obatpelengkapbentuk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstBentukObat",
                schema: "public",
                columns: table => new
                {
                    BentukObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeBentukObat = table.Column<string>(type: "text", nullable: false),
                    NamaBentukObat = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstBentukObat", x => x.BentukObatId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstBentukObat",
                schema: "public");
        }
    }
}
