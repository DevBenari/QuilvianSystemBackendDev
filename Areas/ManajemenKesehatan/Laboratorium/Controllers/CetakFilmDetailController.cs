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
    public class CetakFilmDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly ILogger<CetakFilmDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IKunjunganTransactionGuard _kunjunganTransactionGuard;


        public CetakFilmDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CetakFilmDetailController> logger,
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

                await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)cetakFilm.KunjunganId, ct);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDetail(
            Guid id,
            [FromBody] CetakFilmDetailViewModel vm,
            CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter DetailCetakFilmId tidak valid." });

            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

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
                // Ambil detail existing
                // ======================================
                var detail = await _applicationDbContext.CetakFilmDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetailCetakFilmId == id &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (detail == null)
                    return NotFound(new { message = "Detail Cetak Film tidak ditemukan. || 404 Not Found" });

                if (vm.CetakFilmId.HasValue && vm.CetakFilmId.Value != detail.CetakFilmId)
                {
                    return BadRequest(new
                    {
                        message = "CetakFilmId tidak boleh berbeda dengan data detail yang sedang diubah."
                    });
                }

                // Simpan nilai lama untuk mencari billing lama
                var oldFilmId = detail.FilmId;
                var oldQty = detail.QtyCetakFilm;
                var oldTotal = detail.TotalCetakFilm;

                // ======================================
                // Ambil header CetakFilm
                // ======================================
                if (!detail.CetakFilmId.HasValue)
                    return BadRequest(new { message = "CetakFilmId pada detail kosong." });

                var cetakFilm = await _applicationDbContext.CetakFilms
                    .FirstOrDefaultAsync(x =>
                        x.CetakFilmId == detail.CetakFilmId.Value &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (cetakFilm == null)
                    return NotFound(new { message = "Data Cetak Film tidak ditemukan. || 404 Not Found" });


                await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)cetakFilm.KunjunganId, ct);
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
                // Validasi Film baru
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
                // Cari billing lama
                // Karena Billing.ItemId = FilmId, pencarian pakai nilai lama
                // ======================================
                IQueryable<Billing> billingQuery = _applicationDbContext.Billings
                    .Where(b =>
                        b.KunjunganId == kunjunganId.Value &&
                        b.ItemId == oldFilmId &&
                        b.BillingKode != null &&
                        b.BillingKode.StartsWith("BGLL") &&
                        b.JenisBilling == "Biaya Lain - Lain" &&
                        (b.IsDelete == false || b.IsDelete == null));

                if (oldQty.HasValue)
                {
                    var oldQtyInt = Convert.ToInt32(oldQty.Value);
                    billingQuery = billingQuery.Where(b => b.QtyItem == oldQtyInt);
                }

                if (oldTotal.HasValue)
                {
                    billingQuery = billingQuery.Where(b => b.SubTotalItem == oldTotal.Value);
                }

                var matchedBillings = await billingQuery
                    .OrderByDescending(b => b.CreateDateTime)
                    .ToListAsync(ct);

                if (matchedBillings.Count > 1)
                {
                    await transaction.RollbackAsync(ct);

                    return BadRequest(new
                    {
                        message = "Billing lama tidak bisa ditentukan secara unik karena ada lebih dari satu billing dengan FilmId, Qty, dan SubTotal yang sama. Perlu update manual atau tambahkan field referensi detail pada billing."
                    });
                }

                var billing = matchedBillings.FirstOrDefault();

                if (billing != null && billing.StatusBilling == true)
                {
                    await transaction.RollbackAsync(ct);

                    return BadRequest(new
                    {
                        message = "Detail Cetak Film tidak bisa diubah karena billing sudah lunas."
                    });
                }

                // ======================================
                // Update CetakFilmDetail
                // ======================================
                detail.DetailHasilLabId = vm.DetailHasilLabId;
                detail.LabBookingDetailId = vm.LabBookingDetailId;
                detail.DokterPemeriksaId = vm.DokterPemeriksaId ?? labBookingDetail.DokterPemeriksaId;
                detail.FilmId = vm.FilmId;
                detail.QtyCetakFilm = qtyCetakFilm;
                detail.TotalCetakFilm = totalDetail;
                detail.Keterangan = vm.Keterangan;

                detail.UpdateBy = userActiveId;
                detail.UpdateDateTime = DateTimeOffset.UtcNow;

                // ======================================
                // Jika billing lama tidak ketemu, buat billing baru
                // ======================================
                if (billing == null)
                {
                    var invoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                        kunjunganId.Value,
                        DateTime.UtcNow);

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

                    billing = new Billing
                    {
                        BillingId = Guid.NewGuid(),
                        KunjunganId = kunjunganId.Value,
                        InvoiceBilling = invoiceBilling,

                        BillingKode = $"BGLL{billingIndex:D3}",
                        JenisBilling = "Biaya Lain - Lain",

                        IsListWhiteOff = false,
                        StatusBilling = false,

                        BillingDate = DateTime.UtcNow,
                        TanggalInvoice = DateTime.UtcNow,
                        TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),

                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        IsDelete = false
                    };

                    _applicationDbContext.Billings.Add(billing);
                }
                else
                {
                    billing.UpdateBy = userActiveId;
                    billing.UpdateDateTime = DateTimeOffset.UtcNow;
                }

                // ======================================
                // Update Billing
                // ItemId tetap FilmId karena itu jenis item billing
                // ======================================
                billing.ItemId = film.FilmId;

                billing.NamaItem = !string.IsNullOrWhiteSpace(film.NamaFilm)
                    ? $"{film.NamaFilm} - {labBookingDetail.PemeriksaanLab.NamaPemeriksaan}"
                    : "Cetak Film";

                billing.HargaItem = hargaSatuanFilm;
                billing.QtyItem = qtyCetakFilm;
                billing.SubTotalItem = totalDetail;
                billing.TipeLayanan = labBookingDetail.TipeLayanan;

                await _applicationDbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Ok(new
                {
                    message = "Data Detail Cetak Film dan Billing berhasil diperbarui. || 200 OK",
                    data = new
                    {
                        detail.DetailCetakFilmId,
                        detail.CetakFilmId,
                        detail.DetailHasilLabId,
                        detail.LabBookingDetailId,
                        detail.DokterPemeriksaId,
                        detail.FilmId,
                        film.NamaFilm,
                        film.UkuranFilm,
                        HargaSatuanFilm = hargaSatuanFilm,
                        detail.QtyCetakFilm,
                        detail.TotalCetakFilm,
                        detail.Keterangan,

                        Billing = new
                        {
                            billing.BillingId,
                            billing.InvoiceBilling,
                            billing.BillingKode,
                            billing.JenisBilling,
                            billing.ItemId,
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
        public async Task<IActionResult> DeleteDetail(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter DetailCetakFilmId tidak valid." });

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
                // Ambil detail cetak film
                // ======================================
                var detail = await _applicationDbContext.CetakFilmDetails
                    .FirstOrDefaultAsync(x =>
                        x.DetailCetakFilmId == id &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (detail == null)
                    return NotFound(new { message = "Detail Cetak Film tidak ditemukan. || 404 Not Found" });

                if (!detail.CetakFilmId.HasValue)
                    return BadRequest(new { message = "CetakFilmId pada detail kosong." });

                // ======================================
                // Ambil header CetakFilm
                // ======================================
                var cetakFilm = await _applicationDbContext.CetakFilms
                    .FirstOrDefaultAsync(x =>
                        x.CetakFilmId == detail.CetakFilmId.Value &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (cetakFilm == null)
                    return NotFound(new { message = "Data Cetak Film tidak ditemukan. || 404 Not Found" });

                // ======================================
                // Ambil LabBooking fallback
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

                if (!kunjunganId.HasValue)
                    return BadRequest(new { message = "KunjunganId pada Cetak Film kosong." });

                // ======================================
                // Cari billing terkait detail
                // Karena Billing.ItemId = FilmId, bukan DetailCetakFilmId
                // ======================================
                IQueryable<Billing> billingQuery = _applicationDbContext.Billings
                    .Where(b =>
                        b.KunjunganId == kunjunganId.Value &&
                        b.ItemId == detail.FilmId &&
                        b.BillingKode != null &&
                        b.BillingKode.StartsWith("BGLL") &&
                        b.JenisBilling == "Biaya Lain - Lain" &&
                        (b.IsDelete == false || b.IsDelete == null));

                if (detail.QtyCetakFilm.HasValue)
                {
                    var qtyItem = Convert.ToInt32(detail.QtyCetakFilm.Value);
                    billingQuery = billingQuery.Where(b => b.QtyItem == qtyItem);
                }

                if (detail.TotalCetakFilm.HasValue)
                {
                    billingQuery = billingQuery.Where(b => b.SubTotalItem == detail.TotalCetakFilm.Value);
                }

                var matchedBillings = await billingQuery
                    .OrderByDescending(b => b.CreateDateTime)
                    .ToListAsync(ct);

                if (matchedBillings.Count > 1)
                {
                    await transaction.RollbackAsync(ct);

                    return BadRequest(new
                    {
                        message = "Billing terkait detail ini tidak bisa ditentukan secara unik karena ada lebih dari satu billing dengan FilmId, Qty, dan SubTotal yang sama."
                    });
                }

                var billing = matchedBillings.FirstOrDefault();

                // ======================================
                // Cegah hapus kalau billing sudah lunas
                // ======================================
                if (billing != null && billing.StatusBilling == true)
                {
                    await transaction.RollbackAsync(ct);

                    return BadRequest(new
                    {
                        message = "Detail Cetak Film tidak bisa dihapus karena billing sudah lunas."
                    });
                }

                var now = DateTimeOffset.UtcNow;

                // ======================================
                // Soft delete detail
                // ======================================
                detail.IsDelete = true;
                detail.DeleteBy = userActiveId;
                detail.DeleteDateTime = now;
                detail.UpdateBy = userActiveId;
                detail.UpdateDateTime = now;

                // ======================================
                // Soft delete billing terkait
                // ======================================
                if (billing != null)
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
                    message = "Detail Cetak Film dan Billing berhasil dihapus. || 200 OK",
                    data = new
                    {
                        detail.DetailCetakFilmId,
                        detail.CetakFilmId,
                        detail.FilmId,
                        detail.QtyCetakFilm,
                        detail.TotalCetakFilm,
                        BillingDeleted = billing != null,
                        Billing = billing == null ? null : new
                        {
                            billing.BillingId,
                            billing.BillingKode,
                            billing.JenisBilling,
                            billing.ItemId,
                            billing.NamaItem,
                            billing.HargaItem,
                            billing.QtyItem,
                            billing.SubTotalItem,
                            billing.StatusBilling
                        }
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(ct);

                return StatusCode(500, new
                {
                    message = $"Gagal menghapus data: {dbEx.InnerException?.Message ?? dbEx.Message}"
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

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            Guid? labBookingDetailId = null,
            Guid? labId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.CetakFilmDetails
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false 
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

            if (labId.HasValue)
            {
                query = query.Where(u=>u.LabId == labId.Value);
            }

            if (labBookingDetailId.HasValue)
            {
                query = query.Where(u=>u.LabBookingDetailId == labBookingDetailId.Value);
            }

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLab, search) ||
                    EF.Functions.ILike(u.NamaPemeriksaan, search) ||
                    EF.Functions.ILike(u.NamaDokterPemeriksa, search) ||
                    EF.Functions.ILike(u.NoPhoto, search)

                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

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
