using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class ubahTipeKolomIGDTindakLanjut : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ubah WaktuDirujuk dengan menambahkan tanggal default
            migrationBuilder.Sql(
                "ALTER TABLE \"IGDTindakLanjuts\" " +
                "ALTER COLUMN \"WaktuDirujuk\" TYPE timestamp with time zone " +
                "USING (CURRENT_DATE + \"WaktuDirujuk\")::timestamp with time zone"
            );

            // Ubah WaktuDipulangkan dengan menambahkan tanggal default
            migrationBuilder.Sql(
                "ALTER TABLE \"IGDTindakLanjuts\" " +
                "ALTER COLUMN \"WaktuDipulangkan\" TYPE timestamp with time zone " +
                "USING (CURRENT_DATE + \"WaktuDipulangkan\")::timestamp with time zone"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mengembalikan tipe ke 'time without time zone' saat rollback migrasi
            migrationBuilder.Sql(
                "ALTER TABLE \"IGDTindakLanjuts\" " +
                "ALTER COLUMN \"WaktuDirujuk\" TYPE time without time zone " +
                "USING \"WaktuDirujuk\"::time without time zone"
            );

            migrationBuilder.Sql(
                "ALTER TABLE \"IGDTindakLanjuts\" " +
                "ALTER COLUMN \"WaktuDipulangkan\" TYPE time without time zone " +
                "USING \"WaktuDipulangkan\"::time without time zone"
            );
        }
    }
}
