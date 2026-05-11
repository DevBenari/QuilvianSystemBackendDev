using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Services
{
    using Microsoft.EntityFrameworkCore;
    using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;

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

            var hasAsuransi = asuransiId.HasValue && asuransiId.Value != Guid.Empty;
            var hasExcess = asuransiExcessId.HasValue && asuransiExcessId.Value != Guid.Empty;
            var hasValidItem = !string.IsNullOrWhiteSpace(jenisBilling) &&
                               itemId.HasValue &&
                               itemId.Value != Guid.Empty;

            // 1. Jika dua-duanya ada -> utamakan AsuransiId, fallback ke Excess bila item tidak ter-cover
            if (hasAsuransi && hasExcess)
            {
                result.AsuransiId = asuransiId;

                if (!hasValidItem)
                {
                    result.IsCovered = false;
                    result.IsCoveredExcess = false;
                    return result;
                }

                var coveredByAsuransi = await CheckCoverageByJenisBillingAsync(
                    asuransiId!.Value,
                    jenisBilling!,
                    itemId!.Value,
                    ct);

                if (coveredByAsuransi)
                {
                    result.IsCovered = true;
                    result.IsCoveredExcess = false;
                    return result;
                }

                // fallback ke excess jika tidak ter-cover oleh AsuransiId
                result.AsuransiId = null;
                result.IsCovered = false;
                result.IsCoveredExcess = true;
                return result;
            }

            // 2. Jika hanya AsuransiId ada
            if (hasAsuransi)
            {
                result.AsuransiId = asuransiId;

                if (!hasValidItem)
                {
                    result.IsCovered = false;
                    result.IsCoveredExcess = false;
                    return result;
                }

                result.IsCovered = await CheckCoverageByJenisBillingAsync(
                    asuransiId!.Value,
                    jenisBilling!,
                    itemId!.Value,
                    ct);

                result.IsCoveredExcess = false;
                return result;
            }

            // 3. Jika hanya Excess ada
            if (hasExcess)
            {
                result.AsuransiId = null;
                result.IsCovered = false;
                result.IsCoveredExcess = true;
                return result;
            }

            // 4. Tidak ada keduanya -> mandiri
            return result;
        }

        public async Task RefreshCoverageBillingByKunjunganAsync(
            Guid kunjunganId,
            Guid userActiveId,
            CancellationToken ct = default)
        {
            if (kunjunganId == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak valid.");

            var billings = await _db.Billings
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(ct);

            foreach (var bill in billings)
            {
                var jenisCoverage = ResolveJenisBillingForCoverage(bill.JenisBilling);

                if (string.IsNullOrWhiteSpace(jenisCoverage) || !bill.ItemId.HasValue)
                {
                    ClearBillingCoverage(bill, userActiveId);
                    continue;
                }

                var itemIdForCoverage = await ResolveItemIdForCoverageAsync(
                    bill,
                    jenisCoverage,
                    ct
                );

                if (!itemIdForCoverage.HasValue || itemIdForCoverage.Value == Guid.Empty)
                {
                    ClearBillingCoverage(bill, userActiveId);
                    continue;
                }

                var coverage = await ResolveCoverageAsync(
                    kunjunganId: kunjunganId,
                    jenisBilling: jenisCoverage,
                    itemId: itemIdForCoverage.Value,
                    ct: ct
                );

                bill.AsuransiId = coverage.AsuransiId;
                bill.IsCovered = coverage.IsCovered;
                bill.AsuransiExcessId = coverage.AsuransiExcessId;
                bill.IsCoveredExcess = coverage.IsCoveredExcess;

                bill.UpdateDateTime = DateTimeOffset.UtcNow;
                bill.UpdateBy = userActiveId;
            }
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

        private static string? ResolveJenisBillingForCoverage(string? jenisBilling)
        {
            if (string.IsNullOrWhiteSpace(jenisBilling))
                return null;

            var jenis = jenisBilling.Trim();

            if (jenis.Equals("Tindakan", StringComparison.OrdinalIgnoreCase))
                return "Tindakan";

            /*
             * Kalau Diskon Dokter berasal dari TindakanKunjungan.IsFoC
             * dan Billing.ItemId berisi TindakanId, boleh diperlakukan sebagai Tindakan.
             * Kalau tidak ingin Diskon Dokter ikut coverage, hapus blok ini.
             */
            if (jenis.Equals("Diskon Dokter", StringComparison.OrdinalIgnoreCase))
                return "Tindakan";

            if (jenis.Equals("Pemeriksaan Lab", StringComparison.OrdinalIgnoreCase))
                return "Pemeriksaan Lab";

            if (jenis.Equals("Obat", StringComparison.OrdinalIgnoreCase))
                return "Obat";

            if (jenis.Equals("Alkes", StringComparison.OrdinalIgnoreCase))
                return "Alkes";

            if (jenis.Equals("Kamar", StringComparison.OrdinalIgnoreCase) ||
                jenis.Equals("Kamar Ranap", StringComparison.OrdinalIgnoreCase))
                return "Kamar";

            /*
             * Tidak ada coverage untuk:
             * - Biaya Admin
             * - Biaya Lain - Lain
             * - Visit Dokter
             * dll.
             */
            return null;
        }

        private async Task<Guid?> ResolveItemIdForCoverageAsync(
            Billing bill,
            string jenisCoverage,
            CancellationToken ct)
        {
            if (!bill.ItemId.HasValue || bill.ItemId.Value == Guid.Empty)
                return null;

            /*
             * Billing Pemeriksaan Lab biasanya ItemId = DetailBookingLabId,
             * sedangkan coverage asuransi butuh PemeriksaanLabId.
             */
            if (jenisCoverage == "Pemeriksaan Lab")
            {
                var pemeriksaanLabId = await _db.LabBookingDetails
                    .AsNoTracking()
                    .Where(x =>
                        x.DetailBookingLabId == bill.ItemId.Value &&
                        (x.IsDelete == false || x.IsDelete == null))
                    .Select(x => x.PemeriksaanLabId)
                    .FirstOrDefaultAsync(ct);

                return pemeriksaanLabId ?? bill.ItemId;
            }

            /*
             * Untuk jenis lain:
             * Tindakan => ItemId = TindakanId
             * Obat     => ItemId = ObatId
             * Alkes    => ItemId = ObatId / AlkesId sesuai mapping kamu
             * Kamar    => ItemId = KamarId
             */
            return bill.ItemId;
        }

        private static void ClearBillingCoverage(
            Billing bill,
            Guid userActiveId)
        {
            bill.AsuransiId = null;
            bill.IsCovered = false;
            bill.AsuransiExcessId = null;
            bill.IsCoveredExcess = false;

            bill.UpdateDateTime = DateTimeOffset.UtcNow;
            bill.UpdateBy = userActiveId;
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
