using System.Globalization;
using System.Linq;
using System.Security.Claims;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
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

        public LabBookingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingController> logger,
            IWebHostEnvironment webHostEnvironment,
            //IConfiguration configuration,
            ITTDService ttDService,
            IHubContext<LabBookingHub> hubContext
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
        }
        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                    tanggal,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                var now = DateTime.Now; // atau DateTime.UtcNow jika kamu mau jam UTC
                var finalDateTime = new DateTime(
                    parsedDate.Year,
                    parsedDate.Month,
                    parsedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Local
                ); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }
            return null;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from b in _applicationDbContext.LabBookings
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on b.CreateBy equals u.UserActiveId

                         // join ke lab booking detail
                         join lb in _applicationDbContext.LabBookingDetails
                         on b.BookingLabId equals lb.BookingLabId into labBookings
                         from lb in labBookings.DefaultIfEmpty()

                             // join ke lab
                         join l in _applicationDbContext.Labs
                         on lb.LabId equals l.LabId into labGroup
                         from l in labGroup.DefaultIfEmpty()

                            // join ke lab pemeriksaan
                        join lp in _applicationDbContext.LabPemeriksaans
                        on lb.PemeriksaanLabId equals lp.PemeriksaanLabId into lpGroup
                        from lp in lpGroup.DefaultIfEmpty()

                             // join ke kunjungan
                         join k in _applicationDbContext.Kunjungans
                         on b.KunjunganId equals k.KunjunganID into kGroup
                         from k in kGroup.DefaultIfEmpty()

                             // join ke asuransi
                         join a in _applicationDbContext.Asuransis
                         on b.AsuransiId equals a.AsuransiId into aGroup
                         from a in aGroup.DefaultIfEmpty()

                             // join ke pasien baru
                         join p in _applicationDbContext.PendaftaranPasienBarus
                         on b.PasienId equals p.PendaftaranPasienBaruId into pGroup
                         from p in pGroup.DefaultIfEmpty()

                             //join ke dokter
                         join d1 in _applicationDbContext.Dokters
                         on b.DokterId equals d1.DokterId into d1Group
                         from d1 in d1Group.DefaultIfEmpty()

                             // join ke dokter konsulen
                         join d2 in _applicationDbContext.Dokters
                         on b.DokterKonsulenId equals d2.DokterId into d2Group
                         from d2 in d2Group.DefaultIfEmpty()

                         where b.IsDelete == false || b.IsDelete == null
                         select new
                         {
                             b.CreateDateTime,
                             b.CreateBy,
                             CreateByName = u.FullName,
                             b.BookingLabId,
                             b.NomorSuratJaminan,
                             b.KunjunganId,
                             k.AsalKunjungan,
                             k.TipePasien,
                             b.PasienId,
                             p.NamaLengkap,
                             p.NoRekamMedis,
                             b.TglPemeriksaan,
                             b.TglPenyerahanSampling,
                             b.TglBooking,
                             b.StatusPemeriksaan,
                             b.KelasId,
                             lb.PemeriksaanLabId,
                             lp.NamaPemeriksaan,
                             lp.HargaPemeriksaan,
                             b.DokterId,
                             NamaDokter = d1.NmDokter,
                             b.Keterangan,
                             b.IsCito,
                             b.DiagnosaAwal,
                             b.DokterKonsulenId,
                             DokterKonsulen = d2.NmDokter,
                             b.TerapisId,
                             b.AsuransiId,
                             a.NamaAsuransi,
                             b.HemodialisaKe,
                             NamaLab = l.NamaLab ?? null,
                             AlasanPembatalan = lb.AlasanPembatalan ?? null,
                             TTDPembatalanPath =lb.TTDPembatalanPath ?? null,
                             b.NoLab,
                             b.NoPA,
                             b.StatusBookingLab,
                             b.CatatanJaminan,
                             b.StatusPembayaran,
                             b.ProsesBooking,
                             b.TindakLanjut,
                             b.HasilPenunjangLab,
                             b.AnjuranDiet
                         }).OrderByDescending(a => a.CreateDateTime);

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var baseQuery = from b in _applicationDbContext.LabBookings
                                join u in _applicationDbContext.UserActives on b.CreateBy equals u.UserActiveId into uGroup
                                from u in uGroup.DefaultIfEmpty()

                                join k in _applicationDbContext.Kunjungans on b.KunjunganId equals k.KunjunganID into kGroup
                                from k in kGroup.DefaultIfEmpty()

                                join a in _applicationDbContext.Asuransis on b.AsuransiId equals a.AsuransiId into aGroup
                                from a in aGroup.DefaultIfEmpty()

                                join p in _applicationDbContext.PendaftaranPasienBarus on b.PasienId equals p.PendaftaranPasienBaruId into pGroup
                                from p in pGroup.DefaultIfEmpty()

                                join d1 in _applicationDbContext.Dokters on b.DokterId equals d1.DokterId into d1Group
                                from d1 in d1Group.DefaultIfEmpty()

                                join d2 in _applicationDbContext.Dokters on b.DokterKonsulenId equals d2.DokterId into d2Group
                                from d2 in d2Group.DefaultIfEmpty()

                                join lb in _applicationDbContext.LabBookingDetails on b.BookingLabId equals lb.BookingLabId into lbGroup
                                from lb in lbGroup.DefaultIfEmpty()

                                join l in _applicationDbContext.Labs on lb.LabId equals l.LabId into lGroup
                                from l in lGroup.DefaultIfEmpty()

                                join lp in _applicationDbContext.LabPemeriksaans on lb.PemeriksaanLabId equals lp.PemeriksaanLabId into lpGroup
                                from lp in lpGroup.DefaultIfEmpty()

                                join po in _applicationDbContext.Polikliniks on k.PoliklinikId equals po.PoliklinikId into poGroup
                                from po in poGroup.DefaultIfEmpty()

                                where b.BookingLabId == id && (b.IsDelete == false )
                                     
                                select new
                                {
                                    // Header
                                    b.BookingLabId,
                                    KunjunganId = (Guid?)b.KunjunganId,
                                    PoliId = (Guid?)po.PoliklinikId,
                                    NamaPoli = po.NamaPoliklinik ?? null,
                                    PasienId = (Guid?)b.PasienId,
                                    PasienNama = p.NamaLengkap,
                                    b.NomorSuratJaminan,
                                    b.StatusBookingLab,
                                    b.CatatanJaminan,
                                    b.StatusPembayaran,
                                    p.NoRekamMedis,
                                    b.TglPemeriksaan,
                                    b.TglBooking,
                                    b.TglPenyerahanSampling,
                                    b.KelasId,
                                    b.Keterangan,
                                    b.IsCito,
                                    b.DiagnosaAwal,
                                    b.HemodialisaKe,
                                    b.StatusPemeriksaan,
                                    b.TindakLanjut,
                                    b.HasilPenunjangLab,
                                    b.AnjuranDiet,
                                    b.TTDPathPembatalan,
                                    b.PetugasPembatalan,
                                    AlasanPembatalanLabBooking=b.AlasanPembatalan,
                                    AsuransiId = (Guid?)b.AsuransiId,
                                    AsuransiNama = a.NamaAsuransi ?? null,
                                    DokterId = (Guid?)b.DokterId,
                                    DokterNama = d1.NmDokter ?? null,
                                    DokterKonsulenId = b.DokterKonsulenId ?? null,
                                    DokterKonsulen = d2.NmDokter ?? null,
                                    AsalKunjungan = k != null ? k.AsalKunjungan : null,
                                    TipePasien = k != null ? k.TipePasien : null,
                                    b.CreateBy,
                                    CreateByName = u.FullName,
                                    b.CreateDateTime,
                                    
                                    // Detail
                                    LabBookingDetailId = (Guid?)lb.DetailBookingLabId,
                                    PemeriksaanLabId = (Guid?)lb.PemeriksaanLabId,
                                    PemeriksaanNama = lp.NamaPemeriksaan,
                                    TipeLayanan = lb.TipeLayanan ?? null,
                                    HargaPemeriksaan = (decimal?)(lp.HargaPemeriksaan ?? 0),
                                    NamaLab = l.NamaLab ?? null,
                                    AlasanPembatalan = lb.AlasanPembatalan ?? null,
                                    TTDPembatalanPath = lb.TTDPembatalanPath ?? null,
                                    IsDeleteLBD = lb.IsDelete
                                };

                var rawData = baseQuery.ToList();

                if (!rawData.Any())
                    return NotFound(new { message = "Data tidak ditemukan untuk LabBookingId tersebut." });

                // ✅ Grouping by BookingLabId
                var grouped = rawData
                    .GroupBy(x => x.BookingLabId)
                    .Select(g => new
                    {
                        BookingLabId = g.Key,
                        g.First().KunjunganId,
                        g.First().PoliId,
                        g.First().NamaPoli,
                        g.First().PasienId,
                        g.First().PasienNama,
                        g.First().NoRekamMedis,
                        g.First().NomorSuratJaminan,
                        g.First().TglPemeriksaan,
                        g.First().TglBooking,
                        g.First().TglPenyerahanSampling,
                        g.First().StatusBookingLab,
                        g.First().CatatanJaminan,
                        g.First().StatusPembayaran,
                        g.First().StatusPemeriksaan,
                        g.First().AsuransiId,
                        g.First().AsuransiNama,
                        g.First().DokterId,
                        g.First().DokterNama,
                        g.First().DokterKonsulenId,
                        g.First().DokterKonsulen,
                        g.First().AsalKunjungan,
                        g.First().TipePasien,
                        g.First().IsCito,
                        g.First().DiagnosaAwal,
                        g.First().HemodialisaKe,
                        g.First().TindakLanjut,
                        g.First().HasilPenunjangLab,
                        g.First().AnjuranDiet,
                        g.First().Keterangan,
                        g.First().TTDPathPembatalan,
                        g.First().PetugasPembatalan,
                        g.First().AlasanPembatalanLabBooking,
                        g.First().CreateBy,
                        g.First().CreateByName,
                        g.First().CreateDateTime,

                        Details = g.Where(d => d.LabBookingDetailId != null && !d.IsDeleteLBD).Select(d => new
                        {
                            d.LabBookingDetailId,
                            d.PemeriksaanLabId,
                            d.PemeriksaanNama,
                            d.HargaPemeriksaan,
                            d.NamaLab,
                            d.TipeLayanan,
                            d.AlasanPembatalan,
                            d.TTDPembatalanPath,
                        }).ToList()
                    })
                    .FirstOrDefault();

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = grouped
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabBookingViewModel vm)
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
                    TglPenyerahanSampling = vm.TglPenyerahanSampling,
                    TglBooking = vm.TglBooking,
                    TglPemeriksaan = vm.TglPemeriksaan,
                    KelasId = vm.KelasId,
                    DokterId = vm.DokterId,
                    Keterangan = vm.Keterangan,
                    IsCito = vm.IsCito,
                    DiagnosaAwal = vm.DiagnosaAwal,
                    StatusPemeriksaan = vm.StatusPemeriksaan,
                    DokterKonsulenId = vm.DokterKonsulenId,
                    TerapisId = vm.TerapisId,
                    HemodialisaKe = vm.HemodialisaKe,
                    NomorSuratJaminan = vm.NomorSuratJaminan,
                    CatatanJaminan = vm.CatatanJaminan,
                    NoLab = vm.NoLab,
                    NoPA = vm.NoPA,
                    NoOrder = vm.NoOrder,
                    StatusBookingLab = false,
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
                            entity.IsCito,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] LabBookingViewModel vm)
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
                entity.KunjunganId = vm.KunjunganId;
                entity.PasienId = vm.PasienId;
                entity.AsuransiId = vm.AsuransiId;
                entity.TglPenyerahanSampling = vm.TglPenyerahanSampling;
                entity.TglBooking = vm.TglBooking;
                entity.TglPemeriksaan = vm.TglPemeriksaan;
                entity.KelasId = vm.KelasId;
                entity.DokterId = vm.DokterId;
                entity.Keterangan = vm.Keterangan;
                entity.IsCito = vm.IsCito;
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


                // ======================================
                // 🕒 Update metadata
                // ======================================
                entity.UpdateBy = userActiveId;
                entity.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.LabBookings.Update(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Lab booking changed", new
                    {
                        Action = "changed",
                        TriageId = entity.BookingLabId
                    });

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
                            entity.IsCito,
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

        [HttpPut("StatusPemeriksaanLab/{id}")]
        public async Task<IActionResult> StatusPemeriksaanLab(Guid id, [FromBody] string status)
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
                entity.StatusPemeriksaan = status;

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
                            entity.IsCito,
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
                entity.StatusPembayaran = status;

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
                            entity.IsCito,
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
                            entity.IsCito,
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
                            entity.IsCito,
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

        //[HttpGet("paged")]
        //public IActionResult Paged(
        //    int page = 1,
        //    int perPage = 10,
        //    Guid? kunjunganId = null,
        //    Guid? labBookingId = null,
        //    Guid? labId = null,
        //    string? namaLab = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null)
        //{
        //    // BASE QUERY parent
        //    var parentQuery = _applicationDbContext.LabBookings
        //        .Where(b => b.IsDelete == false || b.IsDelete == null)
        //        .AsQueryable();

        //    // Filter sederhana
        //    if (kunjunganId.HasValue)
        //        parentQuery = parentQuery.Where(b => b.KunjunganId == kunjunganId.Value);

        //    if (labBookingId.HasValue)
        //        parentQuery = parentQuery.Where(b => b.BookingLabId == labBookingId.Value);

        //    if (startDate.HasValue && endDate.HasValue)
        //    {
        //        var start = startDate.Value.Date;
        //        var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
        //        parentQuery = parentQuery.Where(b => b.CreateDateTime >= start && b.CreateDateTime <= end);
        //    }

        //    // Jika ada filter namaLab → lakukan filter di SQL (JOIN)
        //    if (!string.IsNullOrWhiteSpace(namaLab))
        //    {
        //        var nl = namaLab.ToLower().Trim();

        //        parentQuery =
        //            from b in parentQuery
        //            join d in _applicationDbContext.LabBookingDetails on b.BookingLabId equals d.BookingLabId
        //            join lab in _applicationDbContext.Labs on d.LabId equals lab.LabId
        //            where lab.NamaLab.ToLower().Trim().Contains(nl)
        //            select b;

        //        // Distinct penting karena join bisa mengulang baris parent
        //        parentQuery = parentQuery.Distinct();
        //    }

        //    // filter based on lab id
        //    if (labId.HasValue)
        //    {
        //        parentQuery =
        //            from b in parentQuery
        //            join d in _applicationDbContext.LabBookingDetails on b.BookingLabId equals d.BookingLabId
        //            where d.LabId == labId.Value
        //            select b;

        //        parentQuery = parentQuery.Distinct();
        //    }

        //    // Hitung total rows SETELAH semua filter
        //    int totalRows = parentQuery.Count();

        //    // Sorting by parent create date
        //    parentQuery = sortDirection?.ToLower() == "desc"
        //        ? parentQuery.OrderByDescending(b => b.CreateDateTime)
        //        : parentQuery.OrderBy(b => b.CreateDateTime);

        //    // Ambil parentIds hasil paging
        //    var pagedParentIds = parentQuery
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .Select(b => b.BookingLabId)
        //        .ToList();

        //    if (!pagedParentIds.Any())
        //        return Ok(new
        //        {
        //            status = "success",
        //            data = new { Rows = new List<object>(), TotalRows = 0 }
        //        });

        //    // LOAD PARENT DENGAN JOIN (SQL masih optimal)
        //    var parents =
        //        (from b in _applicationDbContext.LabBookings
        //         join u in _applicationDbContext.UserActives on b.CreateBy equals u.UserActiveId into uGroup
        //         from u in uGroup.DefaultIfEmpty()

        //         join k in _applicationDbContext.Kunjungans on b.KunjunganId equals k.KunjunganID into kGroup
        //         from k in kGroup.DefaultIfEmpty()

        //         join a in _applicationDbContext.Asuransis on b.AsuransiId equals a.AsuransiId into aGroup
        //         from a in aGroup.DefaultIfEmpty()

        //         join p in _applicationDbContext.PendaftaranPasienBarus on b.PasienId equals p.PendaftaranPasienBaruId into pGroup
        //         from p in pGroup.DefaultIfEmpty()

        //         join d1 in _applicationDbContext.Dokters on b.DokterId equals d1.DokterId into d1Group
        //         from d1 in d1Group.DefaultIfEmpty()

        //         join po in _applicationDbContext.Polikliniks on k.PoliklinikId equals po.PoliklinikId into poGroup
        //         from po in poGroup.DefaultIfEmpty()

        //         join kl in _applicationDbContext.Kelass on b.KelasId equals kl.KelasId into klGroup
        //         from kl in klGroup.DefaultIfEmpty()

        //         where pagedParentIds.Contains(b.BookingLabId) 
        //         select new
        //         {
        //             b.BookingLabId,
        //             b.KunjunganId,
        //             b.PasienId,
        //             p.NamaLengkap,
        //             p.NoRekamMedis,
        //             b.AsuransiId,
        //             AsuransiNama = a.NamaAsuransi ?? null,
        //             b.DokterId,
        //             DokterNama = d1.NmDokter ?? null,
        //             PoliNama = po.NamaPoliklinik ?? null,
        //             b.TglPemeriksaan,
        //             b.TglBooking,
        //             b.AlasanPembatalan,
        //             b.StatusBookingLab,
        //             b.StatusPembayaran,
        //             b.KelasId,
        //             NamaKelas = kl.NamaKelas ?? null,
        //             b.HemodialisaKe,
        //             b.StatusPemeriksaan,
        //             b.NomorSuratJaminan,
        //             b.DokterKonsulenId,
        //             b.DiagnosaAwal,
        //             b.Keterangan,
        //             b.TTDPathPembatalan,
        //             b.CreateDateTime,
        //             b.TindakLanjut,
        //             b.HasilPenunjangLab,
        //             b.AnjuranDiet,
        //             CreateBy = u.FullName
        //         }).ToList();

        //    // LOAD DETAIL untuk parentIds
        //    var details =
        //        (from d in _applicationDbContext.LabBookingDetails
        //         join lab in _applicationDbContext.Labs on d.LabId equals lab.LabId into labGroup
        //         from lab in labGroup.DefaultIfEmpty()

        //         join lp in _applicationDbContext.LabPemeriksaans on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpGroup
        //         from lp in lpGroup.DefaultIfEmpty()

        //         where pagedParentIds.Contains((Guid)d.BookingLabId) && (d.IsDelete == false || d.IsDelete == null)

        //         select new
        //         {
        //             d.BookingLabId,
        //             d.DetailBookingLabId,
        //             d.PemeriksaanLabId,
        //             PemeriksaanNama = lp.NamaPemeriksaan,
        //             lp.HargaPemeriksaan,
        //             lab.LabId,
        //             NamaLab = lab.NamaLab,
        //             d.IsDelete
        //         }).ToList();

        //    // MERGE parent + detail
        //    var merged = parents.Select(x => new
        //    {
        //        Parent = x,
        //        Details = details.Where(d => d.BookingLabId == x.BookingLabId).ToList()
        //    }).ToList();

        //    return Ok(new
        //    {
        //        status = "success",
        //        data = new
        //        {
        //            Rows = merged,
        //            TotalRows = totalRows,
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
        //        }
        //    });
        //}

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAsync(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            Guid? labId = null,
            Guid? dokterId = null,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
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

            if (dokterId.HasValue)
                parentQuery = parentQuery.Where(b => b.DokterId == dokterId.Value);

            // filter JenisKunjungan
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value;

                parentQuery =
                    from b in parentQuery
                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                        on b.KunjunganId equals k.KunjunganID
                    where k.JenisKunjungan == jk.ToString()
                    select b;
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

            // filter namaLab
            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                var nl = namaLab.Trim();

                parentQuery =
                    (from b in parentQuery
                     join d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                         on b.BookingLabId equals d.BookingLabId
                     join lab in _applicationDbContext.Labs.AsNoTracking()
                         on d.LabId equals lab.LabId
                     where (d.IsDelete == false || d.IsDelete == null)
                           && EF.Functions.ILike(lab.NamaLab, $"%{nl}%")
                     select b)
                    .Distinct();
            }

            // filter labId
            if (labId.HasValue)
            {
                var lid = labId.Value;

                parentQuery =
                    (from b in parentQuery
                     join d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                         on b.BookingLabId equals d.BookingLabId
                     where (d.IsDelete == false || d.IsDelete == null)
                           && d.LabId == lid
                     select b)
                    .Distinct();
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

                join k in _applicationDbContext.Kunjungans.AsNoTracking()
                    on b.KunjunganId equals k.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join a in _applicationDbContext.Asuransis.AsNoTracking()
                    on b.AsuransiId equals a.AsuransiId into aGroup
                from a in aGroup.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on b.PasienId equals p.PendaftaranPasienBaruId into pGroup
                from p in pGroup.DefaultIfEmpty()

                join d1 in _applicationDbContext.Dokters.AsNoTracking()
                    on b.DokterId equals d1.DokterId into d1Group
                from d1 in d1Group.DefaultIfEmpty()

                join d2 in _applicationDbContext.Dokters.AsNoTracking()
                    on b.DokterKonsulenId equals d2.DokterId into d2Join
                from d2 in d2Join.DefaultIfEmpty()

                join po in _applicationDbContext.Polikliniks.AsNoTracking()
                    on k.PoliklinikId equals po.PoliklinikId into poGroup
                from po in poGroup.DefaultIfEmpty()

                join kl in _applicationDbContext.Kelass.AsNoTracking()
                    on b.KelasId equals kl.KelasId into klGroup
                from kl in klGroup.DefaultIfEmpty()

                where pagedIds.Contains(b.BookingLabId)
                select new
                {
                    b.BookingLabId,
                    b.KunjunganId,
                    JenisKunjungan = k != null ? k.JenisKunjungan : null,

                    b.PasienId,
                    NamaLengkap = p != null ? p.NamaLengkap : null,
                    b.NoOrder,
                    NoRekamMedis = p != null ? p.NoRekamMedis : null,

                    b.AsuransiId,
                    AsuransiNama = a != null ? a.NamaAsuransi : null,

                    b.DokterId,
                    DokterNama = d1 != null ? d1.NmDokter : null,

                    PoliNama = po != null ? po.NamaPoliklinik : null,

                    b.TglPemeriksaan,
                    b.TglBooking,
                    b.AlasanPembatalan,
                    b.StatusBookingLab,
                    b.StatusPembayaran,
                    b.KelasId,
                    NamaKelas = kl != null ? kl.NamaKelas : null,
                    b.HemodialisaKe,
                    b.StatusPemeriksaan,
                    b.NomorSuratJaminan,
                    b.DokterKonsulenId,
                    NamaDokterKonsulen = d2 != null ? d2.NmDokter : null,
                    b.DiagnosaAwal,
                    b.Keterangan,
                    b.PetugasPembatalan,
                    b.TTDPathPembatalan,
                    b.CreateDateTime,
                    b.TindakLanjut,
                    b.HasilPenunjangLab,
                    b.AnjuranDiet,
                    b.IsDelete,
                    b.IsCito,
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
                join lab in _applicationDbContext.Labs.AsNoTracking()
                    on d.LabId equals lab.LabId into labGroup
                from lab in labGroup.DefaultIfEmpty()
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
                    d.LabId,
                    d.NoOrder,
                    TipeLayanan = d.TipeLayanan ?? null,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    HargaPemeriksaan = lp != null ? (decimal?)lp.HargaPemeriksaan : null,
                    NamaLab = lab != null ? lab.NamaLab : null,
                    d.Satuan,
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
                        d.PemeriksaanLabId,
                        d.LabId,
                        d.NoOrder,
                        d.NamaPemeriksaan,
                        d.HargaPemeriksaan,
                        d.NamaLab,
                        d.Satuan,
                        d.StatusPemeriksaan,
                        d.StatusVerifikasi,
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
        public async Task<IActionResult> Paged2Radiologi(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            Guid? labBookingId = null,
            Guid? dokterId = null,
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
            // 0) Ambil LabId radiologi sekali
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
            // 1) BASE QUERY parent
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (dokterId.HasValue)
                baseQuery = baseQuery.Where(b => b.DokterId == dokterId.Value);

            // filter JenisKunjungan
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value;

                baseQuery =
                    from b in baseQuery
                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                        on b.KunjunganId equals k.KunjunganID
                    where k.JenisKunjungan == jk.ToString()
                    select b;
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(b => b.CreateDateTime >= start && b.CreateDateTime <= end);
            }

            // hanya booking yang punya detail radiologi
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null) &&
                    radiologiLabIds.Contains(d.LabId)
                )
            );

            // =============================
            // 2) TOTAL rows parent
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 3) SORTING parent
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 4) PAGING parent
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
            // 5) LOAD PARENT DATA
            // =============================
            var parents = await
                (from b in _applicationDbContext.LabBookings.AsNoTracking()
                 where pagedIdSet.Contains(b.BookingLabId)

                 join u in _applicationDbContext.UserActives.AsNoTracking()
                     on b.CreateBy equals u.UserActiveId into uJoin
                 from u in uJoin.DefaultIfEmpty()

                 join k in _applicationDbContext.Kunjungans.AsNoTracking()
                     on b.KunjunganId equals k.KunjunganID into kJoin
                 from k in kJoin.DefaultIfEmpty()

                 join a in _applicationDbContext.Asuransis.AsNoTracking()
                     on b.AsuransiId equals a.AsuransiId into aJoin
                 from a in aJoin.DefaultIfEmpty()

                 join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                     on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                 from p in pJoin.DefaultIfEmpty()

                 join d1 in _applicationDbContext.Dokters.AsNoTracking()
                     on b.DokterId equals d1.DokterId into dJoin
                 from d1 in dJoin.DefaultIfEmpty()

                 join d2 in _applicationDbContext.Dokters.AsNoTracking()
                     on b.DokterKonsulenId equals d2.DokterId into d2Join
                 from d2 in d2Join.DefaultIfEmpty()

                 join po in _applicationDbContext.Polikliniks.AsNoTracking()
                     on k.PoliklinikId equals po.PoliklinikId into poJoin
                 from po in poJoin.DefaultIfEmpty()

                 join kl in _applicationDbContext.Kelass.AsNoTracking()
                     on b.KelasId equals kl.KelasId into klJoin
                 from kl in klJoin.DefaultIfEmpty()

                 select new
                 {
                     b.BookingLabId,
                     b.KunjunganId,
                     PoliklinikId = (Guid?)k.PoliklinikId,
                     JenisKunjungan = k.JenisKunjungan ?? null,
                     k.AsalKunjungan,
                     b.PasienId,
                     NamaLengkap = p.NamaLengkap,
                     b.NoOrder,
                     NoRekamMedis = p.NoRekamMedis,
                     b.AsuransiId,
                     AsuransiNama = a.NamaAsuransi,
                     b.DokterId,
                     DokterNama = d1.NmDokter,
                     PoliNama = po.NamaPoliklinik,
                     b.TglPemeriksaan,
                     b.TglBooking,
                     b.AlasanPembatalan,
                     b.StatusBookingLab,
                     b.StatusPembayaran,
                     b.KelasId,
                     NamaKelas = kl.NamaKelas,
                     b.HemodialisaKe,
                     b.StatusPemeriksaan,
                     b.NomorSuratJaminan,
                     b.DokterKonsulenId,
                     NamaDokterKonsulen = d2.NmDokter,
                     b.DiagnosaAwal,
                     b.Keterangan,
                     b.TTDPathPembatalan,
                     b.PetugasPembatalan,
                     b.CreateDateTime,
                     b.TindakLanjut,
                     b.HasilPenunjangLab,
                     b.AnjuranDiet,
                     b.IsDelete,
                     b.IsCito,
                     CreateBy = u.FullName
                 }).ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 6) LOAD DETAIL RAW (hanya page ini + radiologi)
            // =============================
            var rawDetails = await
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()

                 join b in _applicationDbContext.LabBookings.AsNoTracking()
                     on d.BookingLabId equals b.BookingLabId

                 join lab in _applicationDbContext.Labs.AsNoTracking()
                     on d.LabId equals lab.LabId into labJoin
                 from lab in labJoin.DefaultIfEmpty()

                 join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                 from lp in lpJoin.DefaultIfEmpty()

                 where d.BookingLabId != null
                       && pagedIdSet.Contains(d.BookingLabId.Value)
                       && (d.IsDelete == false || d.IsDelete == null)
                       && radiologiLabIds.Contains(d.LabId)

                 orderby d.CreateDateTime descending
                 select new
                 {
                     BookingLabId = d.BookingLabId.Value,
                     KunjunganId = b.KunjunganId, // ambil dari header booking
                     d.DetailBookingLabId,
                     d.PasienId,
                     TipeLayanan = d.TipeLayanan ?? null,
                     d.PemeriksaanLabId,
                     d.LabId,
                     d.NoOrder,
                     NamaPemeriksaan = lp.NamaPemeriksaan,
                     HargaPemeriksaan = lp.HargaPemeriksaan,
                     Lab = lab.NamaLab,
                     d.Satuan,
                     d.StatusPemeriksaan,
                     d.StatusVerifikasi,
                     d.TanggalSelesai,
                     d.CreateDateTime,
                     d.IsDelete
                 }).ToListAsync();

            // =============================
            // 7) AMBIL STATUS LUNAS PER DETAIL
            // Relasi: KunjunganId + PemeriksaanLabId <-> Billing.KunjunganId + Billing.ItemId
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

            if (kunjunganIds.Count > 0 && pemeriksaanIds.Count > 0)
            {
                var billingStatusList = await (
                    from b in _applicationDbContext.Billings.AsNoTracking()
                    where (b.IsDelete == false || b.IsDelete == null)
                          && b.BillingKode == "LAB"
                          && b.KunjunganId.HasValue
                          && kunjunganIds.Contains(b.KunjunganId.Value)
                          && b.ItemId.HasValue
                          && pemeriksaanIds.Contains(b.ItemId.Value)
                    group b by new
                    {
                        KunjunganId = b.KunjunganId.Value,
                        PemeriksaanLabId = b.ItemId.Value
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
            };

            // =============================
            // 8) MAP DETAIL + FILTER isLunas (OPS A: setelah page parent)
            // =============================
            var finalDetails = rawDetails
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
                        d.PemeriksaanLabId,
                        d.LabId,
                        d.NoOrder,
                        d.NamaPemeriksaan,
                        d.HargaPemeriksaan,
                        d.Lab,
                        d.Satuan,
                        d.StatusPemeriksaan,
                        d.StatusVerifikasi,
                        d.TanggalSelesai,
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
            // 9) MERGE sesuai urutan paging
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
            Guid? dokterId = null,
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
            // 0) Ambil LabId Rehab Medis sekali
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
            // 1) BASE QUERY
            // =============================
            var baseQuery = _applicationDbContext.LabBookings
                .AsNoTracking()
                .Where(b => b.IsDelete == false || b.IsDelete == null);

            if (kunjunganId.HasValue)
                baseQuery = baseQuery.Where(b => b.KunjunganId == kunjunganId.Value);

            if (labBookingId.HasValue)
                baseQuery = baseQuery.Where(b => b.BookingLabId == labBookingId.Value);

            if (dokterId.HasValue)
                baseQuery = baseQuery.Where(b => b.DokterId == dokterId.Value);

            // filter JenisKunjungan
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value;

                baseQuery =
                    from b in baseQuery
                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                        on b.KunjunganId equals k.KunjunganID
                    where k.JenisKunjungan == jk.ToString()
                    select b;
            }

            // =============================
            // 2) Filter tanggal manual (startDate/endDate)
            // =============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1); // exclusive upper bound (lebih bagus dari AddTicks(-1))
                baseQuery = baseQuery.Where(b => b.CreateDateTime >= start && b.CreateDateTime < endExclusive);
            }

            // =============================
            // 3) Filter periode (dibuat sargable: pakai range >= start && < end)
            // =============================
            if (periode.HasValue)
            {
                // NOTE: kamu pakai UTC. Pastikan CreateDateTime memang UTC.
                // Kalau CreateDateTime local time, sebaiknya pakai DateTime.Now.
                var today = DateTime.UtcNow.Date;

                DateTime rangeStart;
                DateTime rangeEndExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        rangeStart = today;
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisWeek:
                        // start minggu: Sunday=0 (default .NET)
                        rangeStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStart = thisWeekStart.AddDays(-7);
                        rangeEndExclusive = thisWeekStart;
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisMonth:
                        rangeStart = new DateTime(today.Year, today.Month, 1);
                        rangeEndExclusive = rangeStart.AddMonths(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        rangeStart = thisMonthStart.AddMonths(-1);
                        rangeEndExclusive = thisMonthStart;
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisYear:
                        rangeStart = new DateTime(today.Year, 1, 1);
                        rangeEndExclusive = rangeStart.AddYears(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTime(today.Year, 1, 1);
                        rangeStart = thisYearStart.AddYears(-1);
                        rangeEndExclusive = thisYearStart;
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last3Months:
                        rangeStart = today.AddMonths(-3);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = today.AddMonths(-6);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;
                }
            }

            // =============================
            // 4) FILTER Rehab Medis pakai EXISTS/Any (tanpa join + Distinct)
            // =============================
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null) &&
                    rehabLabIds.Contains(d.LabId)
                )
            );

            // =============================
            // 5) TOTAL rows
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 6) SORTING (aman)
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 7) PAGING: ambil ID dulu
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
                    data = new { Rows = new List<object>(), TotalRows = 0 }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 8) LOAD PARENT DATA (hanya page ini)
            // =============================
            var parents = await
                (from b in _applicationDbContext.LabBookings.AsNoTracking()
                 where pagedIdSet.Contains(b.BookingLabId)

                 join u in _applicationDbContext.UserActives.AsNoTracking()
                     on b.CreateBy equals u.UserActiveId into uJoin
                 from u in uJoin.DefaultIfEmpty()

                 join k in _applicationDbContext.Kunjungans.AsNoTracking()
                     on b.KunjunganId equals k.KunjunganID into kJoin
                 from k in kJoin.DefaultIfEmpty()

                 join a in _applicationDbContext.Asuransis.AsNoTracking()
                     on b.AsuransiId equals a.AsuransiId into aJoin
                 from a in aJoin.DefaultIfEmpty()

                 join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                     on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                 from p in pJoin.DefaultIfEmpty()

                 join d1 in _applicationDbContext.Dokters.AsNoTracking()
                     on b.DokterId equals d1.DokterId into dJoin
                 from d1 in dJoin.DefaultIfEmpty()

                 join d2 in _applicationDbContext.Dokters.AsNoTracking()
                     on b.DokterKonsulenId equals d2.DokterId into d2Join
                 from d2 in d2Join.DefaultIfEmpty()

                 join po in _applicationDbContext.Polikliniks.AsNoTracking()
                     on k.PoliklinikId equals po.PoliklinikId into poJoin
                 from po in poJoin.DefaultIfEmpty()

                 join kl in _applicationDbContext.Kelass.AsNoTracking()
                     on b.KelasId equals kl.KelasId into klJoin
                 from kl in klJoin.DefaultIfEmpty()

                 select new
                 {
                     b.BookingLabId,
                     b.KunjunganId,
                     JenisKunjungan = k.JenisKunjungan ?? null,
                     AsalKunjungan = k.AsalKunjungan ?? null,
                     b.PasienId,
                     p.NamaLengkap,
                     b.NoOrder,
                     p.NoRekamMedis,
                     b.AsuransiId,
                     AsuransiNama = a.NamaAsuransi ?? null,
                     b.DokterId,
                     DokterNama = d1.NmDokter ?? null,
                     PoliNama = po.NamaPoliklinik ?? null,
                     b.TglPemeriksaan,
                     b.TglBooking,
                     b.AlasanPembatalan,
                     b.StatusBookingLab,
                     b.StatusPembayaran,
                     b.KelasId,
                     NamaKelas = kl.NamaKelas ?? null,
                     b.HemodialisaKe,
                     b.StatusPemeriksaan,
                     b.NomorSuratJaminan,
                     b.DokterKonsulenId,
                     NamaDokterKonsulen = d2.NmDokter ?? null,
                     b.DiagnosaAwal,
                     b.Keterangan,
                     b.PetugasPembatalan,
                     b.TTDPathPembatalan,
                     b.CreateDateTime,
                     b.TindakLanjut,
                     b.HasilPenunjangLab,
                     b.AnjuranDiet,
                     b.IsDelete,
                     b.IsCito,
                     CreateBy = u.FullName
                 }).ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 9) LOAD DETAIL (hanya page ini + rehabmedis)
            // =============================
            var details = await
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                 join lab in _applicationDbContext.Labs.AsNoTracking()
                     on d.LabId equals lab.LabId into labJoin
                 from lab in labJoin.DefaultIfEmpty()

                 join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                 from lp in lpJoin.DefaultIfEmpty()

                 where d.BookingLabId != null
                       && pagedIdSet.Contains((Guid)d.BookingLabId)
                       && (d.IsDelete == false || d.IsDelete == null)
                       && rehabLabIds.Contains(d.LabId)

                 select new
                 {
                     BookingLabId = (Guid?)d.BookingLabId,
                     d.DetailBookingLabId,
                     d.NoOrder,
                     TipeLayanan = d.TipeLayanan ?? null,
                     NamaPemeriksaan = lp.NamaPemeriksaan,
                     HargaPemeriksaan = lp.HargaPemeriksaan,
                     Lab = lab.NamaLab,
                     d.Satuan,
                     d.IsDelete
                 }).ToListAsync();

            // TRIK: list kosong tapi tipe anonymous sama persis seperti details
            var emptyDetails = details.Take(0).ToList();

            // lookup detail by parentId (O(n))
            var detailLookup = details
                .Where(x => x.BookingLabId.HasValue)
                .GroupBy(x => x.BookingLabId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =============================
            // 10) MERGE (urut sesuai paging)
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                });

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
            Guid? dokterId = null,
            string? dokterKonsul = null,
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
            // 0) Ambil LabId untuk "Gizi" sekali
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
            // 2) Filter periode (SARGABLE: range)
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
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisWeek:
                        rangeStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        rangeStart = thisWeekStart.AddDays(-7);
                        rangeEndExclusive = thisWeekStart;
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisMonth:
                        rangeStart = new DateTime(today.Year, today.Month, 1);
                        rangeEndExclusive = rangeStart.AddMonths(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        rangeStart = thisMonthStart.AddMonths(-1);
                        rangeEndExclusive = thisMonthStart;
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.ThisYear:
                        rangeStart = new DateTime(today.Year, 1, 1);
                        rangeEndExclusive = rangeStart.AddYears(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTime(today.Year, 1, 1);
                        rangeStart = thisYearStart.AddYears(-1);
                        rangeEndExclusive = thisYearStart;
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last3Months:
                        rangeStart = today.AddMonths(-3);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;

                    case PeriodeFilter.Last6Months:
                        rangeStart = today.AddMonths(-6);
                        rangeEndExclusive = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                        break;
                }
            }

            // =============================
            // 3) Filter start/end date manual (range)
            // =============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var endExclusive = endDate.Value.Date.AddDays(1);
                baseQuery = baseQuery.Where(b => b.CreateDateTime >= start && b.CreateDateTime < endExclusive);
            }

            // =============================
            // 4) Filter dokter konsulen (tanpa join besar)
            // =============================
            if (!string.IsNullOrWhiteSpace(dokterKonsul))
            {
                var dk = dokterKonsul.Trim().ToLower();

                baseQuery = baseQuery.Where(b =>
                    b.DokterKonsulenId != null &&
                    _applicationDbContext.Dokters.Any(dr =>
                        dr.DokterId == b.DokterKonsulenId &&
                        dr.NmDokter != null &&
                        dr.NmDokter.ToLower().Contains(dk)
                    )
                );
            }

            // filter JenisKunjungan
            if (JenisKunjungan.HasValue)
            {
                var jk = JenisKunjungan.Value;

                baseQuery =
                    from b in baseQuery
                    join k in _applicationDbContext.Kunjungans.AsNoTracking()
                        on b.KunjunganId equals k.KunjunganID
                    where k.JenisKunjungan == jk.ToString()
                    select b;
            }

            // =============================
            // 5) Filter hanya "Gizi" pakai EXISTS/Any (tanpa join+Distinct)
            // =============================
            baseQuery = baseQuery.Where(b =>
                _applicationDbContext.LabBookingDetails.Any(d =>
                    d.BookingLabId == b.BookingLabId &&
                    (d.IsDelete == false || d.IsDelete == null) &&
                    giziLabIds.Contains(d.LabId)
                )
            );

            // =============================
            // 6) TOTAL rows
            // =============================
            int totalRows = await baseQuery.CountAsync();

            // =============================
            // 7) SORTING
            // =============================
            bool desc = (sortDirection ?? "desc")
                .Equals("desc", StringComparison.OrdinalIgnoreCase);

            IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TglBooking" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                "TglPemeriksaan" =>
                    desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                _ =>
                    desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =============================
            // 8) PAGING: ambil BookingLabId dulu
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
                    data = new { Rows = new List<object>(), TotalRows = 0 }
                });
            }

            var pagedIdSet = pagedParentIds.ToHashSet();

            // =============================
            // 9) LOAD PARENT DATA (page ini saja)
            // =============================
            var parents = await
                (from b in _applicationDbContext.LabBookings.AsNoTracking()
                 where pagedIdSet.Contains(b.BookingLabId)

                 join u in _applicationDbContext.UserActives.AsNoTracking()
                     on b.CreateBy equals u.UserActiveId into uJoin
                 from u in uJoin.DefaultIfEmpty()

                 join k in _applicationDbContext.Kunjungans.AsNoTracking()
                     on b.KunjunganId equals k.KunjunganID into kJoin
                 from k in kJoin.DefaultIfEmpty()

                 join a in _applicationDbContext.Asuransis.AsNoTracking()
                     on b.AsuransiId equals a.AsuransiId into aJoin
                 from a in aJoin.DefaultIfEmpty()

                 join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                     on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                 from p in pJoin.DefaultIfEmpty()

                 join d1 in _applicationDbContext.Dokters.AsNoTracking()
                     on b.DokterId equals d1.DokterId into dJoin
                 from d1 in dJoin.DefaultIfEmpty()

                 join d2 in _applicationDbContext.Dokters.AsNoTracking()
                     on b.DokterKonsulenId equals d2.DokterId into d2join
                 from d2 in d2join.DefaultIfEmpty()

                 join po in _applicationDbContext.Polikliniks.AsNoTracking()
                     on k.PoliklinikId equals po.PoliklinikId into poJoin
                 from po in poJoin.DefaultIfEmpty()

                 join kl in _applicationDbContext.Kelass.AsNoTracking()
                     on b.KelasId equals kl.KelasId into klJoin
                 from kl in klJoin.DefaultIfEmpty()

                 select new
                 {
                     b.BookingLabId,
                     b.KunjunganId,
                     AsalKunjungan = k.AsalKunjungan ?? null,
                     JenisKunjungan = k.JenisKunjungan ?? null,
                     b.PasienId,
                     NamaLengkap = p.NamaLengkap,
                     b.NoOrder,
                     NoRekamMedis = p.NoRekamMedis,
                     b.AsuransiId,
                     AsuransiNama = a.NamaAsuransi ?? null,
                     b.DokterId,
                     DokterNama = d1.NmDokter ?? null,
                     PoliNama = po.NamaPoliklinik ?? null,
                     b.TglPemeriksaan,
                     b.TglBooking,
                     b.AlasanPembatalan,
                     b.StatusBookingLab,
                     b.StatusPembayaran,
                     b.KelasId,
                     NamaKelas = kl.NamaKelas ?? null,
                     b.HemodialisaKe,
                     b.StatusPemeriksaan,
                     b.NomorSuratJaminan,
                     b.DokterKonsulenId,
                     NamaDokterKonsulen = d2.NmDokter ?? null,
                     b.DiagnosaAwal,
                     b.Keterangan,
                     b.PetugasPembatalan,
                     b.TTDPathPembatalan,
                     b.CreateDateTime,
                     b.TindakLanjut,
                     b.HasilPenunjangLab,
                     b.AnjuranDiet,
                     b.IsDelete,
                     b.IsCito,
                     CreateBy = u.FullName
                 }).ToListAsync();

            var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

            // =============================
            // 10) LOAD DETAIL (page ini saja + gizi)
            // =============================
            var details = await
                (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                 join lab in _applicationDbContext.Labs.AsNoTracking()
                     on d.LabId equals lab.LabId into labJoin
                 from lab in labJoin.DefaultIfEmpty()

                 join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                     on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                 from lp in lpJoin.DefaultIfEmpty()

                 where d.BookingLabId != null
                       && pagedIdSet.Contains((Guid)d.BookingLabId)
                       && (d.IsDelete == false || d.IsDelete == null)
                       && giziLabIds.Contains(d.LabId)

                 select new
                 {
                     BookingLabId = (Guid?)d.BookingLabId,
                     d.DetailBookingLabId,
                     d.NoOrder,
                     TipeLayanan = d.TipeLayanan ?? null,
                     NamaPemeriksaan = lp.NamaPemeriksaan,
                     HargaPemeriksaan = lp.HargaPemeriksaan,
                     Lab = lab.NamaLab,
                     d.Satuan,
                     d.IsDelete
                 }).ToListAsync();

            // TRIK: empty list dengan tipe anonymous yang sama persis
            var emptyDetails = details.Take(0).ToList();

            var detailLookup = details
                .Where(x => x.BookingLabId.HasValue)
                .GroupBy(x => x.BookingLabId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // =============================
            // 11) MERGE (urut sesuai paging)
            // =============================
            var merged = pagedParentIds
                .Where(id => parentLookup.ContainsKey(id))
                .Select(id => new
                {
                    Parent = parentLookup[id],
                    Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                });

            // =============================
            // 12) RETURN
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
            Guid? dokterId = null,
            string? dokterKonsul = null,
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
                    // 0) Ambil LabId "MCU" sekali
                    // =============================
                    var mcuLabIds = await _applicationDbContext.Labs
                        .AsNoTracking()
                        .Where(l => l.NamaLab != null &&
                                    l.NamaLab.ToLower().Replace(" ", "") == "mcu")
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

                    if (dokterId.HasValue)
                        baseQuery = baseQuery.Where(b => b.DokterId == dokterId.Value);

                    // filter JenisKunjungan
                    if (JenisKunjungan.HasValue)
                    {
                        var jk = JenisKunjungan.Value;

                        baseQuery =
                            from b in baseQuery
                            join k in _applicationDbContext.Kunjungans.AsNoTracking()
                                on b.KunjunganId equals k.KunjunganID
                            where k.JenisKunjungan == jk.ToString()
                            select b;
                    }

            // =============================
            // 2) Filter start/end date manual (range)
            // =============================
            if (startDate.HasValue && endDate.HasValue)
                    {
                        var start = startDate.Value.Date;
                        var endExclusive = endDate.Value.Date.AddDays(1); // exclusive upper bound
                        baseQuery = baseQuery.Where(b => b.CreateDateTime >= start && b.CreateDateTime < endExclusive);
                    }

                    // =============================
                    // 3) Filter periode (SARGABLE: range)
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

                        baseQuery = baseQuery.Where(x => x.CreateDateTime >= rangeStart && x.CreateDateTime < rangeEndExclusive);
                    }

                    // =============================
                    // 4) Filter dokter konsulen (subquery Any, tanpa join Distinct)
                    // =============================
                    if (!string.IsNullOrWhiteSpace(dokterKonsul))
                    {
                        var dk = dokterKonsul.Trim().ToLower();

                        baseQuery = baseQuery.Where(b =>
                            b.DokterKonsulenId != null &&
                            _applicationDbContext.Dokters.Any(dr =>
                                dr.DokterId == b.DokterKonsulenId &&
                                dr.NmDokter != null &&
                                dr.NmDokter.ToLower().Contains(dk)
                            )
                        );
                    }

                    // =============================
                    // 5) Filter hanya MCU pakai EXISTS/Any
                    // =============================
                    baseQuery = baseQuery.Where(b =>
                        _applicationDbContext.LabBookingDetails.Any(d =>
                            d.BookingLabId == b.BookingLabId &&
                            (d.IsDelete == false || d.IsDelete == null) &&
                            mcuLabIds.Contains(d.LabId)
                        )
                    );

                    // =============================
                    // 6) TOTAL rows
                    // =============================
                    int totalRows = await baseQuery.CountAsync();

                    // =============================
                    // 7) SORTING
                    // =============================
                    bool desc = (sortDirection ?? "desc")
                        .Equals("desc", StringComparison.OrdinalIgnoreCase);

                    IQueryable<LabBooking> sortedQuery = (orderBy ?? "CreateDateTime").Trim() switch
                    {
                        "TglBooking" =>
                            desc ? baseQuery.OrderByDescending(x => x.TglBooking) : baseQuery.OrderBy(x => x.TglBooking),

                        "TglPemeriksaan" =>
                            desc ? baseQuery.OrderByDescending(x => x.TglPemeriksaan) : baseQuery.OrderBy(x => x.TglPemeriksaan),

                        _ =>
                            desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
                    };

                    // =============================
                    // 8) PAGING: ambil BookingLabId dulu
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
                    // 9) LOAD PARENT DATA (page ini saja)
                    // =============================
                    var parents = await
                        (from b in _applicationDbContext.LabBookings.AsNoTracking()
                         where pagedIdSet.Contains(b.BookingLabId)

                         join u in _applicationDbContext.UserActives.AsNoTracking()
                             on b.CreateBy equals u.UserActiveId into uJoin
                         from u in uJoin.DefaultIfEmpty()

                         join k in _applicationDbContext.Kunjungans.AsNoTracking()
                             on b.KunjunganId equals k.KunjunganID into kJoin
                         from k in kJoin.DefaultIfEmpty()

                         join a in _applicationDbContext.Asuransis.AsNoTracking()
                             on b.AsuransiId equals a.AsuransiId into aJoin
                         from a in aJoin.DefaultIfEmpty()

                         join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                             on b.PasienId equals p.PendaftaranPasienBaruId into pJoin
                         from p in pJoin.DefaultIfEmpty()

                         join d1 in _applicationDbContext.Dokters.AsNoTracking()
                             on b.DokterId equals d1.DokterId into dJoin
                         from d1 in dJoin.DefaultIfEmpty()

                         join d2 in _applicationDbContext.Dokters.AsNoTracking()
                             on b.DokterKonsulenId equals d2.DokterId into d2join
                         from d2 in d2join.DefaultIfEmpty()

                         join po in _applicationDbContext.Polikliniks.AsNoTracking()
                             on k.PoliklinikId equals po.PoliklinikId into poJoin
                         from po in poJoin.DefaultIfEmpty()

                         join kl in _applicationDbContext.Kelass.AsNoTracking()
                             on b.KelasId equals kl.KelasId into klJoin
                         from kl in klJoin.DefaultIfEmpty()

                         select new
                         {
                             b.BookingLabId,
                             b.KunjunganId,
                             AsalKunjungan = k.AsalKunjungan ?? null,
                             JenisKunjungan = k.JenisKunjungan ?? null,
                             b.PasienId,
                             NamaLengkap = p.NamaLengkap,
                             NoRekamMedis = p.NoRekamMedis,
                             b.AsuransiId,
                             AsuransiNama = a.NamaAsuransi ?? null,
                             b.DokterId,
                             DokterNama = d1.NmDokter ?? null,
                             PoliNama = po.NamaPoliklinik ?? null,
                             b.TglPemeriksaan,
                             b.TglBooking,
                             b.AlasanPembatalan,
                             b.StatusBookingLab,
                             b.StatusPembayaran,
                             b.KelasId,
                             NamaKelas = kl.NamaKelas ?? null,
                             b.HemodialisaKe,
                             b.StatusPemeriksaan,
                             b.NomorSuratJaminan,
                             b.DokterKonsulenId,
                             NamaDokterKonsulen = d2.NmDokter ?? null,
                             b.DiagnosaAwal,
                             b.Keterangan,
                             b.PetugasPembatalan,
                             b.TTDPathPembatalan,
                             b.CreateDateTime,
                             b.TindakLanjut,
                             b.HasilPenunjangLab,
                             b.AnjuranDiet,
                             b.IsDelete,
                             b.IsCito,
                             CreateBy = u.FullName
                         }).ToListAsync();

                    var parentLookup = parents.ToDictionary(x => x.BookingLabId, x => x);

                    // =============================
                    // 10) LOAD DETAIL (page ini saja + MCU)
                    // =============================
                    var details = await
                        (from d in _applicationDbContext.LabBookingDetails.AsNoTracking()
                         join lab in _applicationDbContext.Labs.AsNoTracking()
                             on d.LabId equals lab.LabId into labJoin
                         from lab in labJoin.DefaultIfEmpty()

                         join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                             on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpJoin
                         from lp in lpJoin.DefaultIfEmpty()

                         where d.BookingLabId != null
                               && pagedIdSet.Contains((Guid)d.BookingLabId)
                               && (d.IsDelete == false || d.IsDelete == null)
                               && mcuLabIds.Contains(d.LabId)

                         select new
                         {
                             BookingLabId = (Guid?)d.BookingLabId,
                             d.DetailBookingLabId,
                             d.NoOrder,
                             TipeLayanan = d.TipeLayanan ?? null,
                             NamaPemeriksaan = lp.NamaPemeriksaan,
                             HargaPemeriksaan = lp.HargaPemeriksaan,
                             Lab = lab.NamaLab,
                             d.Satuan,
                             d.IsDelete
                         }).ToListAsync();

                    var emptyDetails = details.Take(0).ToList();

                    var detailLookup = details
                        .Where(x => x.BookingLabId.HasValue)
                        .GroupBy(x => x.BookingLabId!.Value)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // =============================
                    // 11) MERGE (urut sesuai paging)
                    // =============================
                    var merged = pagedParentIds
                        .Where(id => parentLookup.ContainsKey(id))
                        .Select(id => new
                        {
                            Parent = parentLookup[id],
                            Details = detailLookup.TryGetValue(id, out var det) ? det : emptyDetails
                        });

                    // =============================
                    // 12) RETURN
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
