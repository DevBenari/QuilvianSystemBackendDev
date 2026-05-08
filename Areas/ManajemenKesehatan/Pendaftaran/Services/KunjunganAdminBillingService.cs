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
        private const string ADMIN_RANAP_CODE = "IP";

        private const string ASSESSMENT_IGD_CODE = "IGD-ASSESSMENTMEDIS";
        private const string KONSULTASI_UMUM_CODE = "TDK25051600025";

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly IAsuransiCoverageService _asuransiCoverageService;

        public KunjunganAdminBillingService(
            ApplicationDbContext applicationDbContext,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IAsuransiCoverageService asuransiCoverageService)
        {
            _applicationDbContext = applicationDbContext;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _asuransiCoverageService = asuransiCoverageService;
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
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            var tarifKelasKonsultasiUmumId = await GetTarifKelasByKodeTindakanAsync(
                kunjunganId: kunjunganId,
                kodeTindakan: KONSULTASI_UMUM_CODE,
                unitAsal: null,
                wajibRajal: true,
                wajibIgd: false,
                cancellationToken: cancellationToken
            );

            if (!tarifKelasKonsultasiUmumId.HasValue)
                return;

            await ApplyTindakanTarifKelasKeBillingAsync(
                kunjunganId: kunjunganId,
                tarifKelasId: tarifKelasKonsultasiUmumId,
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
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );
        }

        // =====================================================
        // BARU: IGD
        // Ketentuan:
        // Biaya admin IGD + assessment medis masuk billing saat dokter simpan tindakan.
        // =====================================================
        public async Task ApplyAdminIGDAsync(
            Guid? kunjunganId,
            Guid userActiveId,
            CancellationToken cancellationToken = default)
        {
            await ApplyBiayaAdministrasiByKodeAsync(
                kunjunganId: kunjunganId,
                kodeJenis: ADMIN_IGD_CODE,
                userActiveId: userActiveId,
                cancellationToken: cancellationToken
            );

            var tarifKelasAssessmentIgdId = await GetTarifKelasByKodeTindakanAsync(
                kunjunganId: kunjunganId,
                kodeTindakan: ASSESSMENT_IGD_CODE,
                unitAsal: "IGD",
                wajibRajal: false,
                wajibIgd: true,
                cancellationToken: cancellationToken
            );

            if (!tarifKelasAssessmentIgdId.HasValue)
                return;

            await ApplyTindakanTarifKelasKeBillingAsync(
                kunjunganId: kunjunganId,
                tarifKelasId: tarifKelasAssessmentIgdId,
                userActiveId: userActiveId,
                namaItemDefault: "Assessment Medis IGD",
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
        public async Task ApplyAdminTransferRanapAsync(
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

            /*
             * Wajib cek kunjungan sudah ada di tabel Kunjungan.
             * Kalau belum SaveChanges / belum masuk DB, proses billing tidak boleh lanjut.
             */
            var kunjungan = await _applicationDbContext.Kunjungans
                .FirstOrDefaultAsync(x =>
                    x.KunjunganID == kunjunganId.Value &&
                    !x.IsDelete,
                    cancellationToken);

            if (kunjungan == null)
                throw new InvalidOperationException("Data kunjungan tidak ditemukan. Pastikan kunjungan sudah tersimpan sebelum membuat billing biaya admin.");

            if (!kunjungan.PasienId.HasValue || kunjungan.PasienId.Value == Guid.Empty)
                throw new InvalidOperationException("PasienId pada kunjungan tidak valid.");

            /*
             * Ambil master biaya admin target.
             * Contoh kode:
             * OP  = Rawat Jalan
             * IGD = Instalasi Gawat Darurat
             * IP  = Rawat Inap / Ranap
             */
            var targetBiayaAdmin = await _applicationDbContext.BiayaAdministrasis
                .FirstOrDefaultAsync(x =>
                    x.BiayaAdministrasiKode == kodeJenis &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            if (targetBiayaAdmin == null)
                return;

            var (startToday, endToday, now) = GetTodayBillingRange();

            /*
             * Target ranap boleh mengganti biaya admin lama.
             * Sesuaikan jika kode ranap kamu bukan IP.
             */
            bool targetIsRanap =
                kodeJenis == "IP" ||
                kodeJenis == "RANAP";

            /*
             * Cek apakah pasien sudah memiliki biaya admin apapun pada hari ini.
             *
             * Cek berdasarkan:
             * - PasienId dari tabel Kunjungan
             * - JenisBilling = Biaya Admin
             * - BillingKode = 001
             * - BillingDate di antara jam 00:00 sampai sebelum 00:00 besok
             *
             * Ini membuat 1 pasien hanya punya 1 biaya admin per hari.
             */
            var existingAdminBilling = await (
                from b in _applicationDbContext.Billings
                join k in _applicationDbContext.Kunjungans
                    on b.KunjunganId equals k.KunjunganID
                where k.PasienId == kunjungan.PasienId
                      && !k.IsDelete
                      && b.JenisBilling == JENIS_BILLING_BIAYA_ADMIN
                      && b.BillingKode == BILLING_KODE_BIAYA_ADMIN
                      && (b.IsDelete == false || b.IsDelete == null)
                      && b.BillingDate.HasValue
                      && b.BillingDate.Value >= startToday
                      && b.BillingDate.Value < endToday
                orderby b.CreateDateTime descending
                select b
            ).FirstOrDefaultAsync(cancellationToken);

            /*
             * Cek juga entity Billing yang sudah di-Add di DbContext,
             * tapi belum SaveChanges.
             * Ini mencegah double insert dalam 1 request.
             */
            var trackedAdminBilling = _applicationDbContext.ChangeTracker
                .Entries<Billing>()
                .Where(e =>
                    e.State != EntityState.Deleted &&
                    e.Entity.KunjunganId == kunjunganId.Value &&
                    e.Entity.JenisBilling == JENIS_BILLING_BIAYA_ADMIN &&
                    e.Entity.BillingKode == BILLING_KODE_BIAYA_ADMIN &&
                    (e.Entity.IsDelete == false || e.Entity.IsDelete == null) &&
                    e.Entity.BillingDate.HasValue &&
                    e.Entity.BillingDate.Value >= startToday &&
                    e.Entity.BillingDate.Value < endToday)
                .Select(e => e.Entity)
                .OrderByDescending(e => e.CreateDateTime)
                .FirstOrDefault();

            var adminBillingHariIni = existingAdminBilling ?? trackedAdminBilling;

            /*
             * Kalau sudah ada biaya admin hari ini.
             */
            if (adminBillingHariIni != null)
            {
                /*
                 * Kalau biaya admin yang sudah ada adalah item yang sama,
                 * tidak perlu insert/update lagi.
                 */
                if (adminBillingHariIni.ItemId == targetBiayaAdmin.BiayaAdministrasiId)
                    return;

                /*
                 * Kalau target bukan ranap, jangan tambah dan jangan timpa.
                 *
                 * Contoh:
                 * - Sudah ada OP, lalu request IGD pada hari yang sama -> tidak tambah
                 * - Sudah ada IGD, lalu request OP pada hari yang sama -> tidak tambah
                 *
                 * Karena rule bisnis: 1 pasien hanya 1 biaya admin per hari.
                 */
                if (!targetIsRanap)
                    return;

                /*
                 * Kalau target adalah ranap, maka biaya admin lama boleh diganti.
                 *
                 * Contoh:
                 * - OP  -> IP/RANAP
                 * - IGD -> IP/RANAP
                 */
                var invoiceUpdate = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId.Value,
                    now
                );

                adminBillingHariIni.KunjunganId = kunjunganId.Value;
                adminBillingHariIni.ItemId = targetBiayaAdmin.BiayaAdministrasiId;
                adminBillingHariIni.NamaItem = targetBiayaAdmin.NamaBiayaAdministrasi;
                adminBillingHariIni.HargaItem = targetBiayaAdmin.NominalBiayaAdministrasi;
                adminBillingHariIni.QtyItem = 1;
                adminBillingHariIni.SubTotalItem = targetBiayaAdmin.NominalBiayaAdministrasi;
                adminBillingHariIni.InvoiceBilling = invoiceUpdate;
                adminBillingHariIni.BillingDate = now;
                adminBillingHariIni.TanggalInvoice = now;
                adminBillingHariIni.TanggalJatuhTempo = now.Date.AddDays(90);
                adminBillingHariIni.UpdateDateTime = DateTimeOffset.Now;
                adminBillingHariIni.UpdateBy = userActiveId;
                adminBillingHariIni.IsDelete = false;

                return;
            }

            /*
             * Kalau belum ada biaya admin hari ini,
             * insert biaya admin baru.
             */
            var invoice = await _generateInvoiceBillingService.GetOrCreateAsync(
                kunjunganId.Value,
                now
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

                BillingDate = now,
                TanggalInvoice = now,
                TanggalJatuhTempo = now.Date.AddDays(90),

                CreateDateTime = DateTimeOffset.Now,
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
             * Resolve coverage asuransi.
             *
             * Penting:
             * Untuk JenisBilling = "Tindakan",
             * itemId harus TindakanId, bukan TarifKelasId.
             */
            var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                kunjunganId: kunjunganId.Value,
                jenisBilling: JENIS_BILLING_TINDAKAN,
                itemId: tindakanId,
                ct: cancellationToken
            );

            /*
             * =====================================================
             * 1. Cegah double TindakanKunjungan
             * =====================================================
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
             * =====================================================
             * 2. Cegah double Billing
             * =====================================================
             *
             * Untuk data baru:
             * - Billing.ItemId = tindakanId
             *
             * Untuk data lama:
             * - kalau sebelumnya terlanjur ItemId = TarifKelasId,
             *   tetap dikenali agar tidak double.
             */
            var existingBilling = await _applicationDbContext.Billings
                .FirstOrDefaultAsync(x =>
                    x.KunjunganId == kunjunganId.Value &&
                    x.JenisBilling == JENIS_BILLING_TINDAKAN &&
                    x.BillingKode == BILLING_KODE_TINDAKAN &&
                    (
                        x.ItemId == tindakanId ||
                        x.ItemId == tarif.TarifKelasId
                    ) &&
                    (x.IsDelete == false || x.IsDelete == null),
                    cancellationToken);

            if (existingBilling != null)
            {
                /*
                 * Normalisasi billing lama agar ItemId menjadi TindakanId.
                 * Sekaligus update coverage asuransi.
                 */
                existingBilling.ItemId = tindakanId;
                existingBilling.NamaItem = namaItem;
                existingBilling.HargaItem = nominal;
                existingBilling.QtyItem = 1;
                existingBilling.SubTotalItem = nominal;

                existingBilling.AsuransiId = coverage.AsuransiId;
                existingBilling.IsCovered = coverage.IsCovered;
                existingBilling.AsuransiExcessId = coverage.AsuransiExcessId;
                existingBilling.IsCoveredExcess = coverage.IsCoveredExcess;

                existingBilling.UpdateDateTime = DateTimeOffset.UtcNow;
                existingBilling.UpdateBy = userActiveId;

                return;
            }

            var trackedBilling = _applicationDbContext.ChangeTracker
                .Entries<Billing>()
                .Where(e =>
                    e.State != EntityState.Deleted &&
                    e.Entity.KunjunganId == kunjunganId.Value &&
                    e.Entity.JenisBilling == JENIS_BILLING_TINDAKAN &&
                    e.Entity.BillingKode == BILLING_KODE_TINDAKAN &&
                    (
                        e.Entity.ItemId == tindakanId ||
                        e.Entity.ItemId == tarif.TarifKelasId
                    ) &&
                    (e.Entity.IsDelete == false || e.Entity.IsDelete == null))
                .Select(e => e.Entity)
                .FirstOrDefault();

            if (trackedBilling != null)
            {
                trackedBilling.ItemId = tindakanId;
                trackedBilling.NamaItem = namaItem;
                trackedBilling.HargaItem = nominal;
                trackedBilling.QtyItem = 1;
                trackedBilling.SubTotalItem = nominal;

                trackedBilling.AsuransiId = coverage.AsuransiId;
                trackedBilling.IsCovered = coverage.IsCovered;
                trackedBilling.AsuransiExcessId = coverage.AsuransiExcessId;
                trackedBilling.IsCoveredExcess = coverage.IsCoveredExcess;

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

                /*
                 * Untuk billing tindakan, ItemId harus TindakanId.
                 * TarifKelasId hanya dipakai untuk menentukan harga.
                 */
                ItemId = tindakanId,
                NamaItem = namaItem,

                HargaItem = nominal,
                QtyItem = 1,
                SubTotalItem = nominal,

                /*
                 * Coverage asuransi dari service coverage.
                 */
                AsuransiId = coverage.AsuransiId,
                IsCovered = coverage.IsCovered,
                AsuransiExcessId = coverage.AsuransiExcessId,
                IsCoveredExcess = coverage.IsCoveredExcess,

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

        private static (DateTime StartToday, DateTime EndToday, DateTime Now) GetTodayBillingRange()
        {
            var now = DateTime.Now;
            var startToday = now.Date;
            var endToday = startToday.AddDays(1);

            return (startToday, endToday, now);
        }

        private async Task<Guid?> GetTarifKelasByKodeTindakanAsync(
            Guid? kunjunganId,
            string kodeTindakan,
            string? unitAsal,
            bool wajibRajal,
            bool wajibIgd,
            CancellationToken cancellationToken = default)
        {
            if (!kunjunganId.HasValue || kunjunganId.Value == Guid.Empty)
                throw new ArgumentException("KunjunganId tidak valid.");

            if (string.IsNullOrWhiteSpace(kodeTindakan))
                throw new ArgumentException("Kode tindakan tidak valid.");

            var kunjungan = await _applicationDbContext.Kunjungans
                .AsNoTracking()
                .Where(x =>
                    x.KunjunganID == kunjunganId.Value &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.KunjunganID,
                    x.JenisKunjungan,
                    x.AsalKunjungan
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (kunjungan == null)
                throw new InvalidOperationException("Data kunjungan tidak ditemukan.");

            var jenisKunjungan = (kunjungan.JenisKunjungan ?? "").Trim().ToUpper();
            var asalKunjungan = (kunjungan.AsalKunjungan ?? "").Trim().ToUpper();

            var isIgd =
                jenisKunjungan == "IGD" ||
                asalKunjungan == "IGD" ||
                asalKunjungan.Contains("GAWAT DARURAT");

            var isRajal =
                jenisKunjungan == "OP" ||
                jenisKunjungan == "RAJAL" ||
                jenisKunjungan == "RAWAT JALAN" ||
                asalKunjungan == "OP" ||
                asalKunjungan == "RAJAL" ||
                asalKunjungan == "RAWAT JALAN";

            if (wajibIgd && !isIgd)
                throw new InvalidOperationException("Tindakan ini hanya dapat dibuat untuk kunjungan IGD.");

            if (wajibRajal && !isRajal)
                throw new InvalidOperationException("Tindakan ini hanya dapat dibuat untuk kunjungan Rawat Jalan.");

            var kode = kodeTindakan.Trim().ToUpper();
            var unit = unitAsal?.Trim().ToUpper();

            var tarifQuery =
                from tk in _applicationDbContext.TarifKelass.AsNoTracking()
                join t in _applicationDbContext.Tindakans.AsNoTracking()
                    on tk.TindakanId equals t.TindakanId
                where (tk.IsDelete == false || tk.IsDelete == null)
                      && (t.IsDelete == false || t.IsDelete == null)
                      && t.KodeTindakan.ToUpper() == kode
                select new
                {
                    tk.TarifKelasId,
                    tk.CreateDateTime,
                    t.UnitAsal
                };

            if (!string.IsNullOrWhiteSpace(unit))
            {
                tarifQuery = tarifQuery.Where(x =>
                    x.UnitAsal != null &&
                    x.UnitAsal.ToUpper() == unit);
            }

            var tarif = await tarifQuery
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.TarifKelasId
                })
                .FirstOrDefaultAsync(cancellationToken);

            return tarif?.TarifKelasId;
        }
    }
}