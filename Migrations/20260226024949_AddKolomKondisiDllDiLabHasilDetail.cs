using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomKondisiDllDiLabHasilDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Anjuran",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisPA",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KategoriGC",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kondisi",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rincian",
                table: "LabHasilDetails",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anjuran",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "DiagnosisPA",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "KategoriGC",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "Kondisi",
                table: "LabHasilDetails");

            migrationBuilder.DropColumn(
                name: "Rincian",
                table: "LabHasilDetails");
        }
    }
}
