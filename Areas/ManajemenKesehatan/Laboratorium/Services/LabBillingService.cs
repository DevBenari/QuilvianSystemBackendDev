using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services
{
    public interface ILabBillingService
    {
        Task<int> EnsureLabBillingOnConfirmationAsync(
            Guid bookingLabId,
            Guid userActiveId,
            CancellationToken cancellationToken = default);
    }
    public class LabBillingService : ILabBillingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAsuransiCoverageService _asuransiCoverageService;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;

        public LabBillingService(
            ApplicationDbContext context,
            IAsuransiCoverageService asuransiCoverageService,
            IGenerateInvoiceBillingService generateInvoiceBillingService)
        {
            _context = context;
            _asuransiCoverageService = asuransiCoverageService;
            _generateInvoiceBillingService = generateInvoiceBillingService;
        }

        public async Task<int> EnsureLabBillingOnConfirmationAsync(
            Guid bookingLabId,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            if (bookingLabId == Guid.Empty)
                throw new Exception("BookingLabId tidak valid.");

            var booking = await _context.LabBookings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.BookingLabId == bookingLabId &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            if (booking == null)
                throw new Exception("Booking lab tidak ditemukan.");

            if (!booking.KunjunganId.HasValue)
                throw new Exception("KunjunganId pada booking lab kosong.");

            var kunjunganId = booking.KunjunganId.Value;

            var items = await (
                from d in _context.LabBookingDetails.AsNoTracking()
                join p in _context.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals (Guid?)p.PemeriksaanLabId
                where d.BookingLabId == bookingLabId
                      && d.PemeriksaanLabId != null
                      && (d.IsDelete == false || d.IsDelete == null)
                select new
                {
                    d.DetailBookingLabId,
                    PemeriksaanLabId = d.PemeriksaanLabId!.Value,
                    d.TipeLayanan,
                    d.QtyOrder,

                    p.NamaPemeriksaan,
                    p.HargaPemeriksaan
                }
            ).ToListAsync(cancellationToken);

            if (!items.Any())
                return 0;

            /*
             * Gabungkan pemeriksaan yang sama dalam satu booking.
             *
             * Contoh:
             * - Darah Lengkap qty 1
             * - Darah Lengkap qty 2
             *
             * Akan diproses menjadi:
             * - Darah Lengkap qty 3
             */
            var groupedItems = items
                .GroupBy(x => x.PemeriksaanLabId)
                .Select(group =>
                {
                    var firstItem = group.First();

                    var totalQty = group.Sum(x =>
                        x.QtyOrder.HasValue && x.QtyOrder.Value > 0
                            ? Convert.ToInt32(x.QtyOrder.Value)
                            : 1);

                    return new
                    {
                        PemeriksaanLabId = group.Key,
                        firstItem.NamaPemeriksaan,
                        firstItem.HargaPemeriksaan,
                        firstItem.TipeLayanan,
                        Qty = totalQty
                    };
                })
                .ToList();

            var pemeriksaanIds = groupedItems
                .Select(x => x.PemeriksaanLabId)
                .ToList();

            /*
             * Jangan gunakan AsNoTracking karena data billing yang ditemukan
             * akan diperbarui.
             */
            var existingBillings = await _context.Billings
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    x.ItemId.HasValue &&
                    pemeriksaanIds.Contains(x.ItemId.Value) &&
                    x.BillingKode == "LAB" &&
                    x.JenisBilling == "Pemeriksaan Lab" &&
                    x.StatusBilling == false &&
                    (x.IsDelete == false || x.IsDelete == null))
                .ToListAsync(cancellationToken);

            /*
             * GroupBy digunakan untuk mengantisipasi jika sebelumnya sudah terdapat
             * billing duplikat di database.
             *
             * Billing pertama akan menjadi billing utama yang diperbarui.
             */
            var existingBillingMap = existingBillings
                .Where(x => x.ItemId.HasValue)
                .GroupBy(x => x.ItemId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(x => x.CreateDateTime)
                        .First());

            var affectedCount = 0;

            foreach (var item in groupedItems)
            {
                var pemeriksaanLabId = item.PemeriksaanLabId;
                var qtyTambahan = item.Qty;

                var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                    kunjunganId,
                    "Pemeriksaan Lab",
                    pemeriksaanLabId,
                    cancellationToken);

                /*
                 * Jika item yang sama sudah ada pada billing,
                 * tambahkan quantity tanpa membuat baris baru.
                 */
                if (existingBillingMap.TryGetValue(
                        pemeriksaanLabId,
                        out var existingBilling))
                {
                    var qtySebelumnya = Convert.ToInt32(existingBilling.QtyItem);
                    var qtyBaru = qtySebelumnya + qtyTambahan;

                    /*
                     * Utamakan harga yang sudah tersimpan pada billing agar perubahan
                     * harga master tidak mengubah transaksi yang sudah terbentuk.
                     */
                    var hargaSatuan = Convert.ToDecimal(existingBilling.HargaItem);

                    if (hargaSatuan <= 0)
                    {
                        hargaSatuan = item.HargaPemeriksaan ?? 0m;
                    }

                    existingBilling.QtyItem = qtyBaru;
                    existingBilling.HargaItem = hargaSatuan;
                    existingBilling.SubTotalItem = hargaSatuan * qtyBaru;

                    existingBilling.NamaItem = item.NamaPemeriksaan;
                    existingBilling.TipeLayanan = item.TipeLayanan;

                    existingBilling.IsCovered = coverage?.IsCovered;
                    existingBilling.IsCoveredExcess = coverage?.IsCoveredExcess;
                    existingBilling.AsuransiId = coverage?.AsuransiId;
                    existingBilling.AsuransiExcessId = coverage?.AsuransiExcessId;

                    affectedCount++;
                    continue;
                }

                /*
                 * Jika belum ada, buat billing pemeriksaan baru.
                 */
                var harga = item.HargaPemeriksaan ?? 0m;

                var billing = new Billing
                {
                    BillingId = Guid.NewGuid(),

                    KunjunganId = kunjunganId,
                    ItemId = pemeriksaanLabId,
                    NamaItem = item.NamaPemeriksaan,

                    HargaItem = harga,
                    QtyItem = qtyTambahan,
                    SubTotalItem = harga * qtyTambahan,

                    InvoiceBilling = await _generateInvoiceBillingService
                        .GetOrCreateAsync(
                            kunjunganId,
                            DateTime.UtcNow),

                    IsListWhiteOff = false,

                    BillingKode = "LAB",
                    JenisBilling = "Pemeriksaan Lab",

                    StatusBilling = false,
                    TipeLayanan = item.TipeLayanan,

                    BillingDate = DateTime.UtcNow,
                    TanggalInvoice = DateTime.UtcNow,
                    TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                    IsCovered = coverage?.IsCovered,
                    IsCoveredExcess = coverage?.IsCoveredExcess,
                    AsuransiId = coverage?.AsuransiId,
                    AsuransiExcessId = coverage?.AsuransiExcessId,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _context.Billings.Add(billing);

                /*
                 * Masukkan billing baru ke dictionary agar pada proses yang sama
                 * tidak dibuat billing duplikat.
                 */
                existingBillingMap[pemeriksaanLabId] = billing;

                affectedCount++;
            }

            if (affectedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return affectedCount;
        }
    }
}
