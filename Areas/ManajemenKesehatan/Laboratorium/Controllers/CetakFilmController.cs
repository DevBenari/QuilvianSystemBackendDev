using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class CetakFilmController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<CetakFilmController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IKunjunganTransactionGuard _kunjunganTransactionGuard;


        public CetakFilmController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CetakFilmController> logger,
            IWebHostEnvironment webHostEnvironment,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IKunjunganTransactionGuard kunjunganTransactionGuard)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _kunjunganTransactionGuard = kunjunganTransactionGuard;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            var data = await _applicationDbContext.CetakFilms
                .AsNoTracking()
                .Where(x =>
                    x.CetakFilmId == id &&
                    (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new
                {
                    x.CetakFilmId,
                    x.KunjunganId,
                    x.PasienId,
                    x.DokterPerujukId,
                    x.KelasId,
                    x.LabBookingId,
                    x.HasilLabId,

                    x.NoOrder,
                    x.TglOrder,
                    x.WaktuOrder,
                    x.TglSelesai,
                    x.TotalCetakFilm,
                    x.Keterangan,

                    // =========================
                    // Data tambahan dari relasi
                    // =========================

                    NamaPasien = x.Pasien != null
                        ? x.Pasien.NamaLengkap
                        : null,

                    NoRekamMedis = x.Pasien != null
                        ? x.Pasien.NoRekamMedis
                        : null,

                    JenisKelamin = x.Pasien != null
                        ? x.Pasien.JenisKelamin
                        : null,

                    NoRegistrasi = x.Kunjungan != null
                        ? x.Kunjungan.NoRegistrasi
                        : null,

                    JenisKunjungan = x.Kunjungan != null
                        ? x.Kunjungan.JenisKunjungan
                        : null,

                    NamaDokterPerujuk = x.DokterPerujuk != null
                        ? x.DokterPerujuk.NmDokter
                        : null,

                    NamaKelas = x.Kelas != null
                        ? x.Kelas.NamaKelas
                        : null,

                    NoOrderBooking = x.LabBooking != null
                        ? x.LabBooking.NoOrder
                        : null,

                    NoLab = x.LabBooking != null
                        ? x.LabBooking.NoLab
                        : null,

                    NoPA = x.LabBooking != null
                        ? x.LabBooking.NoPA
                        : null,

                    // =========================
                    // Metadata
                    // =========================

                    x.CreateDateTime,
                    x.CreateBy,
                    x.UpdateDateTime,
                    x.UpdateBy,
                    x.DeleteDateTime,
                    x.DeleteBy,
                    x.IsDelete,

                    // =========================
                    // Detail Cetak Film
                    // =========================

                    Details = x.Details
                        .Where(d => d.IsDelete == false || d.IsDelete == null)
                        .OrderBy(d => d.CreateDateTime)
                        .Select(d => new
                        {
                            d.DetailCetakFilmId,
                            d.CetakFilmId,
                            d.DetailHasilLabId,
                            d.LabBookingDetailId,

                            LabId = d.LabBookingDetail != null ? d.LabBookingDetail.LabId : null,
                            NamaLab = d.LabBookingDetail.Lab != null ? d.LabBookingDetail.Lab.NamaLab : null,

                            PemeriksaanId = d.LabBookingDetail != null ? d.LabBookingDetail.PemeriksaanLabId : null,
                            NamaPemeriksaan = d.LabBookingDetail.PemeriksaanLab != null ? d.LabBookingDetail.PemeriksaanLab.NamaPemeriksaan : null,
                            NoPhoto = d.LabBookingDetail != null ? d.LabBookingDetail.NoPhoto : null,

                            d.DokterPemeriksaId,
                            NamaDokterPemeriksa = d.DokterPemeriksa != null ? d.DokterPemeriksa.NmDokter : null,

                            PathHasilPhoto = d.LabHasilDetail != null ? d.LabHasilDetail.PhotoLabPath : null,
                            HasilLab = d.LabHasilDetail != null ? d.LabHasilDetail.HasilLabManual : null,
                            HasilLabAI = d.LabHasilDetail != null ? d.LabHasilDetail.HasilLabAI : null,

                            d.FilmId,
                            NamaFilm = d.Film != null ? d.Film.NamaFilm : null,
                            UkuranFilm = d.Film != null ? d.Film.UkuranFilm : null,
                            d.QtyCetakFilm,
                            d.TotalCetakFilm,
                            d.Keterangan,

                            d.CreateDateTime,
                            d.CreateBy,
                            d.UpdateDateTime,
                            d.UpdateBy,
                            d.DeleteDateTime,
                            d.DeleteBy,
                            d.IsDelete
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Data Cetak Film tidak ditemukan. || 404 Not Found"
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CetakFilmViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Detail cetak film wajib diisi." });

            await using var transaction = await _applicationDbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

            try
            {
                // ======================================
                // Ambil user login
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)vm.KunjunganId, ct);

                // ======================================
                // Ambil data LabBooking sebagai fallback
                // ======================================
                LabBooking? labBooking = null;

                if (vm.LabBookingId.HasValue)
                {
                    labBooking = await _applicationDbContext.LabBookings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.BookingLabId == vm.LabBookingId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);
                }

                var kunjunganId = vm.KunjunganId ?? labBooking?.KunjunganId;
                var pasienId = vm.PasienId ?? labBooking?.PasienId;
                var kelasId = vm.KelasId ?? labBooking?.KelasId;

                if (!kunjunganId.HasValue)
                    return BadRequest(new { message = "KunjunganId wajib diisi." });

                if (!pasienId.HasValue)
                    return BadRequest(new { message = "PasienId wajib diisi." });

                if (!kelasId.HasValue)
                    return BadRequest(new { message = "KelasId wajib diisi untuk mengambil tarif film." });

                // ======================================
                // Buat parent CetakFilm
                // ======================================
                var cetakFilmId = Guid.NewGuid();

                var cetakFilm = new CetakFilm
                {
                    CetakFilmId = cetakFilmId,

                    KunjunganId = kunjunganId,
                    PasienId = pasienId,
                    DokterPerujukId = vm.DokterPerujukId ?? labBooking?.DokterPerujukId,
                    KelasId = kelasId,
                    LabBookingId = vm.LabBookingId,
                    HasilLabId = vm.HasilLabId,

                    NoOrder = !string.IsNullOrWhiteSpace(vm.NoOrder)
                        ? vm.NoOrder
                        : labBooking?.NoOrder,

                    TglOrder = vm.TglOrder ?? DateOnly.FromDateTime(DateTime.Now),
                    WaktuOrder = vm.WaktuOrder ?? TimeOnly.FromDateTime(DateTime.Now),
                    TglSelesai = vm.TglSelesai,

                    Keterangan = vm.Keterangan,

                    TotalCetakFilm = vm.TotalCetakFilm,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.CetakFilms.Add(cetakFilm);

                var detailResponses = new List<object>();

                var invoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId.Value,
                    DateTime.UtcNow);

                // Ambil index awal billing cetak film.
                // Karena billing dibuat banyak dalam 1 transaksi, index berikutnya dinaikkan manual di memory.
                var existingBgllCodes = await _applicationDbContext.Billings
                    .AsNoTracking()
                    .Where(b =>
                        b.KunjunganId == kunjunganId.Value &&
                        b.BillingKode != null &&
                        b.BillingKode.StartsWith("BGLL") &&
                        (b.IsDelete == false || b.IsDelete == null))
                    .Select(b => b.BillingKode!)
                    .ToListAsync(ct);

                var billingIndex = 0;

                foreach (var code in existingBgllCodes)
                {
                    if (code.Length >= 7 &&
                        int.TryParse(code.Substring(4), out var number) &&
                        number > billingIndex)
                    {
                        billingIndex = number;
                    }
                }

                // ======================================
                // Buat detail CetakFilm
                // ======================================
                foreach (var detailVm in vm.Details)
                {
                    if (!detailVm.FilmId.HasValue)
                        return BadRequest(new { message = "FilmId wajib diisi pada detail cetak film." });

                    if (!detailVm.LabBookingDetailId.HasValue)
                        return BadRequest(new { message = "LabBookingDetailId wajib diisi pada detail cetak film." });

                    var film = await _applicationDbContext.Films
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.FilmId == detailVm.FilmId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (film == null)
                        return BadRequest(new { message = $"FilmId {detailVm.FilmId} tidak ditemukan." });

                    var tarifFilm = await _applicationDbContext.TarifFilms
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.FilmId == detailVm.FilmId.Value &&
                            x.KelasId == kelasId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    var labBookingDetail = await _applicationDbContext.LabBookingDetails
                        .AsNoTracking()
                        .Include(x => x.Lab)
                        .Include(x => x.PemeriksaanLab)
                        .Include(x => x.DokterPemeriksa)
                        .FirstOrDefaultAsync(x =>
                            x.DetailBookingLabId == detailVm.LabBookingDetailId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (labBookingDetail == null)
                        return BadRequest(new { message = $"LabBookingDetailId {detailVm.LabBookingDetailId} tidak ditemukan." });

                    var hargaSatuanFilm =
                        tarifFilm?.TarifTotal ??
                        0m;

                    if (hargaSatuanFilm <= 0)
                    {
                        return BadRequest(new
                        {
                            message = $"Harga satuan film tidak ditemukan untuk FilmId {detailVm.FilmId} dan KelasId {kelasId}."
                        });
                    }

                    var qtyDecimal = detailVm.QtyCetakFilm ?? 1m;

                    if (qtyDecimal <= 0)
                        return BadRequest(new { message = "QtyCetakFilm harus lebih dari 0." });

                    if (qtyDecimal % 1 != 0)
                        return BadRequest(new { message = "QtyCetakFilm harus bilangan bulat." });

                    var qtyCetakFilm = Convert.ToInt32(qtyDecimal);

                    var totalDetail = hargaSatuanFilm * qtyCetakFilm;

                    var detailCetakFilmId = Guid.NewGuid();

                    var detail = new CetakFilmDetail
                    {
                        DetailCetakFilmId = detailCetakFilmId,
                        CetakFilmId = cetakFilmId,

                        DetailHasilLabId = detailVm.DetailHasilLabId,
                        LabBookingDetailId = detailVm.LabBookingDetailId,


                        DokterPemeriksaId = detailVm.DokterPemeriksaId ?? labBookingDetail.DokterPemeriksaId,

                        FilmId = detailVm.FilmId,
                        QtyCetakFilm = qtyCetakFilm,
                        TotalCetakFilm = totalDetail,

                        Keterangan = detailVm.Keterangan,

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    };

                    _applicationDbContext.CetakFilmDetails.Add(detail);

                    // ======================================
                    // Billing per detail film
                    // ======================================
                    billingIndex++;

                    var kode = $"BGLL{billingIndex:D3}";

                    var billing = new Billing
                    {
                        BillingId = Guid.NewGuid(),

                        KunjunganId = kunjunganId.Value,

                        // ItemId dibuat per detail cetak film,
                        // karena billing juga per film/detail yang dicetak.
                        ItemId = film.FilmId,

                        NamaItem = !string.IsNullOrWhiteSpace(film.NamaFilm)
                            ? $"{film.NamaFilm} - {labBookingDetail.PemeriksaanLab.NamaPemeriksaan}"
                            : "Cetak Film",

                        HargaItem = hargaSatuanFilm,
                        QtyItem = qtyCetakFilm,
                        SubTotalItem = totalDetail,

                        InvoiceBilling = invoiceBilling,

                        IsListWhiteOff = false,

                        BillingKode = kode,
                        JenisBilling = "Biaya Lain - Lain",

                        StatusBilling = false,
                        TipeLayanan = vm.TipeLayanan,

                        BillingDate = DateTime.UtcNow,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    };

                    _applicationDbContext.Billings.Add(billing);

                    detailResponses.Add(new
                    {
                        detail.DetailCetakFilmId,
                        detail.FilmId,
                        film.NamaFilm,
                        film.UkuranFilm,
                        detail.LabBookingDetailId,
                        detail.QtyCetakFilm,
                        detail.TotalCetakFilm,

                        Billing = new
                        {
                            billing.BillingId,
                            billing.InvoiceBilling,
                            billing.BillingKode,
                            billing.JenisBilling,
                            billing.NamaItem,
                            billing.HargaItem,
                            billing.QtyItem,
                            billing.SubTotalItem,
                            billing.StatusBilling
                        }
                    });
                }

                await _applicationDbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Created("", new
                {
                    message = "Data Cetak Film dan Billing berhasil dibuat. || 201 Created",
                    data = new
                    {
                        cetakFilm.CetakFilmId,
                        cetakFilm.KunjunganId,
                        cetakFilm.PasienId,
                        cetakFilm.LabBookingId,
                        cetakFilm.NoOrder,
                        cetakFilm.TglOrder,
                        cetakFilm.WaktuOrder,
                        cetakFilm.TotalCetakFilm,
                        Details = detailResponses
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(ct);
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CetakFilmViewModel vm, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.Details == null || !vm.Details.Any())
                return BadRequest(new { message = "Detail cetak film wajib diisi." });

            await using var transaction = await _applicationDbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

            try
            {
                // ======================================
                // Ambil user login
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)vm.KunjunganId, ct);

                // ======================================
                // Ambil parent CetakFilm
                // ======================================
                var cetakFilm = await _applicationDbContext.CetakFilms
                    .FirstOrDefaultAsync(x =>
                        x.CetakFilmId == id &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (cetakFilm == null)
                    return NotFound(new { message = "Data Cetak Film tidak ditemukan. || 404 Not Found" });

                // ======================================
                // Ambil LabBooking fallback
                // ======================================
                LabBooking? labBooking = null;

                var labBookingId = vm.LabBookingId ?? cetakFilm.LabBookingId;

                if (labBookingId.HasValue)
                {
                    labBooking = await _applicationDbContext.LabBookings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.BookingLabId == labBookingId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);
                }

                var kunjunganId = vm.KunjunganId ?? cetakFilm.KunjunganId ?? labBooking?.KunjunganId;
                var pasienId = vm.PasienId ?? cetakFilm.PasienId ?? labBooking?.PasienId;
                var kelasId = vm.KelasId ?? cetakFilm.KelasId ?? labBooking?.KelasId;

                if (!kunjunganId.HasValue)
                    return BadRequest(new { message = "KunjunganId wajib diisi." });

                if (!pasienId.HasValue)
                    return BadRequest(new { message = "PasienId wajib diisi." });

                if (!kelasId.HasValue)
                    return BadRequest(new { message = "KelasId wajib diisi untuk mengambil tarif film." });

                // ======================================
                // Cek detail lama dan billing lama
                // ======================================
                var existingDetails = await _applicationDbContext.CetakFilmDetails
                    .Where(x =>
                        x.CetakFilmId == id &&
                        (x.IsDelete == false || x.IsDelete == null))
                    .ToListAsync(ct);

                var existingDetailIds = existingDetails
                    .Select(x => x.DetailCetakFilmId)
                    .ToList();

                var existingBillings = await _applicationDbContext.Billings
                    .Where(b =>
                        b.KunjunganId == kunjunganId.Value &&
                        b.ItemId.HasValue &&
                        existingDetailIds.Contains(b.ItemId.Value) &&
                        b.BillingKode != null &&
                        b.BillingKode.StartsWith("BGLL") &&
                        b.JenisBilling == "Biaya Lain - Lain" &&
                        (b.IsDelete == false || b.IsDelete == null))
                    .ToListAsync(ct);

                if (existingBillings.Any(x => x.StatusBilling == true))
                {
                    return BadRequest(new
                    {
                        message = "Data cetak film tidak bisa diubah karena ada billing cetak film yang sudah lunas."
                    });
                }

                // ======================================
                // Update header CetakFilm
                // TotalCetakFilm tetap dari frontend
                // ======================================
                cetakFilm.KunjunganId = kunjunganId;
                cetakFilm.PasienId = pasienId;
                cetakFilm.DokterPerujukId = vm.DokterPerujukId ?? labBooking?.DokterPerujukId;
                cetakFilm.KelasId = kelasId;
                cetakFilm.LabBookingId = vm.LabBookingId ?? cetakFilm.LabBookingId;
                cetakFilm.HasilLabId = vm.HasilLabId;

                cetakFilm.NoOrder = !string.IsNullOrWhiteSpace(vm.NoOrder)
                    ? vm.NoOrder
                    : labBooking?.NoOrder ?? cetakFilm.NoOrder;

                cetakFilm.TglOrder = vm.TglOrder ?? cetakFilm.TglOrder;
                cetakFilm.WaktuOrder = vm.WaktuOrder ?? cetakFilm.WaktuOrder;
                cetakFilm.TglSelesai = vm.TglSelesai;
                cetakFilm.Keterangan = vm.Keterangan;

                // Header total tetap dari frontend
                cetakFilm.TotalCetakFilm = vm.TotalCetakFilm;

                cetakFilm.UpdateBy = userActiveId;
                cetakFilm.UpdateDateTime = DateTimeOffset.UtcNow;

                // ======================================
                // Soft delete semua detail lama
                // ======================================
                foreach (var oldDetail in existingDetails)
                {
                    oldDetail.IsDelete = true;
                    oldDetail.DeleteBy = userActiveId;
                    oldDetail.DeleteDateTime = DateTimeOffset.UtcNow;
                    oldDetail.UpdateBy = userActiveId;
                    oldDetail.UpdateDateTime = DateTimeOffset.UtcNow;
                }

                // ======================================
                // Soft delete semua billing lama terkait detail lama
                // ======================================
                foreach (var oldBilling in existingBillings)
                {
                    oldBilling.IsDelete = true;
                    oldBilling.DeleteBy = userActiveId;
                    oldBilling.DeleteDateTime = DateTimeOffset.UtcNow;
                    oldBilling.UpdateBy = userActiveId;
                    oldBilling.UpdateDateTime = DateTimeOffset.UtcNow;
                }

                // ======================================
                // Ambil nomor BGLL terbesar
                // ======================================
                var existingBgllCodes = await _applicationDbContext.Billings
                    .AsNoTracking()
                    .Where(b =>
                        b.KunjunganId == kunjunganId.Value &&
                        b.BillingKode != null &&
                        b.BillingKode.StartsWith("BGLL"))
                    .Select(b => b.BillingKode!)
                    .ToListAsync(ct);

                var billingIndex = 0;

                foreach (var code in existingBgllCodes)
                {
                    if (code.Length >= 7 &&
                        int.TryParse(code.Substring(4), out var number) &&
                        number > billingIndex)
                    {
                        billingIndex = number;
                    }
                }

                var invoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId.Value,
                    DateTime.UtcNow);

                var detailResponses = new List<object>();

                // ======================================
                // Insert ulang detail + billing per detail
                // ======================================
                foreach (var detailVm in vm.Details)
                {
                    if (!detailVm.FilmId.HasValue)
                        return BadRequest(new { message = "FilmId wajib diisi pada detail cetak film." });

                    if (!detailVm.LabBookingDetailId.HasValue)
                        return BadRequest(new { message = "LabBookingDetailId wajib diisi pada detail cetak film." });

                    var film = await _applicationDbContext.Films
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.FilmId == detailVm.FilmId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (film == null)
                        return BadRequest(new { message = $"FilmId {detailVm.FilmId} tidak ditemukan." });

                    var tarifFilm = await _applicationDbContext.TarifFilms
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.FilmId == detailVm.FilmId.Value &&
                            x.KelasId == kelasId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    var labBookingDetail = await _applicationDbContext.LabBookingDetails
                        .AsNoTracking()
                        .Include(x => x.Lab)
                        .Include(x => x.PemeriksaanLab)
                        .Include(x => x.DokterPemeriksa)
                        .FirstOrDefaultAsync(x =>
                            x.DetailBookingLabId == detailVm.LabBookingDetailId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (labBookingDetail == null)
                        return BadRequest(new { message = $"LabBookingDetailId {detailVm.LabBookingDetailId} tidak ditemukan." });

                    var hargaSatuanFilm =
                        tarifFilm?.TarifTotal ??
                        0m;

                    if (hargaSatuanFilm <= 0)
                    {
                        return BadRequest(new
                        {
                            message = $"Harga satuan film tidak ditemukan untuk FilmId {detailVm.FilmId} dan KelasId {kelasId}."
                        });
                    }

                    var qtyDecimal = detailVm.QtyCetakFilm ?? 1m;

                    if (qtyDecimal <= 0)
                        return BadRequest(new { message = "QtyCetakFilm harus lebih dari 0." });

                    if (qtyDecimal % 1 != 0)
                        return BadRequest(new { message = "QtyCetakFilm harus bilangan bulat." });

                    var qtyCetakFilm = Convert.ToInt32(qtyDecimal);

                    var totalDetail = hargaSatuanFilm * qtyCetakFilm;

                    var detailCetakFilmId = Guid.NewGuid();

                    var detail = new CetakFilmDetail
                    {
                        DetailCetakFilmId = detailCetakFilmId,
                        CetakFilmId = id,

                        DetailHasilLabId = detailVm.DetailHasilLabId,
                        LabBookingDetailId = detailVm.LabBookingDetailId,

                        FilmId = detailVm.FilmId,
                        QtyCetakFilm = qtyCetakFilm,
                        TotalCetakFilm = totalDetail,

                        Keterangan = detailVm.Keterangan,

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    };

                    _applicationDbContext.CetakFilmDetails.Add(detail);

                    // ======================================
                    // Insert billing baru per detail film
                    // ======================================
                    billingIndex++;

                    var billing = new Billing
                    {
                        BillingId = Guid.NewGuid(),

                        KunjunganId = kunjunganId.Value,

                        // Billing per detail film
                        ItemId = film.FilmId,

                        NamaItem = !string.IsNullOrWhiteSpace(film.NamaFilm)
                            ? $"{film.NamaFilm} - {labBookingDetail.PemeriksaanLab.NamaPemeriksaan}"
                            : "Cetak Film",

                        HargaItem = hargaSatuanFilm,
                        QtyItem = qtyCetakFilm,
                        SubTotalItem = totalDetail,

                        InvoiceBilling = invoiceBilling,

                        IsListWhiteOff = false,

                        BillingKode = $"BGLL{billingIndex:D3}",
                        JenisBilling = "Biaya Lain - Lain",

                        StatusBilling = false,
                        TipeLayanan = vm.TipeLayanan,

                        BillingDate = DateTime.UtcNow,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    };

                    _applicationDbContext.Billings.Add(billing);

                    detailResponses.Add(new
                    {
                        detail.DetailCetakFilmId,
                        detail.FilmId,
                        film.NamaFilm,
                        film.UkuranFilm,
                        detail.LabBookingDetailId,
                        detail.QtyCetakFilm,
                        detail.TotalCetakFilm,

                        Billing = new
                        {
                            billing.BillingId,
                            billing.InvoiceBilling,
                            billing.BillingKode,
                            billing.JenisBilling,
                            billing.NamaItem,
                            billing.HargaItem,
                            billing.QtyItem,
                            billing.SubTotalItem,
                            billing.StatusBilling
                        }
                    });
                }

                await _applicationDbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Ok(new
                {
                    message = "Data Cetak Film dan Billing berhasil diperbarui. || 200 OK",
                    data = new
                    {
                        cetakFilm.CetakFilmId,
                        cetakFilm.KunjunganId,
                        cetakFilm.PasienId,
                        cetakFilm.LabBookingId,
                        cetakFilm.NoOrder,
                        cetakFilm.TglOrder,
                        cetakFilm.WaktuOrder,
                        cetakFilm.TotalCetakFilm,
                        Details = detailResponses
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = $"Gagal memperbarui data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            try
            {
                // ======================================
                // Ambil user login
                // ======================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                await using var transaction = await _applicationDbContext.Database
                    .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

                try
                {
                    // ======================================
                    // Ambil parent CetakFilm
                    // ======================================
                    var cetakFilm = await _applicationDbContext.CetakFilms
                        .FirstOrDefaultAsync(x =>
                            x.CetakFilmId == id &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);

                    if (cetakFilm == null)
                        return NotFound(new { message = "Data Cetak Film tidak ditemukan. || 404 Not Found" });

                    // ======================================
                    // Ambil semua detail aktif
                    // ======================================
                    var details = await _applicationDbContext.CetakFilmDetails
                        .Where(x =>
                            x.CetakFilmId == id &&
                            (x.IsDelete == false || x.IsDelete == null))
                        .ToListAsync(ct);

                    var detailIds = details
                        .Select(x => x.DetailCetakFilmId)
                        .ToList();

                    // ======================================
                    // Ambil billing terkait detail cetak film
                    // ======================================
                    var billings = await _applicationDbContext.Billings
                        .Where(b =>
                            b.ItemId.HasValue &&
                            detailIds.Contains(b.ItemId.Value) &&
                            b.BillingKode != null &&
                            b.BillingKode.StartsWith("BGLL") &&
                            b.JenisBilling == "Biaya Lain - Lain" &&
                            (b.IsDelete == false || b.IsDelete == null))
                        .ToListAsync(ct);

                    // ======================================
                    // Optional: cegah hapus jika billing sudah lunas
                    // ======================================
                    if (billings.Any(x => x.StatusBilling == true))
                    {
                        await transaction.RollbackAsync(ct);

                        return BadRequest(new
                        {
                            message = "Data Cetak Film tidak bisa dihapus karena ada billing yang sudah lunas."
                        });
                    }

                    var now = DateTimeOffset.UtcNow;

                    // ======================================
                    // Soft delete parent CetakFilm
                    // ======================================
                    cetakFilm.IsDelete = true;
                    cetakFilm.DeleteBy = userActiveId;
                    cetakFilm.DeleteDateTime = now;
                    cetakFilm.UpdateBy = userActiveId;
                    cetakFilm.UpdateDateTime = now;

                    // ======================================
                    // Soft delete detail CetakFilm
                    // ======================================
                    foreach (var detail in details)
                    {
                        detail.IsDelete = true;
                        detail.DeleteBy = userActiveId;
                        detail.DeleteDateTime = now;
                        detail.UpdateBy = userActiveId;
                        detail.UpdateDateTime = now;
                    }

                    // ======================================
                    // Soft delete billing terkait
                    // ======================================
                    foreach (var billing in billings)
                    {
                        billing.IsDelete = true;
                        billing.DeleteBy = userActiveId;
                        billing.DeleteDateTime = now;
                        billing.UpdateBy = userActiveId;
                        billing.UpdateDateTime = now;
                    }

                    await _applicationDbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return Ok(new
                    {
                        message = "Data Cetak Film, Detail, dan Billing berhasil dihapus. || 200 OK",
                        data = new
                        {
                            CetakFilmId = cetakFilm.CetakFilmId,
                            TotalDetailDeleted = details.Count,
                            TotalBillingDeleted = billings.Count
                        }
                    });
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menghapus data: {dbEx.InnerException?.Message ?? dbEx.Message}"
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

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            page = page < 1 ? 1 : page;
            perPage = perPage < 1 ? 10 : perPage;
            perPage = perPage > 200 ? 200 : perPage;

            // ======================================================
            // Query Header CetakFilm
            // ======================================================
            var query = _applicationDbContext.CetakFilms
                .AsNoTracking()
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .Select(x => new
                {
                    x.CetakFilmId,
                    x.KunjunganId,
                    x.PasienId,
                    x.DokterPerujukId,
                    x.KelasId,
                    x.LabBookingId,
                    x.HasilLabId,

                    x.NoOrder,
                    x.TglOrder,
                    x.WaktuOrder,
                    x.TglSelesai,
                    x.TotalCetakFilm,
                    x.Keterangan,

                    // =========================
                    // Data tambahan dari relasi
                    // =========================

                    NamaPasien = x.Pasien != null
                        ? x.Pasien.NamaLengkap
                        : null,

                    NoRekamMedis = x.Pasien != null
                        ? x.Pasien.NoRekamMedis
                        : null,

                    JenisKelamin = x.Pasien != null
                        ? x.Pasien.JenisKelamin
                        : null,

                    NoRegistrasi = x.Kunjungan != null
                        ? x.Kunjungan.NoRegistrasi
                        : null,

                    JenisKunjungan = x.Kunjungan != null
                        ? x.Kunjungan.JenisKunjungan
                        : null,

                    NamaDokterPerujuk = x.DokterPerujuk != null
                        ? x.DokterPerujuk.NmDokter
                        : null,

                    NamaKelas = x.Kelas != null
                        ? x.Kelas.NamaKelas
                        : null,

                    NoOrderBooking = x.LabBooking != null
                        ? x.LabBooking.NoOrder
                        : null,

                    NoLab = x.LabBooking != null
                        ? x.LabBooking.NoLab
                        : null,

                    NoPA = x.LabBooking != null
                        ? x.LabBooking.NoPA
                        : null,

                    // =========================
                    // Metadata
                    // =========================

                    x.CreateDateTime,
                    x.CreateBy,

                    CreateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    x.UpdateDateTime,
                    x.UpdateBy,
                    x.DeleteDateTime,
                    x.DeleteBy,
                    x.IsDelete
                });

            // ======================================================
            // Search
            // ======================================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NoOrder ?? "", keyword) ||
                    EF.Functions.ILike(x.Keterangan ?? "", keyword) ||
                    EF.Functions.ILike(x.NamaPasien ?? "", keyword) ||
                    EF.Functions.ILike(x.NoRekamMedis ?? "", keyword) ||
                    EF.Functions.ILike(x.NoRegistrasi ?? "", keyword) ||
                    EF.Functions.ILike(x.NamaDokterPerujuk ?? "", keyword) ||
                    EF.Functions.ILike(x.NamaKelas ?? "", keyword) ||
                    EF.Functions.ILike(x.NoOrderBooking ?? "", keyword) ||
                    EF.Functions.ILike(x.NoLab ?? "", keyword) ||
                    EF.Functions.ILike(x.NoPA ?? "", keyword) ||
                    EF.Functions.ILike(x.CreateByName ?? "", keyword) ||

                    _applicationDbContext.CetakFilmDetails.Any(d =>
                        d.CetakFilmId == x.CetakFilmId &&
                        (d.IsDelete == false || d.IsDelete == null) &&
                        (
                            EF.Functions.ILike(d.Keterangan ?? "", keyword) ||

                            d.LabBookingDetail != null &&
                            (
                                EF.Functions.ILike(d.LabBookingDetail.NoPhoto ?? "", keyword) ||

                                d.LabBookingDetail.PemeriksaanLab != null &&
                                EF.Functions.ILike(d.LabBookingDetail.PemeriksaanLab.NamaPemeriksaan ?? "", keyword)
                            ) ||

                            d.DokterPemeriksa != null &&
                            EF.Functions.ILike(d.DokterPemeriksa.NmDokter ?? "", keyword) ||

                            d.Film != null &&
                            (
                                EF.Functions.ILike(d.Film.NamaFilm ?? "", keyword) ||
                                EF.Functions.ILike(d.Film.UkuranFilm ?? "", keyword)
                            )
                        )
                    )
                );
            }

            // ======================================================
            // Filter tanggal berdasarkan CreateDateTime
            // ======================================================
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = startDate.Value.Date == endDate.Value.Date
                    ? endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime()
                    : endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            // ======================================================
            // Filter periode
            // ======================================================
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            x.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        {
                            var lastMonth = today.AddMonths(-1);

                            query = query.Where(x =>
                                x.CreateDateTime.Month == lastMonth.Month &&
                                x.CreateDateTime.Year == lastMonth.Year);
                            break;
                        }

                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // ======================================================
            // Sorting
            // ======================================================
            var isDesc = sortDirection?.ToLower() == "desc";

            query = isDesc
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "NoOrder" => query.OrderByDescending(x => x.NoOrder),
                    "TglOrder" => query.OrderByDescending(x => x.TglOrder),
                    "WaktuOrder" => query.OrderByDescending(x => x.WaktuOrder),
                    "TglSelesai" => query.OrderByDescending(x => x.TglSelesai),
                    "TotalCetakFilm" => query.OrderByDescending(x => x.TotalCetakFilm),
                    "NamaPasien" => query.OrderByDescending(x => x.NamaPasien),
                    "NoRekamMedis" => query.OrderByDescending(x => x.NoRekamMedis),
                    "NoRegistrasi" => query.OrderByDescending(x => x.NoRegistrasi),
                    "NamaDokterPerujuk" => query.OrderByDescending(x => x.NamaDokterPerujuk),
                    "NamaKelas" => query.OrderByDescending(x => x.NamaKelas),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "NoOrder" => query.OrderBy(x => x.NoOrder),
                    "TglOrder" => query.OrderBy(x => x.TglOrder),
                    "WaktuOrder" => query.OrderBy(x => x.WaktuOrder),
                    "TglSelesai" => query.OrderBy(x => x.TglSelesai),
                    "TotalCetakFilm" => query.OrderBy(x => x.TotalCetakFilm),
                    "NamaPasien" => query.OrderBy(x => x.NamaPasien),
                    "NoRekamMedis" => query.OrderBy(x => x.NoRekamMedis),
                    "NoRegistrasi" => query.OrderBy(x => x.NoRegistrasi),
                    "NamaDokterPerujuk" => query.OrderBy(x => x.NamaDokterPerujuk),
                    "NamaKelas" => query.OrderBy(x => x.NamaKelas),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };

            // ======================================================
            // Pagination Header
            // ======================================================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "No data found",
                    data = new
                    {
                        Rows = Array.Empty<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });
            }

            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            var headerRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            var cetakFilmIds = headerRows
                .Select(x => x.CetakFilmId)
                .ToList();

            // ======================================================
            // Ambil Detail CetakFilm sesuai struktur GetById terbaru
            // ======================================================
            var detailRows = await _applicationDbContext.CetakFilmDetails
                .AsNoTracking()
                .Where(d =>
                    d.CetakFilmId.HasValue &&
                    cetakFilmIds.Contains(d.CetakFilmId.Value) &&
                    (d.IsDelete == false || d.IsDelete == null))
                .OrderBy(d => d.CreateDateTime)
                .Select(d => new
                {
                    CetakFilmId = d.CetakFilmId!.Value,

                    d.DetailCetakFilmId,
                    d.DetailHasilLabId,
                    d.LabBookingDetailId,

                    LabId = d.LabBookingDetail != null
                        ? d.LabBookingDetail.LabId
                        : null,

                    NamaLab = d.LabBookingDetail != null && d.LabBookingDetail.Lab != null
                        ? d.LabBookingDetail.Lab.NamaLab
                        : null,

                    PemeriksaanId = d.LabBookingDetail != null
                        ? d.LabBookingDetail.PemeriksaanLabId
                        : null,

                    NamaPemeriksaan = d.LabBookingDetail != null && d.LabBookingDetail.PemeriksaanLab != null
                        ? d.LabBookingDetail.PemeriksaanLab.NamaPemeriksaan
                        : null,

                    NoPhoto = d.LabBookingDetail != null
                        ? d.LabBookingDetail.NoPhoto
                        : null,

                    d.DokterPemeriksaId,

                    NamaDokterPemeriksa = d.DokterPemeriksa != null
                        ? d.DokterPemeriksa.NmDokter
                        : null,

                    PathHasilPhoto = d.LabHasilDetail != null
                        ? d.LabHasilDetail.PhotoLabPath
                        : null,

                    HasilLab = d.LabHasilDetail != null
                        ? d.LabHasilDetail.HasilLabManual
                        : null,

                    HasilLabAI = d.LabHasilDetail != null
                        ? d.LabHasilDetail.HasilLabAI
                        : null,

                    d.FilmId,

                    NamaFilm = d.Film != null
                        ? d.Film.NamaFilm
                        : null,

                    UkuranFilm = d.Film != null
                        ? d.Film.UkuranFilm
                        : null,

                    d.QtyCetakFilm,
                    d.TotalCetakFilm,
                    d.Keterangan,

                    d.CreateDateTime,
                    d.CreateBy,
                    d.UpdateDateTime,
                    d.UpdateBy,
                    d.DeleteDateTime,
                    d.DeleteBy,
                    d.IsDelete,

                    Billing = _applicationDbContext.Billings
                        .AsNoTracking()
                        .Where(b =>
                            b.ItemId == d.DetailCetakFilmId &&
                            b.BillingKode != null &&
                            b.BillingKode.StartsWith("BGLL") &&
                            b.JenisBilling == "Biaya Lain - Lain" &&
                            (b.IsDelete == false || b.IsDelete == null))
                        .Select(b => new
                        {
                            b.BillingId,
                            b.InvoiceBilling,
                            b.BillingKode,
                            b.JenisBilling,
                            b.NamaItem,
                            b.HargaItem,
                            b.QtyItem,
                            b.SubTotalItem,
                            b.StatusBilling,
                            b.TipeLayanan,
                            b.BillingDate,
                            b.TanggalInvoice,
                            b.TanggalJatuhTempo
                        })
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            // ======================================================
            // Gabungkan Header + Detail
            // ======================================================
            var rows = headerRows.Select(h => new
            {
                h.CetakFilmId,
                h.KunjunganId,
                h.PasienId,
                h.DokterPerujukId,
                h.KelasId,
                h.LabBookingId,
                h.HasilLabId,

                h.NoOrder,
                h.TglOrder,
                h.WaktuOrder,
                h.TglSelesai,
                h.TotalCetakFilm,
                h.Keterangan,

                h.NamaPasien,
                h.NoRekamMedis,
                h.JenisKelamin,
                h.NoRegistrasi,
                h.JenisKunjungan,
                h.NamaDokterPerujuk,
                h.NamaKelas,
                h.NoOrderBooking,
                h.NoLab,
                h.NoPA,

                h.CreateDateTime,
                h.CreateBy,
                h.CreateByName,
                h.UpdateDateTime,
                h.UpdateBy,
                h.DeleteDateTime,
                h.DeleteBy,
                h.IsDelete,

                Details = detailRows
                    .Where(d => d.CetakFilmId == h.CetakFilmId)
                    .ToList()
            }).ToList();

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
