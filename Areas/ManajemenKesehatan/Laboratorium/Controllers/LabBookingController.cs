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
                         where b.IsDelete == false || b.IsDelete == null
                         select new
                         {
                             b.CreateDateTime,
                             b.CreateBy,
                             CreateByName = u.FullName,
                             b.BookingLabId,
                             b.KunjunganId,
                             b.PasienId,
                             b.TglPenyerahanSampling,
                             b.TglBooking,
                             b.KelasId,
                             b.DokterId,
                             b.Keterangan,
                             b.IsCito,
                             b.DiagnosaAwal,
                             b.DokterKonsulenId,
                             b.TerapisId,
                             b.TglPemeriksaan,
                             b.StatusPemeriksaan,
                             b.AsuransiId,
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
            var listdata = _applicationDbContext.LabBookings.Find(id);
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
        public async Task<IActionResult> Create([FromForm] LabBookingViewModel vm) // pakai FromForm biar bisa upload file
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==================================================
                // ✅ PROSES UPLOAD SURAT JAMINAN
                // ==================================================
                string? suratJaminanPath = null;

                if (vm.SuratJaminan != null && vm.SuratJaminan.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024; // maksimal 2MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".pdf" };
                    var fileExtension = Path.GetExtension(vm.SuratJaminan.FileName).ToLower();

                    if (vm.SuratJaminan.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimal 2MB." });

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG, PNG, atau PDF." });

                    // Nama file unik
                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var safeFileName = $"{vm.PasienId}_{safeTime}_SuratJaminan{fileExtension}";

                    // Folder tujuan
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SuratJaminan");

                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    var filePath = Path.Combine(uploadFolder, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.SuratJaminan.CopyToAsync(stream);
                    }

                    // Simpan path relatif (misal untuk disajikan via API)
                    suratJaminanPath = $"/SuratJaminan/{safeFileName}";
                }

                // ==================================================
                // ✅ BUAT DATA LAB BOOKING
                // ==================================================
                var data = new LabBooking
                {
                    BookingLabId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    AsuransiId = vm.AsuransiId,
                    TglPenyerahanSampling = vm.TglPenyerahanSampling,
                    TglBooking = vm.TglBooking ?? DateTime.UtcNow,
                    TglPemeriksaan = vm.TglPemeriksaan,
                    KelasId = vm.KelasId,
                    DokterId = vm.DokterId,
                    DiagnosaAwal = vm.DiagnosaAwal,
                    Keterangan = vm.Keterangan,
                    IsCito = vm.IsCito ?? false,
                    DokterKonsulenId = vm.DokterKonsulenId,
                    TerapisId = vm.TerapisId,
                    StatusPemeriksaan = vm.StatusPemeriksaan ?? "Menunggu",
                    SuratJaminanPath = suratJaminanPath, // simpan path hasil upload

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.LabBookings.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Booking Lab Berhasil || 201 Created",
                        data.BookingLabId,
                        data.SuratJaminanPath
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] LabBookingViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // 🔹 Ambil data lama dari database
                var existing = await _applicationDbContext.LabBookings.FindAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Data Booking Lab tidak ditemukan." });
                }

                // 🔹 Ambil user aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==================================================
                // ✅ PROSES UPLOAD SURAT JAMINAN (Baru)
                // ==================================================
                string? suratJaminanPath = existing.SuratJaminanPath;

                if (vm.SuratJaminan != null && vm.SuratJaminan.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024; // 2MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".pdf" };
                    var fileExtension = Path.GetExtension(vm.SuratJaminan.FileName).ToLower();

                    if (vm.SuratJaminan.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimal 2MB." });

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG, PNG, atau PDF." });

                    // 🔸 Hapus file lama jika ada
                    if (!string.IsNullOrEmpty(existing.SuratJaminanPath))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.SuratJaminanPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // 🔸 Upload file baru
                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var safeFileName = $"{vm.PasienId}_{safeTime}_SuratJaminan{fileExtension}";

                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SuratJaminan");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    var filePath = Path.Combine(uploadFolder, safeFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.SuratJaminan.CopyToAsync(stream);
                    }

                    suratJaminanPath = $"/SuratJaminan/{safeFileName}";
                }

                // ==================================================
                // ✅ UPDATE DATA BOOKING LAB
                // ==================================================
                existing.KunjunganId = vm.KunjunganId ?? existing.KunjunganId;
                existing.PasienId = vm.PasienId ?? existing.PasienId;
                existing.AsuransiId = vm.AsuransiId ?? existing.AsuransiId;
                existing.TglPenyerahanSampling = vm.TglPenyerahanSampling ?? existing.TglPenyerahanSampling;
                existing.TglBooking = vm.TglBooking ?? existing.TglBooking;
                existing.TglPemeriksaan = vm.TglPemeriksaan ?? existing.TglPemeriksaan;
                existing.KelasId = vm.KelasId ?? existing.KelasId;
                existing.DokterId = vm.DokterId ?? existing.DokterId;
                existing.DiagnosaAwal = vm.DiagnosaAwal ?? existing.DiagnosaAwal;
                existing.Keterangan = vm.Keterangan ?? existing.Keterangan;
                existing.IsCito = vm.IsCito ?? existing.IsCito;
                existing.DokterKonsulenId = vm.DokterKonsulenId ?? existing.DokterKonsulenId;
                existing.TerapisId = vm.TerapisId ?? existing.TerapisId;
                existing.StatusPemeriksaan = vm.StatusPemeriksaan ?? existing.StatusPemeriksaan;
                existing.SuratJaminanPath = suratJaminanPath;

                existing.UpdateBy = userActiveId;
                existing.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabBookings.Update(existing);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Booking Lab Berhasil || 200 OK",
                        data = new
                        {
                            existing.BookingLabId,
                            existing.SuratJaminanPath
                        }
                    });
                }

                return StatusCode(500, new { message = "Tidak ada perubahan yang disimpan." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal update data: {dbEx.InnerException?.Message}" });
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
        public IActionResult Paged(
        int page = 1,
        int perPage = 10,
        Guid? kunjunganid = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from b in _applicationDbContext.LabBookings
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on b.CreateBy equals u.UserActiveId

                         // join ke lab booking detail

                         where b.IsDelete == false || b.IsDelete == null
                         select new
                         {
                             b.CreateDateTime,
                             b.CreateBy,
                             CreateByName = u.FullName,
                             b.BookingLabId,
                             b.KunjunganId,
                             b.PasienId,
                             b.TglPenyerahanSampling,
                             b.TglBooking,
                             b.KelasId,
                             b.DokterId,
                             b.Keterangan,
                             b.IsCito,
                             b.DiagnosaAwal,
                             b.DokterKonsulenId,
                             b.TerapisId
                         });

            // filter berdasarkan kunjunganId
            if (kunjunganid.HasValue)
            {
                query = query.Where(u=> u.KunjunganId == kunjunganid.Value);
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
