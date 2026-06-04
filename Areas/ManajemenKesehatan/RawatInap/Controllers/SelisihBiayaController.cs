using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class SelisihBiayaController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly string _uploadUrl;
        private readonly ILogger<SelisihBiayaController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SelisihBiayaController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SelisihBiayaController> logger,
            ITTDService ttdService,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
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
            var listdata = _applicationDbContext.SelisihBiayas.Find(id);
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
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Create([FromForm] SelisihBiayaViewModel vm)
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
                var sbId = Guid.NewGuid();

                // =====================================================
                // 🔹 Helper upload ke Flask (SESUAI pola PraOperasi)
                // =====================================================
                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix)
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
                    var fileName = $"{sbId}_{prefix}_{safeTime}{ext}";
                    var folderTarget = "SelisihBiaya";
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
                // 🔹 Upload file TTD (PARALEL kalau nanti ada banyak file)
                // =====================================================
                var uploadTTDTask = UploadToFlaskAsync(vm.TTDPenandaTangan, "TTDPenandaTanganSB");
                await Task.WhenAll(uploadTTDTask);

                // cek path ttd
                var ttdPetugas = await _ttdService.CheckTTDAsync((Guid)vm.PetugasId);

                // =====================================================
                // 🔹 Simpan ke DB
                // =====================================================
                var data = new SelisihBiaya
                {
                    SelisihBiayaId = sbId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,

                    NamaPasien = vm.NamaPasien,
                    AlamatPasien = vm.AlamatPasien,
                    NoRM = vm.NoRM,
                    Kelas = vm.Kelas,

                    NamaPenandaTangan = vm.NamaPenandaTangan,
                    AlamatPenandaTangan = vm.AlamatPenandaTangan,
                    PekerjaanPenandaTangan = vm.PekerjaanPenandaTangan,
                    NoPengenalPenandaTangan = vm.NoPengenalPenandaTangan,
                    TipeTandaPengenal = vm.TipeTandaPengenal,
                    PathTTDPenandaTangan = uploadTTDTask.Result,

                    NoHpPenandaTangan = vm.NoHpPenandaTangan,
                    NoTelpKantorPenandaTangan = vm.NoTelpKantorPenandaTangan,

                    HubunganPasien = vm.HubunganPasien,
                    TanggalTTD = vm.TanggalTTD,

                    PetugasId = vm.PetugasId,
                    PathTTDPetugas = ttdPetugas.Path,
                    Keterangan = vm.Keterangan,
                    CreateBy = userId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.SelisihBiayas.Add(data);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new
                {
                    message = "Berhasil tambah Pelunasan Deposit",
                    data.SelisihBiayaId,
                    data.PathTTDPenandaTangan,
                    data.PathTTDPetugas,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Update(Guid id, [FromForm] SelisihBiayaViewModel vm)
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
                var data = await _applicationDbContext.SelisihBiayas
                    .FirstOrDefaultAsync(x => x.SelisihBiayaId == id && (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = "Data Selisih Biaya tidak ditemukan." });

                // =====================================================
                // 🔹 Helper upload ke Flask (pakai id existing untuk nama file)
                // =====================================================
                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix)
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
                    var folderTarget = "SelisihBiaya";
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
                // 🔹 Upload TTD penanda tangan (kalau ada file baru)
                // =====================================================
                string? newPathTtdPenandaTangan = null;
                if (vm.TTDPenandaTangan != null && vm.TTDPenandaTangan.Length > 0)
                {
                    newPathTtdPenandaTangan = await UploadToFlaskAsync(vm.TTDPenandaTangan, "TTDPenandaTanganSB");
                }

                // =====================================================
                // 🔹 Cek path ttd petugas
                // =====================================================
                var ttdPetugas = await _ttdService.CheckTTDAsync((Guid)vm.PetugasId);

                // =====================================================
                // 🔹 Update DB (field-field)
                // =====================================================
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;

                data.NamaPasien = vm.NamaPasien;
                data.AlamatPasien = vm.AlamatPasien;
                data.NoRM = vm.NoRM;
                data.Kelas = vm.Kelas;

                data.NamaPenandaTangan = vm.NamaPenandaTangan;
                data.AlamatPenandaTangan = vm.AlamatPenandaTangan;
                data.PekerjaanPenandaTangan = vm.PekerjaanPenandaTangan;
                data.NoPengenalPenandaTangan = vm.NoPengenalPenandaTangan;
                data.TipeTandaPengenal = vm.TipeTandaPengenal;
                data.PathTTDPenandaTangan = newPathTtdPenandaTangan;

                data.NoHpPenandaTangan = vm.NoHpPenandaTangan;
                data.NoTelpKantorPenandaTangan = vm.NoTelpKantorPenandaTangan;

                data.HubunganPasien = vm.HubunganPasien;
                data.TanggalTTD = vm.TanggalTTD;

                data.PetugasId = vm.PetugasId;
                data.PathTTDPetugas = ttdPetugas.Path;
                data.Keterangan = vm.Keterangan;

                // update path TTD penanda tangan hanya jika upload baru
                if (!string.IsNullOrWhiteSpace(newPathTtdPenandaTangan))
                    data.PathTTDPenandaTangan = newPathTtdPenandaTangan;

                // audit update (sesuaikan kalau field ini ada di entity)
                data.UpdateBy = userId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Berhasil update Selisih Biaya",
                    data.SelisihBiayaId,
                    data.PathTTDPenandaTangan,
                    data.PathTTDPetugas,
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
                var data = await _applicationDbContext.SelisihBiayas.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.SelisihBiayas.Update(data);
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
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            Guid? petugasId = null,
            string? orderBy = "CreatedAt",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ==========================================
                // 1) BASE QUERY (SelisihBiaya)
                // ==========================================
                var baseQuery = _applicationDbContext.SelisihBiayas
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false); // kalau nullable: (x.IsDelete == false || x.IsDelete == null)

                // filter by kunjunganId
                if (kunjunganId.HasValue)
                    baseQuery = baseQuery.Where(x => x.KunjunganId == kunjunganId.Value);

                // filter by pasienId
                if (pasienId.HasValue)
                    baseQuery = baseQuery.Where(x => x.PasienId == pasienId.Value);

                // filter by petugasId
                if (petugasId.HasValue)
                    baseQuery = baseQuery.Where(x => x.PetugasId == petugasId.Value);

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
                        x.SelisihBiayaId,
                        x.KunjunganId,
                        x.PasienId,
                        x.NamaPasien,
                        x.AlamatPasien,
                        x.NoRM,
                        x.Kelas,

                        x.NamaPenandaTangan,
                        x.AlamatPenandaTangan,
                        x.PekerjaanPenandaTangan,
                        x.NoPengenalPenandaTangan,
                        x.TipeTandaPengenal,
                        x.NoHpPenandaTangan,
                        x.NoTelpKantorPenandaTangan,
                        x.HubunganPasien,
                        x.TanggalTTD,

                        x.PetugasId,
                        x.PathTTDPetugas,
                        x.PathTTDPenandaTangan,
                        x.Keterangan,

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
                        EF.Functions.ILike(x.NamaPasien ?? "", pattern)
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
                        "NamaPasien" => query.OrderByDescending(x => x.NamaPasien),
                        _ => query.OrderByDescending(x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreatedAt" => query.OrderBy(x => x.CreateDateTime),
                        "NamaPasien" => query.OrderBy(x => x.NamaPasien),
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
