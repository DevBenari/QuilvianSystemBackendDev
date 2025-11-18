using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
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
        private readonly string _uploadUrl;

        private readonly ILogger<LabBookingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabBookingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabBookingController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
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
                             b.StatusPembayaran
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

                                where (b.IsDelete == false || b.IsDelete == null)
                                      && b.BookingLabId == id
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
                                    HargaPemeriksaan = (decimal?)(lp.HargaPemeriksaan ?? 0),
                                    NamaLab = l.NamaLab ?? null,
                                    AlasanPembatalan = lb.AlasanPembatalan ?? null,
                                    TTDPembatalanPath = lb.TTDPembatalanPath ?? null,
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
                        g.First().Keterangan,
                        g.First().CreateBy,
                        g.First().CreateByName,
                        g.First().CreateDateTime,

                        Details = g.Where(d => d.LabBookingDetailId != null).Select(d => new
                        {
                            d.LabBookingDetailId,
                            d.PemeriksaanLabId,
                            d.PemeriksaanNama,
                            d.HargaPemeriksaan,
                            d.NamaLab,
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
                    StatusBookingLab = vm.StatusBookingLab,
                    AlasanPembatalan = vm.AlasanPembatalan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTime.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.LabBookings.Add(entity);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
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
        public IActionResult Paged(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganid = null,
            string? namaLab = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
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

                            where b.IsDelete == false || b.IsDelete == null
                            select new
                            {
                                // Header
                                b.BookingLabId,
                                KunjunganId = (Guid?)b.KunjunganId,
                                AsalKunjungan = k != null ? k.AsalKunjungan : null,
                                PoliId = (Guid?)po.PoliklinikId,
                                NamaPoli = po.NamaPoliklinik ?? null,
                                PasienId = (Guid?)b.PasienId,
                                PasienNama = p.NamaLengkap,
                                p.NoRekamMedis,
                                b.NomorSuratJaminan,
                                b.TglPemeriksaan,
                                b.TglBooking,
                                b.TglPenyerahanSampling,
                                b.KelasId,
                                b.Keterangan,
                                b.IsCito,
                                b.DiagnosaAwal,
                                b.HemodialisaKe,
                                b.StatusPemeriksaan,
                                AsuransiId = (Guid?)b.AsuransiId,
                                AsuransiNama = a.NamaAsuransi ?? null,
                                DokterId = (Guid?)b.DokterId,
                                DokterNama = d1.NmDokter ?? null,
                                DokterKonsulenId = b.DokterKonsulenId ?? null,
                                DokterKonsulen = d2.NmDokter ?? null,
                                TipePasien = k != null ? k.TipePasien : null,
                                b.CreateBy,
                                CreateByName = u.FullName,
                                b.StatusBookingLab,
                                b.CatatanJaminan,
                                b.StatusPembayaran,
                                b.CreateDateTime,

                                // Detail
                                LabBookingDetailId = (Guid?)lb.DetailBookingLabId,
                                PemeriksaanLabId = (Guid?)lb.PemeriksaanLabId,
                                PemeriksaanNama = lp.NamaPemeriksaan,
                                HargaPemeriksaan = (decimal?)(lp.HargaPemeriksaan ?? 0),
                                NamaLab = l.NamaLab ?? null,
                                AlasanPembatalan = lb.AlasanPembatalan ?? null,
                                TTDPembatalanPath = lb.TTDPembatalanPath ?? null,
                            };

            // 🔍 Filter nama lab
            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                var pattern = $"%{namaLab.ToLower()}%";
                baseQuery = baseQuery.Where(u => EF.Functions.ILike(u.NamaLab, pattern));
            }

            // 🔍 Filter kunjungan
            if (kunjunganid.HasValue)
                baseQuery = baseQuery.Where(u => u.KunjunganId == kunjunganid.Value);

            // 🔍 Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                baseQuery = baseQuery.Where(u => u.CreateDateTime >= startUtc && u.CreateDateTime <= endUtc);
            }

            // 🔍 Filter periode
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        baseQuery = baseQuery.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        baseQuery = baseQuery.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;
                    case PeriodeFilter.ThisMonth:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Month == today.Month && u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        baseQuery = baseQuery.Where(u => u.CreateDateTime.Month == lastMonth.Month && u.CreateDateTime.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.Last3Months:
                        baseQuery = baseQuery.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                }
            }

            // Sorting
            baseQuery = sortDirection?.ToLower() == "desc"
                ? baseQuery.OrderByDescending(u => u.CreateDateTime)
                : baseQuery.OrderBy(u => u.CreateDateTime);

            // Eksekusi query ke memory (hanya 1 query SQL)
            var rawData = baseQuery.ToList();

            // ======================================================
            // ✅ Grouping by BookingLabId tanpa N+1
            // ======================================================
            var grouped = rawData
                .GroupBy(x => x.BookingLabId)
                .Select(g => new
                {
                    // Header
                    BookingLabId = g.Key,
                    g.First().KunjunganId,
                    g.First().PoliId,
                    g.First().NamaPoli,
                    g.First().PasienId,
                    g.First().PasienNama,
                    g.First().NomorSuratJaminan,
                    g.First().NoRekamMedis,
                    g.First().StatusBookingLab,
                    g.First().StatusPembayaran,
                    g.First().CatatanJaminan,
                    g.First().TglPemeriksaan,
                    g.First().TglBooking,
                    g.First().TglPenyerahanSampling,
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
                    g.First().Keterangan,
                    g.First().CreateBy,
                    g.First().CreateByName,
                    g.First().CreateDateTime,

                    // Array detail lab
                    Details = g.Where(d => d.LabBookingDetailId != null).Select(d => new
                    {
                        d.LabBookingDetailId,
                        d.PemeriksaanLabId,
                        d.PemeriksaanNama,
                        d.HargaPemeriksaan,
                        d.NamaLab,
                        d.AlasanPembatalan,
                        d.TTDPembatalanPath,
                    }).ToList()
                });

            // Pagination manual
            var totalRows = grouped.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = grouped.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (!rows.Any())
                return NotFound(new { message = "Page not found." });

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
