using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addICollectionAlatPemakaian : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlatPemakaianDetails_AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails",
                column: "AlatPemakaianPemakaianAlatId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlatPemakaianDetails_AlatPemakaians_AlatPemakaianPemakaianA~",
                table: "AlatPemakaianDetails",
                column: "AlatPemakaianPemakaianAlatId",
                principalTable: "AlatPemakaians",
                principalColumn: "PemakaianAlatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlatPemakaianDetails_AlatPemakaians_AlatPemakaianPemakaianA~",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropIndex(
                name: "IX_AlatPemakaianDetails_AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails");

            migrationBuilder.DropColumn(
                name: "AlatPemakaianPemakaianAlatId",
                table: "AlatPemakaianDetails");
        }
    }
}
