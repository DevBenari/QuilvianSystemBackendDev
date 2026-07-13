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

        public async Task<string?> GenerateNoAntrianAsync(
            string kodeJenis,
            string? asal,
            Guid? poliklinikId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(kodeJenis))
                throw new ArgumentException("Kode jenis kunjungan tidak valid.");

            kodeJenis = kodeJenis.Trim();

            var asalNormalized = (asal ?? "").Trim().ToUpperInvariant();

            var now = GetLocalNow();

            var startToday = new DateTimeOffset(now.Date);
            var endToday = startToday.AddDays(1);

            /*
             * IGD dan IP tidak pakai nomor antrean.
             */
            if (kodeJenis == "IGD" || kodeJenis == "IP")
                return null;

            /*
             * Jika jenis kunjungan OP tapi asal dari IGD,
             * maka tidak perlu dibuatkan nomor antrean poli.
             */
            if (kodeJenis == "OP" && asalNormalized == "IGD")
                return null;

            /*
             * OPLab dan OPRad tidak wajib PoliklinikId,
             * jadi prefix-nya dari jenis kunjungan.
             */
            if (kodeJenis == "OPLab" || kodeJenis == "OPRad")
            {
                var prefix = kodeJenis == "OPLab"
                    ? "LAB"
                    : "RAD";

                var lockKey = $"KUNJUNGAN_ANTRIAN_{now:yyMMdd}_{kodeJenis.ToUpperInvariant()}";

                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext({lockKey}))",
                    cancellationToken
                );

                var jumlahAntrianHariIni = await _db.Kunjungans
                    .AsNoTracking()
                    .CountAsync(k =>
                        k.JenisKunjungan == kodeJenis &&
                        k.CreateDateTime >= startToday &&
                        k.CreateDateTime < endToday &&
                        !k.IsDelete,
                        cancellationToken);

                var nomorAntrian = jumlahAntrianHariIni + 1;

                if (nomorAntrian > 999)
                    throw new InvalidOperationException($"Nomor antrean {prefix} hari ini sudah mencapai batas 999.");

                return $"{prefix}{nomorAntrian:000}";
            }

            /*
             * Rawat Jalan / OP dari non-IGD tetap logic lama:
             * nomor antrean berdasarkan Poliklinik.KodeAntreanPoli.
             */
            if (kodeJenis == "OP")
            {
                if (!poliklinikId.HasValue || poliklinikId.Value == Guid.Empty)
                    throw new ArgumentException("Poliklinik wajib dipilih untuk membuat nomor antrean.");

                var lockKey = $"KUNJUNGAN_ANTRIAN_POLI_{now:yyMMdd}_{poliklinikId.Value}";

                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext({lockKey}))",
                    cancellationToken
                );

                var kodePoli = await _db.Polikliniks
                    .AsNoTracking()
                    .Where(p => p.PoliklinikId == poliklinikId.Value)
                    .Select(p => p.KodeAntreanPoli)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(kodePoli))
                    throw new InvalidOperationException("Kode antrean poli tidak ditemukan untuk poliklinik ini.");

                var jumlahAntrianHariIni = await _db.Kunjungans
                    .AsNoTracking()
                    .CountAsync(k =>
                        k.PoliklinikId == poliklinikId.Value &&
                        k.CreateDateTime >= startToday &&
                        k.CreateDateTime < endToday &&
                        !k.IsDelete,
                        cancellationToken);

                var nomorAntrian = jumlahAntrianHariIni + 1;

                if (nomorAntrian > 999)
                    throw new InvalidOperationException($"Nomor antrean poli {kodePoli} hari ini sudah mencapai batas 999.");

                return $"{kodePoli}{nomorAntrian:000}";
            }

            throw new ArgumentException("Kode jenis kunjungan tidak valid untuk nomor antrean.");
        }

        public string ValidasiJenisKunjungan(
            string? jenisKunjungan,
            string? asal,
            Guid? poliklinikId,
            decimal? depositRanap)
        {
            var inputJenis = string.IsNullOrWhiteSpace(jenisKunjungan) ||
                             jenisKunjungan.Equals("string", StringComparison.OrdinalIgnoreCase)
                ? "Rawat Jalan"
                : jenisKunjungan.Trim();

            if (!new[] { "Rawat Inap", "Rawat Jalan", "IGD", "OPLab", "OPRad" }
                .Contains(inputJenis, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap', 'Rawat Jalan', 'IGD', 'OPLab', atau 'OPRad'.");
            }

            string kodeJenis;

            if (inputJenis.Equals("Rawat Inap", StringComparison.OrdinalIgnoreCase))
            {
                kodeJenis = "IP";
            }
            else if (inputJenis.Equals("Rawat Jalan", StringComparison.OrdinalIgnoreCase))
            {
                kodeJenis = "OP";
            }
            else if (inputJenis.Equals("IGD", StringComparison.OrdinalIgnoreCase))
            {
                kodeJenis = "IGD";
            }
            else if (inputJenis.Equals("OPLab", StringComparison.OrdinalIgnoreCase))
            {
                kodeJenis = "OPLab";
            }
            else if (inputJenis.Equals("OPRad", StringComparison.OrdinalIgnoreCase))
            {
                kodeJenis = "OPRad";
            }
            else
            {
                throw new ArgumentException("Jenis kunjungan tidak valid.");
            }

            if (kodeJenis == "IP" &&
                (depositRanap == null || depositRanap <= 0))
            {
                throw new ArgumentException(
                    "Kunjungan IP (rawat inap) wajib mengisi nominal deposit.");
            }

            if (kodeJenis == "OP" &&
                (poliklinikId == null || poliklinikId == Guid.Empty) && asal != "IGD")
            {
                throw new ArgumentException(
                    "Poliklinik wajib dipilih untuk kunjungan Rawat Jalan.");
            }

            return kodeJenis;
        }

        private static DateTime GetLocalNow()
        {
            return DateTime.Now;
        }
    }
}
