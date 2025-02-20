using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addPeralatanandKategoriPeralatan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstKategoriPeralatan",
                schema: "dbo",
                columns: table => new
                {
                    KategoriPeralatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodeKategoriPeralatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaKategoriPeralatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_MstKategoriPeralatan", x => x.KategoriPeralatanId);
                });

            migrationBuilder.CreateTable(
                name: "MstPeralatan",
                schema: "dbo",
                columns: table => new
                {
                    PeralatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodePeralatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPeralatan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Purchase_date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Maintenance_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operational_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KategoriPeralatanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_MstPeralatan", x => x.PeralatanId);
                    table.ForeignKey(
                        name: "FK_MstPeralatan_MstKategoriPeralatan_KategoriPeralatanId",
                        column: x => x.KategoriPeralatanId,
                        principalSchema: "dbo",
                        principalTable: "MstKategoriPeralatan",
                        principalColumn: "KategoriPeralatanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPeralatan_KategoriPeralatanId",
                schema: "dbo",
                table: "MstPeralatan",
                column: "KategoriPeralatanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPeralatan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MstKategoriPeralatan",
                schema: "dbo");
        }
    }
}
