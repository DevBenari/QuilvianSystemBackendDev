using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DokumenPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _uploadUrl;
        private readonly ILogger<DokumenPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DokumenPasienController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DokumenPasienController> logger,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.DokumenPasiens.Find(id);
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
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> UploadDokumenPasien([FromForm] DokumenPasienViewModel vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Request tidak valid." });

            if (vm.PasienId == null || vm.PasienId == Guid.Empty)
                return BadRequest(new { message = "PasienId wajib diisi." });

            if (string.IsNullOrWhiteSpace(vm.JenisDokumen))
                return BadRequest(new { message = "Jenis dokumen wajib diisi." });

            if (vm.Dokumen == null || vm.Dokumen.Length == 0)
                return BadRequest(new { message = "File dokumen tidak valid." });

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Validasi pasien
                var pasien = await _applicationDbContext.PendaftaranPasienBarus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PendaftaranPasienBaruId == vm.PasienId);

                if (pasien == null)
                    return NotFound(new { message = "Data pasien tidak ditemukan." });

                string fileName = string.Empty;

                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix)
                {
                    if (file == null || file.Length == 0)
                        return null;

                    var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!allowedExt.Contains(ext))
                        throw new Exception($"{prefix} harus berupa PDF/JPG/JPEG/PNG.");

                    if (file.Length > 20_000_000)
                        throw new Exception($"{prefix} maksimal 20MB.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");

                    var safeJenisDokumen = Regex.Replace(
                        vm.JenisDokumen.Trim(),
                        @"[^a-zA-Z0-9_-]",
                        "_"
                    );

                    fileName = $"{vm.PasienId}_{safeJenisDokumen}_{safeTime}{ext}";

                    var folderTarget = "DokumenPasien";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? GetContentTypeFromExtension(ext)
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

                static string GetContentTypeFromExtension(string ext)
                {
                    return ext switch
                    {
                        ".pdf" => "application/pdf",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    };
                }

                // ✅ Upload file ke Flask
                var path = await UploadToFlaskAsync(vm.Dokumen, "DokumenPasien");

                if (string.IsNullOrEmpty(path))
                    return StatusCode(500, new { message = "Dokumen gagal diupload." });

                // ✅ Simpan ke tabel DokumenPasien
                var data = new DokumenPasien
                {
                    DokumenPasienId = Guid.NewGuid(),
                    PasienId = vm.PasienId,
                    JenisDokumen = vm.JenisDokumen,
                    PathDokumen = path,
                    Keterangan = vm.Keterangan,

                    // hapus kalau field ini tidak ada di entity kamu
                    CreateBy = userActiveId,
                    CreateDateTime = DateTime.Now,
                    IsDelete = false
                };

                _applicationDbContext.DokumenPasiens.Add(data);
                var result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Dokumen pasien berhasil diupload.",
                        dokumenPasienId = data.DokumenPasienId,
                        pasienId = data.PasienId,
                    });
                }

                return StatusCode(500, new { message = "Dokumen pasien gagal disimpan." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> UploadDokumenPasien(Guid id, [FromForm] DokumenPasienViewModel vm)
        {
            if (vm == null)
                return BadRequest(new { message = "Request tidak valid." });

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Ambil data dokumen pasien lama
                var data = await _applicationDbContext.DokumenPasiens
                    .FirstOrDefaultAsync(x => x.DokumenPasienId == id && (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = "Data dokumen pasien tidak ditemukan." });

                // ✅ Validasi pasien jika PasienId dikirim
                if (vm.PasienId.HasValue && vm.PasienId != Guid.Empty)
                {
                    var pasien = await _applicationDbContext.PendaftaranPasienBarus
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.PendaftaranPasienBaruId == vm.PasienId);

                    if (pasien == null)
                        return NotFound(new { message = "Data pasien tidak ditemukan." });

                    data.PasienId = vm.PasienId;
                }

                if (!string.IsNullOrWhiteSpace(vm.JenisDokumen))
                    data.JenisDokumen = vm.JenisDokumen;

                if (vm.Keterangan != null)
                    data.Keterangan = vm.Keterangan;

                string? newPath = null;
                string? newFileName = null;

                async Task<(string filePath, string fileName)> UploadToFlaskAsync(IFormFile file, string prefix)
                {
                    var allowedExt = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!allowedExt.Contains(ext))
                        throw new Exception($"{prefix} harus berupa PDF, JPG, JPEG, atau PNG.");

                    if (file.Length > 20_000_000)
                        throw new Exception($"{prefix} maksimal 20MB.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");

                    var safeJenisDokumen = string.IsNullOrWhiteSpace(vm.JenisDokumen)
                        ? "Dokumen"
                        : System.Text.RegularExpressions.Regex.Replace(vm.JenisDokumen.Trim(), @"[^a-zA-Z0-9_-]", "_");

                    var generatedFileName = $"{data.PasienId}_{safeJenisDokumen}_{safeTime}{ext}";
                    var folderTarget = "DokumenPasien";
                    var filePath = $"/{folderTarget}/{generatedFileName}";

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? GetContentTypeFromExtension(ext)
                        : file.ContentType;

                    var fileContent = new StreamContent(ms);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    using var form = new MultipartFormDataContent();
                    form.Add(fileContent, "file", generatedFileName);
                    form.Add(new StringContent(folderTarget), "folderTarget");

                    using var client = new HttpClient();
                    var response = await client.PostAsync(_uploadUrl, form);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke Flask.");

                    return (filePath, generatedFileName);
                }

                async Task DeleteOldFileFromFlaskAsync(string? oldPath)
                {
                    if (string.IsNullOrWhiteSpace(oldPath))
                        return;

                    // contoh: /DokumenPasien/namafile.pdf
                    using var client = new HttpClient();

                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(oldPath), "filePath");

                    // ganti endpoint ini sesuai endpoint delete di Flask kamu
                    var deleteUrl = $"{_uploadUrl}/delete-file";
                    var response = await client.PostAsync(deleteUrl, form);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("File lama gagal dihapus dari storage.");
                }

                static string GetContentTypeFromExtension(string ext)
                {
                    return ext switch
                    {
                        ".pdf" => "application/pdf",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    };
                }

                // ✅ Jika ada file baru, upload baru lalu hapus file lama
                if (vm.Dokumen != null && vm.Dokumen.Length > 0)
                {
                    var oldPath = data.PathDokumen;

                    var uploadResult = await UploadToFlaskAsync(vm.Dokumen, "DokumenPasien");
                    newPath = uploadResult.filePath;
                    newFileName = uploadResult.fileName;

                    //// hapus file lama setelah file baru sukses diupload
                    if (!string.IsNullOrWhiteSpace(oldPath))
                    {
                        await DeleteOldFileFromFlaskAsync(oldPath);
                    }

                    data.PathDokumen = newPath;
                }

                // opsional: field audit update
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.Now;

                _applicationDbContext.DokumenPasiens.Update(data);
                var result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Dokumen pasien berhasil diperbarui.",
                        dokumenPasienId = data.DokumenPasienId,
                        pasienId = data.PasienId,
                        jenisDokumen = data.JenisDokumen,
                        keterangan = data.Keterangan
                    });
                }

                return StatusCode(500, new { message = "Dokumen pasien gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDokumenPasien(Guid id)
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user login
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Ambil data dokumen pasien
                var data = await _applicationDbContext.DokumenPasiens
                    .FirstOrDefaultAsync(x => x.DokumenPasienId == id && (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = "Data dokumen pasien tidak ditemukan." });

                // ✅ Hapus file fisik dari storage/flask
                async Task DeleteFileFromFlaskAsync(string? filePath)
                {
                    if (string.IsNullOrWhiteSpace(filePath))
                        return;

                    using var client = new HttpClient();
                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(filePath), "filePath");

                    // sesuaikan dengan endpoint hapus file di Flask kamu
                    var deleteUrl = $"{_uploadUrl}/delete-file";

                    var response = await client.PostAsync(deleteUrl, form);

                    // kalau file tidak ditemukan di storage, boleh lanjut soft delete db
                    if (!response.IsSuccessStatusCode &&
                        response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        throw new Exception("Gagal menghapus file dokumen dari storage.");
                    }
                }

                await DeleteFileFromFlaskAsync(data.PathDokumen);

                // ✅ Soft delete data
                data.IsDelete = true;

                // opsional, hapus kalau field ini tidak ada
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.Now;

                _applicationDbContext.DokumenPasiens.Update(data);
                var result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Dokumen pasien berhasil dihapus.",
                        dokumenPasienId = data.DokumenPasienId,
                        pasienId = data.PasienId
                    });
                }

                return StatusCode(500, new { message = "Dokumen pasien gagal dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.DokumenPasiens.AsNoTracking()
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                         on a.PasienId equals p.PendaftaranPasienBaruId into pG
                         from p in pG.DefaultIfEmpty()
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DokumenPasienId,
                             a.PasienId,
                             NamaPasien = p != null ? p.NamaLengkap : null,
                             NoRekamMedis = p!= null ? p.NoRekamMedis : null,
                             a.JenisDokumen,
                             a.PathDokumen,
                             a.Keterangan,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaPasien, search)
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
