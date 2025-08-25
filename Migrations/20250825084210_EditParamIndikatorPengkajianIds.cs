using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class EditParamIndikatorPengkajianIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndikatorPengkajianId",
                table: "DetailKetergantungans");

            migrationBuilder.AddColumn<Guid[]>(
                name: "IndikatorPengkajianIds",
                table: "DetailKetergantungans",
                type: "uuid[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndikatorPengkajianIds",
                table: "DetailKetergantungans");

            migrationBuilder.AddColumn<Guid>(
                name: "IndikatorPengkajianId",
                table: "DetailKetergantungans",
                type: "uuid",
                nullable: true);
        }
    }
}
