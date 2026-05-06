using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services
{
    public class KunjunganAdminBillingService : IKunjunganAdminBillingService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;

        public KunjunganAdminBillingService(
            ApplicationDbContext applicationDbContext,
            IGenerateInvoiceBillingService generateInvoiceBillingService)
        {
            _applicationDbContext = applicationDbContext;
            _generateInvoiceBillingService = generateInvoiceBillingService;
        }

        public async Task ApplyBiayaAdminAsync(
            Guid? kunjunganId,
            string kodeJenis,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            if (!kunjunganId.HasValue || kunjunganId.Value == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak valid.");

            kodeJenis = kodeJenis?.Trim().ToUpper();

            if (kodeJenis != "OP" && kodeJenis != "IP")
                throw new ArgumentException("Kode jenis kunjungan hanya boleh OP atau IP.");

            var kunjungan = await _applicationDbContext.Kunjungans
                .FirstOrDefaultAsync(x =>
                    x.KunjunganID == kunjunganId.Value &&
                    !x.IsDelete,
                    cancellationToken);

            if (kunjungan == null)
                throw new InvalidOperationException("Data kunjungan tidak ditemukan.");

            if (!kunjungan.PasienId.HasValue || kunjungan.PasienId == Guid.Empty)
                throw new InvalidOperationException("PasienId pada kunjungan tidak valid.");

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            /*
             * Ambil tarif admin target.
             * OP = admin rawat jalan
             * IP = admin rawat inap
             */
            var targetBiayaAdmin = await _applicationDbContext.BiayaAdministrasis
                .FirstOrDefaultAsync(x =>
                    x.BiayaAdministrasiKode == kodeJenis,
                    cancellationToken);

            if (targetBiayaAdmin == null)
                return;

            /*
             * Ambil tarif IP untuk mendeteksi apakah billing admin existing
             * sudah memakai admin rawat inap.
             */
            var biayaAdminIp = await _applicationDbContext.BiayaAdministrasis
                .FirstOrDefaultAsync(x =>
                    x.BiayaAdministrasiKode == "IP",
                    cancellationToken);

            /*
             * Cek apakah pasien sudah punya billing biaya admin hari ini.
             * Pencarian berdasarkan PasienId, bukan hanya KunjunganId,
             * supaya admin tidak double saat pasien punya lebih dari satu kunjungan
             * pada hari yang sama.
             */
            var existingAdminBilling = await _applicationDbContext.Billings
                .Include(b => b.Kunjungan)
                .Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.PasienId == kunjungan.PasienId &&
                    b.JenisBilling == "Biaya Admin" &&
                    b.BillingKode == "001" &&
                    !b.Kunjungan.IsDelete &&
                    b.BillingDate.HasValue &&
                    b.BillingDate.Value >= today &&
                    b.BillingDate.Value < tomorrow)
                .OrderByDescending(b => b.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            /*
             * Kalau sudah ada biaya admin hari ini.
             */
            if (existingAdminBilling != null)
            {
                var existingIsIpAdmin =
                    biayaAdminIp != null &&
                    existingAdminBilling.ItemId == biayaAdminIp.BiayaAdministrasiId;

                /*
                 * Kalau billing admin sudah IP, jangan insert lagi
                 * dan jangan downgrade ke OP.
                 */
                if (existingIsIpAdmin)
                    return;

                /*
                 * Kalau request baru OP, sedangkan sudah ada admin OP,
                 * tidak perlu insert lagi.
                 */
                if (kodeJenis == "OP")
                    return;

                /*
                 * Kalau existing admin masih OP lalu request sekarang IP,
                 * update billing admin menjadi tarif IP.
                 */
                if (kodeJenis == "IP")
                {
                    var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                        kunjunganId.Value,
                        DateTime.UtcNow
                    );

                    existingAdminBilling.KunjunganId = kunjunganId.Value;
                    existingAdminBilling.ItemId = targetBiayaAdmin.BiayaAdministrasiId;
                    existingAdminBilling.NamaItem = targetBiayaAdmin.NamaBiayaAdministrasi;
                    existingAdminBilling.HargaItem = targetBiayaAdmin.NominalBiayaAdministrasi;
                    existingAdminBilling.QtyItem = 1;
                    existingAdminBilling.SubTotalItem = targetBiayaAdmin.NominalBiayaAdministrasi;
                    existingAdminBilling.InvoiceBilling = invoice;
                    existingAdminBilling.BillingDate = DateTime.UtcNow;
                    existingAdminBilling.TanggalInvoice = DateTime.UtcNow;
                    existingAdminBilling.TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90);
                    existingAdminBilling.CreateBy = userActiveId;

                    return;
                }
            }

            /*
             * Kalau belum ada biaya admin hari ini,
             * insert billing admin baru.
             */
            var newInvoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                kunjunganId.Value,
                DateTime.UtcNow
            );

            var bill = new Billing
            {
                BillingId = Guid.NewGuid(),
                KunjunganId = kunjunganId.Value,
                ItemId = targetBiayaAdmin.BiayaAdministrasiId,
                NamaItem = targetBiayaAdmin.NamaBiayaAdministrasi,
                HargaItem = targetBiayaAdmin.NominalBiayaAdministrasi,
                QtyItem = 1,
                SubTotalItem = targetBiayaAdmin.NominalBiayaAdministrasi,

                InvoiceBilling = newInvoice,
                IsListWhiteOff = false,
                BillingKode = "001",
                JenisBilling = "Biaya Admin",
                StatusBilling = false,
                BillingDate = DateTime.UtcNow,
                TanggalInvoice = DateTime.UtcNow,
                TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                CreateDateTime = DateTimeOffset.UtcNow,
                CreateBy = userActiveId,
                IsDelete = false
            };

            _applicationDbContext.Billings.Add(bill);
        }
    }
}