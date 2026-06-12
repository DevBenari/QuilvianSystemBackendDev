using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class CetakFilmDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<CetakFilmDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CetakFilmDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CetakFilmDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            IGenerateInvoiceBillingService generateInvoiceBillingService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateInvoiceBillingService = generateInvoiceBillingService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = (from a in _applicationDbContext.CetakFilmDetails
                            join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false && a.DetailCetakFilmId == id
                            select new
                            {
                                CreateDateTime = a.CreateDateTime,
                                CreateBy = a.CreateBy,
                                CreateByName = u.FullName,
                                a.DetailCetakFilmId,
                                a.CetakFilmId,
                                a.DetailHasilLabId,
                                a.LabBookingDetailId,

                                LabId = a.LabBookingDetail != null ? a.LabBookingDetail.LabId : null,
                                NamaLab = a.LabBookingDetail.Lab != null ? a.LabBookingDetail.Lab.NamaLab : null,

                                PemeriksaanId = a.LabBookingDetail != null ? a.LabBookingDetail.PemeriksaanLabId : null,
                                NamaPemeriksaan = a.LabBookingDetail.PemeriksaanLab != null ? a.LabBookingDetail.PemeriksaanLab.NamaPemeriksaan : null,
                                NoPhoto = a.LabBookingDetail != null ? a.LabBookingDetail.NoPhoto : null,

                                a.DokterPemeriksaId,
                                NamaDokterPemeriksa = a.DokterPemeriksa != null ? a.DokterPemeriksa.NmDokter : null,

                                PathHasilPhoto = a.LabHasilDetail != null ? a.LabHasilDetail.PhotoLabPath : null,
                                HasilLab = a.LabHasilDetail != null ? a.LabHasilDetail.HasilLabManual : null,
                                HasilLabAI = a.LabHasilDetail != null ? a.LabHasilDetail.HasilLabAI : null,

                                a.FilmId,
                                NamaFilm = a.Film != null ? a.Film.NamaFilm : null,
                                UkuranFilm = a.Film != null ? a.Film.UkuranFilm : null,
                                a.QtyCetakFilm,
                                a.TotalCetakFilm,
                                a.Keterangan
                            });
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDetail(
        [FromBody] CetakFilmDetailViewModel vm,
        CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!vm.CetakFilmId.HasValue)
                return BadRequest(new { message = "CetakFilmId wajib diisi." });

            if (!vm.FilmId.HasValue)
                return BadRequest(new { message = "FilmId wajib diisi." });

            if (!vm.LabBookingDetailId.HasValue)
                return BadRequest(new { message = "LabBookingDetailId wajib diisi." });

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

                // ======================================
                // Ambil header CetakFilm
                // ======================================
                var cetakFilm = await _applicationDbContext.CetakFilms
                    .FirstOrDefaultAsync(x =>
                        x.CetakFilmId == vm.CetakFilmId.Value &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (cetakFilm == null)
                    return NotFound(new { message = "Data Cetak Film tidak ditemukan. || 404 Not Found" });

                // ======================================
                // Ambil LabBooking sebagai fallback
                // ======================================
                LabBooking? labBooking = null;

                if (cetakFilm.LabBookingId.HasValue)
                {
                    labBooking = await _applicationDbContext.LabBookings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.BookingLabId == cetakFilm.LabBookingId.Value &&
                            (x.IsDelete == false || x.IsDelete == null),
                            ct);
                }

                var kunjunganId = cetakFilm.KunjunganId ?? labBooking?.KunjunganId;
                var kelasId = cetakFilm.KelasId ?? labBooking?.KelasId;

                if (!kunjunganId.HasValue)
                    return BadRequest(new { message = "KunjunganId pada Cetak Film kosong." });

                if (!kelasId.HasValue)
                    return BadRequest(new { message = "KelasId wajib ada untuk mengambil tarif film." });

                // ======================================
                // Validasi Film
                // ======================================
                var film = await _applicationDbContext.Films
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.FilmId == vm.FilmId.Value &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (film == null)
                    return BadRequest(new { message = $"FilmId {vm.FilmId} tidak ditemukan." });

                // ======================================
                // Ambil tarif film berdasarkan FilmId + KelasId
                // ======================================
                var tarifFilm = await _applicationDbContext.TarifFilms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.FilmId == vm.FilmId.Value &&
                        x.KelasId == kelasId.Value &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                var hargaSatuanFilm = tarifFilm?.TarifTotal ?? 0m;

                if (hargaSatuanFilm <= 0)
                {
                    return BadRequest(new
                    {
                        message = $"Harga satuan film tidak ditemukan untuk FilmId {vm.FilmId} dan KelasId {kelasId}."
                    });
                }

                // ======================================
                // Ambil LabBookingDetail
                // ======================================
                var labBookingDetail = await _applicationDbContext.LabBookingDetails
                    .AsNoTracking()
                    .Include(x => x.DokterPemeriksa)
                    .Include(x => x.PemeriksaanLab)
                    .Include(x => x.Lab)
                    .FirstOrDefaultAsync(x =>
                        x.DetailBookingLabId == vm.LabBookingDetailId.Value &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (labBookingDetail == null)
                {
                    return BadRequest(new
                    {
                        message = $"LabBookingDetailId {vm.LabBookingDetailId} tidak ditemukan."
                    });
                }

                // ======================================
                // Validasi Qty
                // ======================================
                var qtyDecimal = vm.QtyCetakFilm ?? 1m;

                if (qtyDecimal <= 0)
                    return BadRequest(new { message = "QtyCetakFilm harus lebih dari 0." });

                if (qtyDecimal % 1 != 0)
                    return BadRequest(new { message = "QtyCetakFilm harus bilangan bulat." });

                var qtyCetakFilm = Convert.ToInt32(qtyDecimal);

                var totalDetail = hargaSatuanFilm * qtyCetakFilm;

                // ======================================
                // Insert CetakFilmDetail
                // ======================================
                var detailCetakFilmId = Guid.NewGuid();

                var detail = new CetakFilmDetail
                {
                    DetailCetakFilmId = detailCetakFilmId,
                    CetakFilmId = vm.CetakFilmId.Value,

                    DetailHasilLabId = vm.DetailHasilLabId,
                    LabBookingDetailId = vm.LabBookingDetailId,

                    DokterPemeriksaId = vm.DokterPemeriksaId ?? labBookingDetail.DokterPemeriksaId,

                    FilmId = vm.FilmId,
                    QtyCetakFilm = qtyCetakFilm,
                    TotalCetakFilm = totalDetail,

                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.CetakFilmDetails.Add(detail);

                // ======================================
                // Ambil invoice billing
                // ======================================
                var invoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                    kunjunganId.Value,
                    DateTime.UtcNow);

                // ======================================
                // Generate kode BGLL berikutnya
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

                billingIndex++;

                var kode = $"BGLL{billingIndex:D3}";

                // ======================================
                // Insert Billing per detail cetak film
                // ======================================
                var billing = new Billing
                {
                    BillingId = Guid.NewGuid(),

                    KunjunganId = kunjunganId.Value,

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

                    // Ambil dari LabBookingDetail karena DetailViewModel tidak punya TipeLayanan
                    TipeLayanan = labBookingDetail.TipeLayanan,

                    BillingDate = DateTime.UtcNow,
                    TanggalInvoice = DateTime.UtcNow,
                    TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.Billings.Add(billing);

                await _applicationDbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Created("", new
                {
                    message = "Data Detail Cetak Film dan Billing berhasil dibuat. || 201 Created",
                    data = new
                    {
                        detail.DetailCetakFilmId,
                        detail.CetakFilmId,
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
                            billing.StatusBilling,
                            billing.TipeLayanan
                        }
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
    }
}
