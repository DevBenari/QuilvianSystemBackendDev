using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtipeuser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartemenId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipeUserId",
                schema: "public",
                table: "MstUserActive",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstTipeUser",
                schema: "public",
                columns: table => new
                {
                    TipeUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeTipeUser = table.Column<string>(type: "text", nullable: false),
                    NamaTipeUser = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_MstTipeUser", x => x.TipeUserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstTipeUser",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "DepartemenId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "public",
                table: "MstUserActive");

            migrationBuilder.DropColumn(
                name: "TipeUserId",
                schema: "public",
                table: "MstUserActive");
        }
    }
}
