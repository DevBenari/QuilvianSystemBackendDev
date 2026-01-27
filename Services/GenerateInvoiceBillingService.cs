using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackendDev.Services
{
    public class GenerateInvoiceBillingService : IGenerateInvoiceBillingService
    {
        private readonly ApplicationDbContext _db;

        public GenerateInvoiceBillingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GetOrCreateAsync(
            Guid kunjunganId,
            DateTime tanggalPembayaran,
            CancellationToken cancellationToken = default)
        {
            if (kunjunganId == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak boleh kosong.", nameof(kunjunganId));

            // 1) Kalau sudah ada invoice di item manapun untuk kunjungan ini, pakai itu.
            var existing = await _db.MainKasirs
                .AsNoTracking()
                .Where(d => d.KunjunganId == kunjunganId && d.InvoiceBilling != null && d.InvoiceBilling != "")
                .Select(d => d.InvoiceBilling!)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(existing))
                return existing;

            // 2) Generate running number dari SEQUENCE Postgres (aman concurrency & aman dari nested transaction bug)
            var seq = await NextInvoiceBillingSequenceAsync(cancellationToken);

            // 0001, 0002, ...
            var running = seq.ToString("D4", CultureInfo.InvariantCulture);

            // HariBulanTahun dari tanggal pembayaran yang diinput
            var datePart = tanggalPembayaran.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            var invoice = $"INVB{running}{datePart}";

            // 3) Set ke semua item (detail) untuk kunjungan ini yang belum punya invoice
            var items = await _db.MainKasirs
                .Where(d => d.KunjunganId == kunjunganId && (d.InvoiceBilling == null || d.InvoiceBilling == ""))
                .ToListAsync(cancellationToken);

            foreach (var it in items)
                it.InvoiceBilling = invoice;

            if (items.Count > 0)
                await _db.SaveChangesAsync(cancellationToken);

            return invoice;
        }

        public async Task<bool> UpdateIsListWhiteOffAsync(
            Guid kunjunganId,
            DateTime today,
            CancellationToken cancellationToken = default)
        {
            if (kunjunganId == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak boleh kosong.", nameof(kunjunganId));

            // 1) Ambil billing header untuk kunjungan
            var billing = await _db.Billings
                .FirstOrDefaultAsync(b => b.KunjunganId == kunjunganId, cancellationToken);

            if (billing == null)
                return false; // atau throw

            // 2) Belum dibayar jika StatusBilling == false
            var belumDibayar = billing.StatusBilling == false;

            // 3) Tanggal terakhir bayar (sesuaikan sumber pembayaranmu)
            // Contoh: dari MainKasirDetails.TanggalBayar (kalau itu memang transaksi bayar)
            DateTime? lastPaymentDate = await _db.MainKasirDetails
                .AsNoTracking()
                .Where(d => d.KunjunganId == kunjunganId && d.TglPembayaran.HasValue)
                .MaxAsync(d => (DateTime?)d.TglPembayaran, cancellationToken);

            // 4) Rule WhiteOff:
            // true jika belum dibayar dan today > (tanggal terakhir bayar + 90 hari)
            var isWhiteOff = false;
            if (belumDibayar && lastPaymentDate.HasValue)
            {
                isWhiteOff = today.Date > lastPaymentDate.Value.Date.AddDays(90);
            }

            // defaultnya false (kalau belum memenuhi syarat / belum ada lastPaymentDate)
            billing.IsListWhiteOff = isWhiteOff;

            await _db.SaveChangesAsync(cancellationToken);
            return isWhiteOff;
        }



        private async Task<long> NextInvoiceBillingSequenceAsync(CancellationToken cancellationToken)
        {
            // Pastikan SEQUENCE ini ada di database:
            // CREATE SEQUENCE "InvoiceBillingSeq" START 1 INCREMENT 1;
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT nextval('\"InvoiceBillingSeq\"')";
            var result = await cmd.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt64(result);
        }
    }
}
