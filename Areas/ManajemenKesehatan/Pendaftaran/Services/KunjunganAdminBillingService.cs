using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Services
{
    public class KunjunganAdminBillingService : IKunjunganAdminBillingService
    {
        private const string BILLING_KODE_BIAYA_ADMIN = "001";
        private const string BILLING_KODE_TINDAKAN = "002";

        private const string JENIS_BILLING_BIAYA_ADMIN = "Biaya Admin";
        private const string JENIS_BILLING_TINDAKAN = "Tindakan";

        private const string ADMIN_RAJAL_CODE = "OP";
        private const string ADMIN_IGD_CODE = "IGD";

        /*
         * Kalau kode admin ranap yang dipakai di database kamu adalah "RANAP",
         * ubah value ini menjadi "RANAP".
         *
         * Dari function lama kamu, kode rawat inap sebelumnya memakai "IP".
         */
        private const string ADMIN_RANAP_CODE = "IP";

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;

        public KunjunganAdminBillingService(
            ApplicationDbContext applicationDbContext,
            IGenerateInvoiceBillingService generateInvoiceBillingService)
        {
            _applicationDbContext = applicationDbContext;
            _generateInvoiceBillingService = generateInvoiceBillingService;
        }

        // =====================================================
        // FUNCTION LAMA: BIAYA ADMIN
        // Dibiarkan tetap ada.
        // =====================================================
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

            var targetBiayaAdmin = await _applicationDbContext.BiayaAdministrasis
                .FirstOrDefaultAsync(x =>
                    x.BiayaAdministrasiKode == kodeJenis,
                    cancellationToken);

            if (targetBiayaAdmin == null)
                return;

            var biayaAdminIp = await _applicationDbContext.BiayaAdministrasis
                .FirstOrDefaultAsync(x =>
                    x.BiayaAdministrasiKode == "IP",
                    cancellationToken);

            var existingAdminBilling = await _applicationDbContext.Billings
                .Include(b => b.Kunjungan)
                .Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.PasienId == kunjungan.PasienId &&
                    b.JenisBilling == JENIS_BILLING_BIAYA_ADMIN &&
                    b.BillingKode == BILLING_KODE_BIAYA_ADMIN &&
                    !b.Kunjungan.IsDelete &&
                    b.BillingDate.HasValue &&
                    b.BillingDate.Value >= today &&
                    b.BillingDate.Value < tomorrow)
                .OrderByDescending(b => b.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAdminBilling != null)
            {
                var existingIsIpAdmin =
                    biayaAdminIp != null &&
                    existingAdminBilling.ItemId == biayaAdminIp.BiayaAdministrasiId;

                if (existingIsIpAdmin)
                    return;

                if (kodeJenis == "OP")
                    return;

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
                    existingAdminBilling.UpdateDateTime = DateTimeOffset.UtcNow;
                    existingAdminBilling.UpdateBy = userActiveId;

                    return;
                }
            }

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

                BillingKode = BILLING_KODE_BIAYA_ADMIN,
                JenisBilling = JENIS_BILLING_BIAYA_ADMIN,
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

        // =====================================================
        // FUNCTION LAMA/EXISTING: KONSULTASI DOKTER
        // Sekarang sekaligus insert ke TindakanKunjungans dan Billing.
        // Parameter tarifKelasId adalah TarifKelasId.
        // TindakanId diambil dari TarifKelas.TindakanId.
        // =====================================================
        public async Task ApplyBiayaKonsultasiDokterAsync(
            Guid? kunjunganId,
            Guid? tarifKelasId,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            await ApplyTindakanTarifKelasKeBillingAsync(
                kunjunganId: kunjunganId,
                tarifKelasId: tarifKelasId,
                userActiveId: userActiveId,
                namaItemDefault: "Konsultasi Dokter",
                tipeLayanan: "Rawat Jalan",
                gunakanTarifDokterOnly: true,
                appendNamaDokter: true,
                cancellationToken: cancellationToken
            );
        }

        // =====================================================
        // BARU: RAWAT JALAN
        // Ketentuan:
        // Biaya admin + konsultasi dokter masuk billing saat dokter simpan SOAP.
        // =====================================================
        public async Task ApplyBillingRawatJalanSaatSimpanSoapAsync(
            Guid? kunjunganId,
            Guid? tarifKelasIdKonsultasiDokter,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            await ApplyBiayaAdministrasiByKodeAsync(
                kunjunganId: kunjunganId,
                kodeJenis: ADMIN_RAJAL_CODE,
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );

            await ApplyBiayaKonsultasiDokterAsync(
                kunjunganId: kunjunganId,
                tarifKelasId: tarifKelasIdKonsultasiDokter,
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );
        }

        // =====================================================
        // BARU: IGD
        // Ketentuan:
        // Biaya admin IGD + assessment medis masuk billing saat dokter simpan tindakan.
        // =====================================================
        public async Task ApplyBillingIgdSaatSimpanTindakanAsync(
            Guid? kunjunganId,
            Guid? tarifKelasIdAssessmentMedis,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            await ApplyBiayaAdministrasiByKodeAsync(
                kunjunganId: kunjunganId,
                kodeJenis: ADMIN_IGD_CODE,
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );

            await ApplyTindakanTarifKelasKeBillingAsync(
                kunjunganId: kunjunganId,
                tarifKelasId: tarifKelasIdAssessmentMedis,
                userActiveId: userActiveId,
                namaItemDefault: "Assessment Medis",
                tipeLayanan: "IGD",
                gunakanTarifDokterOnly: false,
                appendNamaDokter: false,
                cancellationToken: cancellationToken
            );
        }

        // =====================================================
        // BARU: TRANSFER KE RANAP
        // Ketentuan:
        // Biaya admin ranap masuk billing ketika transfer pasien.
        // Biaya admin rajal/IGD yang lama diubah menjadi admin ranap.
        // =====================================================
        public async Task ApplyBillingTransferRanapAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            await ApplyBiayaAdministrasiByKodeAsync(
                kunjunganId: kunjunganId,
                kodeJenis: ADMIN_RANAP_CODE,
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );
        }

        // =====================================================
        // BARU: ADMISI RANAP BARU
        // Ketentuan:
        // Biaya admin ranap masuk saat pembuatan pasien/kunjungan ranap baru.
        // =====================================================
        public async Task ApplyBillingAdmisiRanapBaruAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            await ApplyBiayaAdministrasiByKodeAsync(
                kunjunganId: kunjunganId,
                kodeJenis: ADMIN_RANAP_CODE,
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );
        }

        // =====================================================
        // HELPER: BIAYA ADMIN GENERIC
        // Bisa untuk OP, IGD, IP/RANAP.
        // =====================================================
        private async Task ApplyBiayaAdministrasiByKodeAsync(
            Guid? kunjunganId,
            string kodeJenis,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            if (!kunjunganId.HasValue || kunjunganId.Value == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak valid.");

            kodeJenis = kodeJenis?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(kodeJenis))
                throw new ArgumentException("Kode jenis biaya administrasi tidak valid.");

            var kunjungan = await _applicationDbContext.Kunjungans
                .FirstOrDefaultAsync(x =>
                    x.KunjunganID == kunjunganId.Value &&
                    !x.IsDelete,
                    cancellationToken);

            if (kunjungan == null)
                throw new InvalidOperationException("Data kunjungan tidak ditemukan.");

            if (!kunjungan.PasienId.HasValue || kunjungan.PasienId == Guid.Empty)
                throw new InvalidOperationException("PasienId pada kunjungan tidak valid.");

            var targetBiayaAdmin = await _applicationDbContext.BiayaAdministrasis
                .FirstOrDefaultAsync(x =>
                    x.BiayaAdministrasiKode == kodeJenis &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            if (targetBiayaAdmin == null)
                return;

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var targetIsRanap =
                kodeJenis == "IP" ||
                kodeJenis == "RANAP";

            var existingAdminBilling = await _applicationDbContext.Billings
                .Include(b => b.Kunjungan)
                .Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.PasienId == kunjungan.PasienId &&
                    b.JenisBilling == JENIS_BILLING_BIAYA_ADMIN &&
                    b.BillingKode == BILLING_KODE_BIAYA_ADMIN &&
                    (b.IsDelete == false || b.IsDelete == null) &&
                    !b.Kunjungan.IsDelete &&
                    b.BillingDate.HasValue &&
                    b.BillingDate.Value >= today &&
                    b.BillingDate.Value < tomorrow)
                .OrderByDescending(b => b.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAdminBilling != null)
            {
                if (existingAdminBilling.ItemId == targetBiayaAdmin.BiayaAdministrasiId)
                    return;

                /*
                 * Kalau bukan target ranap, jangan update admin yang sudah ada.
                 * Tujuannya agar OP/IGD tidak saling timpa.
                 */
                if (!targetIsRanap)
                    return;

                /*
                 * Kalau target ranap, update admin existing OP/IGD menjadi admin ranap.
                 */
                var invoiceUpdate = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId.Value,
                    DateTime.UtcNow
                );

                existingAdminBilling.KunjunganId = kunjunganId.Value;
                existingAdminBilling.ItemId = targetBiayaAdmin.BiayaAdministrasiId;
                existingAdminBilling.NamaItem = targetBiayaAdmin.NamaBiayaAdministrasi;
                existingAdminBilling.HargaItem = targetBiayaAdmin.NominalBiayaAdministrasi;
                existingAdminBilling.QtyItem = 1;
                existingAdminBilling.SubTotalItem = targetBiayaAdmin.NominalBiayaAdministrasi;
                existingAdminBilling.InvoiceBilling = invoiceUpdate;
                existingAdminBilling.BillingDate = DateTime.UtcNow;
                existingAdminBilling.TanggalInvoice = DateTime.UtcNow;
                existingAdminBilling.TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90);
                existingAdminBilling.UpdateDateTime = DateTimeOffset.UtcNow;
                existingAdminBilling.UpdateBy = userActiveId;

                return;
            }

            var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                kunjunganId.Value,
                DateTime.UtcNow
            );

            var billing = new Billing
            {
                BillingId = Guid.NewGuid(),
                KunjunganId = kunjunganId.Value,

                ItemId = targetBiayaAdmin.BiayaAdministrasiId,
                NamaItem = targetBiayaAdmin.NamaBiayaAdministrasi,
                HargaItem = targetBiayaAdmin.NominalBiayaAdministrasi,
                QtyItem = 1,
                SubTotalItem = targetBiayaAdmin.NominalBiayaAdministrasi,

                InvoiceBilling = invoice,
                IsListWhiteOff = false,

                BillingKode = BILLING_KODE_BIAYA_ADMIN,
                JenisBilling = JENIS_BILLING_BIAYA_ADMIN,
                StatusBilling = false,

                BillingDate = DateTime.UtcNow,
                TanggalInvoice = DateTime.UtcNow,
                TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                CreateDateTime = DateTimeOffset.UtcNow,
                CreateBy = userActiveId,
                IsDelete = false
            };

            _applicationDbContext.Billings.Add(billing);
        }

        // =====================================================
        // HELPER: TARIF KELAS -> TINDAKAN KUNJUNGAN + BILLING
        // Dipakai untuk:
        // - Konsultasi Dokter Rawat Jalan
        // - Assessment Medis IGD
        // =====================================================
        private async Task ApplyTindakanTarifKelasKeBillingAsync(
            Guid? kunjunganId,
            Guid? tarifKelasId,
            Guid userActiveId,
            string namaItemDefault,
            string tipeLayanan,
            bool gunakanTarifDokterOnly,
            bool appendNamaDokter,
            CancellationToken cancellationToken = default)
        {
            if (!kunjunganId.HasValue || kunjunganId.Value == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak valid.");

            if (!tarifKelasId.HasValue || tarifKelasId.Value == Guid.Empty)
                throw new ArgumentException("TarifKelasId tidak valid.");

            var kunjungan = await _applicationDbContext.Kunjungans
                .FirstOrDefaultAsync(x =>
                    x.KunjunganID == kunjunganId.Value &&
                    !x.IsDelete,
                    cancellationToken);

            if (kunjungan == null)
                throw new InvalidOperationException("Data kunjungan tidak ditemukan.");

            if (!kunjungan.PasienId.HasValue || kunjungan.PasienId == Guid.Empty)
                throw new InvalidOperationException("PasienId pada kunjungan tidak valid.");

            if (!kunjungan.DokterId.HasValue || kunjungan.DokterId.Value == Guid.Empty)
                throw new InvalidOperationException("DokterId pada kunjungan tidak valid.");

            var tarif = await _applicationDbContext.TarifKelass
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TarifKelasId == tarifKelasId.Value &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            if (tarif == null)
                return;

            if (!tarif.TindakanId.HasValue || tarif.TindakanId.Value == Guid.Empty)
                throw new InvalidOperationException("TindakanId pada TarifKelas tidak valid.");

            var tindakanId = tarif.TindakanId.Value;

            decimal nominal;

            if (gunakanTarifDokterOnly)
            {
                nominal = tarif.TarifDokter ?? 0;
            }
            else
            {
                nominal =
                    tarif.TarifTotal ??
                    tarif.TarifDokter ??
                    tarif.TarifRs ??
                    0;
            }

            if (nominal <= 0)
                return;

            var dokter = await _applicationDbContext.Dokters
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DokterId == kunjungan.DokterId.Value &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            var namaItem = namaItemDefault;

            if (appendNamaDokter &&
                dokter != null &&
                !string.IsNullOrWhiteSpace(dokter.NmDokter))
            {
                namaItem = $"{namaItemDefault} - {dokter.NmDokter}";
            }

            /*
             * Cegah double TindakanKunjungan.
             */
            var existingTindakanKunjungan = await _applicationDbContext.TindakanKunjungans
                .FirstOrDefaultAsync(x =>
                    x.KunjunganId == kunjunganId.Value &&
                    x.TindakanId == tindakanId &&
                    x.TipeLayanan == tipeLayanan &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            var trackedTindakanKunjunganExists = _applicationDbContext.ChangeTracker
                .Entries<TindakanKunjungan>()
                .Any(e =>
                    e.State != EntityState.Deleted &&
                    e.Entity.KunjunganId == kunjunganId.Value &&
                    e.Entity.TindakanId == tindakanId &&
                    e.Entity.TipeLayanan == tipeLayanan &&
                    (e.Entity.IsDelete == false || e.Entity.IsDelete == null));

            if (existingTindakanKunjungan == null && !trackedTindakanKunjunganExists)
            {
                var tindakanKunjungan = new TindakanKunjungan
                {
                    TindakanKunjunganId = Guid.NewGuid(),

                    KunjunganId = kunjunganId.Value,
                    TindakanId = tindakanId,

                    Quantity = 1,
                    Total = nominal,

                    Disposition = null,

                    DepartementId = null,
                    DokterPemeriksaId = kunjungan.DokterId,
                    KelasId = tarif.KelasId,

                    TanggalPemeriksaan = DateTime.UtcNow,

                    Keterangan = namaItem,
                    TipeLayanan = tipeLayanan,
                    IsFoC = false,

                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveId,

                    IsDelete = false
                };

                _applicationDbContext.TindakanKunjungans.Add(tindakanKunjungan);
            }

            /*
             * Cegah double Billing.
             */
            var existingBilling = await _applicationDbContext.Billings
                .FirstOrDefaultAsync(x =>
                    x.KunjunganId == kunjunganId.Value &&
                    x.JenisBilling == JENIS_BILLING_TINDAKAN &&
                    x.BillingKode == BILLING_KODE_TINDAKAN &&
                    x.ItemId == tarif.TarifKelasId &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            var trackedBillingExists = _applicationDbContext.ChangeTracker
                .Entries<Billing>()
                .Any(e =>
                    e.State != EntityState.Deleted &&
                    e.Entity.KunjunganId == kunjunganId.Value &&
                    e.Entity.JenisBilling == JENIS_BILLING_TINDAKAN &&
                    e.Entity.BillingKode == BILLING_KODE_TINDAKAN &&
                    e.Entity.ItemId == tarif.TarifKelasId &&
                    (e.Entity.IsDelete == false || e.Entity.IsDelete == null));

            if (existingBilling != null || trackedBillingExists)
                return;

            var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                kunjunganId.Value,
                DateTime.UtcNow
            );

            var billing = new Billing
            {
                BillingId = Guid.NewGuid(),
                KunjunganId = kunjunganId.Value,

                ItemId = tarif.TarifKelasId,
                NamaItem = namaItem,

                HargaItem = nominal,
                QtyItem = 1,
                SubTotalItem = nominal,

                InvoiceBilling = invoice,
                IsListWhiteOff = false,

                BillingKode = BILLING_KODE_TINDAKAN,
                JenisBilling = JENIS_BILLING_TINDAKAN,
                StatusBilling = false,

                BillingDate = DateTime.UtcNow,
                TanggalInvoice = DateTime.UtcNow,
                TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                CreateDateTime = DateTimeOffset.UtcNow,
                CreateBy = userActiveId,
                IsDelete = false
            };

            _applicationDbContext.Billings.Add(billing);
        }
    }
}