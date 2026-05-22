using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services
{
    public class KunjunganNoRegistrasiService : IKunjunganNoRegistrasiService
    {
        private readonly ApplicationDbContext _db;

        public KunjunganNoRegistrasiService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateNoRegistrasiAsync(
            CancellationToken cancellationToken = default)
        {
            var now = GetLocalNow();

            var prefix = now.ToString("yyMMdd");
            var lockKey = $"KUNJUNGAN_NOREG_{prefix}";

            /*
             * Penting:
             * pg_advisory_xact_lock hanya efektif jika function ini dipanggil
             * di dalam transaction.
             */
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext({lockKey}))",
                cancellationToken
            );

            var lastNoRegistrasi = await _db.Kunjungans
                .AsNoTracking()
                .Where(x =>
                    x.NoRegistrasi != null &&
                    x.NoRegistrasi.StartsWith(prefix))
                .OrderByDescending(x => x.NoRegistrasi)
                .Select(x => x.NoRegistrasi)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNoRegistrasi) &&
                lastNoRegistrasi.Length >= 10)
            {
                var lastSequenceText = lastNoRegistrasi.Substring(6, 4);

                if (int.TryParse(lastSequenceText, out var lastSequence))
                {
                    nextNumber = lastSequence + 1;
                }
            }

            if (nextNumber > 9999)
            {
                throw new InvalidOperationException(
                    $"Nomor registrasi kunjungan untuk tanggal {prefix} sudah mencapai batas 9999.");
            }

            return $"{prefix}{nextNumber:D4}";
        }

        private static DateTime GetLocalNow()
        {
            /*
             * Jika server kamu sudah pakai timezone Indonesia,
             * DateTime.Now sudah cukup.
             */
            return DateTime.Now;
        }
    }
}
