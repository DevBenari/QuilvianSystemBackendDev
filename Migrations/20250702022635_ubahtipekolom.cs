using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ubahtipekolom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tambah kolom baru bertipe bool
            migrationBuilder.AddColumn<bool>(
                name: "StatusPengambilanBool",
                table: "MstResep",
                type: "boolean",
                nullable: true); // gunakan nullable sementara

            // 2. Isi nilai bool berdasarkan nilai string sebelumnya
            migrationBuilder.Sql(@"
        UPDATE ""MstResep""
        SET ""StatusPengambilanBool"" = 
            CASE 
                WHEN ""StatusPengambilanResep"" = 'Diambil Semua' THEN true
                ELSE false
            END;
    ");

            // 3. Hapus kolom lama
            migrationBuilder.DropColumn(
                name: "StatusPengambilanResep",
                table: "MstResep");

            // 4. Rename kolom baru menjadi nama asli
            migrationBuilder.RenameColumn(
                name: "StatusPengambilanBool",
                table: "MstResep",
                newName: "StatusPengambilanResep");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tambah kembali kolom string
            migrationBuilder.AddColumn<string>(
                name: "StatusPengambilanResep",
                table: "MstResep",
                type: "text",
                nullable: true);

            // Hapus kolom bool (karena rollback)
            migrationBuilder.DropColumn(
                name: "StatusPengambilanResep",
                table: "MstResep");
        }
    }
}
