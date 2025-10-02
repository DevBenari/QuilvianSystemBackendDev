using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PraOperasiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PraOperasiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PraOperasiController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PraOperasiController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
                    DateTimeKind.Local); // atau Utc jika perlu

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
            var query = (from a in _applicationDbContext.PraOperasis
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             a.PraOperasiId,
                             a.KunjunganId,
                             a.PasienId,
                             a.PainAssessmentId,
                             a.VitalSignId,
                             a.StatusMental,
                             a.PengobatanSaatIni,
                             a.AlatBantu,
                             a.JenisOperasi,
                             a.TempatOperasi,
                             a.HasilLab,
                             a.IsBatukFluDemam,
                             a.IsHaid,
                             a.ProsedurOperasi,
                             a.TanggalOperasi,
                             a.PerawatBedahId,
                             a.PerawatRuanganId,
                             a.DokterId,
                             a.Keterangan,
                             a.TTDPerawatRuanganId,
                             a.TTDPerawatBedahId,
                             a.TTDDokterId,
                             a.TTDPerawatPrimerId,
                             a.TTDPerawatPrimerPath,
                             a.TTDPerawatBedahPath,
                             a.TTDDokterPath,
                             a.TTDPerawatRuanganPath,
                             a.TTDKeluarga,
                             a.PenandaanOperasiBag1,
                             a.PenandaanOperasiBag2,
                             a.TglCatatan,
                             a.TglPernyataanPasien,
                             a.TglPernyataanDokter,
                             CreateByName = u.FullName,
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
            var listdata = _applicationDbContext.PraOperasis.Find(id);
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
        public async Task<IActionResult> Create([FromForm] PraOperasiViewModel vm)
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

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==============================================
                // 🔹 Helper upload file dengan folder berbeda
                // ==============================================
                async Task<string?> UploadFileAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0) return null;

                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 1MB.");
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{getUserActive.FullName}_{safeTime}{fileExtension}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                { new StreamContent(ms) {
                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                }, "file", fileName },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    return jsonResp.fileUrl;
                }

                // ==============================================
                // 🔹 Proses upload sesuai folder
                // ==============================================
                string? ttdPerawatRuanganPath = null;
                Guid? ttdPerawatRuanganId = null;
                string? penandaanBag1Path = null;
                string? penandaanBag2Path = null;

                // Jika ada file TTDPerawatRuangan
                if (vm.FileTTDPerawatRuangan != null)
                {
                    ttdPerawatRuanganPath = await UploadFileAsync(vm.FileTTDPerawatRuangan, "TTDPerawatRuangan", "TTDUser");

                    // Sekalian insert ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = ttdPerawatRuanganPath,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();

                    ttdPerawatRuanganId = newTTD.TTDId;
                }

                if (vm.FilePenandaanOperasiBag1 != null)
                    penandaanBag1Path = await UploadFileAsync(vm.FilePenandaanOperasiBag1, "PenandaanOperasiBag1", "PenandaanOperasi");

                if (vm.FilePenandaanOperasiBag2 != null)
                    penandaanBag2Path = await UploadFileAsync(vm.FilePenandaanOperasiBag2, "PenandaanOperasiBag2", "PenandaanOperasi");

                // ==============================================
                // 🔹 Simpan ke database PraOperasi
                // ==============================================
                var data = new PraOperasi
                {
                    PraOperasiId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    PainAssessmentId = vm.PainAssessmentId,
                    VitalSignId = vm.VitalSignId,
                    StatusMental = vm.StatusMental,
                    PengobatanSaatIni = vm.PengobatanSaatIni,
                    AlatBantu = vm.AlatBantu,
                    JenisOperasi = vm.JenisOperasi,
                    WaktuOperasi = vm.WaktuOperasi,
                    TempatOperasi = vm.TempatOperasi,
                    HasilLab = vm.HasilLab,
                    IsBatukFluDemam = vm.IsBatukFluDemam,
                    IsHaid = vm.IsHaid,
                    ProsedurOperasi = vm.ProsedurOperasi,
                    TanggalOperasi = TryParseTanggalToUtc(vm.TanggalOperasi),
                    PerawatBedahId = vm.PerawatBedahId,
                    Keterangan = vm.Keterangan,

                    TTDPerawatRuanganId = ttdPerawatRuanganId,
                    TTDPerawatRuanganPath = ttdPerawatRuanganPath,

                    PenandaanOperasiBag1 = penandaanBag1Path,
                    PenandaanOperasiBag2 = penandaanBag2Path,

                    TglCatatan = DateTime.UtcNow,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.PraOperasis.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Data Pra-Operasi berhasil ditambahkan", PraOperasiId = data.PraOperasiId });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromForm] PraOperasiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // Cari data existing
                var data = await _applicationDbContext.PraOperasis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data Pra-Operasi tidak ditemukan." });
                }

                // ==============================================
                // 🔹 Helper upload file
                // ==============================================
                async Task<string?> UploadFileAsync(IFormFile? file, string prefix, string folderTarget)
                {
                    if (file == null || file.Length == 0) return null;

                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 1MB.");
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{getUserActive.FullName}_{safeTime}{fileExtension}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                { new StreamContent(ms) {
                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                }, "file", fileName },
                { new StringContent(folderTarget), "folderTarget" }
            };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    return jsonResp.fileUrl;
                }

                // ==============================================
                // 🔹 Update field basic
                // ==============================================
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.PainAssessmentId = vm.PainAssessmentId;
                data.VitalSignId = vm.VitalSignId;
                data.StatusMental = vm.StatusMental;
                data.PengobatanSaatIni = vm.PengobatanSaatIni;
                data.AlatBantu = vm.AlatBantu;
                data.JenisOperasi = vm.JenisOperasi;
                data.WaktuOperasi = vm.WaktuOperasi;
                data.TempatOperasi = vm.TempatOperasi;
                data.HasilLab = vm.HasilLab;
                data.IsBatukFluDemam = vm.IsBatukFluDemam;
                data.IsHaid = vm.IsHaid;
                data.ProsedurOperasi = vm.ProsedurOperasi;
                data.TanggalOperasi = TryParseTanggalToUtc(vm.TanggalOperasi);
                data.PerawatBedahId = vm.PerawatBedahId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // ==============================================
                // 🔹 Update file (jika ada upload baru)
                // ==============================================
                if (vm.FileTTDPerawatRuangan != null)
                {
                    var newPath = await UploadFileAsync(vm.FileTTDPerawatRuangan, "TTDPerawatRuangan", "TTDUser");

                    // Insert ke MasterTTD
                    var newTTD = new MasterTTD
                    {
                        TTDId = Guid.NewGuid(),
                        UserActiveId = userActiveId,
                        TTDPath = newPath,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = userActiveId
                    };

                    _applicationDbContext.MasterTTDs.Add(newTTD);
                    await _applicationDbContext.SaveChangesAsync();

                    data.TTDPerawatRuanganId = newTTD.TTDId;
                    data.TTDPerawatRuanganPath = newPath;
                }

                if (vm.FilePenandaanOperasiBag1 != null)
                {
                    var newPath = await UploadFileAsync(vm.FilePenandaanOperasiBag1, "PenandaanOperasiBag1", "PenandaanOperasi");
                    data.PenandaanOperasiBag1 = newPath;
                }

                if (vm.FilePenandaanOperasiBag2 != null)
                {
                    var newPath = await UploadFileAsync(vm.FilePenandaanOperasiBag2, "PenandaanOperasiBag2", "PenandaanOperasi");
                    data.PenandaanOperasiBag2 = newPath;
                }

                // ==============================================
                // 🔹 Simpan perubahan
                // ==============================================
                _applicationDbContext.PraOperasis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Data Pra-Operasi berhasil diperbarui", id = data.PraOperasiId });

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("UploadTTDPerawatBedah/{id}")]
        public async Task<IActionResult> UploadTTDPerawatBedah(Guid id, [FromForm] PraOperasiTTDPerawatBedahViewModel vm)
        {
            if (vm == null || vm.TTDPerawatBedah == null || vm.TTDPerawatBedah.Length == 0)
            {
                return BadRequest(new { message = "File TTD Perawat Bedah tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari PraOperasi berdasarkan ID
                var praOperasi = await _applicationDbContext.PraOperasis.FindAsync(id);
                if (praOperasi == null)
                {
                    return NotFound(new { message = "Data Pra-Operasi tidak ditemukan." });
                }

                // ✅ Proses upload file TTD
                async Task<string?> UploadFileAsync(IFormFile file, string prefix, string folderTarget)
                {
                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 1MB.");
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{getUserActive.FullName}_{safeTime}{fileExtension}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                        }, "file", fileName },
                        { new StringContent(folderTarget), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    return jsonResp.fileUrl;
                }

                // Upload file → folder TTDUser
                var ttdPath = await UploadFileAsync(vm.TTDPerawatBedah, "TTDPerawatBedah", "TTDUser");

                // ✅ Insert ke MasterTTD
                var newTTD = new MasterTTD
                {
                    TTDId = Guid.NewGuid(),
                    UserActiveId = userActiveId,
                    TTDPath = ttdPath,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveId
                };
                _applicationDbContext.MasterTTDs.Add(newTTD);
                await _applicationDbContext.SaveChangesAsync();

                // ✅ Update PraOperasi
                praOperasi.TTDPerawatBedahId = newTTD.TTDId;
                praOperasi.TTDPerawatBedahPath = ttdPath;

                _applicationDbContext.PraOperasis.Update(praOperasi);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "TTD Perawat Bedah berhasil diupload", ttdPath, praOperasiId = praOperasi.PraOperasiId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("UploadTTDKeluarga/{id}")]
        public async Task<IActionResult> UploadTTDKeluarga(Guid id, [FromForm] PraOperasiTTDKeluargaViewModel vm)
        {
            if (vm == null || vm.TTDKeluarga == null || vm.TTDKeluarga.Length == 0)
            {
                return BadRequest(new { message = "File TTD Perawat Bedah tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari PraOperasi berdasarkan ID
                var praOperasi = await _applicationDbContext.PraOperasis.FindAsync(id);
                if (praOperasi == null)
                {
                    return NotFound(new { message = "Data Pra-Operasi tidak ditemukan." });
                }

                // ✅ Proses upload file TTD
                async Task<string?> UploadFileAsync(IFormFile file, string prefix, string folderTarget)
                {
                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 1MB.");
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{getUserActive.FullName}_{safeTime}{fileExtension}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                        }, "file", fileName },
                        { new StringContent(folderTarget), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    return jsonResp.fileUrl;
                }


                // Upload file → folder TTDUser
                var ttdPath = await UploadFileAsync(vm.TTDKeluarga, "TTDKeluarga", "TTDKeluargaPasien");

                // ✅ Update PraOperasi
                praOperasi.TTDKeluarga = ttdPath;
                praOperasi.TglPernyataanPasien = TryParseTanggalToUtc(vm.TglPernyataanPasien);

                _applicationDbContext.PraOperasis.Update(praOperasi);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "TTD Perawat Bedah berhasil diupload", ttdPath, praOperasiId = praOperasi.PraOperasiId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("UploadTTDPerawatPrimer/{id}")]
        public async Task<IActionResult> TTDPerawatPrimer(Guid id, [FromForm] PraOperasiTTDPerawatPrimerViewModel vm)
        {
            if (vm == null || vm.TTDPerawatPrimer == null || vm.TTDPerawatPrimer.Length == 0)
            {
                return BadRequest(new { message = "File TTD Perawat Bedah tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari PraOperasi berdasarkan ID
                var praOperasi = await _applicationDbContext.PraOperasis.FindAsync(id);
                if (praOperasi == null)
                {
                    return NotFound(new { message = "Data Pra-Operasi tidak ditemukan." });
                }

                // ✅ Proses upload file TTD
                async Task<string?> UploadFileAsync(IFormFile file, string prefix, string folderTarget)
                {
                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 1MB.");
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{getUserActive.FullName}_{safeTime}{fileExtension}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                        }, "file", fileName },
                        { new StringContent(folderTarget), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    return jsonResp.fileUrl;
                }

                // Upload file → folder TTDUser
                var ttdPath = await UploadFileAsync(vm.TTDPerawatPrimer, "TTDPerawatPrimer", "TTDUser");

                // ✅ Insert ke MasterTTD
                var newTTD = new MasterTTD
                {
                    TTDId = Guid.NewGuid(),
                    UserActiveId = userActiveId,
                    TTDPath = ttdPath,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveId
                };
                _applicationDbContext.MasterTTDs.Add(newTTD);
                await _applicationDbContext.SaveChangesAsync();

                // ✅ Update PraOperasi
                praOperasi.TTDPerawatBedahId = newTTD.TTDId;
                praOperasi.TTDPerawatBedahPath = ttdPath;

                _applicationDbContext.PraOperasis.Update(praOperasi);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "TTD Perawat Bedah berhasil diupload", ttdPath, praOperasiId = praOperasi.PraOperasiId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("UploadTTDDokterBedah/{id}")]
        public async Task<IActionResult> TTDDokter(Guid id, [FromForm] PraOperasiTTDDokterBedahViewModel vm)
        {
            if (vm == null || vm.TTDDokter == null || vm.TTDDokter.Length == 0)
            {
                return BadRequest(new { message = "File TTD Perawat Bedah tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari PraOperasi berdasarkan ID
                var praOperasi = await _applicationDbContext.PraOperasis.FindAsync(id);
                if (praOperasi == null)
                {
                    return NotFound(new { message = "Data Pra-Operasi tidak ditemukan." });
                }

                // ✅ Proses upload file TTD
                async Task<string?> UploadFileAsync(IFormFile file, string prefix, string folderTarget)
                {
                    var maxSize = 1 * 1024 * 1024; // 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (file.Length > maxSize)
                        throw new Exception($"{prefix} terlalu besar! Maksimal 1MB.");
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var fileName = $"{prefix}_{getUserActive.FullName}_{safeTime}{fileExtension}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) }
                        }, "file", fileName },
                        { new StringContent(folderTarget), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke server Flask.");

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    return jsonResp.fileUrl;
                }

                // Upload file → folder TTDUser
                var ttdPath = await UploadFileAsync(vm.TTDDokter, "TTDDokter", "TTDUser");

                // ✅ Insert ke MasterTTD
                var newTTD = new MasterTTD
                {
                    TTDId = Guid.NewGuid(),
                    UserActiveId = userActiveId,
                    TTDPath = ttdPath,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveId
                };
                _applicationDbContext.MasterTTDs.Add(newTTD);
                await _applicationDbContext.SaveChangesAsync();

                // ✅ Update PraOperasi
                praOperasi.TTDDokterId = newTTD.TTDId;
                praOperasi.TglPernyataanDokter = TryParseTanggalToUtc(vm.TglPernyataanDokter);
                praOperasi.TTDDokterPath = ttdPath;

                _applicationDbContext.PraOperasis.Update(praOperasi);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "TTD Perawat Bedah berhasil diupload", ttdPath, praOperasiId = praOperasi.PraOperasiId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
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
                var data = await _applicationDbContext.PraOperasis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PraOperasis.Update(data);
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
            var query = from a in _applicationDbContext.PraOperasis
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             a.PraOperasiId,
                             a.KunjunganId,
                             a.PasienId,
                             a.PainAssessmentId,
                             a.VitalSignId,
                             a.StatusMental,
                             a.PengobatanSaatIni,
                             a.AlatBantu,
                             a.JenisOperasi,
                             a.TempatOperasi,
                             a.HasilLab,
                             a.IsBatukFluDemam,
                             a.IsHaid,
                             a.ProsedurOperasi,
                             a.TanggalOperasi,
                             a.PerawatBedahId,
                             a.PerawatRuanganId,
                             a.DokterId,
                             a.Keterangan,
                             a.TTDPerawatRuanganId,
                             a.TTDPerawatBedahId,
                             a.TTDDokterId,
                             a.TTDPerawatPrimerId,
                             a.TTDPerawatPrimerPath,
                             a.TTDPerawatBedahPath,
                             a.TTDDokterPath,
                             a.TTDPerawatRuanganPath,
                             a.TTDKeluarga,
                             a.PenandaanOperasiBag1,
                             a.PenandaanOperasiBag2,
                             a.TglCatatan,
                             a.TglPernyataanPasien,
                             a.TglPernyataanDokter,
                             CreateByName = u.FullName,
                         };
            // filter berdasarkan kunjungan Id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=> u.KunjunganId == kunjunganId.Value);
            }

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
