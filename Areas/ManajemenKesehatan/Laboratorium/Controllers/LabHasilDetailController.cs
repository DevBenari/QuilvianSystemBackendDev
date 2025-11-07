using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class LabHasilDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _uploadUrl;

        private readonly ILogger<LabHasilDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabHasilDetailController> logger,
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

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.LabHasilDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailHasilLabId,
                             a.HasilLabId,
                             a.PemeriksaanLabId,
                             a.KelasId,
                             a.TanggalSelesai,
                             a.NoPhotoLab,
                             a.PhotoLabPath,
                             a.HasilLabManual,
                             a.HasilLabAI,
                             a.JumlahFilm,
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
            var listdata = _applicationDbContext.LabHasilDetails.Find(id);
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
        public async Task<IActionResult> Create([FromForm] LabHasilDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ==============================
                // 🔐 Ambil User Aktif dari JWT
                // ==============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // =============================
                // ✅ Ambil prefix dari tabel MstLab lewat join ke HasilLab
                // =============================
                var labData = await (from hl in _applicationDbContext.LabHasils
                                     join ml in _applicationDbContext.Labs on hl.LabId equals ml.LabId
                                     where hl.HasilLabId == vm.HasilLabId
                                     select new { ml.KodeKategori }).FirstOrDefaultAsync();

                if (labData == null)
                    return BadRequest(new { message = "Data lab tidak ditemukan atau tidak valid!" });

                string prefix = labData.KodeKategori ?? "LAB";

                // =============================
                // ✅ Generate NoPhotoLab unik per jenis lab dan tanggal
                // =============================
                var today = DateTime.UtcNow.ToString("yyMMdd");

                int urutan = _applicationDbContext.LabHasilDetails
                    .Where(a => a.NoPhotoLab.StartsWith(prefix + today))
                    .Count() + 1;

                string noPhotoLab = $"{prefix}{today}{urutan:0000}";


                // ================================================
                // ✅ Upload File PhotoLab ke Flask
                // ================================================
                string photoPath = "";
                if (vm.PhotoLab != null && vm.PhotoLab.Length > 0)
                {
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var maxSize = 5 * 1024 * 1024; // 5 MB
                    var ext = Path.GetExtension(vm.PhotoLab.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                        return BadRequest(new { message = "Format foto tidak valid. Gunakan JPG/PNG." });

                    if (vm.PhotoLab.Length > maxSize)
                        return BadRequest(new { message = "Ukuran foto terlalu besar! Maks 5MB." });

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{noPhotoLab}_{safeTime}{ext}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.PhotoLab.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                        { new StreamContent(ms)
                            { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.PhotoLab.ContentType) } },
                            "file", fileName },
                        { new StringContent("HasilLabPhoto"), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync(_uploadUrl, content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        return StatusCode(500, new { message = "Gagal upload foto hasil lab ke server Flask." });

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    photoPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? "";
                }

                // ================================================
                // ✅ Simpan ke Database
                // ================================================
                var data = new LabHasilDetail
                {
                    DetailHasilLabId = Guid.NewGuid(),
                    HasilLabId = vm.HasilLabId,
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    KelasId = vm.KelasId,
                    TanggalSelesai = vm.TanggalSelesai ?? DateTime.UtcNow,
                    NoPhotoLab = noPhotoLab,
                    PhotoLabPath = photoPath,
                    HasilLabManual = vm.HasilLabManual,
                    HasilLabAI = vm.HasilLabAI,
                    JumlahFilm = vm.JumlahFilm,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTime.UtcNow,
                };

                _applicationDbContext.LabHasilDetails.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created", data });

                return StatusCode(500, new { message = "Gagal menyimpan data ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menambahkan DetailHasilLab");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] LabHasilDetailViewModel vm)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Parameter ID tidak valid." });

            try
            {
                // ==================================================
                // 🔐 Ambil user aktif dari JWT
                // ==================================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                // ==================================================
                // 🔎 Cari data lama
                // ==================================================
                var data = await _applicationDbContext.LabHasilDetails
                    .FirstOrDefaultAsync(a => a.DetailHasilLabId == id && !a.IsDelete);

                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });

                // ==================================================
                // ✅ Ambil prefix dari tabel MstLab lewat join ke HasilLab
                // ==================================================
                var labData = await (from hl in _applicationDbContext.LabHasils
                                     join ml in _applicationDbContext.Labs on hl.LabId equals ml.LabId
                                     where hl.HasilLabId == vm.HasilLabId
                                     select new { ml.KodeKategori }).FirstOrDefaultAsync();

                if (labData == null)
                    return BadRequest(new { message = "Data lab tidak ditemukan atau tidak valid!" });

                string prefix = labData.KodeKategori ?? "LAB";

                // ==================================================
                // ✅ Upload foto baru (jika ada)
                // ==================================================
                string photoPath = data.PhotoLabPath; // default pakai yang lama

                if (vm.PhotoLab != null && vm.PhotoLab.Length > 0)
                {
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var maxSize = 5 * 1024 * 1024; // 5 MB
                    var ext = Path.GetExtension(vm.PhotoLab.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                        return BadRequest(new { message = "Format foto tidak valid. Gunakan JPG/PNG." });

                    if (vm.PhotoLab.Length > maxSize)
                        return BadRequest(new { message = "Ukuran foto terlalu besar! Maks 5MB." });

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{data.NoPhotoLab}_{safeTime}{ext}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.PhotoLab.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
            {
                { new StreamContent(ms)
                    { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.PhotoLab.ContentType) } },
                    "file", fileName },
                { new StringContent("HasilLabPhoto"), "folderTarget" }
            };

                    var flaskResponse = await client.PostAsync(_uploadUrl, content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        return StatusCode(500, new { message = "Gagal upload foto hasil lab ke server Flask." });

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    photoPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? "";
                }

                // ==================================================
                // ✅ Update nilai field
                // ==================================================
                data.HasilLabId = vm.HasilLabId;
                data.PemeriksaanLabId = vm.PemeriksaanLabId;
                data.KelasId = vm.KelasId;
                data.TanggalSelesai = vm.TanggalSelesai ?? data.TanggalSelesai;
                data.PhotoLabPath = photoPath;
                data.HasilLabManual = vm.HasilLabManual;
                data.HasilLabAI = vm.HasilLabAI;
                data.JumlahFilm = vm.JumlahFilm;
                data.Keterangan = vm.Keterangan;
                data.UpdateBy = getUserActive.UserActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.LabHasilDetails.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Data berhasil diperbarui. || 200 OK",
                        data = new
                        {
                            data.DetailHasilLabId,
                            data.NoPhotoLab,
                            data.PhotoLabPath,
                            data.HasilLabManual,
                            data.HasilLabAI,
                            data.JumlahFilm,
                            data.Keterangan,
                            data.UpdateDateTime,
                        }
                    });
                }

                return StatusCode(500, new { message = "Gagal menyimpan perubahan ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat memperbarui DetailHasilLab");
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
                var data = await _applicationDbContext.LabHasilDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabHasilDetails.Update(data);
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
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.LabHasilDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailHasilLabId,
                             a.HasilLabId,
                             a.PemeriksaanLabId,
                             a.KelasId,
                             a.TanggalSelesai,
                             a.NoPhotoLab,
                             a.PhotoLabPath,
                             a.HasilLabManual,
                             a.HasilLabAI,
                             a.JumlahFilm,
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
