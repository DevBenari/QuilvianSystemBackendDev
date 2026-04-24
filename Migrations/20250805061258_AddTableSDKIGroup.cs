using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableSDKIGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SDKIDiagnosaGroupId",
                schema: "public",
                table: "SDKIDiagnosa",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SDKIGroup",
                schema: "public",
                columns: table => new
                {
                    SDKIGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaGroupSDKI = table.Column<string>(type: "text", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_SDKIGroup", x => x.SDKIGroupId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SDKIGroup",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "SDKIDiagnosaGroupId",
                schema: "public",
                table: "SDKIDiagnosa");
        }
    }
}
