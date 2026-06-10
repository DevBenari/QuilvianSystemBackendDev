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

            var pemeriksaanIds = items
                .Select(x => x.PemeriksaanLabId)
                .Distinct()
                .ToList();

            var existingBillingItemIds = await _context.Billings
                .AsNoTracking()
                .Where(x =>
                    x.KunjunganId == kunjunganId &&
                    x.ItemId.HasValue &&
                    pemeriksaanIds.Contains(x.ItemId.Value) &&
                    x.BillingKode == "LAB" &&
                    x.JenisBilling == "Pemeriksaan Lab" &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => x.ItemId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            var createdCount = 0;

            foreach (var item in items)
            {
                var pemeriksaanLabId = item.PemeriksaanLabId;

                // Cegah billing double kalau endpoint PUT dipanggil ulang.
                if (existingBillingItemIds.Contains(pemeriksaanLabId))
                    continue;

                var harga = item.HargaPemeriksaan ?? 0m;

                var qty = item.QtyOrder.HasValue && item.QtyOrder.Value > 0
                    ? Convert.ToInt32(item.QtyOrder.Value)
                    : 1;

                var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                    kunjunganId,
                    "Pemeriksaan Lab",
                    pemeriksaanLabId,
                    cancellationToken);

                var billing = new Billing
                {
                    BillingId = Guid.NewGuid(),

                    KunjunganId = kunjunganId,
                    ItemId = pemeriksaanLabId,
                    NamaItem = item.NamaPemeriksaan,

                    HargaItem = harga,
                    QtyItem = qty,
                    SubTotalItem = harga * qty,

                    InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
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

                existingBillingItemIds.Add(pemeriksaanLabId);
                createdCount++;
            }

            if (createdCount > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return createdCount;
        }
    }
}
