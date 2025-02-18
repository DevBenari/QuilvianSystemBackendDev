using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class userrole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoleDepartemen",
                schema: "dbo",
                columns: table => new
                {
                    RoleDepartemenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolePositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleDepartemen", x => x.RoleDepartemenId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleUser",
                schema: "dbo",
                columns: table => new
                {
                    RoleUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleUser", x => x.RoleUserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleDepartemen",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetRoleUser",
                schema: "dbo");
        }
    }
}
