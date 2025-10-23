using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class AssessmentEdukasiDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AssessmentEdukasiDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadUrl;

        public AssessmentEdukasiDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AssessmentEdukasiDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
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
            var query = (from a in _applicationDbContext.AssesmentEdukasiDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         // join ke tabel mst topik edukasi
                         join te in _applicationDbContext.TopikEdukasis
                         on a.TopikEdukasiId equals te.TopikEdukasiId into teGroup
                         from te in teGroup.DefaultIfEmpty()

                         // join ke tabel assesment edukasi
                         join ae in _applicationDbContext.AssesmentEdukasis
                         on a.AsesmenEdukasiId equals ae.AsesmenEdukasiId into aeGroup
                         from ae in aeGroup.DefaultIfEmpty()

                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailAsesmenEdukasiId,
                             a.AsesmenEdukasiId,
                             a.TopikEdukasiId,
                             ae.KunjunganId,
                             te.NamaTopik,
                             a.TglDetailAsesmenEdukasi,
                             a.DurasiWaktu,
                             a.TTDWaliId,
                             a.NamaWali,
                             a.TTDWaliPath,
                             a.TingkatPemahaman,
                             a.MetodeEdukasi,
                             a.SaranaEdukasi,
                             a.TTDPerawatId,
                             a.TTDPerawatPath,
                             a.EvaluasiEdukasi,
                             a.TglEvaluasiEdukasi,
                             a.Keterangan,
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
            var listdata = _applicationDbContext.AssesmentEdukasiDetails.Find(id);
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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)] // Maksimal 50 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> Create([FromForm] AssessmentEdukasiDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ===============================
                // 🔹 Cek koneksi database
                // ===============================
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ===============================
                // 🔹 Ambil user dari JWT
                // ===============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userId = user.UserActiveId;

                // ===============================
                // 🔹 Helper untuk upload file JPEG
                // ===============================
                async Task<(string? fileUrl, Guid? ttdId)> UploadFileAsync(IFormFile? file, string prefix, string folderTarget, bool saveToTTD = false)
                {
                    if (file == null) return (null, null);

                    var maxSize = 2 * 1024 * 1024; // 2 MB
                    var allowedExtensions = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 2MB.");

                    if (!allowedExtensions.Contains(ext))
                        throw new Exception($"{prefix} harus berupa file JPG atau JPEG.");

                    var safeTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{user.FullName}_{safeTime}{ext}";

                    using var client = new HttpClient();
                    await using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    using var content = new MultipartFormDataContent
            {
                { new StreamContent(ms) { Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) } }, "file", fileName },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var response = await client.PostAsync(_uploadUrl, content);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var body = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(body);
                    string fileUrl = json.fileUrl;

                    Guid? ttdId = null;
                    if (saveToTTD)
                    {
                        var newTTD = new MasterTTD
                        {
                            TTDId = Guid.NewGuid(),
                            UserActiveId = userId,
                            TTDPath = fileUrl,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };
                        _applicationDbContext.MasterTTDs.Add(newTTD);
                        await _applicationDbContext.SaveChangesAsync();
                        ttdId = newTTD.TTDId;
                    }

                    return (fileUrl, ttdId);
                }

                // ===============================
                // 🔹 Upload dua file tanda tangan
                // ===============================
                var (ttdWaliPath, ttdWaliId) = await UploadFileAsync(vm.TTDWali, "TTDWali", "TTDUser", saveToTTD: true);
                var (ttdPerawatPath, ttdPerawatId) = await UploadFileAsync(vm.TTDPerawat, "TTDPerawat", "TTDUser", saveToTTD: true);

                // ===============================
                // 🔹 Simpan ke tabel DetailAsesmenEdukasi
                // ===============================
                var data = new AssesmentEdukasiDetail
                {
                    DetailAsesmenEdukasiId = Guid.NewGuid(),
                    AsesmenEdukasiId = vm.AsesmenEdukasiId,
                    TopikEdukasiId = vm.TopikEdukasiId,
                    TglDetailAsesmenEdukasi = vm.TglDetailAsesmenEdukasi,
                    DurasiWaktu = vm.DurasiWaktu,
                    NamaWali = vm.NamaWali,
                    TingkatPemahaman = vm.TingkatPemahaman,
                    MetodeEdukasi = vm.MetodeEdukasi,
                    SaranaEdukasi = vm.SaranaEdukasi,
                    EvaluasiEdukasi = vm.EvaluasiEdukasi,
                    Keterangan = vm.Keterangan,
                    TglEvaluasiEdukasi = vm.TglEvaluasiEdukasi,

                    // hasil upload
                    TTDWaliPath = ttdWaliPath,
                    TTDWaliId = ttdWaliId,
                    TTDPerawatPath = ttdPerawatPath,
                    TTDPerawatId = ttdPerawatId,

                    CreateBy = userId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.AssesmentEdukasiDetails.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", 
                        new 
                        { 
                            message = "Berhasil menambahkan Detail Asesmen Edukasi", data.DetailAsesmenEdukasiId 
                        });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> Update(Guid id, [FromForm] AssessmentEdukasiDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ===============================
                // 🔹 Pastikan koneksi database aktif
                // ===============================
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ===============================
                // 🔹 Ambil user dari JWT
                // ===============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userId = user.UserActiveId;

                // ===============================
                // 🔹 Cari data yang akan diupdate
                // ===============================
                var existingData = await _applicationDbContext.AssesmentEdukasiDetails
                    .FirstOrDefaultAsync(x => x.DetailAsesmenEdukasiId == id && (x.IsDelete == false || x.IsDelete == null));

                if (existingData == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // ===============================
                // 🔹 Helper untuk upload file JPEG
                // ===============================
                async Task<(string? fileUrl, Guid? ttdId)> UploadFileAsync(IFormFile? file, string prefix, string folderTarget, bool saveToTTD = false)
                {
                    if (file == null) return (null, null);

                    var maxSize = 2 * 1024 * 1024; // 2MB
                    var allowedExtensions = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 2MB.");

                    if (!allowedExtensions.Contains(ext))
                        throw new Exception($"{prefix} harus berupa file JPG atau JPEG.");

                    var safeTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{user.FullName}_{safeTime}{ext}";

                    using var client = new HttpClient();
                    await using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    using var content = new MultipartFormDataContent
            {
                { new StreamContent(ms) { Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) } }, "file", fileName },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var response = await client.PostAsync(_uploadUrl, content);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var body = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(body);
                    string fileUrl = json.fileUrl;

                    Guid? ttdId = null;
                    if (saveToTTD)
                    {
                        var newTTD = new MasterTTD
                        {
                            TTDId = Guid.NewGuid(),
                            UserActiveId = userId,
                            TTDPath = fileUrl,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };
                        _applicationDbContext.MasterTTDs.Add(newTTD);
                        await _applicationDbContext.SaveChangesAsync();
                        ttdId = newTTD.TTDId;
                    }

                    return (fileUrl, ttdId);
                }

                // ===============================
                // 🔹 Upload file baru (jika dikirim)
                // ===============================
                var (ttdWaliPath, ttdWaliId) = await UploadFileAsync(vm.TTDWali, "TTDWali", "TTDUser", saveToTTD: true);
                var (ttdPerawatPath, ttdPerawatId) = await UploadFileAsync(vm.TTDPerawat, "TTDPerawat", "TTDUser", saveToTTD: true);

                // ===============================
                // 🔹 Update field satu per satu
                // ===============================
                existingData.AsesmenEdukasiId = vm.AsesmenEdukasiId ?? existingData.AsesmenEdukasiId;
                existingData.TopikEdukasiId = vm.TopikEdukasiId ?? existingData.TopikEdukasiId;
                existingData.TglDetailAsesmenEdukasi = vm.TglDetailAsesmenEdukasi ?? existingData.TglDetailAsesmenEdukasi;
                existingData.DurasiWaktu = vm.DurasiWaktu ?? existingData.DurasiWaktu;
                existingData.NamaWali = vm.NamaWali ?? existingData.NamaWali;
                existingData.TingkatPemahaman = vm.TingkatPemahaman ?? existingData.TingkatPemahaman;
                existingData.MetodeEdukasi = vm.MetodeEdukasi ?? existingData.MetodeEdukasi;
                existingData.SaranaEdukasi = vm.SaranaEdukasi ?? existingData.SaranaEdukasi;
                existingData.EvaluasiEdukasi = vm.EvaluasiEdukasi ?? existingData.EvaluasiEdukasi;
                existingData.Keterangan = vm.Keterangan ?? existingData.Keterangan;
                existingData.TglEvaluasiEdukasi = vm.TglEvaluasiEdukasi ?? existingData.TglEvaluasiEdukasi;

                // 🔹 Jika ada file baru, update path-nya
                if (!string.IsNullOrEmpty(ttdWaliPath))
                {
                    existingData.TTDWaliPath = ttdWaliPath;
                    existingData.TTDWaliId = ttdWaliId ?? existingData.TTDWaliId;

                }

                if (!string.IsNullOrEmpty(ttdPerawatPath))
                {
                    existingData.TTDPerawatPath = ttdPerawatPath;
                    existingData.TTDPerawatId = ttdPerawatId ?? existingData.TTDPerawatId;
                }

                // 🔹 Audit info
                existingData.UpdateBy = userId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                // ===============================
                // 🔹 Simpan perubahan ke database
                // ===============================
                _applicationDbContext.AssesmentEdukasiDetails.Update(existingData);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Data berhasil diperbarui || 200 OK", existingData.DetailAsesmenEdukasiId });

                return StatusCode(500, new { message = "Gagal memperbarui data di database." });
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
                var data = await _applicationDbContext.AssesmentEdukasiDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.AssesmentEdukasiDetails.Update(data);
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
            Guid? kunjunganId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.AssesmentEdukasiDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         // join ke tabel mst topik edukasi
                         join te in _applicationDbContext.TopikEdukasis
                         on a.TopikEdukasiId equals te.TopikEdukasiId into teGroup
                         from te in teGroup.DefaultIfEmpty()

                         // join ke tabel assesment edukasi
                         join ae in _applicationDbContext.AssesmentEdukasis
                         on a.AsesmenEdukasiId equals ae.AsesmenEdukasiId into aeGroup
                         from ae in aeGroup.DefaultIfEmpty()

                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailAsesmenEdukasiId,
                             a.AsesmenEdukasiId,
                             a.TopikEdukasiId,
                             ae.KunjunganId,
                             te.NamaTopik,
                             a.TglDetailAsesmenEdukasi,
                             a.DurasiWaktu,
                             a.TTDWaliId,
                             a.NamaWali,
                             a.TTDWaliPath,
                             a.TingkatPemahaman,
                             a.MetodeEdukasi,
                             a.SaranaEdukasi,
                             a.TTDPerawatId,
                             a.TTDPerawatPath,
                             a.EvaluasiEdukasi,
                             a.TglEvaluasiEdukasi,
                             a.Keterangan,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter berdasarkan kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u => u.KunjunganId == kunjunganId.Value);
            }

            //// filter berdasarkan pasien id
            //if (pasienId.HasValue)
            //{
            //    query = query.Where(u => u.PasienId == pasienId.Value);
            //}
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
