using System.Data;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services
{
    public class NoRMGeneratorService : INoRMGeneratorService
    {
        private readonly ApplicationDbContext _db;

        public NoRMGeneratorService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateNoRekamMedisAsync(CancellationToken ct = default)
        {
            var n = await NextNoRmNumberAsync(ct);
            return FormatNoRekamMedis(n);
        }

        private async Task<long> NextNoRmNumberAsync(CancellationToken ct)
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT nextval('public.no_rm_seq')";
            var result = await cmd.ExecuteScalarAsync(ct);

            return Convert.ToInt64(result);
        }

        private static string FormatNoRekamMedis(long n)
        {
            // base-100 (4 segmen)
            var s1 = n / 1_000_000;
            var s2 = (n / 10_000) % 100;
            var s3 = (n / 100) % 100;
            var s4 = n % 100;

            // selalu 2 digit per segmen (kalau s1 > 99, otomatis jadi 100 dst, tetap valid)
            return $"{s1:D2}-{s2:D2}-{s3:D2}-{s4:D2}";
        }
    }
}
