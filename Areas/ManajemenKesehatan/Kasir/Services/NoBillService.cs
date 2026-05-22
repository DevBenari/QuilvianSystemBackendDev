using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services
{
    public class NoBillService : INoBillService
    {
        private const int SequenceDigits = 6;
        private const int MaxSequence = 999999;

        private readonly ApplicationDbContext _db;

        public NoBillService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateNoBillAsync(
            Guid kunjunganId,
            CancellationToken cancellationToken = default)
        {
            if (kunjunganId == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak valid.");

            var kunjungan = await _db.Kunjungans
                .AsNoTracking()
                .Where(x =>
                    x.KunjunganID == kunjunganId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.JenisKunjungan,
                    x.AsalKunjungan
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (kunjungan == null)
                throw new InvalidOperationException("Data kunjungan tidak ditemukan.");

            var prefix = ResolvePrefix(
                kunjungan.JenisKunjungan,
                kunjungan.AsalKunjungan
            );

            /*
             * Wajib dipanggil di dalam transaction.
             * Lock ini mencegah dua request membuat nomor yang sama.
             */
            var lockKey = $"NO_BILL_{prefix}";

            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext({lockKey}))",
                cancellationToken
            );

            var prefixSearch = $"{prefix}-";

            var lastNoBill = await _db.MainKasirs
                .AsNoTracking()
                .Where(x =>
                    x.NoBill != null &&
                    x.NoBill.StartsWith(prefixSearch))
                .OrderByDescending(x => x.NoBill)
                .Select(x => x.NoBill)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNoBill))
            {
                var parts = lastNoBill.Split('-');

                if (parts.Length == 2 &&
                    int.TryParse(parts[1], out var lastSequence))
                {
                    nextNumber = lastSequence + 1;
                }
            }

            if (nextNumber > MaxSequence)
            {
                throw new InvalidOperationException(
                    $"Nomor bill untuk prefix {prefix} sudah mencapai batas {MaxSequence}.");
            }

            return $"{prefix}-{nextNumber.ToString($"D{SequenceDigits}")}";
        }

        private static string ResolvePrefix(
            string? jenisKunjungan,
            string? asalKunjungan)
        {
            var jenis = (jenisKunjungan ?? "").Trim().ToUpper();
            var asal = (asalKunjungan ?? "").Trim().ToUpper();

            if (jenis == "IGD" ||
                asal == "IGD" ||
                asal.Contains("GAWAT DARURAT"))
            {
                return "IGD";
            }

            if (jenis == "IP" ||
                jenis == "RI" ||
                jenis == "RANAP" ||
                jenis == "RAWAT INAP" ||
                asal == "IP" ||
                asal == "RI" ||
                asal == "RANAP" ||
                asal == "RAWAT INAP")
            {
                return "RI";
            }

            if (jenis == "OP" ||
                jenis == "RJ" ||
                jenis == "RAJAL" ||
                jenis == "RAWAT JALAN" ||
                asal == "OP" ||
                asal == "RJ" ||
                asal == "RAJAL" ||
                asal == "RAWAT JALAN")
            {
                return "RJ";
            }

            throw new InvalidOperationException(
                $"Jenis kunjungan tidak dikenali. JenisKunjungan: {jenisKunjungan}, AsalKunjungan: {asalKunjungan}");
        }
    }
}
