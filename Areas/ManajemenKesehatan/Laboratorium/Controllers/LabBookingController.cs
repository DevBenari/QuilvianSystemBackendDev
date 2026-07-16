using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class LabBookingController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly string _uploadUrl;
        private readonly ITTDService _ttdService;
        private readonly ILogger<LabBookingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<LabBookingHub> _hubContext;
        private readonly INoPhotoGeneratorService _noPhotoGeneratorService;
        private readonly ILabBillingService _labBillingService;

        public LabBookingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingController> logger,
            IWebHostEnvironment webHostEnvironment,
            //IConfiguration configuration,
            ITTDService ttDService,
            IHubContext<LabBookingHub> hubContext,
            INoPhotoGeneratorService noPhotoGeneratorService,
            ILabBillingService labBillingService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            //_uploadUrl = configuration["FileStorage:UploadUrl"];
            _hubContext = hubContext;
            _ttdService = ttDService;
            _noPhotoGeneratorService = noPhotoGeneratorService;
            _labBillingService = labBillingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            try
            {
                // =====================================================
                // 1) Query LabBooking utama + LEFT JOIN UserActive
                // Jangan join detail dulu supaya data tidak dobel
                // =====================================================
                var baseQuery =
                    from b in _applicationDbContext.LabBookings.AsNoTracking()

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on b.CreateBy equals u0.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    where b.IsDelete == false || b.IsDelete == null

                    select new
                    {
                        Booking = b,
                        CreateByName = u != null ? u.FullName : null
                    };

                var totalRows = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var rows = await baseQuery
                    .OrderByDescending(x => x.Booking.CreateDateTime)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(x => new
                    {
                        x.Booking.CreateDateTime,
                        x.Booking.CreateBy,
                        x.CreateByName,

                        x.Booking.BookingLabId,
                        x.Booking.NomorSuratJaminan,

                        x.Booking.KunjunganId,
                        AsalKunjungan = x.Booking.Kunjungan != null ? x.Booking.Kunjungan.AsalKunjungan : null,
                        TipePasien = x.Booking.Kunjungan != null ? x.Booking.Kunjungan.TipePasien : null,
                        NoRegistrasi = x.Booking.Kunjungan != null ? x.Booking.Kunjungan.NoRegistrasi : null,
                        x.Booking.IsPasienPersiapan,

                        x.Booking.PasienId,
                        NamaLengkap = x.Booking.Pasien != null ? x.Booking.Pasien.NamaLengkap : null,
                        NoRekamMedis = x.Booking.Pasien != null ? x.Booking.Pasien.NoRekamMedis : null,
                        NoIdentitas = x.Booking.Pasien != null ? x.Booking.Pasien.NoIdentitas : null,

                        x.Booking.DiskonId,
                        NamaDiskon = x.Booking.Diskon != null ? x.Booking.Diskon.NamaDiskon : null,

                        x.Booking.TglPemeriksaan,
                        x.Booking.TglSampling,
                        x.Booking.TglBooking,
                        x.Booking.StatusPemeriksaan,

                        x.Booking.KelasId,
                        x.Booking.Keterangan,
                        x.Booking.DiagnosaAwal,

                        x.Booking.DokterKonsulenId,
                        DokterKonsulen = x.Booking.DokterKonsulen != null ? x.Booking.DokterKonsulen.NmDokter : null,

                        x.Booking.TerapisId,

                        x.Booking.AsuransiId,
                        NamaAsuransi = x.Booking.Asuransi != null ? x.Booking.Asuransi.NamaAsuransi : null,

                        x.Booking.WaktuPemeriksaan,
                        x.Booking.WaktuPemeriksaanPersiapan,
                        x.Booking.SuratRujukan,
                        x.Booking.HemodialisaKe,
                        x.Booking.NoLab,
                        x.Booking.NoPA,
                        x.Booking.StatusBookingLab,
                        x.Booking.CatatanJaminan,
                        x.Booking.IsLunas,
                        x.Booking.ProsesBooking,
                        x.Booking.TindakLanjut,
                        x.Booking.HasilPenunjangLab,
                        x.Booking.AnjuranDiet,
                        x.Booking.StatusKonfirmasi,
                    })
                    .ToListAsync();

                if (!rows.Any())
                {
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
                }

                // =====================================================
                // 2) Ambil detail hanya untuk booking yang tampil di page ini
                // =====================================================
                var bookingIds = rows.Select(x => x.BookingLabId).ToList();

                var detailRows = await _applicationDbContext.LabBookingDetails
                    .AsNoTracking()
                    .Where(d =>
                        (d.IsDelete == false || d.IsDelete == null) &&
                        d.BookingLabId.HasValue &&
                        bookingIds.Contains(d.BookingLabId.Value))
                    .Select(d => new
                    {
                        d.BookingLabId,
                        d.DetailBookingLabId,

                        d.PemeriksaanLabId,
                        NamaPemeriksaan = d.PemeriksaanLab != null ? d.PemeriksaanLab.NamaPemeriksaan : null,
                        HargaPemeriksaan = d.PemeriksaanLab != null ? d.PemeriksaanLab.HargaPemeriksaan : null,

                        d.AsalSpecimenId,
                        d.KategoriPatologiAnatomi,
                        d.JenisSpecimen,
                        d.LokasiSpecimen,
                        d.KeteranganKlinik,
                        d.PenyakitSebelumnya,
                        d.PenggunaanFiksasi,
                        d.JenisPemeriksaanGC,
                        d.JenisGC,
                        d.BahanNonGC,
                        d.BahanMicrobiologi,
                        d.MasaHaidTerakhir,

                        d.QtyOrder,
                        d.NoPhoto,
                        d.StatusPemeriksaan,
                        d.TanggalSelesai,
                        d.StatusVerifikasi,

                        d.AlasanPembatalan,
                        d.TTDPembatalanPath,
                        d.TipeLayanan
                    })
                    .ToListAsync();

                var detailMap = detailRows
                    .Where(x => x.BookingLabId.HasValue)
                    .GroupBy(x => x.BookingLabId!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Cast<object>().ToList()
                    );

                // =====================================================
                // 3) Gabungkan response
                // =====================================================
                var result = rows.Select(b =>
                {
                    detailMap.TryGetValue(b.BookingLabId, out var details);

                    return new
                    {
                        b.CreateDateTime,
                        b.CreateBy,
                        b.CreateByName,

                        b.BookingLabId,
                        b.SuratRujukan,
                        b.NomorSuratJaminan,

                        b.KunjunganId,
                        b.AsalKunjungan,
                        b.TipePasien,
                        b.NoRegistrasi,
                        b.IsPasienPersiapan,

                        b.PasienId,
                        b.NamaLengkap,
                        b.NoRekamMedis,
                        b.NoIdentitas,

                        b.DiskonId,
                        b.NamaDiskon,

                        b.TglPemeriksaan,
                        b.TglSampling,
                        b.TglBooking,
                        b.StatusPemeriksaan,

                        b.KelasId,
                        b.Keterangan,
                        b.DiagnosaAwal,

                        b.DokterKonsulenId,
                        b.DokterKonsulen,

                        b.TerapisId,

                        b.AsuransiId,
                        b.NamaAsuransi,
                        b.WaktuPemeriksaan,
                        b.WaktuPemeriksaanPersiapan,
                        b.HemodialisaKe,
                        b.NoLab,
                        b.NoPA,
                        b.StatusBookingLab,
                        b.CatatanJaminan,
                        b.IsLunas,
                        b.ProsesBooking,
                        b.TindakLanjut,
                        b.HasilPenunjangLab,
                        b.AnjuranDiet,
                        b.StatusKonfirmasi,
                        Details = details ?? new List<object>()
                    };
                }).ToList();

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = result,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "Parameter ID tidak valid."
                });
            }

            try
            {
                // =====================================================
                // 1) Ambil header LabBooking + LEFT JOIN UserActive
                // =====================================================
                var header = await (
                    from b in _applicationDbContext.LabBookings.AsNoTracking()

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on b.CreateBy equals u0.UserActiveId into uGroup
                    from u in uGroup.DefaultIfEmpty()

                    where b.BookingLabId == id
                          && (b.IsDelete == false || b.IsDelete == null)

                    select new
                    {
                        b.BookingLabId,
                        b.SuratRujukan,

                        KunjunganId = b.KunjunganId,
                        AsalKunjungan = b.Kunjungan != null ? b.Kunjungan.AsalKunjungan : null,
                        TipePasien = b.Kunjungan != null ? b.Kunjungan.TipePasien : null,
                        JenisKunjungan = b.Kunjungan != null ? b.Kunjungan.JenisKunjungan : null,
                        NoRegistrasi = b.Kunjungan != null ? b.Kunjungan.NoRegistrasi : null,
                        b.IsPasienPersiapan,

                        PoliId = b.Kunjungan != null ? b.Kunjungan.PoliklinikId : null,
                        NamaPoli = b.Kunjungan != null && b.Kunjungan.Poliklinik != null
                            ? b.Kunjungan.Poliklinik.NamaPoliklinik
                            : null,

                        PasienId = b.PasienId,
                        PasienNama = b.Pasien != null ? b.Pasien.NamaLengkap : null,
                        NoRekamMedis = b.Pasien != null ? b.Pasien.NoRekamMedis : null,
                        NoIdentitas = b.Pasien != null ? b.Pasien.NoIdentitas : null,
                        JenisKelamin = b.Pasien != null ? b.Pasien.JenisKelamin : null,

                        b.DiskonId,
                        NamaDiskon = b.Diskon != null ? b.Diskon.NamaDiskon : null,

                        b.NomorSuratJaminan,
                        b.StatusBookingLab,
                        b.CatatanJaminan,
                        b.IsLunas,

                        b.TglPemeriksaan,
                        b.TglBooking,
                        b.TglSampling,

                        b.WaktuPemeriksaan,
                        b.WaktuPemeriksaanPersiapan,

                        b.KelasId,
                        b.Keterangan,
                        b.DiagnosaAwal,
                        b.HemodialisaKe,
                        b.StatusPemeriksaan,

                        b.TindakLanjut,
                        b.HasilPenunjangLab,
                        b.AnjuranDiet,

                        b.TTDPathPembatalan,
                        b.PetugasPembatalan,
                        AlasanPembatalanLabBooking = b.AlasanPembatalan,

                        AsuransiId = b.AsuransiId,
                        AsuransiNama = b.Asuransi != null ? b.Asuransi.NamaAsuransi : null,

                        DokterKonsulenId = b.DokterKonsulenId,
                        DokterKonsulen = b.DokterKonsulen != null ? b.DokterKonsulen.NmDokter : null,

                        DokterPerujukId = b.DokterPerujukId,
                        NamaDokterPerujuk = b.DokterPerujuk != null ? b.DokterPerujuk.NmDokter : null,

                        DokterPemeriksaId = b.DokterPemeriksaId,
                        NamaDokterPemeriksa = b.DokterPemeriksa != null ? b.DokterPemeriksa.NmDokter : null,

                        KonfirmatorId = b.KonfirmatorId,
                        NamaKonfirmator = b.Konfirmator != null ? b.Konfirmator.FullName : null,
                        TglKonfrimasi = b.TglKonfirmasi,
                        b.WaktuKonfirmasi,
                        b.StatusKonfirmasi,

                        b.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        b.CreateDateTime
                    }
                ).FirstOrDefaultAsync(ct);

                if (header == null)
                {
                    return NotFound(new
                    {
                        message = "Data tidak ditemukan untuk LabBookingId tersebut."
                    });
                }

                // =====================================================
                // 2) Ambil raw detail berdasarkan BookingLabId
                // =====================================================
                var rawDetails = await _applicationDbContext.LabBookingDetails
                    .AsNoTracking()
                    .Where(d =>
                        d.BookingLabId == id &&
                        (d.IsDelete == false || d.IsDelete == null))
                    .Select(d => new
                    {
                        LabBookingDetailId = d.DetailBookingLabId,
                        d.BookingLabId,
                        d.LabId,
                        NamaLab = d.Lab != null ? d.Lab.NamaLab : null,

                        d.PemeriksaanLabId,
                        PemeriksaanNama = d.PemeriksaanLab != null
                            ? d.PemeriksaanLab.NamaPemeriksaan
                            : null,

                        HargaPemeriksaan = d.PemeriksaanLab != null
                            ? (decimal?)d.PemeriksaanLab.HargaPemeriksaan
                            : null,

                        d.AsalSpecimenId,
                        d.SpecimenJenisId,
                        d.SpecimenMethodId,

                        d.KategoriPatologiAnatomi,
                        d.JenisSpecimen,
                        d.LokasiSpecimen,
                        d.KeteranganKlinik,
                        d.PenyakitSebelumnya,
                        d.PenggunaanFiksasi,
                        d.JenisPemeriksaanGC,
                        d.JenisGC,
                        d.BahanNonGC,
                        d.BahanMicrobiologi,
                        d.MasaHaidTerakhir,

                        d.QtyOrder,
                        d.NoPhoto,
                        d.StatusPemeriksaan,
                        d.TanggalSelesai,
                        d.StatusVerifikasi,

                        d.TipeLayanan,
                        d.AlasanPembatalan,
                        d.TTDPembatalanPath,

                        d.CreateDateTime,
                        d.IsDelete
                    })
                    .ToListAsync(ct);

                // =====================================================
                // 3) Ambil status lunas per detail dari Billing
                // LabBooking.KunjunganId + PemeriksaanLabId
                // <-> Billing.KunjunganId + Billing.ItemId
                // =====================================================
                var billingStatusDict = new Dictionary<Guid, object>();

                if (header.KunjunganId.HasValue)
                {
                    var pemeriksaanIds = rawDetails
                        .Where(x => x.PemeriksaanLabId.HasValue)
                        .Select(x => x.PemeriksaanLabId!.Value)
                        .Distinct()
                        .ToList();

                    if (pemeriksaanIds.Count > 0)
                    {
                        var billingStatusList = await (
                            from bill in _applicationDbContext.Billings.AsNoTracking()
                            where (bill.IsDelete == false || bill.IsDelete == null)
                                  && bill.BillingKode == "LAB"
                                  && bill.KunjunganId.HasValue
                                  && bill.KunjunganId.Value == header.KunjunganId.Value
                                  && bill.ItemId.HasValue
                                  && pemeriksaanIds.Contains(bill.ItemId.Value)
                            group bill by bill.ItemId.Value into g
                            select new
                            {
                                PemeriksaanLabId = g.Key,

                                IsLunas = !g.Any(x => x.StatusBilling != true),

                                BillingId = g
                                    .OrderByDescending(x => x.CreateDateTime)
                                    .Select(x => (Guid?)x.BillingId)
                                    .FirstOrDefault(),

                                BillingKode = g
                                    .OrderByDescending(x => x.CreateDateTime)
                                    .Select(x => x.BillingKode)
                                    .FirstOrDefault(),

                                JenisBilling = g
                                    .OrderByDescending(x => x.CreateDateTime)
                                    .Select(x => x.JenisBilling)
                                    .FirstOrDefault(),

                                StatusBilling = g
                                    .OrderByDescending(x => x.CreateDateTime)
                                    .Select(x => x.StatusBilling)
                                    .FirstOrDefault()
                            }
                        ).ToListAsync(ct);

                        billingStatusDict = billingStatusList.ToDictionary(
                            x => x.PemeriksaanLabId,
                            x => (object)new
                            {
                                x.IsLunas,
                                x.BillingId,
                                x.BillingKode,
                                x.JenisBilling,
                                x.StatusBilling
                            }
                        );
                    }
                }

                // =====================================================
                // 4) Mapping detail + IsLunas
                // =====================================================
                var details = rawDetails
                    .Select(d =>
                    {
                        var isLunas = false;
                        Guid? billingId = null;
                        string? billingKode = null;
                        string? jenisBilling = null;
                        bool? statusBilling = null;

                        if (d.PemeriksaanLabId.HasValue &&
                            billingStatusDict.TryGetValue(d.PemeriksaanLabId.Value, out var billingObj))
                        {
                            dynamic billing = billingObj;

                            isLunas = billing.IsLunas;
                            billingId = billing.BillingId;
                            billingKode = billing.BillingKode;
                            jenisBilling = billing.JenisBilling;
                            statusBilling = billing.StatusBilling;
                        }

                        return new
                        {
                            d.LabBookingDetailId,
                            d.BookingLabId,
                            d.LabId,
                            d.NamaLab,

                            d.PemeriksaanLabId,
                            d.PemeriksaanNama,
                            d.HargaPemeriksaan,

                            d.AsalSpecimenId,
                            d.SpecimenJenisId,
                            d.SpecimenMethodId,

                            d.KategoriPatologiAnatomi,
                            d.JenisSpecimen,
                            d.LokasiSpecimen,
                            d.KeteranganKlinik,
                            d.PenyakitSebelumnya,
                            d.PenggunaanFiksasi,
                            d.JenisPemeriksaanGC,
                            d.JenisGC,
                            d.BahanNonGC,
                            d.BahanMicrobiologi,
                            d.MasaHaidTerakhir,

                            d.QtyOrder,
                            d.NoPhoto,
                            d.StatusPemeriksaan,
                            d.TanggalSelesai,
                            d.StatusVerifikasi,

                            d.TipeLayanan,
                            d.AlasanPembatalan,
                            d.TTDPembatalanPath,

                            BillingId = billingId,
                            BillingKode = billingKode,
                            JenisBilling = jenisBilling,
                            StatusBilling = statusBilling,
                            IsLunas = isLunas
                        };
                    })
                    .ToList();

                // =====================================================
                // 5) Response
                // =====================================================
                var result = new
                {
                    header.BookingLabId,
                    header.SuratRujukan,
                    header.KunjunganId,
                    header.AsalKunjungan,
                    header.TipePasien,
                    header.JenisKunjungan,
                    header.NoRegistrasi,
                    header.IsPasienPersiapan,

                    header.PoliId,
                    header.NamaPoli,

                    header.PasienId,
                    header.PasienNama,
                    header.NoRekamMedis,
                    header.NoIdentitas,
                    header.JenisKelamin,

                    header.DiskonId,
                    header.NamaDiskon,

                    header.NomorSuratJaminan,
                    header.TglPemeriksaan,
                    header.TglBooking,
                    header.TglSampling,

                    header.StatusBookingLab,
                    header.CatatanJaminan,
                    header.IsLunas,
                    header.StatusPemeriksaan,

                    header.AsuransiId,
                    header.AsuransiNama,

                    header.DokterKonsulenId,
                    header.DokterKonsulen,

                    header.DokterPerujukId,
                    header.NamaDokterPerujuk,

                    header.DokterPemeriksaId,
                    header.NamaDokterPemeriksa,

                    header.KonfirmatorId,
                    header.NamaKonfirmator,
                    header.TglKonfrimasi,
                    header.WaktuKonfirmasi,
                    header.StatusKonfirmasi,

                    header.WaktuPemeriksaan,
                    header.WaktuPemeriksaanPersiapan,

                    header.DiagnosaAwal,
                    header.HemodialisaKe,

                    header.TindakLanjut,
                    header.HasilPenunjangLab,
                    header.AnjuranDiet,

                    header.Keterangan,
                    header.TTDPathPembatalan,
                    header.PetugasPembatalan,
                    header.AlasanPembatalanLabBooking,

                    header.CreateBy,
                    header.CreateByName,
                    header.CreateDateTime,

                    Details = details
                };

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabBookingViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ======================================
                // 🔐 Ambil user aktif dari JWT
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ======================================
                // ✅ Simpan ke Database
                // ======================================
                var entity = new LabBooking
                {
                    BookingLabId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    AsuransiId = vm.AsuransiId,
                    DiskonId = vm.DiskonId,
                    TglBooking = vm.TglBooking,
                    KelasId = vm.KelasId,
                    DokterPerujukId = vm.DokterPerujukId,
                    Keterangan = vm.Keterangan,
                    IsPasienPersiapan = vm.IsPasienPersiapan,
                    DiagnosaAwal = vm.DiagnosaAwal,
                    StatusPemeriksaan = vm.StatusPemeriksaan,
                    DokterKonsulenId = vm.DokterKonsulenId,
                    TerapisId = vm.TerapisId,
                    DokterPemeriksaId = vm.DokterPemeriksaId,
                    HemodialisaKe = vm.HemodialisaKe,
                    NomorSuratJaminan = vm.NomorSuratJaminan,
                    CatatanJaminan = vm.CatatanJaminan,
                    NoLab = vm.NoLab,
                    NoPA = vm.NoPA,
                    WaktuPemeriksaan = vm.WaktuPemeriksaan,
                    WaktuPemeriksaanPersiapan = vm.WaktuPemeriksaanPersiapan,
                    StatusBookingLab = false,
                    StatusKonfirmasi = null,
                    SuratRujukan = vm.SuratRujukan,
                    AlasanPembatalan = vm.AlasanPembatalan,
                    ProsesBooking = vm.ProsesBooking,
                    TindakLanjut = vm.TindakLanjut,
                    HasilPenunjangLab = vm.HasilPenunjangLab,
                    AnjuranDiet = vm.AnjuranDiet,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTime.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.LabBookings.Add(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Lab booking Created", new
                    {
                        Action = "create",
                        id = entity.BookingLabId
                    });

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        data = new
                        {
                            entity.BookingLabId,
                            entity.NoOrder,
                            entity.NomorSuratJaminan,
                            entity.CatatanJaminan,
                            entity.TglBooking,
                            entity.CreateDateTime
                        }
                    });
                }

                return StatusCode(500, new { message = "Gagal menyimpan data ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menambahkan booking lab");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LabBookingEditViewModel vm, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                var entity = await _applicationDbContext.LabBookings
                    .FirstOrDefaultAsync(b =>
                        b.BookingLabId == id &&
                        (b.IsDelete == false || b.IsDelete == null),
                        ct);

                if (entity == null)
                    return NotFound(new { message = "Data Booking Lab tidak ditemukan. || 404 Not Found" });

                // ======================================
                // Update nilai field dulu
                // ======================================
                entity.KunjunganId = vm.KunjunganId;
                entity.PasienId = vm.PasienId;
                entity.AsuransiId = vm.AsuransiId;
                entity.DiskonId = vm.DiskonId;
                entity.DokterPemeriksaId = vm.DokterPemeriksaId;
                entity.TglBooking = vm.TglBooking;
                entity.KelasId = vm.KelasId;
                entity.DokterPerujukId = vm.DokterPerujukId;
                entity.SuratRujukan = vm.SuratRujukan;
                entity.KonfirmatorId = vm.KonfirmatorId;
                entity.WaktuKonfirmasi = vm.WaktuKonfirmasi;
                entity.TglKonfirmasi = DateTime.UtcNow;
                entity.Keterangan = vm.Keterangan;
                entity.WaktuPemeriksaan = vm.WaktuPemeriksaan;
                entity.WaktuPemeriksaanPersiapan = vm.WaktuPemeriksaanPersiapan;
                entity.IsPasienPersiapan = vm.IsPasienPersiapan;
                entity.DiagnosaAwal = vm.DiagnosaAwal;
                entity.StatusPemeriksaan = vm.StatusPemeriksaan;
                entity.DokterKonsulenId = vm.DokterKonsulenId;
                entity.TerapisId = vm.TerapisId;
                entity.HemodialisaKe = vm.HemodialisaKe;
                entity.NomorSuratJaminan = vm.NomorSuratJaminan;
                entity.CatatanJaminan = vm.CatatanJaminan;
                entity.TindakLanjut = vm.TindakLanjut;
                entity.HasilPenunjangLab = vm.HasilPenunjangLab;
                entity.AnjuranDiet = vm.AnjuranDiet;

                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = DateTime.UtcNow;

                // Simpan dulu supaya KonfirmatorId sudah masuk ke DB
                await _applicationDbContext.SaveChangesAsync(ct);

                // ======================================
                // Generate NoPhoto dan billing hanya kalau sudah konfirmasi
                // ======================================
                var generatedCount = 0;
                var labBillingCreated = 0;
                if (entity.KonfirmatorId.HasValue)
                {
                    generatedCount = await _noPhotoGeneratorService
                        .GenerateNoPhotosByLabBookingIdAsync(entity.BookingLabId, ct);

                    labBillingCreated = await _labBillingService
                    .EnsureLabBillingOnConfirmationAsync(
                        entity.BookingLabId,
                        userActiveId,
                        ct);
                }

                await transaction.CommitAsync(ct);

                await _hubContext.Clients.All.SendAsync("Lab booking changed", new
                {
                    Action = "changed",
                    TriageId = entity.BookingLabId
                }, ct);

                return Ok(new
                {
                    message = "Data berhasil diperbarui. || 200 OK",
                    data = new
                    {
                        entity.BookingLabId,
                        totalNoPhotoGenerated = generatedCount,
                        totalLabBillingCreated = labBillingCreated,
                        entity.NoOrder,
                        entity.NomorSuratJaminan,
                        entity.CatatanJaminan,
                        entity.TglBooking,
                        entity.TglPemeriksaan,
                        entity.TglKonfirmasi,
                        entity.KonfirmatorId,
                        entity.UpdateDateTime
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error saat memperbarui booking lab");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("StatusPemeriksaanLab/{id}")]
        public async Task<IActionResult> StatusPemeriksaanLab(
            Guid id,
            [FromBody] StatusPemeriksaanLabViewModel vm)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ======================================
                // 🔐 Ambil user aktif dari JWT
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ======================================
                // 🔎 Ambil Header + Detail
                // ======================================
                var entity = await _applicationDbContext.LabBookings
                    .Include(x => x.LabBookingDetails)
                    .FirstOrDefaultAsync(x =>
                        x.BookingLabId == id &&
                        (x.IsDelete == false || x.IsDelete == null));

                if (entity == null)
                {
                    return NotFound(new
                    {
                        message = "Data Booking Lab tidak ditemukan. || 404 Not Found"
                    });
                }

                // ======================================
                // 🕒 Waktu update
                // ======================================
                var now = DateTime.UtcNow;

                // ======================================
                // Update Header
                // ======================================
                entity.StatusPemeriksaan = vm.Status;
                entity.TglPemeriksaan = vm.TglPemeriksaan;
                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = now;

                // ======================================
                // Update Seluruh Detail
                // ======================================
                foreach (var detail in entity.LabBookingDetails.Where(x => x.IsDelete != true))
                {
                    detail.StatusPemeriksaan = vm.Status;
                    detail.UpdateBy = userActiveId;
                    detail.UpdateDateTime = now;
                }

                // ======================================
                // Simpan perubahan
                // ======================================
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Data berhasil diperbarui. || 200 OK",
                    data = new
                    {
                        entity.BookingLabId,
                        entity.NoOrder,
                        entity.NomorSuratJaminan,
                        entity.CatatanJaminan,
                        entity.StatusPemeriksaan,
                        entity.TglPemeriksaan,
                        entity.TglBooking,
                        entity.UpdateDateTime,
                        TotalDetailDiupdate = entity.LabBookingDetails.Count(x => x.IsDelete != true)
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Kesalahan database: {dbEx.InnerException?.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat memperbarui status pemeriksaan lab");

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPut("StatusPembayaranLab/{id}")]
        public async Task<IActionResult> StatusPembayaranLab(Guid id, [FromBody] string status)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (status == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ======================================
                // 🔐 Ambil user aktif dari JWT
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ======================================
                // 🔎 Cek apakah data booking ada
                // ======================================
                var entity = await _applicationDbContext.LabBookings
                    .FirstOrDefaultAsync(b => b.BookingLabId == id && (b.IsDelete == false || b.IsDelete == null));

                if (entity == null)
                    return NotFound(new { message = "Data Booking Lab tidak ditemukan. || 404 Not Found" });

                // ======================================
                // ⚙️ Update nilai field
                // ======================================
                entity.IsLunas = status;

                // ======================================
                // 🕒 Update metadata
                // ======================================
                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.LabBookings.Update(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil diperbarui. || 200 OK",
                        data = new
                        {
                            entity.BookingLabId,
                            entity.NoOrder,
                            entity.NomorSuratJaminan,
                            entity.CatatanJaminan,
                            entity.TglBooking,
                            entity.UpdateDateTime
                        }
                    });
                }

                return StatusCode(500, new { message = "Gagal memperbarui data ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat memperbarui booking lab");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("ProsesBookingLab/{id}")]
        public async Task<IActionResult> ProsesBookingLab(Guid id, [FromBody] string status)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (status == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ======================================
                // 🔐 Ambil user aktif dari JWT
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ======================================
                // 🔎 Cek apakah data booking ada
                // ======================================
                var entity = await _applicationDbContext.LabBookings
                    .FirstOrDefaultAsync(b => b.BookingLabId == id && (b.IsDelete == false || b.IsDelete == null));

                if (entity == null)
                    return NotFound(new { message = "Data Booking Lab tidak ditemukan. || 404 Not Found" });

                // ======================================
                // ⚙️ Update nilai field
                // ======================================
                entity.ProsesBooking = status;

                // ======================================
                // 🕒 Update metadata
                // ======================================
                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.LabBookings.Update(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil diperbarui. || 200 OK",
                        data = new
                        {
                            entity.BookingLabId,
                            entity.NoOrder,
                            entity.NomorSuratJaminan,
                            entity.CatatanJaminan,
                            entity.TglBooking,
                            entity.UpdateDateTime
                        }
                    });
                }

                return StatusCode(500, new { message = "Gagal memperbarui data ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat memperbarui booking lab");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("StatusKonfirmasiBooking/{id}")]
        public async Task<IActionResult> KonfirmasiLab(
            Guid id,
            [FromBody] StatusKonfirmasiBookingViewModel vm,
            CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync(ct);

            try
            {
                // ======================================
                // Ambil user aktif dari JWT
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;
                var now = DateTimeOffset.UtcNow;

                // ======================================
                // Cek apakah data booking ada
                // ======================================
                var entity = await _applicationDbContext.LabBookings
                    .FirstOrDefaultAsync(b =>
                        b.BookingLabId == id &&
                        (b.IsDelete == false || b.IsDelete == null),
                        ct);

                if (entity == null)
                    return NotFound(new { message = "Data Booking Lab tidak ditemukan. || 404 Not Found" });

                // ======================================
                // Update LabBooking
                // ======================================
                entity.StatusKonfirmasi = vm.Status;
                entity.KonfirmatorId = vm.KonfirmatorId;
                entity.TglKonfirmasi = DateTime.UtcNow;
                entity.DokterPemeriksaId = vm.DokterPemeriksaId;
                entity.WaktuKonfirmasi = vm.WaktuKonfirmasi;
                entity.TglSampling = vm.TglSampling;

                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = now;

                // Update dokter pemeriksa di LabBookingDetail
                var updatedDetailCount = await _applicationDbContext.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE ""LabBookingDetail""
                    SET 
                        ""DokterPemeriksaId"" = {vm.DokterPemeriksaId},
                        ""UpdateBy"" = {userActiveId},
                        ""UpdateDateTime"" = {now}
                    WHERE ""BookingLabId"" = {id}
                      AND (""IsDelete"" = false OR ""IsDelete"" IS NULL)
                ", ct);

                var result = await _applicationDbContext.SaveChangesAsync(ct);

                await trx.CommitAsync(ct);

                return Ok(new
                {
                    message = "Data berhasil diperbarui. || 200 OK",
                    data = new
                    {
                        entity.BookingLabId,
                        entity.NoOrder,
                        entity.NomorSuratJaminan,
                        entity.CatatanJaminan,
                        entity.TglBooking,
                        entity.StatusKonfirmasi,
                        entity.KonfirmatorId,
                        entity.DokterPemeriksaId,
                        entity.TglSampling,
                        entity.UpdateDateTime,
                        UpdatedLabBookingDetail = updatedDetailCount
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = "Kesalahan database.",
                    error = dbEx.Message,
                    innerError = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(ct);

                _logger.LogError(ex, "Error saat memperbarui booking lab");

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("StatusBookingLab/{id}")]
        public async Task<IActionResult> StatusBookingLab(Guid id, [FromBody] bool status)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (status == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ======================================
                // 🔐 Ambil user aktif dari JWT
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ======================================
                // 🔎 Cek apakah data booking ada
                // ======================================
                var entity = await _applicationDbContext.LabBookings
                    .FirstOrDefaultAsync(b => b.BookingLabId == id && (b.IsDelete == false || b.IsDelete == null));

                if (entity == null)
                    return NotFound(new { message = "Data Booking Lab tidak ditemukan. || 404 Not Found" });

                // ======================================
                // ⚙️ Update nilai field
                // ======================================
                entity.StatusBookingLab = status;

                // ======================================
                // 🕒 Update metadata
                // ======================================
                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.LabBookings.Update(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil diperbarui. || 200 OK",
                        data = new
                        {
                            entity.BookingLabId,
                            entity.NoOrder,
                            entity.NomorSuratJaminan,
                            entity.CatatanJaminan,
                            entity.TglBooking,
                            entity.UpdateDateTime
                        }
                    });
                }

                return StatusCode(500, new { message = "Gagal memperbarui data ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat memperbarui booking lab");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("BatalLabBooking/{id}")]
        //[RequestSizeLimit(10_000_000)]
        //[RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> BatalLabBooking(
        Guid id,
        [FromBody] LabBookingDetailBatalVM vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // 🔍 Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // 🔍 Ambil user dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;


                // ==========================================================
                // 🔍 Ambil LabBooking (HEADER saja)
                // ==========================================================
                var booking = await _applicationDbContext.LabBookings
                    .FirstOrDefaultAsync(x => x.BookingLabId == id);

                if (booking == null)
                    return NotFound(new { message = "Lab Booking tidak ditemukan." });


                // ==========================================================
                // 🔧 Upload TTD Pembatalan
                // ==========================================================
                //    async Task<(string? filePath, Guid? ttdId)> UploadTTDAsync(IFormFile? file)
                //    {
                //        if (file == null || file.Length == 0) return (null, null);

                //        var allowedExtensions = new[] { ".jpg", ".jpeg" };
                //        var ext = Path.GetExtension(file.FileName).ToLower();

                //        if (!allowedExtensions.Contains(ext))
                //            throw new Exception("Format TTD tidak valid! Gunakan JPG atau JPEG.");

                //        if (file.Length > 1 * 1024 * 1024)
                //            throw new Exception("Ukuran file TTD terlalu besar! Maksimal 1MB.");

                //        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                //        var fileName = $"{getUserActive.FullName}_{timestamp}_TTDPembatalan{ext}";
                //        var filePath = $"/TTDUser/{fileName}";

                //        // Upload ke Flask
                //        using var client = new HttpClient();
                //        using var ms = new MemoryStream();
                //        await file.CopyToAsync(ms);
                //        ms.Position = 0;

                //        using var content = new MultipartFormDataContent
                //{
                //    {
                //        new StreamContent(ms)
                //        {
                //            Headers =
                //            {
                //                ContentType =
                //                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType)
                //            }
                //        },
                //        "file",
                //        fileName
                //    },
                //    { new StringContent("TTDUser"), "folderTarget" }
                //};

                //        var response = await client.PostAsync(_uploadUrl, content);
                //        if (!response.IsSuccessStatusCode)
                //            throw new Exception("Gagal upload TTD ke server Flask.");

                //        // Simpan metadata ke database
                //        var newTTD = new MasterTTD
                //        {
                //            TTDId = Guid.NewGuid(),
                //            UserActiveId = userActiveId,
                //            TTDPath = filePath,
                //            CreateDateTime = DateTimeOffset.UtcNow,
                //            CreateBy = userActiveId
                //        };

                //        _applicationDbContext.MasterTTDs.Add(newTTD);
                //        await _applicationDbContext.SaveChangesAsync();

                //        return (filePath, newTTD.TTDId);
                //    }


                //    string? ttdPath = null;
                //    Guid? ttdId = null;

                //    if (vm.TTDPembatalan != null)
                //        (ttdPath, ttdId) = await UploadTTDAsync(vm.TTDPembatalan);



                // ==========================================================
                // 🔄 UPDATE HEADER LAB BOOKING SAJA
                // ==========================================================

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync(vm.TTDPetugasId ?? Guid.Empty);

                var petugas = await _applicationDbContext.UserActives
                    .FindAsync(vm.TTDPetugasId);
                
                booking.AlasanPembatalan = vm.AlasanPembatalan;
                booking.TTDPathPembatalan = ttd.Path;
                booking.PetugasPembatalan = petugas?.FullName;
                booking.UpdateBy = userActiveId;
                booking.UpdateDateTime = DateTimeOffset.UtcNow;


                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new
                    {
                        message = "Pembatalan Lab Booking berhasil.",
                        TTDID = ttd.TTDId
                    });

                return StatusCode(500, new { message = "Gagal menyimpan data ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.LabBookings.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabBookings.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAsync(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            bool? isPasienPersiapan = null,
            Guid? labId = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            string? search = null,
            string? namaLab = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? isLunas = null)
        {
            // basic guard
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            // whitelist sorting
            var allowedOrderBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CreateDateTime",
                "TglBooking",
                "TglPemeriksaan",
                "NoOrder",
                "StatusBookingLab",
                "StatusPembayaran"
            };

            if (string.IsNullOrWhiteSpace(orderBy) || !allowedOrderBy.Contains(orderBy))
                orderBy = "CreateDateTime";

            sortDirection = (sortDirection ?? "desc").ToLower();

            // =========================================
            // 1) BASE QUERY PARENT
            // =========================================
            IQueryable<LabBooking> parentQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                parentQuery = parentQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                parentQuery = parentQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (isPasienPersiapan.HasValue)
                parentQuery = parentQuery.Where(b => b.IsPasienPersiapan == isPasienPersiapan);


            // filter JenisKunjungan
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value;

                parentQuery =
                    from b in parentQuery
                    where b.Kunjungan.JenisKunjungan == jk.ToString()
                    select b;
            }

            // filter norm dan noidentitas
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                parentQuery = parentQuery.Where(b =>
                    b.Pasien != null &&
                    (
                        (
                            b.Pasien.NoRekamMedis != null &&
                            EF.Functions.ILike(b.Pasien.NoRekamMedis, $"%{keyword}%")
                        )
                        ||
                        (
                            b.Pasien.NoIdentitas != null &&
                            EF.Functions.ILike(b.Pasien.NoIdentitas, $"%{keyword}%")
                        )
                    )
                );
            }

            // filter periode
            if (periode.HasValue)
            {
                DateTime todayUtc = DateTime.UtcNow.Date;

                DateTime? rangeStart = null;
                DateTime? rangeEndExclusive = null;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStart = todayUtc;
                        rangeEndExclusive = todayUtc.AddDays(1);
                        break;

                    case PeriodeFilter.ThisWeek:
                        int diff = (7 + ((int)todayUtc.DayOfWeek == 0 ? 7 : (int)todayUtc.DayOfWeek) - (int)DayOfWeek.Monday) % 7;
                        var startWeek = todayUtc.AddDays(-diff);
                        rangeStart = startWeek;
                        rangeEndExclusive = todayUtc.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        {
                            int diff2 = (7 + ((int)todayUtc.DayOfWeek == 0 ? 7 : (int)todayUtc.DayOfWeek) - (int)DayOfWeek.Monday) % 7;
                            var thisWeekStart = todayUtc.AddDays(-diff2);
                            rangeStart = thisWeekStart.AddDays(-7);
                            rangeEndExclusive = thisWeekStart;
                            break;
                        }

                    case PeriodeFilter.ThisMonth:
                        {
                            var startMonth = new DateTime(todayUtc.Year, todayUtc.Month, 1);
                            rangeStart = startMonth;
                            rangeEndExclusive = startMonth.AddMonths(1);
                            break;
                        }

                    case PeriodeFilter.LastMonth:
                        {
                            var lastMonth = todayUtc.AddMonths(-1);
                            var startLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                            rangeStart = startLastMonth;
                            rangeEndExclusive = startLastMonth.AddMonths(1);
                            break;
                        }

                    case PeriodeFilter.ThisYear:
                        {
                            var startYear = new DateTime(todayUtc.Year, 1, 1);
                            rangeStart = startYear;
                            rangeEndExclusive = startYear.AddYears(1);
                            break;
                        }

                    case PeriodeFilter.LastYear:
                        {
                            var startLastYear = new DateTime(todayUtc.Year - 1, 1, 1);
                            rangeStart = startLastYear;
                            rangeEndExclusive = startLastYear.AddYears(1);
                            break;
                        }

                    case PeriodeFilter.Last3Months:
                        rangeStart = todayUtc.AddMonths(-3);
                        rangeEndExclusive = todayUtc.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = todayUtc.AddMonths(-6);
                        rangeEndExclusive = todayUtc.AddDays(1);
                        break;
                }

                if (rangeStart.HasValue && rangeEndExclusive.HasValue)
                {
                    parentQuery = parentQuery.Where(u =>
                        u.CreateDateTime >= rangeStart.Value &&
                        u.CreateDateTime < rangeEndExclusive.Value);
                }
            }

            // filter date range
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                parentQuery = parentQuery.Where(b =>
                    b.CreateDateTime >= start && b.CreateDateTime < endExclusive);
            }

            // =========================================
            // 2) TOTAL ROWS
            // =========================================
            int totalRows = await parentQuery.CountAsync();

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            // =========================================
            // 3) SORTING
            // =========================================
            parentQuery = sortDirection == "asc"
                ? parentQuery
                    .OrderBy(e => EF.Property<object>(e, orderBy!))
                    .ThenBy(e => e.BookingLabId)
                : parentQuery
                    .OrderByDescending(e => EF.Property<object>(e, orderBy!))
                    .ThenByDescending(e => e.BookingLabId);

            // =========================================
            // 4) PAGED IDS
            // =========================================
            var pagedIds = await parentQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync();

            var orderMap = pagedIds
                .Select((id, idx) => new { id, idx })
                .ToDictionary(x => x.id, x => x.idx);

            // =========================================
            // 5) LOAD PARENTS
            // =========================================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()
                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where pagedIds.Contains(b.BookingLabId)
                select new
                {
                    b.BookingLabId,
                    b.SuratRujukan,
                    b.NoOrder,
                    b.NoLab,
                    b.NoPA,
                    b.NomorSuratJaminan,

                    KunjunganId = b.KunjunganId,
                    AsalKunjungan = b.Kunjungan != null ? b.Kunjungan.AsalKunjungan : null,
                    TipePasien = b.Kunjungan != null ? b.Kunjungan.TipePasien : null,
                    JenisKunjungan = b.Kunjungan != null ? b.Kunjungan.JenisKunjungan : null,
                    NoRegistrasi = b.Kunjungan != null ? b.Kunjungan.NoRegistrasi : null,
                    IsPasienPersiapan= b.IsPasienPersiapan,

                    PasienId = b.PasienId,
                    NamaLengkap = b.Pasien != null ? b.Pasien.NamaLengkap : null,
                    NoRekamMedis = b.Pasien != null ? b.Pasien.NoRekamMedis : null,
                    NoIdentitas = b.Pasien != null ? b.Pasien.NoIdentitas : null,
                    JenisKelamin = b.Pasien != null ? b.Pasien.JenisKelamin : null,

                    PoliId = b.Kunjungan != null ? b.Kunjungan.PoliklinikId : null,
                    NamaPoli = b.Kunjungan != null && b.Kunjungan.Poliklinik != null
                            ? b.Kunjungan.Poliklinik.NamaPoliklinik
                            : null,

                    b.DiskonId,
                    NamaDiskon = b.Diskon != null ? b.Diskon.NamaDiskon : null,

                    b.AsuransiId,
                    AsuransiNama = b.Asuransi != null ? b.Asuransi.NamaAsuransi : null,

                    DokterKonsulenId = b.DokterKonsulenId,
                    DokterKonsulen = b.DokterKonsulen != null ? b.DokterKonsulen.NmDokter : null,

                    DokterPerujukId = b.DokterPerujukId,
                    NamaDokterPerujuk = b.DokterPerujuk != null ? b.DokterPerujuk.NmDokter : null,

                    DokterPemeriksaId = b.DokterPemeriksaId,
                    NamaDokterPemeriksa = b.DokterPemeriksa != null ? b.DokterPemeriksa.NmDokter : null,

                    KonfirmatorId = b.KonfirmatorId,
                    NamaKonfirmator = b.Konfirmator != null ? b.Konfirmator.FullName : null,
                    TglKonfrimasi = b.TglKonfirmasi,
                    b.WaktuKonfirmasi,
                    b.WaktuPemeriksaan,
                    b.WaktuPemeriksaanPersiapan,
                    b.StatusKonfirmasi,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.IsLunas,
                    b.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,

                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.PetugasPembatalan,
                    b.TTDPathPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    CreateBy = u != null ? u.FullName : null
                })
                .ToListAsync();

            parents = parents
                .OrderBy(x => orderMap.TryGetValue(x.BookingLabId, out var idx) ? idx : int.MaxValue)
                .ToList();

            // =========================================
            // 6) LOAD DETAIL RAW
            // =========================================
            var detailQ =
                from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                join b in _applicationDbContext.LabBookings.AsNoTracking()
                    on d.BookingLabId equals b.BookingLabId
                join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()
                where d.BookingLabId.HasValue
                      && pagedIds.Contains(d.BookingLabId.Value)
                      && (d.IsDelete == false || d.IsDelete == null)
                select new
                {
                    BookingLabId = d.BookingLabId.Value,
                    KunjunganId = b.KunjunganId,
                    d.DetailBookingLabId,
                    d.PasienId,
                    d.PemeriksaanLabId,
                    d.DokterPemeriksaId,
                    NamaDokter = d.DokterPemeriksa != null ? d.DokterPemeriksa.NmDokter : null,
                    TipeLayanan = d.TipeLayanan ?? null,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    HargaPemeriksaan = lp != null ? (decimal?)lp.HargaPemeriksaan : null,
                    d.LabId,
                    NamaLab = d.Lab != null ? d.Lab.NamaLab : null,
                    d.QtyOrder,
                    d.NoPhoto,
                    d.StatusPemeriksaan,
                    d.StatusVerifikasi,
                    d.TanggalSelesai,
                    d.CreateDateTime,
                    d.IsDelete
                };

            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                var nl = namaLab.Trim();
                detailQ = detailQ.Where(x => x.NamaLab != null && EF.Functions.ILike(x.NamaLab, $"%{nl}%"));
            }

            if (labId.HasValue)
            {
                var lid = labId.Value;
                detailQ = detailQ.Where(x => x.LabId == lid);
            }

            var rawDetails = await detailQ
                .OrderByDescending(x => x.CreateDateTime)
                .ToListAsync();

            // =========================================
            // 7) AMBIL STATUS LUNAS PER DETAIL
            // LabBooking.KunjunganId + PemeriksaanLabId
            // <-> Billing.KunjunganId + ItemId
            // =========================================
            var kunjunganIds = rawDetails
                .Where(x => x.KunjunganId.HasValue)
                .Select(x => x.KunjunganId!.Value)
                .Distinct()
                .ToList();

            var pemeriksaanIds = rawDetails
                .Where(x => x.PemeriksaanLabId.HasValue)
                .Select(x => x.PemeriksaanLabId!.Value)
                .Distinct()
                .ToList();

            var billingStatusDict = new Dictionary<(Guid KunjunganId, Guid PemeriksaanLabId), bool>();

            if (kunjunganIds.Count > 0 && pemeriksaanIds.Count > 0)
            {
                var billingStatusList = await (
                    from bill in _applicationDbContext.Billings.AsNoTracking()
                    where (bill.IsDelete == false || bill.IsDelete == null)
                          && bill.BillingKode == "LAB"
                          && bill.KunjunganId.HasValue
                          && kunjunganIds.Contains(bill.KunjunganId.Value)
                          && bill.ItemId.HasValue
                          && pemeriksaanIds.Contains(bill.ItemId.Value)
                    group bill by new
                    {
                        KunjunganId = bill.KunjunganId.Value,
                        PemeriksaanLabId = bill.ItemId.Value
                    } into g
                    select new
                    {
                        g.Key.KunjunganId,
                        g.Key.PemeriksaanLabId,
                        IsLunas = !g.Any(x => x.StatusBilling != true)
                    }
                ).ToListAsync();

                billingStatusDict = billingStatusList.ToDictionary(
                    x => (x.KunjunganId, x.PemeriksaanLabId),
                    x => x.IsLunas
                );
            }

            // =========================================
            // 8) MAP DETAIL + FILTER isLunas (OPS A)
            // =========================================
            var details = rawDetails
                .Select(d =>
                {
                    bool detailIsLunas = false;

                    if (d.KunjunganId.HasValue && d.PemeriksaanLabId.HasValue)
                    {
                        billingStatusDict.TryGetValue(
                            (d.KunjunganId.Value, d.PemeriksaanLabId.Value),
                            out detailIsLunas
                        );
                    }

                    return new
                    {
                        d.BookingLabId,
                        d.DetailBookingLabId,
                        d.KunjunganId,
                        d.PasienId,
                        d.LabId,
                        d.NamaLab,
                        d.PemeriksaanLabId,
                        d.NamaPemeriksaan,
                        d.HargaPemeriksaan,
                        d.QtyOrder,
                        d.NoPhoto,
                        d.StatusPemeriksaan,
                        d.StatusVerifikasi,
                        d.DokterPemeriksaId,
                        d.NamaDokter,
                        d.TanggalSelesai,
                        d.CreateDateTime,
                        d.IsDelete,
                        IsLunas = detailIsLunas
                    };
                })
                .Where(x => !isLunas.HasValue || x.IsLunas == isLunas.Value)
                .ToList();

            var emptyDetails = new List<object>();

            var detailLookup = details
                .GroupBy(x => x.BookingLabId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (object)x).ToList()
                );

            // =========================================
            // 9) MERGE
            // =========================================
            var merged = parents.Select(p => new
            {
                Parent = p,
                Details = detailLookup.TryGetValue(p.BookingLabId, out var list)
                    ? list
                    : emptyDetails
            }).ToList();

            // =========================================
            // 10) RETURN
            // =========================================
            return Ok(new
            {
                status = "success",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }

        [HttpGet("pagedRadiologi")]
        public async Task<IActionResult> PagedRadiologi(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            bool? isPasienPersiapan = null,
            string? search = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? isLunas = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =============================
            // 0) Ambil LabId Radiologi
            // =============================
            var radiologiLabIds = await _applicationDbContext.Labs
                .AsNoTracking()
                .Where(l => l.NamaLab != null &&
                            l.NamaLab.ToLower().Replace(" ", "") == "radiologi")
                .Select(l => l.LabId)
                .ToListAsync();

            if (radiologiLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Radiologi retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            // =============================
            // 1) BASE QUERY HEADER
            // LabId sekarang ada di LabBooking
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (isPasienPersiapan.HasValue)
                baseQuery = baseQuery.Where(b => b.IsPasienPersiapan == isPasienPersiapan);


            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value.ToString();

                baseQuery = baseQuery.Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.JenisKunjungan == jk);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                baseQuery = baseQuery.Where(b =>
                    b.Pasien != null &&
                    (
                        (
                            b.Pasien.NoRekamMedis != null &&
                            EF.Functions.ILike(b.Pasien.NoRekamMedis, $"%{keyword}%")
                        )
                        ||
                        (
                            b.Pasien.NoIdentitas != null &&
                            EF.Functions.ILike(b.Pasien.NoIdentitas, $"%{keyword}%")
                        )
                    )
                );
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);

                baseQuery = baseQuery.Where(b =>
                    b.CreateDateTime >= start &&
                    b.CreateDateTime <= end);
            }

            // Pastikan booking punya detail aktif
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null) &&
                    radiologiLabIds.Contains(d.LabId)
                )
            );

            // =============================
            // 2) TOTAL ROWS HEADER
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 3) SORTING HEADER
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking)
                         : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan)
                         : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime)
                         : baseQuery.OrderBy(x => x.CreateDateTime)
            };

            // =============================
            // 4) PAGING HEADER
            // =============================
            var pagedParentIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync();

            if (pagedParentIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Radiologi retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                    }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 5) LOAD HEADER DATA
            // =============================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where pagedIdSet.Contains(b.BookingLabId)

                select new
                {
                    b.BookingLabId,
                    b.SuratRujukan,
                    b.NoOrder,

                    KunjunganId = b.KunjunganId,
                    AsalKunjungan = b.Kunjungan != null ? b.Kunjungan.AsalKunjungan : null,
                    TipePasien = b.Kunjungan != null ? b.Kunjungan.TipePasien : null,
                    JenisKunjungan = b.Kunjungan != null ? b.Kunjungan.JenisKunjungan : null,
                    NoRegistrasi = b.Kunjungan != null ? b.Kunjungan.NoRegistrasi : null,
                    isPasienPersiapan = b.IsPasienPersiapan,

                    PasienId = b.PasienId,
                    NamaLengkap = b.Pasien != null ? b.Pasien.NamaLengkap : null,
                    NoRekamMedis = b.Pasien != null ? b.Pasien.NoRekamMedis : null,
                    NoIdentitas = b.Pasien != null ? b.Pasien.NoIdentitas : null,
                    JenisKelamin = b.Pasien != null ? b.Pasien.JenisKelamin : null,

                    PoliId = b.Kunjungan != null ? b.Kunjungan.PoliklinikId : null,
                    NamaPoli = b.Kunjungan != null && b.Kunjungan.Poliklinik != null
                        ? b.Kunjungan.Poliklinik.NamaPoliklinik
                        : null,

                    b.DiskonId,
                    NamaDiskon = b.Diskon != null ? b.Diskon.NamaDiskon : null,

                    b.AsuransiId,
                    AsuransiNama = b.Asuransi != null ? b.Asuransi.NamaAsuransi : null,

                    DokterKonsulenId = b.DokterKonsulenId,
                    DokterKonsulen = b.DokterKonsulen != null ? b.DokterKonsulen.NmDokter : null,

                    DokterPerujukId = b.DokterPerujukId,
                    NamaDokterPerujuk = b.DokterPerujuk != null ? b.DokterPerujuk.NmDokter : null,

                    KonfirmatorId = b.KonfirmatorId,
                    NamaKonfirmator = b.Konfirmator != null ? b.Konfirmator.FullName : null,
                    b.WaktuKonfirmasi,
                    TglKonfirmasi = b.TglKonfirmasi,
                    b.WaktuPemeriksaan,
                    b.WaktuPemeriksaanPersiapan,
                    b.StatusKonfirmasi,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.IsLunas,
                    b.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,

                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.PetugasPembatalan,
                    b.TTDPathPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    CreateBy = u != null ? u.FullName : null
                })
                .ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 6) LOAD DETAIL
            // Tidak filter d.LabId lagi
            // =============================
            var rawDetails = await (
                from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                join b in _applicationDbContext.LabBookings.AsNoTracking()
                    on d.BookingLabId equals b.BookingLabId

                join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                from lp in lpJoin.DefaultIfEmpty()

                where d.BookingLabId != null
                      && pagedIdSet.Contains(d.BookingLabId.Value)
                      && (d.IsDelete == false || d.IsDelete == null)

                orderby d.CreateDateTime descending

                select new
                {
                    BookingLabId = d.BookingLabId.Value,
                    LabId = d.LabId,
                    NamaLab = d.Lab != null ? d.Lab.NamaLab : null,

                    KunjunganId = b.KunjunganId,
                    d.DetailBookingLabId,
                    d.PasienId,
                    TipeLayanan = d.TipeLayanan,
                    d.PemeriksaanLabId,
                    d.NoPhoto,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    HargaPemeriksaan = lp != null ? lp.HargaPemeriksaan : null,
                    d.DokterPemeriksaId,
                    NamaDokter = d.DokterPemeriksa != null ? d.DokterPemeriksa.NmDokter : null,
                    d.QtyOrder,
                    d.StatusPemeriksaan,
                    d.StatusVerifikasi,
                    d.TanggalSelesai,
                    d.CreateDateTime,
                    d.IsDelete
                })
                .ToListAsync();

            // =============================
            // 7) STATUS LUNAS PER DETAIL
            // Relasi: KunjunganId + PemeriksaanLabId
            // =============================
            var kunjunganIds = rawDetails
                .Where(x => x.KunjunganId.HasValue)
                .Select(x => x.KunjunganId!.Value)
                .Distinct()
                .ToList();

            var pemeriksaanIds = rawDetails
                .Where(x => x.PemeriksaanLabId.HasValue)
                .Select(x => x.PemeriksaanLabId!.Value)
                .Distinct()
                .ToList();

            var billingStatusDict = new Dictionary<(Guid KunjunganId, Guid PemeriksaanLabId), bool>();

            if (kunjunganIds.Any() && pemeriksaanIds.Any())
            {
                var billingStatusList = await (
                    from bill in _applicationDbContext.Billings.AsNoTracking()
                    where (bill.IsDelete == false || bill.IsDelete == null)
                          && bill.BillingKode == "LAB"
                          && bill.KunjunganId.HasValue
                          && kunjunganIds.Contains(bill.KunjunganId.Value)
                          && bill.ItemId.HasValue
                          && pemeriksaanIds.Contains(bill.ItemId.Value)
                    group bill by new
                    {
                        KunjunganId = bill.KunjunganId.Value,
                        PemeriksaanLabId = bill.ItemId.Value
                    } into g
                    select new
                    {
                        g.Key.KunjunganId,
                        g.Key.PemeriksaanLabId,
                        IsLunas = !g.Any(x => x.StatusBilling != true)
                    })
                    .ToListAsync();

                billingStatusDict = billingStatusList.ToDictionary(
                    x => (x.KunjunganId, x.PemeriksaanLabId),
                    x => x.IsLunas);
            }

            // =============================
            // 8) MAP DETAIL + FILTER isLunas
            // =============================
            var finalDetails = rawDetails
                .Select(d =>
                {
                    var detailIsLunas = false;

                    if (d.KunjunganId.HasValue && d.PemeriksaanLabId.HasValue)
                    {
                        billingStatusDict.TryGetValue(
                            (d.KunjunganId.Value, d.PemeriksaanLabId.Value),
                            out detailIsLunas);
                    }

                    return new
                    {
                        d.BookingLabId,
                        d.LabId,
                        d.NamaLab,
                        d.DetailBookingLabId,
                        d.KunjunganId,
                        d.PasienId,
                        d.PemeriksaanLabId,
                        d.NoPhoto,
                        d.NamaPemeriksaan,
                        d.HargaPemeriksaan,
                        d.QtyOrder,
                        d.StatusPemeriksaan,
                        d.StatusVerifikasi,
                        d.TanggalSelesai,
                        d.DokterPemeriksaId,
                        d.NamaDokter,
                        d.CreateDateTime,
                        d.IsDelete,
                        IsLunas = detailIsLunas
                    };
                })
                .Where(x => !isLunas.HasValue || x.IsLunas == isLunas.Value)
                .ToList();

            var emptyDetails = finalDetails.Take(0).ToList();

            var detailLookup = finalDetails
                .GroupBy(x => x.BookingLabId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =============================
            // 9) MERGE
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                })
                .ToList();

            // =============================
            // 10) RETURN
            // =============================
            return Ok(new
            {
                status = "success",
                message = "Data Radiologi retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }


        [HttpGet("pagedRehabMedis")]
        public async Task<IActionResult> Paged2RehabMedis(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            string? search = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =============================
            // 0) Ambil LabId Rehab Medis
            // =============================
            var rehabLabIds = await _applicationDbContext.Labs
                .AsNoTracking()
                .Where(l => l.NamaLab != null &&
                            l.NamaLab.ToLower().Replace(" ", "") == "rehabmedis")
                .Select(l => l.LabId)
                .ToListAsync();

            if (rehabLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Rehabmedis retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            // =============================
            // 1) BASE QUERY HEADER
            // LabId sekarang ada di LabBooking
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value.ToString();

                baseQuery = baseQuery.Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.JenisKunjungan == jk);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                baseQuery = baseQuery.Where(b =>
                    b.Pasien != null &&
                    (
                        (
                            b.Pasien.NoRekamMedis != null &&
                            EF.Functions.ILike(b.Pasien.NoRekamMedis, $"%{keyword}%")
                        )
                        ||
                        (
                            b.Pasien.NoIdentitas != null &&
                            EF.Functions.ILike(b.Pasien.NoIdentitas, $"%{keyword}%")
                        )
                    )
                );
            }

            // =============================
            // 2) Filter tanggal manual
            // =============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                baseQuery = baseQuery.Where(b =>
                    b.CreateDateTime >= start &&
                    b.CreateDateTime < endExclusive);
            }

            // =============================
            // 3) Filter periode
            // =============================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                DateTime rangeStart;
                DateTime rangeEndExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStart = today;
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisWeek:
                        rangeStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStart = thisWeekStart.AddDays(-7);
                        rangeEndExclusive = thisWeekStart;
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisMonth:
                        rangeStart = new DateTime(today.Year, today.Month, 1);
                        rangeEndExclusive = rangeStart.AddMonths(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        rangeStart = thisMonthStart.AddMonths(-1);
                        rangeEndExclusive = thisMonthStart;
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisYear:
                        rangeStart = new DateTime(today.Year, 1, 1);
                        rangeEndExclusive = rangeStart.AddYears(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTime(today.Year, 1, 1);
                        rangeStart = thisYearStart.AddYears(-1);
                        rangeEndExclusive = thisYearStart;
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last3Months:
                        rangeStart = today.AddMonths(-3);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = today.AddMonths(-6);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;
                }
            }

            // =============================
            // 4) Pastikan booking punya detail aktif
            // Tidak perlu filter d.LabId lagi
            // =============================
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null)));

            // =============================
            // 5) TOTAL rows
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 6) SORTING
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking)
                         : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan)
                         : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime)
                         : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 7) PAGING HEADER
            // =============================
            var pagedParentIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync();

            if (pagedParentIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Rehabmedis retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                    }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 8) LOAD HEADER DATA
            // =============================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where pagedIdSet.Contains(b.BookingLabId)

                select new
                {
                    b.BookingLabId,
                    b.SuratRujukan,
                    b.NoOrder,

                    KunjunganId = b.KunjunganId,
                    AsalKunjungan = b.Kunjungan != null ? b.Kunjungan.AsalKunjungan : null,
                    TipePasien = b.Kunjungan != null ? b.Kunjungan.TipePasien : null,
                    JenisKunjungan = b.Kunjungan != null ? b.Kunjungan.JenisKunjungan : null,
                    NoRegistrasi = b.Kunjungan != null ? b.Kunjungan.NoRegistrasi : null,

                    PasienId = b.PasienId,
                    NamaLengkap = b.Pasien != null ? b.Pasien.NamaLengkap : null,
                    NoRekamMedis = b.Pasien != null ? b.Pasien.NoRekamMedis : null,
                    NoIdentitas = b.Pasien != null ? b.Pasien.NoIdentitas : null,
                    JenisKelamin = b.Pasien != null ? b.Pasien.JenisKelamin : null,

                    PoliId = b.Kunjungan != null ? b.Kunjungan.PoliklinikId : null,
                    NamaPoli = b.Kunjungan != null && b.Kunjungan.Poliklinik != null
                        ? b.Kunjungan.Poliklinik.NamaPoliklinik
                        : null,

                    b.DiskonId,
                    NamaDiskon = b.Diskon != null ? b.Diskon.NamaDiskon : null,

                    b.AsuransiId,
                    AsuransiNama = b.Asuransi != null ? b.Asuransi.NamaAsuransi : null,

                    DokterKonsulenId = b.DokterKonsulenId,
                    DokterKonsulen = b.DokterKonsulen != null ? b.DokterKonsulen.NmDokter : null,

                    DokterPerujukId = b.DokterPerujukId,
                    NamaDokterPerujuk = b.DokterPerujuk != null ? b.DokterPerujuk.NmDokter : null,

                    KonfirmatorId = b.KonfirmatorId,
                    NamaKonfirmator = b.Konfirmator != null ? b.Konfirmator.FullName : null,
                    TglKonfirmasi = b.TglKonfirmasi,
                    b.WaktuKonfirmasi,
                    b.StatusKonfirmasi,
                    b.WaktuPemeriksaan,
                    b.WaktuPemeriksaanPersiapan,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.IsLunas,
                    b.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,

                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.PetugasPembatalan,
                    b.TTDPathPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    CreateBy = u != null ? u.FullName : null
                })
                .ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 9) LOAD DETAIL
            // Tidak filter d.LabId lagi
            // Lab diambil dari header LabBooking
            // =============================
            var details = await
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                 join b in _applicationDbContext.LabBookings.AsNoTracking()
                     on d.BookingLabId equals b.BookingLabId

                 join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                 from lp in lpJoin.DefaultIfEmpty()

                 where d.BookingLabId != null
                       && pagedIdSet.Contains(d.BookingLabId.Value)
                       && (d.IsDelete == false || d.IsDelete == null)

                 select new
                 {
                     BookingLabId = d.BookingLabId,

                     LabId = d.LabId,
                     NamaLab = d.Lab != null ? d.Lab.NamaLab : null,

                     d.DetailBookingLabId,
                     d.NoPhoto,
                     TipeLayanan = d.TipeLayanan,

                     d.PemeriksaanLabId,
                     NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                     HargaPemeriksaan = lp != null ? lp.HargaPemeriksaan : null,


                     d.QtyOrder,
                     d.IsDelete
                 })
                .ToListAsync();

            var emptyDetails = details.Take(0).ToList();

            var detailLookup = details
                .Where(x => x.BookingLabId.HasValue)
                .GroupBy(x => x.BookingLabId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =============================
            // 10) MERGE
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                })
                .ToList();

            // =============================
            // 11) RETURN
            // =============================
            return Ok(new
            {
                status = "success",
                message = "Data Rehabmedis retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }



        [HttpGet("pagedLabGizi")]
        public async Task<IActionResult> Paged2LabGizi(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            Guid? dokterPemeriksaId = null,
            string? dokterKonsul = null,
            string? search = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =============================
            // 0) Ambil LabId Gizi
            // =============================
            var giziLabIds = await _applicationDbContext.Labs
                .AsNoTracking()
                .Where(l => l.NamaLab != null &&
                            l.NamaLab.ToLower().Replace(" ", "") == "gizi")
                .Select(l => l.LabId)
                .ToListAsync();

            if (giziLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Lab Gizi retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            // =============================
            // 1) BASE QUERY HEADER
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                baseQuery = baseQuery.Where(b =>
                    b.Pasien != null &&
                    (
                        (
                            b.Pasien.NoRekamMedis != null &&
                            EF.Functions.ILike(b.Pasien.NoRekamMedis, $"%{keyword}%")
                        )
                        ||
                        (
                            b.Pasien.NoIdentitas != null &&
                            EF.Functions.ILike(b.Pasien.NoIdentitas, $"%{keyword}%")
                        )
                    )
                );
            }

            // =============================
            // 2) Filter periode
            // =============================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                DateTime rangeStart;
                DateTime rangeEndExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStart = today;
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisWeek:
                        rangeStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStart = thisWeekStart.AddDays(-7);
                        rangeEndExclusive = thisWeekStart;
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisMonth:
                        rangeStart = new DateTime(today.Year, today.Month, 1);
                        rangeEndExclusive = rangeStart.AddMonths(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        rangeStart = thisMonthStart.AddMonths(-1);
                        rangeEndExclusive = thisMonthStart;
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisYear:
                        rangeStart = new DateTime(today.Year, 1, 1);
                        rangeEndExclusive = rangeStart.AddYears(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTime(today.Year, 1, 1);
                        rangeStart = thisYearStart.AddYears(-1);
                        rangeEndExclusive = thisYearStart;
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last3Months:
                        rangeStart = today.AddMonths(-3);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = today.AddMonths(-6);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x =>
                            x.CreateDateTime >= rangeStart &&
                            x.CreateDateTime < rangeEndExclusive);
                        break;
                }
            }

            // =============================
            // 3) Filter tanggal manual
            // =============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                baseQuery = baseQuery.Where(b =>
                    b.CreateDateTime >= start &&
                    b.CreateDateTime < endExclusive);
            }

            // =============================
            // 4) Filter dokter konsulen
            // =============================
            if (!string.IsNullOrWhiteSpace(dokterKonsul))
            {
                var dk = dokterKonsul.Trim().ToLower();

                baseQuery = baseQuery.Where(b =>
                    b.DokterKonsulenId != null &&
                    _applicationDbContext.Dokters.Any(dr =>
                        dr.DokterId == b.DokterKonsulenId &&
                        dr.NmDokter != null &&
                        dr.NmDokter.ToLower().Contains(dk)));
            }

            // =============================
            // 5) Filter jenis kunjungan
            // =============================
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value.ToString();

                baseQuery = baseQuery.Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.JenisKunjungan == jk);
            }

            // =============================
            // 6) Pastikan booking punya detail aktif
            // Tidak perlu filter d.LabId lagi
            // =============================
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null)));

            // =============================
            // 7) TOTAL rows
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 8) SORTING
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking)
                         : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan)
                         : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime)
                         : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 9) PAGING HEADER
            // =============================
            var pagedParentIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync();

            if (pagedParentIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Lab Gizi retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                    }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 10) LOAD HEADER DATA
            // =============================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where pagedIdSet.Contains(b.BookingLabId)

                select new
                {
                    b.BookingLabId,
                    b.SuratRujukan,
                    b.NoOrder,

                    KunjunganId = b.KunjunganId,
                    AsalKunjungan = b.Kunjungan != null ? b.Kunjungan.AsalKunjungan : null,
                    TipePasien = b.Kunjungan != null ? b.Kunjungan.TipePasien : null,
                    JenisKunjungan = b.Kunjungan != null ? b.Kunjungan.JenisKunjungan : null,
                    NoRegistrasi = b.Kunjungan != null ? b.Kunjungan.NoRegistrasi : null,

                    PasienId = b.PasienId,
                    NamaLengkap = b.Pasien != null ? b.Pasien.NamaLengkap : null,
                    NoRekamMedis = b.Pasien != null ? b.Pasien.NoRekamMedis : null,
                    JenisKelamin = b.Pasien != null ? b.Pasien.JenisKelamin : null,

                    PoliId = b.Kunjungan != null ? b.Kunjungan.PoliklinikId : null,
                    NamaPoli = b.Kunjungan != null && b.Kunjungan.Poliklinik != null
                        ? b.Kunjungan.Poliklinik.NamaPoliklinik
                        : null,

                    b.DiskonId,
                    NamaDiskon = b.Diskon != null ? b.Diskon.NamaDiskon : null,

                    b.AsuransiId,
                    AsuransiNama = b.Asuransi != null ? b.Asuransi.NamaAsuransi : null,

                    DokterKonsulenId = b.DokterKonsulenId,
                    DokterKonsulen = b.DokterKonsulen != null ? b.DokterKonsulen.NmDokter : null,

                    DokterPerujukId = b.DokterPerujukId,
                    NamaDokterPerujuk = b.DokterPerujuk != null ? b.DokterPerujuk.NmDokter : null,

                    KonfirmatorId = b.KonfirmatorId,
                    NamaKonfirmator = b.Konfirmator != null ? b.Konfirmator.FullName : null,
                    TglKonfirmasi = b.TglKonfirmasi,
                    b.WaktuKonfirmasi,
                    b.StatusKonfirmasi,
                    b.WaktuPemeriksaan,
                    b.WaktuPemeriksaanPersiapan,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.IsLunas,
                    b.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,

                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.PetugasPembatalan,
                    b.TTDPathPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    CreateBy = u != null ? u.FullName : null
                })
                .ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 11) LOAD DETAIL
            // Tidak filter d.LabId lagi
            // Lab diambil dari header LabBooking
            // =============================
            var details = await
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                 join b in _applicationDbContext.LabBookings.AsNoTracking()
                     on d.BookingLabId equals b.BookingLabId

                 join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                 from lp in lpJoin.DefaultIfEmpty()

                 where d.BookingLabId != null
                       && pagedIdSet.Contains(d.BookingLabId.Value)
                       && (d.IsDelete == false || d.IsDelete == null)

                 select new
                 {
                     BookingLabId = d.BookingLabId,

                     LabId = d.LabId,
                     NamaLab = d.Lab != null ? d.Lab.NamaLab : null,

                     d.DetailBookingLabId,
                     TipeLayanan = d.TipeLayanan,

                     d.PemeriksaanLabId,
                     NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                     HargaPemeriksaan = lp != null ? lp.HargaPemeriksaan : null,
                     d.DokterPemeriksaId,
                     NamaDokter = d.DokterPemeriksa != null ? d.DokterPemeriksa.NmDokter : null,
                     d.QtyOrder,
                     d.NoPhoto,
                     d.IsDelete
                 })
                .ToListAsync();

            var emptyDetails = details.Take(0).ToList();

            var detailLookup = details
                .Where(x => x.BookingLabId.HasValue)
                .GroupBy(x => x.BookingLabId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =============================
            // 12) MERGE
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                })
                .ToList();

            // =============================
            // 13) RETURN
            // =============================
            return Ok(new
            {
                status = "success",
                message = "Data Lab Gizi retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }


        [HttpGet("pagedLabMCU")]
        public async Task<IActionResult> Paged2LabMCU(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            string? dokterKonsul = null,
            string? search = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =============================
            // 0) Ambil LabId MCU
            // =============================
            var mcuLabIds = await _applicationDbContext.Labs
                .AsNoTracking()
                .Where(l =>
                    l.NamaLab != null &&
                    l.NamaLab.ToLower().Replace(" ", "") == "mcu" &&
                    (l.IsDelete == false || l.IsDelete == null))
                .Select(l => l.LabId)
                .ToListAsync();

            if (mcuLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Lab MCU retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

                    // =============================
                    // 1) BASE QUERY
                    // =============================
                    var baseQuery = _applicationDbContext.LabBookings
                        .AsNoTracking()
                        .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);


            // =============================
            // 2) Filter jenis kunjungan
            // =============================
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value.ToString();

                baseQuery = baseQuery.Where(b =>
                    b.Kunjungan != null &&
                    b.Kunjungan.JenisKunjungan == jk);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                baseQuery = baseQuery.Where(b =>
                    b.Pasien != null &&
                    (
                        (
                            b.Pasien.NoRekamMedis != null &&
                            EF.Functions.ILike(b.Pasien.NoRekamMedis, $"%{keyword}%")
                        )
                        ||
                        (
                            b.Pasien.NoIdentitas != null &&
                            EF.Functions.ILike(b.Pasien.NoIdentitas, $"%{keyword}%")
                        )
                    )
                );
            }

            // =============================
            // 3) Filter start/end date manual
            // =============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);

                baseQuery = baseQuery.Where(b =>
                    b.CreateDateTime >= start &&
                    b.CreateDateTime < endExclusive);
            }

            // =============================
            // 4) Filter periode
            // =============================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                DateTime rangeStart;
                DateTime rangeEndExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStart = today;
                        rangeEndExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.ThisWeek:
                        rangeStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeEndExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStart = thisWeekStart.AddDays(-7);
                        rangeEndExclusive = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        rangeStart = new DateTime(today.Year, today.Month, 1);
                        rangeEndExclusive = rangeStart.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        rangeStart = thisMonthStart.AddMonths(-1);
                        rangeEndExclusive = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        rangeStart = new DateTime(today.Year, 1, 1);
                        rangeEndExclusive = rangeStart.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTime(today.Year, 1, 1);
                        rangeStart = thisYearStart.AddYears(-1);
                        rangeEndExclusive = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        rangeStart = today.AddMonths(-3);
                        rangeEndExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = today.AddMonths(-6);
                        rangeEndExclusive = today.AddDays(1);
                        break;

                    default:
                        rangeStart = DateTime.MinValue;
                        rangeEndExclusive = DateTime.MaxValue;
                        break;
                }

                baseQuery = baseQuery.Where(x =>
                    x.CreateDateTime >= rangeStart &&
                    x.CreateDateTime < rangeEndExclusive);
            }

            // =============================
            // 5) Filter dokter konsulen
            // =============================
            if (!string.IsNullOrWhiteSpace(dokterKonsul))
            {
                var dk = dokterKonsul.Trim().ToLower();

                baseQuery = baseQuery.Where(b =>
                    b.DokterKonsulenId != null &&
                    _applicationDbContext.Dokters.Any(dr =>
                        dr.DokterId == b.DokterKonsulenId &&
                        dr.NmDokter != null &&
                        dr.NmDokter.ToLower().Contains(dk)));
            }

            // =============================
            // 6) Pastikan booking punya detail aktif
            // Tidak perlu filter d.LabId lagi
            // =============================
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null)));

            // =============================
            // 7) TOTAL rows
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 8) SORTING
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking)
                         : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan)
                         : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime)
                         : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 9) PAGING HEADER
            // =============================
            var pagedParentIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(b => b.BookingLabId)
                .ToListAsync();

            if (pagedParentIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data Lab MCU retrieved successfully",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                    }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 10) LOAD HEADER DATA
            // =============================
            var parents = await (
                from b in _applicationDbContext.LabBookings.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on b.CreateBy equals u.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where pagedIdSet.Contains(b.BookingLabId)

                select new
                {
                    b.BookingLabId,
                    b.SuratRujukan,
                    b.NoOrder,

                    KunjunganId = b.KunjunganId,
                    AsalKunjungan = b.Kunjungan != null ? b.Kunjungan.AsalKunjungan : null,
                    TipePasien = b.Kunjungan != null ? b.Kunjungan.TipePasien : null,
                    JenisKunjungan = b.Kunjungan != null ? b.Kunjungan.JenisKunjungan : null,
                    NoRegistrasi = b.Kunjungan != null ? b.Kunjungan.NoRegistrasi : null,

                    PasienId = b.PasienId,
                    NamaLengkap = b.Pasien != null ? b.Pasien.NamaLengkap : null,
                    NoRekamMedis = b.Pasien != null ? b.Pasien.NoRekamMedis : null,
                    JenisKelamin = b.Pasien != null ? b.Pasien.JenisKelamin : null,

                    PoliId = b.Kunjungan != null ? b.Kunjungan.PoliklinikId : null,
                    NamaPoli = b.Kunjungan != null && b.Kunjungan.Poliklinik != null
                        ? b.Kunjungan.Poliklinik.NamaPoliklinik
                        : null,

                    b.DiskonId,
                    NamaDiskon = b.Diskon != null ? b.Diskon.NamaDiskon : null,

                    b.AsuransiId,
                    AsuransiNama = b.Asuransi != null ? b.Asuransi.NamaAsuransi : null,

                    DokterKonsulenId = b.DokterKonsulenId,
                    DokterKonsulen = b.DokterKonsulen != null ? b.DokterKonsulen.NmDokter : null,

                    DokterPerujukId = b.DokterPerujukId,
                    NamaDokterPerujuk = b.DokterPerujuk != null ? b.DokterPerujuk.NmDokter : null,

                    KonfirmatorId = b.KonfirmatorId,
                    NamaKonfirmator = b.Konfirmator != null ? b.Konfirmator.FullName : null,
                    TglKonfirmasi = b.TglKonfirmasi,
                    b.WaktuKonfirmasi,
                    b.StatusKonfirmasi,
                    b.WaktuPemeriksaan,
                    b.WaktuPemeriksaanPersiapan,
                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.IsLunas,
                    b.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,

                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.PetugasPembatalan,
                    b.TTDPathPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    CreateBy = u != null ? u.FullName : null
                })
                .ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 11) LOAD DETAIL
            // Tidak filter d.LabId lagi
            // Lab diambil dari header LabBooking
            // =============================
            var details = await
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                 join b in _applicationDbContext.LabBookings.AsNoTracking()
                     on d.BookingLabId equals b.BookingLabId

                 join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                 from lp in lpJoin.DefaultIfEmpty()

                 where d.BookingLabId != null
                       && pagedIdSet.Contains(d.BookingLabId.Value)
                       && (d.IsDelete == false || d.IsDelete == null)

                 select new
                 {
                     BookingLabId = d.BookingLabId,

                     LabId = d.LabId,
                     NamaLab = d.Lab != null ? d.Lab.NamaLab : null,

                     d.DetailBookingLabId,
                     d.NoPhoto,
                     TipeLayanan = d.TipeLayanan,

                     d.PemeriksaanLabId,
                     NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                     HargaPemeriksaan = lp != null ? lp.HargaPemeriksaan : null,
                     d.QtyOrder,
                     d.DokterPemeriksaId,
                     NamaDokter = d.DokterPemeriksa != null ? d.DokterPemeriksa.NmDokter : null,
                     d.IsDelete
                 })
                .ToListAsync();

            var emptyDetails = details.Take(0).ToList();

            var detailLookup = details
                .Where(x => x.BookingLabId.HasValue)
                .GroupBy(x => x.BookingLabId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =============================
            // 12) MERGE
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                })
                .ToList();

            // =============================
            // 13) RETURN
            // =============================
            return Ok(new
            {
                status = "success",
                message = "Data Lab MCU retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }

    }
}
