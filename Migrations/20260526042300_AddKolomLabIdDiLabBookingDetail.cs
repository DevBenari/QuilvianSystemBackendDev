using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomLabIdDiLabBookingDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LabId",
                schema: "public",
                table: "LabBookingDetail",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabBookingDetail_LabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "LabId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabBookingDetail_MstLab_LabId",
                schema: "public",
                table: "LabBookingDetail",
                column: "LabId",
                principalSchema: "public",
                principalTable: "MstLab",
                principalColumn: "LabId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabBookingDetail_MstLab_LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropIndex(
                name: "IX_LabBookingDetail_LabId",
                schema: "public",
                table: "LabBookingDetail");

            migrationBuilder.DropColumn(
                name: "LabId",
                schema: "public",
                table: "LabBookingDetail");
        }
    }
}
