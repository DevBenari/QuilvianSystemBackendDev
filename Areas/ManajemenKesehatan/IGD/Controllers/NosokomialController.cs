using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class NosokomialController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _uploadUrl;

        private readonly ILogger<NosokomialController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<NosokomialHub> _hubContext;

        public NosokomialController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<NosokomialController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IHubContext<NosokomialHub> hubContext)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.Nosokomials
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.NosokomialId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TB,
                             a.BB,
                             a.CaraMasukRS,
                             a.TglMasukRs,
                             a.TglKeluarRs,
                             a.DokterId1,
                             a.DokterId2,
                             a.DokterId3,
                             a.IPCLN1,
                             a.IPCLN2,
                             a.IPCLN3,
                             a.KondisiKeluar,
                             a.DiagnosaAwal,
                             a.DiagnosaAkhir,
                             a.TTDKepalaRuangan,
                             a.NamaKepalaRuangan,
                             a.TTDPerawat,
                             a.NamaPerawat,

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
            var listdata = _applicationDbContext.Nosokomials.Find(id);
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
        [RequestSizeLimit(10_000_000)] // 10 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> Create([FromForm] NosokomialViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ✅ Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==================================================
                // 🔹 Fungsi Upload TTD ke Flask
                // ==================================================
                async Task<(string? filePath, Guid? ttdId, string? fileName)> UploadTTDAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0) return (null, null, null);

                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"Ukuran file {prefix} terlalu besar! Maksimal 1MB.");

                    if (!allowedExtensions.Contains(ext))
                        throw new Exception($"Format file {prefix} tidak valid! Gunakan JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{getUserActive.FullName}_{safeTime}_{prefix}{ext}";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    using var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(ms)
                    {
                        Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                    },
                    "file",
                    fileName
                },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var response = await client.PostAsync(_uploadUrl, content);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload file {prefix} ke server Flask (Status: {response.StatusCode}).");

                    // 💾 Simpan metadata ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = filePath,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();

                    return (filePath, newTTD.TTDId, fileName);
                }

                // ==================================================
                // ✅ Upload tanda tangan perawat dan kepala ruangan
                // ==================================================
                var (ttdPerawatPath, ttdPerawatId, _) = await UploadTTDAsync(vm.TTDPerawat, "TTDPerawat", "TTDUser");
                var (ttdKepalaPath, ttdKepalaId, _) = await UploadTTDAsync(vm.TTDKepalaRuangan, "TTDKepalaRuangan", "TTDUser");

                // ==================================================
                // ✅ Simpan ke tabel Nosokomial
                // ==================================================
                var data = new Nosokomial
                {
                    NosokomialId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TB = vm.TB,
                    BB = vm.BB,
                    CaraMasukRS = vm.CaraMasukRS,
                    TglMasukRs = vm.TglMasukRs,
                    TglKeluarRs = vm.TglKeluarRs,
                    DokterId1 = vm.DokterId1,
                    DokterId2 = vm.DokterId2,
                    DokterId3 = vm.DokterId3,
                    IPCLN1 = vm.IPCLN1,
                    IPCLN2 = vm.IPCLN2,
                    IPCLN3 = vm.IPCLN3,
                    KondisiKeluar = vm.KondisiKeluar,
                    DiagnosaAwal = vm.DiagnosaAwal,
                    DiagnosaAkhir = vm.DiagnosaAkhir,
                    NamaKepalaRuangan = vm.NamaKepalaRuangan,
                    NamaPerawat = vm.NamaPerawat,
                    TTDKepalaRuangan = ttdKepalaPath,
                    TTDPerawat = ttdPerawatPath,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.Nosokomials.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("Nosokomia Created", new
                {
                    Action = "create",
                    id = data.NosokomialId
                });

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Nosokomial Berhasil || 201 Created",
                        ttdPerawatPath,
                        ttdKepalaPath
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
        [RequestSizeLimit(10_000_000)] // 10 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> Update(Guid id, [FromForm] NosokomialViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ✅ Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari data yang akan diupdate
                var data = await _applicationDbContext.Nosokomials
                    .FirstOrDefaultAsync(n => n.NosokomialId == id && (n.IsDelete == false || n.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = $"Data Nosokomial dengan ID {id} tidak ditemukan." });

                // ==================================================
                // 🔹 Fungsi Upload TTD ke Flask
                // ==================================================
                async Task<(string? filePath, Guid? ttdId, string? fileName)> UploadTTDAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0) return (null, null, null);

                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"Ukuran file {prefix} terlalu besar! Maksimal 1MB.");

                    if (!allowedExtensions.Contains(ext))
                        throw new Exception($"Format file {prefix} tidak valid! Gunakan JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{getUserActive.FullName}_{safeTime}_{prefix}{ext}";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    using var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(ms)
                    {
                        Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                    },
                    "file",
                    fileName
                },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var response = await client.PostAsync(_uploadUrl, content);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload file {prefix} ke server Flask (Status: {response.StatusCode}).");

                    // 💾 Simpan metadata ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = filePath,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();

                    return (filePath, newTTD.TTDId, fileName);
                }

                // ==================================================
                // ✅ Upload ulang tanda tangan (jika dikirim)
                // ==================================================
                string? ttdPerawatPath = data.TTDPerawat;
                string? ttdKepalaPath = data.TTDKepalaRuangan;

                if (vm.TTDPerawat != null)
                {
                    var result = await UploadTTDAsync(vm.TTDPerawat, "TTDPerawat", "TTDUser");
                    ttdPerawatPath = result.filePath;
                }

                if (vm.TTDKepalaRuangan != null)
                {
                    var result = await UploadTTDAsync(vm.TTDKepalaRuangan, "TTDKepalaRuangan", "TTDUser");
                    ttdKepalaPath = result.filePath;
                }

                // ==================================================
                // ✅ Update data Nosokomial
                // ==================================================
                data.KunjunganId = vm.KunjunganId ?? data.KunjunganId;
                data.PasienId = vm.PasienId ?? data.PasienId;
                data.TB = vm.TB ?? data.TB;
                data.BB = vm.BB ?? data.BB;
                data.CaraMasukRS = vm.CaraMasukRS ?? data.CaraMasukRS;
                data.TglMasukRs = vm.TglMasukRs ?? data.TglMasukRs;
                data.TglKeluarRs = vm.TglKeluarRs ?? data.TglKeluarRs;
                data.DokterId1 = vm.DokterId1 ?? data.DokterId1;
                data.DokterId2 = vm.DokterId2 ?? data.DokterId2;
                data.DokterId3 = vm.DokterId3 ?? data.DokterId3;
                data.IPCLN1 = vm.IPCLN1 ?? data.IPCLN1;
                data.IPCLN2 = vm.IPCLN2 ?? data.IPCLN2;
                data.IPCLN3 = vm.IPCLN3 ?? data.IPCLN3;
                data.KondisiKeluar = vm.KondisiKeluar ?? data.KondisiKeluar;
                data.DiagnosaAwal = vm.DiagnosaAwal ?? data.DiagnosaAwal;
                data.DiagnosaAkhir = vm.DiagnosaAkhir ?? data.DiagnosaAkhir;
                data.NamaKepalaRuangan = vm.NamaKepalaRuangan ?? data.NamaKepalaRuangan;
                data.NamaPerawat = vm.NamaPerawat ?? data.NamaPerawat;
                data.TTDPerawat = ttdPerawatPath;
                data.TTDKepalaRuangan = ttdKepalaPath;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Nosokomials.Update(data);
                int resultSave = await _applicationDbContext.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("Nosokomial changed", new
                {
                    Action = "changed",
                    id = data.NosokomialId
                });

                if (resultSave > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Nosokomial Berhasil || 200 OK",
                        ttdPerawatPath,
                        ttdKepalaPath
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
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

        [HttpPut("update-ttd-kepalaruangan/{id}")]
        [RequestSizeLimit(10_000_000)] // 10 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> UpdateTTDKepalaRuangan(Guid id, [FromForm] UpdateTTDKepalaRuanganVM vm)
        {
            if (vm == null )
                return BadRequest(new { message = "File TTD Kepala Ruangan tidak ditemukan." });

            try
            {
                // ✅ Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari data yang akan diupdate
                var data = await _applicationDbContext.Nosokomials
                    .FirstOrDefaultAsync(n => n.NosokomialId == id && (n.IsDelete == false || n.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = $"Data Nosokomial dengan ID {id} tidak ditemukan." });

                // ==================================================
                // 🔹 Fungsi Upload TTD ke Flask
                // ==================================================
                async Task<(string? filePath, Guid? ttdId, string? fileName)> UploadTTDAsync(IFormFile file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0) return (null, null, null);

                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"Ukuran file {prefix} terlalu besar! Maksimal 1MB.");

                    if (!allowedExtensions.Contains(ext))
                        throw new Exception($"Format file {prefix} tidak valid! Gunakan JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{getUserActive.FullName}_{safeTime}_{prefix}{ext}";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    using var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(ms)
                    {
                        Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                    },
                    "file",
                    fileName
                },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var response = await client.PostAsync(_uploadUrl, content);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload file {prefix} ke server Flask (Status: {response.StatusCode}).");

                    // 💾 Simpan metadata ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = filePath,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();

                    return (filePath, newTTD.TTDId, fileName);
                }

                // ==================================================
                // ✅ Upload ulang tanda tangan Kepala Ruangan
                // ==================================================
                string? ttdKepalaPath = data.TTDKepalaRuangan;

                if (vm != null)
                {
                    var result = await UploadTTDAsync(vm.TTDKepalaRuanganFile, "TTDKepalaRuangan", "TTDUser");
                    ttdKepalaPath = result.filePath;
                }

                // ==================================================
                // ✅ Update data Nosokomial (hanya TTD Kepala Ruangan)
                // ==================================================
                data.TTDKepalaRuangan = ttdKepalaPath;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Nosokomials.Update(data);
                int resultSave = await _applicationDbContext.SaveChangesAsync();

                // Kirim notifikasi via SignalR
                await _hubContext.Clients.All.SendAsync("Nosokomial changed", new
                {
                    Action = "changed",
                    id = data.NosokomialId
                });

                if (resultSave > 0)
                {
                    return Ok(new
                    {
                        message = "Update TTD Kepala Ruangan Berhasil || 200 OK",
                        ttdKepalaPath
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
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
                var data = await _applicationDbContext.Nosokomials.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Nosokomials.Update(data);
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
            var query = (from a in _applicationDbContext.Nosokomials
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.NosokomialId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TB,
                             a.BB,
                             a.CaraMasukRS,
                             a.TglMasukRs,
                             a.TglKeluarRs,
                             a.DokterId1,
                             a.DokterId2,
                             a.DokterId3,
                             a.IPCLN1,
                             a.IPCLN2,
                             a.IPCLN3,
                             a.KondisiKeluar,
                             a.DiagnosaAwal,
                             a.DiagnosaAkhir,
                             a.TTDKepalaRuangan,
                             a.NamaKepalaRuangan,
                             a.TTDPerawat,
                             a.NamaPerawat,
                         });
            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter based on kunjungan id 
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
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

