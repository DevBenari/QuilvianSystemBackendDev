using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditKolomSDKIDiagnosa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SDKIDiagnosaGroupId",
                schema: "public",
                table: "SDKIDiagnosa");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SDKIDiagnosaGroupId",
                schema: "public",
                table: "SDKIDiagnosa",
                type: "uuid",
                nullable: true);
        }
    }
}
