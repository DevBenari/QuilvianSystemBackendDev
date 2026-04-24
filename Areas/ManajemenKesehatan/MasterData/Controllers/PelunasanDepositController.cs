using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PelunasanDepositController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<PelunasanDepositController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadUrl;

        public PelunasanDepositController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITTDService ttdService,
            ILogger<PelunasanDepositController> logger,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment

        )
        {
            _applicationDbContext = context;
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
                var result = await (
                    from x in _applicationDbContext.PelunasanDeposits.AsNoTracking()
                    where x.PelunasanDepositId == id
                          && x.IsDelete == false

                    join u in _applicationDbContext.UserActives.AsNoTracking().Where(u => u.IsDelete == false)
                        on x.CreateBy equals u.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()

                    select new
                    {
                        x.PelunasanDepositId,
                        x.KunjunganId,
                        x.PasienId,
                        x.Urutan,
                        x.NoRevisi,
                        x.TanggalTTD,
                        x.NamaPenandaTangan,
                        x.AlamatPenandaTangan,
                        x.TelpPenandaTangan,
                        x.TanggalJatuhTempo,
                        x.TTDPenandaTanganPath,
                        x.PetugasId,
                        x.PathTTDPetugas,
                        x.Keterangan,
                        x.CreateBy,
                        x.IsDelete,
                        x.CreateDateTime,
                        x.UpdateDateTime,
                        x.DeleteDateTime,

                        // ✅ nama pembuat dari UserActive
                        CreateByName = u != null ? u.FullName : null
                    }
                ).FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound(new { message = "Data Pelunasan Deposit tidak ditemukan." });
                }

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Create([FromForm] PelunasanDepositViewModel vm)
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
                var pelunasanDepositId = Guid.NewGuid();

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
                    var fileName = $"{pelunasanDepositId}_{prefix}_{safeTime}{ext}";
                    var folderTarget = "PelunasanDeposit";
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
                var uploadTTDTask = UploadToFlaskAsync(vm.TTDPenandaTangan, "TTDPenandaTangan");
                await Task.WhenAll(uploadTTDTask);

                // =====================================================
                // 🔹 Generate Urutan & NoRevisi
                // =====================================================
                // Urutan: auto increment dari 001 berdasarkan awal bulan
                // NoRevisi: auto increment dari 01 tiap edit (untuk create default = 1)
                var now = DateTimeOffset.UtcNow;

                var startMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var endMonth = startMonth.AddMonths(1).AddTicks(-1);

                var maxUrutanThisMonth = await _applicationDbContext.PelunasanDeposits
                    .Where(x =>
                        x.CreateDateTime >= startMonth &&
                        x.CreateDateTime <= endMonth &&
                        x.IsDelete == false)
                    .MaxAsync(x => (decimal?)x.Urutan);   // <-- Max bisa handle NULL

                decimal? nextUrutan = (maxUrutanThisMonth ?? 0) + 1;

                
                var nextNoRevisi = 0;

                // cek path ttd
                var ttdPetugas = await _ttdService.CheckTTDAsync((Guid)vm.PetugasId);

                // =====================================================
                // 🔹 Simpan ke DB
                // =====================================================
                var data = new PelunasanDeposit
                {
                    PelunasanDepositId = pelunasanDepositId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,

                    Urutan = nextUrutan,
                    NoRevisi = nextNoRevisi,

                    TanggalTTD = vm.TanggalTTD,
                    NamaPenandaTangan = vm.NamaPenandaTangan,
                    AlamatPenandaTangan = vm.AlamatPenandaTangan,
                    TelpPenandaTangan = vm.TelpPenandaTangan,
                    TanggalJatuhTempo = vm.TanggalJatuhTempo,

                    TTDPenandaTanganPath = uploadTTDTask.Result,

                    PetugasId = vm.PetugasId,
                    PathTTDPetugas = ttdPetugas.Path,
                    Keterangan = vm.Keterangan,
                    CreateBy = userId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.PelunasanDeposits.Add(data);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new
                {
                    message = "Berhasil tambah Pelunasan Deposit",
                    data.PelunasanDepositId,
                    data.TTDPenandaTanganPath,
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
        public async Task<IActionResult> Update(Guid id, [FromForm] PelunasanDepositViewModel vm)
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
                var data = await _applicationDbContext.PelunasanDeposits
                    .FirstOrDefaultAsync(x => x.PelunasanDepositId == id && x.IsDelete == false);

                if (data == null)
                    return NotFound(new { message = "Data Pelunasan Deposit tidak ditemukan." });

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
                    var fileName = $"{data.PelunasanDepositId}_{prefix}_{safeTime}{ext}";
                    var folderTarget = "PelunasanDeposit";
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
                // 🔹 Upload file TTD jika ada (kalau tidak ada pakai path lama)
                // =====================================================
                string? newTTDPath = null;

                if (vm.TTDPenandaTangan != null && vm.TTDPenandaTangan.Length > 0)
                {
                    newTTDPath = await UploadToFlaskAsync(vm.TTDPenandaTangan, "TTDPenandaTangan");
                }

                // =====================================================
                // 🔹 Increment NoRevisi (update = revisi +1)
                // =====================================================
                // jika NoRevisi null, anggap 0
                var currentNoRevisi = data.NoRevisi ?? 0;
                var nextNoRevisi = currentNoRevisi + 1;

                // =====================================================
                // 🔹 cek path ttd petugas (jika PetugasId diupdate)
                // =====================================================
                var ttdPetugas = await _ttdService.CheckTTDAsync((Guid)vm.PetugasId);

                // =====================================================
                // 🔹 Update data (Urutan tidak diubah)
                // =====================================================
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;

                data.TanggalTTD = vm.TanggalTTD;
                data.NamaPenandaTangan = vm.NamaPenandaTangan;
                data.AlamatPenandaTangan = vm.AlamatPenandaTangan;
                data.TelpPenandaTangan = vm.TelpPenandaTangan;
                data.TanggalJatuhTempo = vm.TanggalJatuhTempo;

                // jika upload baru, update path
                if (!string.IsNullOrEmpty(newTTDPath))
                    data.TTDPenandaTanganPath = newTTDPath;

                data.PetugasId = vm.PetugasId;
                data.PathTTDPetugas = ttdPetugas.Path;

                data.Keterangan = vm.Keterangan;

                data.NoRevisi = nextNoRevisi;

                // OPTIONAL: kalau kamu punya UpdateBy / UpdateDateTime
                data.UpdateBy = userId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // =====================================================
                // 🔹 Save DB
                // =====================================================
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Berhasil update Pelunasan Deposit",
                    data.PelunasanDepositId,
                    data.TTDPenandaTanganPath,
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
                var data = await _applicationDbContext.PelunasanDeposits.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PelunasanDeposits.Update(data);
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
            [FromQuery, JsonConverter(typeof(StringEnumConverter))]
            PeriodeFilter? periode = null
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ==========================================
                // 1) BASE QUERY (PelunasanDeposit)
                // ==========================================
                var baseQuery = _applicationDbContext.PelunasanDeposits
                    .AsNoTracking()
                    .Where(x => x.IsDelete == false);


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
                // 2) FILTER TANGGAL (CreatedAt)
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
                    join u in _applicationDbContext.UserActives.AsNoTracking().Where(x => x.IsDelete == false)
                    on x.CreateBy equals u.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()
                    select new
                    {
                        x.PelunasanDepositId,
                        x.KunjunganId,
                        x.PasienId,
                        x.Urutan,
                        x.NoRevisi,
                        x.TanggalTTD,
                        x.NamaPenandaTangan,
                        x.AlamatPenandaTangan,
                        x.TelpPenandaTangan,
                        x.TanggalJatuhTempo,
                        x.TTDPenandaTanganPath,
                        x.PetugasId,
                        x.PathTTDPetugas,
                        x.Keterangan,
                        x.IsDelete,
                        x.CreateDateTime,
                        CreateByName = u != null ? u.FullName : null,
                        x.CreateBy,
                    };

                // ==========================================
                // 4) SEARCH (setelah join)
                // ==========================================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.ToLower()}%";

                    query = query.Where(x =>
                        EF.Functions.ILike(x.NamaPenandaTangan, pattern)
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
                        _ => query.OrderByDescending(x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreatedAt" => query.OrderBy(x => x.CreateDateTime),
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
                {
                    return NotFound(new { message = "Page not found." });
                }

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
