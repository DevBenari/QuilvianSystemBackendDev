using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Services
{
    using Microsoft.EntityFrameworkCore;

    public class AsuransiCoverageService : IAsuransiCoverageService
    {
        private readonly ApplicationDbContext _db;

        public AsuransiCoverageService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AsuransiCoverageResult> ResolveCoverageAsync(
            Guid? kunjunganId,
            string? jenisBilling,
            Guid? itemId = null,
            CancellationToken ct = default)
        {
            var result = new AsuransiCoverageResult
            {
                AsuransiId = null,
                AsuransiExcessId = null,
                IsCovered = false,
                IsCoveredExcess = false
            };

            if (kunjunganId == null || kunjunganId == Guid.Empty)
                return result;

            var kunjungan = await _db.Kunjungans
                .AsNoTracking()
                .Where(x => x.KunjunganID == kunjunganId &&
                            (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new
                {
                    x.AsuransiId,
                    x.AsuransiExcessId
                })
                .FirstOrDefaultAsync(ct);

            if (kunjungan == null)
                return result;

            var asuransiId = kunjungan.AsuransiId;
            var asuransiExcessId = kunjungan.AsuransiExcessId;

            result.AsuransiExcessId = asuransiExcessId;

            // =========================================================
            // PRIORITAS 1: ASURANSI ID
            // Jika AsuransiId ada, maka ini yang dipakai lebih dulu.
            // =========================================================
            if (asuransiId.HasValue && asuransiId.Value != Guid.Empty)
            {
                result.AsuransiId = asuransiId;
                result.IsCoveredExcess = false;

                // kalau jenis billing / item belum valid, tetap return dengan AsuransiId terisi
                if (string.IsNullOrWhiteSpace(jenisBilling) || itemId == null || itemId == Guid.Empty)
                {
                    result.IsCovered = false;
                    return result;
                }

                result.IsCovered = await CheckCoverageByJenisBillingAsync(
                    asuransiId.Value,
                    jenisBilling,
                    itemId.Value,
                    ct);

                return result;
            }

            // =========================================================
            // PRIORITAS 2: ASURANSI EXCESS
            // Dipakai hanya jika AsuransiId kosong.
            // Semua billing dianggap excess.
            // =========================================================
            if (asuransiExcessId.HasValue && asuransiExcessId.Value != Guid.Empty)
            {
                result.AsuransiId = null;
                result.IsCovered = false;
                result.IsCoveredExcess = true;
                return result;
            }

            // =========================================================
            // Tidak ada asuransi maupun excess => pasien mandiri
            // =========================================================
            return result;
        }

        private async Task<bool> CheckCoverageByJenisBillingAsync(
            Guid asuransiId,
            string jenisBilling,
            Guid itemId,
            CancellationToken ct)
        {
            switch (jenisBilling.Trim())
            {
                case "Pemeriksaan Lab":
                    return await _db.PemeriksaanAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.PemeriksaanLabId == itemId &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                case "Obat":
                    return await _db.ObatAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.ObatId == itemId &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                case "Alkes":
                    return await _db.ObatAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.ObatId == itemId &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                case "Tindakan":
                    return await _db.TindakanAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.TindakanId == itemId &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                case "Kamar":
                    return await _db.KamarAsuransis
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.AsuransiId == asuransiId &&
                            x.KamarId == itemId &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                default:
                    return false;
            }
        }
    }

    public class AsuransiCoverageResult
    {
        public Guid? AsuransiId { get; set; }
        public Guid? AsuransiExcessId { get; set; }

        public bool IsCovered { get; set; }
        public bool IsCoveredExcess { get; set; }

        public bool HasAsuransi => AsuransiId.HasValue && AsuransiId != Guid.Empty;
        public bool HasAsuransiExcess => AsuransiExcessId.HasValue && AsuransiExcessId != Guid.Empty;
    }
}
