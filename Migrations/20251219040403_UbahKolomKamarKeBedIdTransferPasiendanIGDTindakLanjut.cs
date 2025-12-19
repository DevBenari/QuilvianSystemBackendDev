using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class UbahKolomKamarKeBedIdTransferPasiendanIGDTindakLanjut : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KamarId",
                table: "TransferPasiens",
                newName: "BedId");

            migrationBuilder.RenameColumn(
                name: "KamarId",
                table: "IGDTindakLanjuts",
                newName: "BedId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BedId",
                table: "TransferPasiens",
                newName: "KamarId");

            migrationBuilder.RenameColumn(
                name: "BedId",
                table: "IGDTindakLanjuts",
                newName: "KamarId");
        }
    }
}
