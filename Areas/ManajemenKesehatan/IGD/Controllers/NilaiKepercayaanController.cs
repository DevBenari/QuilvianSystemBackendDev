using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class NilaiKepercayaanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly string _uploadUrl;
        private readonly ILogger<NilaiKepercayaanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NilaiKepercayaanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<NilaiKepercayaanController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ITTDService ttdService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
            _ttdService = ttdService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // 1. Validasi jika ID yang dikirim kosong
                if (id == Guid.Empty)
                {
                    return BadRequest(new { message = "ID tidak valid." });
                }

                // 2. Cari data di database berdasarkan ID (Primary Key)
                var data = await _applicationDbContext.NilaiKepercayaans
                    .FirstOrDefaultAsync(x => x.NilaiKepercayaanId == id);

                // 3. Cek apakah data ditemukan (Pencegahan NullReferenceException)
                if (data == null)
                {
                    return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
                }

                // 4. Kembalikan data jika ditemukan
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Handle jika terjadi error pada server
                return StatusCode(500, new { message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> Create([FromForm] NilaiKepercayaanViewModel vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // =====================================================
                // 🔹 Cek DB
                // =====================================================
                if (!await _applicationDbContext.Database.CanConnectAsync())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // =====================================================
                // 🔹 Ambil user login
                // =====================================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin && u.IsDelete == false);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userId = user.UserActiveId;

                // =====================================================
                // 🔹 Generate ID lebih awal (dipakai untuk nama file)
                // =====================================================
                var nilaiKepercayaanId = Guid.NewGuid();

                // =====================================================
                // 🔹 Helper upload ke Flask (bisa beda folder)
                // =====================================================
                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0)
                        return null;

                    var allowedExt = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExt.Contains(ext))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    if (file.Length > 5 * 1024 * 1024)
                        throw new Exception($"{prefix} maksimal 5MB.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{nilaiKepercayaanId}_{prefix}_{safeTime}{ext}";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "image/jpeg"
                        : file.ContentType;

                    var fileContent = new StreamContent(ms);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    using var form = new MultipartFormDataContent();
                    form.Add(fileContent, "file", fileName);
                    form.Add(new StringContent(folderTarget), "folderTarget");

                    using var client = new HttpClient();
                    var response = await client.PostAsync(_uploadUrl, form);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke Flask.");

                    return filePath;
                }

                // =====================================================
                // 🔹 Upload 2 file (PARALEL)
                // =====================================================
                var uploadTTDTask = UploadToFlaskAsync(vm.TTDPenandaTangan, "TTD_PenandaTangan", "NilaiKepercayaan");
                var uploadLabelTask = UploadToFlaskAsync(vm.LabelPasien, "Label_Pasien", "LabelPasien");

                await Task.WhenAll(uploadTTDTask, uploadLabelTask);

                // =====================================================
                // 🔹 Generate Urutan & NoRevisi (contoh per-bulan)
                // =====================================================
                var now = DateTimeOffset.UtcNow;
                var startMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var endMonth = startMonth.AddMonths(1).AddTicks(-1);

                var maxUrutanThisMonth = await _applicationDbContext.NilaiKepercayaans
                    .Where(x =>
                        x.CreateDateTime >= startMonth &&
                        x.CreateDateTime <= endMonth &&
                        x.IsDelete == false)
                    .MaxAsync(x => (decimal?)x.Urutan);

                decimal? nextUrutan = (maxUrutanThisMonth ?? 0) + 1;
                decimal? nextNoRevisi = 1;

                // =====================================================
                // 🔹 Simpan ke DB
                // =====================================================
                var data = new NilaiKepercayaan
                {
                    NilaiKepercayaanId = nilaiKepercayaanId,
                    PasienId = vm.PasienId,

                    Urutan = nextUrutan,
                    NoRevisi = nextNoRevisi,

                    TanggalTTD = vm.TanggalTTD,
                    NamaPenandaTangan = vm.NamaPenandaTangan,
                    TanggalLahirPenandaTangan = vm.TanggalLahirPenandaTangan,
                    UmurPenandaTangan = vm.UmurPenandaTangan,
                    GenderPenandaTangan = vm.GenderPenandaTangan,
                    AlamatPenandaTangan = vm.AlamatPenandaTangan,

                    HubDenganPasien = vm.HubDenganPasien,
                    AgamaPasien = vm.AgamaPasien,
                    GenderPasien = vm.GenderPasien,

                    NilaiBertentangan = vm.NilaiBertentangan,

                    // path file
                    TTDPenandaTanganPath = uploadTTDTask.Result,
                    PathLabelPasien = uploadLabelTask.Result,

                    CreateBy = userId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.NilaiKepercayaans.Add(data);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new
                {
                    message = "Berhasil tambah Nilai Kepercayaan",
                    data.NilaiKepercayaanId,
                    data.TTDPenandaTanganPath,
                    data.PathLabelPasien,
                    data.Urutan,
                    data.NoRevisi
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(20_000_000)]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> Update(Guid id, [FromForm] NilaiKepercayaanViewModel vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // =====================================================
                // 🔹 Cek DB
                // =====================================================
                if (!await _applicationDbContext.Database.CanConnectAsync())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // =====================================================
                // 🔹 Ambil user login
                // =====================================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin && u.IsDelete == false);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userId = user.UserActiveId;

                // =====================================================
                // 🔹 Ambil data existing
                // =====================================================
                var data = await _applicationDbContext.NilaiKepercayaans
                    .FirstOrDefaultAsync(x => x.NilaiKepercayaanId == id && x.IsDelete == false);

                if (data == null)
                    return NotFound(new { message = "Data Nilai Kepercayaan tidak ditemukan." });

                // =====================================================
                // 🔹 Helper upload ke Flask (bisa beda folder)
                // =====================================================
                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0)
                        return null;

                    var allowedExt = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExt.Contains(ext))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    if (file.Length > 5 * 1024 * 1024)
                        throw new Exception($"{prefix} maksimal 5MB.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{id}_{prefix}_{safeTime}{ext}";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "image/jpeg"
                        : file.ContentType;

                    var fileContent = new StreamContent(ms);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    using var form = new MultipartFormDataContent();
                    form.Add(fileContent, "file", fileName);
                    form.Add(new StringContent(folderTarget), "folderTarget");

                    using var client = new HttpClient();
                    var response = await client.PostAsync(_uploadUrl, form);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke Flask.");

                    return filePath;
                }

                // =====================================================
                // 🔹 Upload file (hanya jika ada di request)
                // =====================================================
                Task<string?> uploadTTDTask = Task.FromResult<string?>(null);
                Task<string?> uploadLabelTask = Task.FromResult<string?>(null);

                if (vm.TTDPenandaTangan != null && vm.TTDPenandaTangan.Length > 0)
                    uploadTTDTask = UploadToFlaskAsync(vm.TTDPenandaTangan, "TTD_PenandaTangan", "NilaiKepercayaan");

                if (vm.LabelPasien != null && vm.LabelPasien.Length > 0)
                    uploadLabelTask = UploadToFlaskAsync(vm.LabelPasien, "Label_Pasien", "LabelPasien");

                await Task.WhenAll(uploadTTDTask, uploadLabelTask);

                // =====================================================
                // 🔹 Increment NoRevisi saat update
                // =====================================================
                decimal? nextNoRevisi = (data.NoRevisi ?? 0) + 1;

                // =====================================================
                // 🔹 Update Data (Urutan tidak diubah)
                // =====================================================
                data.PasienId = vm.PasienId;

                data.NoRevisi = nextNoRevisi;

                data.TanggalTTD = vm.TanggalTTD;
                data.NamaPenandaTangan = vm.NamaPenandaTangan;
                data.TanggalLahirPenandaTangan = vm.TanggalLahirPenandaTangan;
                data.UmurPenandaTangan = vm.UmurPenandaTangan;
                data.GenderPenandaTangan = vm.GenderPenandaTangan;
                data.AlamatPenandaTangan = vm.AlamatPenandaTangan;

                data.HubDenganPasien = vm.HubDenganPasien;
                data.AgamaPasien = vm.AgamaPasien;
                data.GenderPasien = vm.GenderPasien;

                data.NilaiBertentangan = vm.NilaiBertentangan;

                // ✅ Update path file hanya jika ada upload baru
                var newTTDPath = uploadTTDTask.Result;
                if (!string.IsNullOrWhiteSpace(newTTDPath))
                    data.TTDPenandaTanganPath = newTTDPath;

                var newLabelPath = uploadLabelTask.Result;
                if (!string.IsNullOrWhiteSpace(newLabelPath))
                    data.PathLabelPasien = newLabelPath;

                // metadata update (kalau ada)
                data.UpdateBy = userId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.NilaiKepercayaans.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Berhasil update Nilai Kepercayaan",
                    data.NilaiKepercayaanId,
                    data.TTDPenandaTanganPath,
                    data.PathLabelPasien,
                    data.Urutan,
                    data.NoRevisi
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
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
                var data = await _applicationDbContext.NilaiKepercayaans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.NilaiKepercayaans.Update(data);
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
        public async Task<IActionResult> GetAll(
            int page = 1,
            int perPage = 10,
            string? search = null,
            Guid? pasienId = null,
            string? orderBy = "CreatedAt",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))]
            PeriodeFilter? periode = null
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ==========================================
                // 1) BASE QUERY (NilaiKepercayaan)
                // ==========================================
                var baseQuery = _applicationDbContext.NilaiKepercayaans
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);

                // filter by pasienId
                if (pasienId.HasValue)
                    baseQuery = baseQuery.Where(x => x.PasienId == pasienId.Value);

                // ==========================================
                // 2) FILTER TANGGAL (CreateDateTime)
                // ==========================================
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

                    baseQuery = baseQuery.Where(x =>
                        x.CreateDateTime >= startUtc &&
                        x.CreateDateTime <= endUtc);
                }

                // filter periode
                if (periode.HasValue)
                {
                    var today = DateTime.UtcNow.Date;

                    switch (periode)
                    {
                        case PeriodeFilter.Today:
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Date == today);
                            break;

                        case PeriodeFilter.ThisWeek:
                            var startWeek = today.AddDays(-(int)today.DayOfWeek);
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Date >= startWeek && x.CreateDateTime.Date <= today);
                            break;

                        case PeriodeFilter.LastWeek:
                            var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                            var lastWeekEnd = lastWeekStart.AddDays(6);
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Date >= lastWeekStart && x.CreateDateTime.Date <= lastWeekEnd);
                            break;

                        case PeriodeFilter.ThisMonth:
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Month == today.Month && x.CreateDateTime.Year == today.Year);
                            break;

                        case PeriodeFilter.LastMonth:
                            var lastMonth = today.AddMonths(-1);
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Month == lastMonth.Month && x.CreateDateTime.Year == lastMonth.Year);
                            break;

                        case PeriodeFilter.ThisYear:
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Year == today.Year);
                            break;

                        case PeriodeFilter.LastYear:
                            baseQuery = baseQuery.Where(x => x.CreateDateTime.Year == today.Year - 1);
                            break;

                        case PeriodeFilter.Last3Months:
                            baseQuery = baseQuery.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                            break;

                        case PeriodeFilter.Last6Months:
                            baseQuery = baseQuery.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                            break;
                    }
                }

                // ==========================================
                // 3) JOIN USERACTIVE (CreateByName)
                // ==========================================
                var query =
                    from x in baseQuery
                    join u in _applicationDbContext.UserActives.AsNoTracking().Where(u => u.IsDelete == false)
                        on x.CreateBy equals u.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()
                    select new
                    {
                        x.NilaiKepercayaanId,
                        x.PasienId,
                        x.Urutan,
                        x.NoRevisi,

                        x.TanggalTTD,
                        x.NamaPenandaTangan,
                        x.TanggalLahirPenandaTangan,
                        x.UmurPenandaTangan,
                        x.GenderPenandaTangan,
                        x.AlamatPenandaTangan,

                        x.HubDenganPasien,
                        x.AgamaPasien,
                        x.GenderPasien,

                        x.PathLabelPasien,
                        x.NilaiBertentangan,
                        x.TTDPenandaTanganPath,

                        x.IsDelete,
                        x.CreateDateTime,
                        x.CreateBy,
                        CreateByName = u != null ? u.FullName : null
                    };

                // ==========================================
                // 4) SEARCH (setelah join)
                // ==========================================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NamaPenandaTangan, pattern) ||
                        EF.Functions.ILike(x.HubDenganPasien, pattern) ||
                        EF.Functions.ILike(x.NilaiBertentangan, pattern)
                    );
                }

                // ==========================================
                // 5) SORTING
                // ==========================================
                bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

                query = desc
                    ? orderBy switch
                    {
                        "CreatedAt" => query.OrderByDescending(x => x.CreateDateTime),
                        "NamaPenandaTangan" => query.OrderByDescending(x => x.NamaPenandaTangan),
                        "Urutan" => query.OrderByDescending(x => x.Urutan),
                        _ => query.OrderByDescending(x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreatedAt" => query.OrderBy(x => x.CreateDateTime),
                        "NamaPenandaTangan" => query.OrderBy(x => x.NamaPenandaTangan),
                        "Urutan" => query.OrderBy(x => x.Urutan),
                        _ => query.OrderBy(x => x.CreateDateTime)
                    };

                // ==========================================
                // 6) PAGING
                // ==========================================
                var totalRows = await query.CountAsync();
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
                    return NotFound(new { message = "Page not found." });

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

    }
}
