using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev
    .Areas.ManajemenKesehatan.Laboratorium.Services
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
        private readonly IAsuransiCoverageService
            _asuransiCoverageService;
        private readonly IGenerateInvoiceBillingService
            _generateInvoiceBillingService;

        public LabBillingService(
            ApplicationDbContext context,
            IAsuransiCoverageService asuransiCoverageService,
            IGenerateInvoiceBillingService generateInvoiceBillingService)
        {
            _context = context;
            _asuransiCoverageService =
                asuransiCoverageService;
            _generateInvoiceBillingService =
                generateInvoiceBillingService;
        }

        public async Task<int> EnsureLabBillingOnConfirmationAsync(
            Guid bookingLabId,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            if (bookingLabId == Guid.Empty)
            {
                throw new ArgumentException(
                    "BookingLabId tidak valid.",
                    nameof(bookingLabId));
            }

            if (userActiveId == Guid.Empty)
            {
                throw new ArgumentException(
                    "UserActiveId tidak valid.",
                    nameof(userActiveId));
            }

            /*
             * Ambil booking untuk mendapatkan KunjunganId.
             */
            var booking = await _context.LabBookings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.BookingLabId == bookingLabId &&
                        (x.IsDelete == false ||
                         x.IsDelete == null),
                    cancellationToken);

            if (booking == null)
            {
                throw new KeyNotFoundException(
                    "Booking lab tidak ditemukan.");
            }

            if (!booking.KunjunganId.HasValue ||
                booking.KunjunganId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "KunjunganId pada booking lab kosong.");
            }

            var kunjunganId =
                booking.KunjunganId.Value;

            /*
             * Ambil seluruh detail pemeriksaan aktif dari seluruh
             * booking yang berada pada kunjungan yang sama.
             *
             * Ini penting agar:
             * - pemeriksaan sama dari booking berbeda tetap digabung;
             * - pemanggilan ulang service tidak menggandakan quantity;
             * - Qty billing selalu sesuai total detail aktif.
             */
            var items = await (
                from lb in _context.LabBookings.AsNoTracking()

                join d in _context.LabBookingDetails.AsNoTracking()
                    on (Guid?)lb.BookingLabId
                    equals d.BookingLabId

                join p in _context.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId
                    equals (Guid?)p.PemeriksaanLabId

                where lb.KunjunganId == kunjunganId
                      && d.PemeriksaanLabId.HasValue
                      && (lb.IsDelete == false ||
                          lb.IsDelete == null)
                      && (d.IsDelete == false ||
                          d.IsDelete == null)

                select new
                {
                    d.DetailBookingLabId,

                    PemeriksaanLabId =
                        d.PemeriksaanLabId!.Value,

                    d.TipeLayanan,
                    d.QtyOrder,
                    d.CreateDateTime,

                    p.NamaPemeriksaan,
                    p.HargaPemeriksaan
                }
            ).ToListAsync(cancellationToken);

            if (!items.Any())
            {
                return 0;
            }

            /*
             * Gabungkan pemeriksaan yang sama berdasarkan
             * PemeriksaanLabId.
             */
            var groupedItems = items
                .GroupBy(x => x.PemeriksaanLabId)
                .Select(group =>
                {
                    var latestItem = group
                        .OrderByDescending(x => x.CreateDateTime)
                        .First();

                    var totalQty = group.Sum(x =>
                        x.QtyOrder.HasValue &&
                        x.QtyOrder.Value > 0
                            ? Convert.ToInt32(x.QtyOrder.Value)
                            : 1);

                    return new
                    {
                        PemeriksaanLabId = group.Key,

                        latestItem.NamaPemeriksaan,
                        latestItem.HargaPemeriksaan,
                        latestItem.TipeLayanan,

                        Qty = totalQty
                    };
                })
                .ToList();

            var pemeriksaanIds = groupedItems
                .Select(x => x.PemeriksaanLabId)
                .Distinct()
                .ToList();

            /*
             * Cari billing pemeriksaan lab yang belum dibayar.
             *
             * StatusBilling != true akan mengambil:
             * - false
             * - null
             *
             * tetapi tidak mengambil billing yang sudah lunas.
             */
            var existingBillings = await _context.Billings
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    x.ItemId.HasValue &&
                    pemeriksaanIds.Contains(x.ItemId.Value) &&
                    x.BillingKode == "LAB" &&
                    x.JenisBilling == "Pemeriksaan Lab" &&
                    x.StatusBilling != true &&
                    (x.IsDelete == false ||
                     x.IsDelete == null))
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            /*
             * Antisipasi apabila data lama memiliki billing duplikat.
             * Billing pertama digunakan sebagai billing utama.
             */
            var existingBillingMap = existingBillings
                .Where(x => x.ItemId.HasValue)
                .GroupBy(x => x.ItemId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

            var affectedCount = 0;
            string? invoiceBilling = null;

            foreach (var item in groupedItems)
            {
                var pemeriksaanLabId =
                    item.PemeriksaanLabId;

                var qtyFinal =
                    item.Qty > 0
                        ? item.Qty
                        : 1;

                var coverage =
                    await _asuransiCoverageService
                        .ResolveCoverageAsync(
                            kunjunganId,
                            "Pemeriksaan Lab",
                            pemeriksaanLabId,
                            cancellationToken);

                /*
                 * Billing sudah ada:
                 * sinkronkan quantity dengan total detail aktif.
                 *
                 * Jangan ditambah dengan QtyItem lama karena akan
                 * menggandakan quantity saat service dipanggil ulang.
                 */
                if (existingBillingMap.TryGetValue(
                        pemeriksaanLabId,
                        out var existingBilling))
                {
                    var hargaSatuan =
                        existingBilling.HargaItem ??
                        item.HargaPemeriksaan ??
                        0m;

                    existingBilling.QtyItem =
                        qtyFinal;

                    existingBilling.HargaItem =
                        hargaSatuan;

                    existingBilling.SubTotalItem =
                        hargaSatuan * qtyFinal;

                    existingBilling.NamaItem =
                        item.NamaPemeriksaan;

                    existingBilling.TipeLayanan =
                        item.TipeLayanan;

                    existingBilling.IsCovered =
                        coverage?.IsCovered;

                    existingBilling.IsCoveredExcess =
                        coverage?.IsCoveredExcess;

                    existingBilling.AsuransiId =
                        coverage?.AsuransiId;

                    existingBilling.AsuransiExcessId =
                        coverage?.AsuransiExcessId;

                    existingBilling.UpdateBy =
                        userActiveId;

                    existingBilling.UpdateDateTime =
                        DateTimeOffset.UtcNow;

                    affectedCount++;
                    continue;
                }

                /*
                 * Billing belum ada:
                 * buat satu baris billing baru.
                 */
                var harga =
                    item.HargaPemeriksaan ?? 0m;

                var now =
                    DateTime.UtcNow;

                /*
                 * Invoice cukup dibuat satu kali untuk seluruh
                 * billing baru dalam proses ini.
                 */
                if (string.IsNullOrWhiteSpace(invoiceBilling))
                {
                    invoiceBilling =
                        await _generateInvoiceBillingService
                            .GetOrCreateAsync(
                                kunjunganId,
                                now);
                }

                var billing = new Billing
                {
                    BillingId = Guid.NewGuid(),

                    KunjunganId = kunjunganId,
                    ItemId = pemeriksaanLabId,
                    NamaItem = item.NamaPemeriksaan,

                    HargaItem = harga,
                    QtyItem = qtyFinal,
                    SubTotalItem = harga * qtyFinal,

                    InvoiceBilling = invoiceBilling,

                    IsListWhiteOff = false,

                    BillingKode = "LAB",
                    JenisBilling = "Pemeriksaan Lab",
                    StatusBilling = false,

                    TipeLayanan = item.TipeLayanan,

                    BillingDate = now,
                    TanggalInvoice = now,
                    TanggalJatuhTempo =
                        now.Date.AddDays(90),

                    IsCovered = coverage?.IsCovered,
                    IsCoveredExcess =
                        coverage?.IsCoveredExcess,

                    AsuransiId = coverage?.AsuransiId,
                    AsuransiExcessId =
                        coverage?.AsuransiExcessId,

                    CreateBy = userActiveId,
                    CreateDateTime =
                        DateTimeOffset.UtcNow,

                    IsDelete = false
                };

                _context.Billings.Add(billing);

                /*
                 * Masukkan ke map agar pemeriksaan sama tidak
                 * dibuat lagi selama proses yang sama.
                 */
                existingBillingMap[pemeriksaanLabId] =
                    billing;

                affectedCount++;
            }

            if (affectedCount > 0)
            {
                await _context.SaveChangesAsync(
                    cancellationToken);
            }

            return affectedCount;
        }
    }
}