using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamDetailICD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NamaDiagnosa",
                schema: "public",
                table: "MstICD-10",
                newName: "NamaDtd");

            migrationBuilder.AddColumn<Guid>(
                name: "SoapId",
                schema: "public",
                table: "MstDetailICD",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoapId",
                schema: "public",
                table: "MstDetailICD");

            migrationBuilder.RenameColumn(
                name: "NamaDtd",
                schema: "public",
                table: "MstICD-10",
                newName: "NamaDiagnosa");
        }
    }
}
