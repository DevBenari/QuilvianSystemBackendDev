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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PraOperasiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<PraOperasiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadUrl;


        public PraOperasiController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PraOperasiController> logger,
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

        async Task<string?> UploadFileToFlaskAsync(IFormFile? file, string prefix, string folderTarget, string uploaderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validasi size & ekstensi
            var maxSize = 2 * 1024 * 1024; // 2MB
            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (file.Length > maxSize)
                throw new Exception($"{prefix} terlalu besar! Maksimal 2MB.");

            if (!allowedExtensions.Contains(ext))
                throw new Exception($"{prefix} harus JPG atau JPEG.");

            // Nama file aman (menghindari spasi & karakter aneh)
            var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{uploaderName}_{safeTime}_{prefix}{ext}".Replace(" ", "_");

            using var client = new HttpClient();
            await using var ms = new MemoryStream();

            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var content = new MultipartFormDataContent
    {
        {
            new StreamContent(ms) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) } },
            "file",
            fileName
        },
        { new StringContent(folderTarget), "folderTarget" }
    };

            var response = await client.PostAsync(_uploadUrl, content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gagal upload {prefix} ke server Flask.");

            var body = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(body);

            return json.fileUrl;
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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> Create([FromForm] PraOperasiViewModel vm)
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
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userId = user.UserActiveId;

                // =====================================================
                // 🔹 Cek TTD Perawat Ruangan
                // =====================================================
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.TTDPerawatRuanganId);

                // =====================================================
                // 🔹 Generate ID lebih awal (dipakai untuk nama file)
                // =====================================================
                var praOperasiId = Guid.NewGuid();

                // =====================================================
                // 🔹 Helper upload ke Flask (SESUAI KODE KAMU)
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
                    var fileName = $"{praOperasiId}_{prefix}_{safeTime}{ext}";
                    var folderTarget = "PenandaanOperasi";
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
                // 🔹 Upload file BAG1 & BAG2 (PARALEL)
                // =====================================================
                var uploadBag1Task = UploadToFlaskAsync(vm.FilePenandaanOperasiBag1, "Bag1");
                var uploadBag2Task = UploadToFlaskAsync(vm.FilePenandaanOperasiBag2, "Bag2");

                await Task.WhenAll(uploadBag1Task, uploadBag2Task);

                // =====================================================
                // 🔹 Simpan ke DB
                // =====================================================
                var praOperasi = new PraOperasi
                {
                    PraOperasiId = praOperasiId,
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

                    TTDPerawatRuanganId = vm.TTDPerawatRuanganId,
                    TTDPerawatRuanganPath = ttd.Path,

                    PenandaanOperasiBag1 = uploadBag1Task.Result,
                    PenandaanOperasiBag2 = uploadBag2Task.Result,

                    TglCatatan = DateTime.UtcNow,
                    CreateBy = userId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.PraOperasis.Add(praOperasi);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new
                {
                    message = "Berhasil tambah PraOperasi",
                    praOperasi.PraOperasiId,
                    praOperasi.PenandaanOperasiBag1,
                    praOperasi.PenandaanOperasiBag2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        //[HttpPost]
        //[Consumes("multipart/form-data")]
        //[RequestSizeLimit(50_000_000)]
        //[RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        //public async Task<IActionResult> Create([FromBody] PraOperasiViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //        return BadRequest(new { message = "Data tidak valid." });

        //    try
        //    {
        //        // Cek koneksi database
        //        if (!_applicationDbContext.Database.CanConnect())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        // Ambil user login
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });

        //        var user = await _applicationDbContext.UserActives
        //            .FirstOrDefaultAsync(u => u.Email == emailLogin);

        //        if (user == null)
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        //        var userId = user.UserActiveId;
        //        var uploaderName = user.FullName ?? "User";

        //        // -------------------------------------------------------
        //        // 🔹 Cek TTD
        //        // -------------------------------------------------------
        //        var ttd = await _ttdService.CheckTTDAsync((Guid)vm.TTDPerawatRuanganId);

        //        // -------------------------------------------------------
        //        // 🔹 Simpan ke DB PraOperasi
        //        // -------------------------------------------------------
        //        var praOperasi = new PraOperasi
        //        {
        //            PraOperasiId = Guid.NewGuid(),
        //            KunjunganId = vm.KunjunganId,
        //            PasienId = vm.PasienId,
        //            PainAssessmentId = vm.PainAssessmentId,
        //            VitalSignId = vm.VitalSignId,
        //            StatusMental = vm.StatusMental,
        //            PengobatanSaatIni = vm.PengobatanSaatIni,
        //            AlatBantu = vm.AlatBantu,
        //            JenisOperasi = vm.JenisOperasi,
        //            WaktuOperasi = vm.WaktuOperasi,
        //            TempatOperasi = vm.TempatOperasi,
        //            HasilLab = vm.HasilLab,
        //            IsBatukFluDemam = vm.IsBatukFluDemam,
        //            IsHaid = vm.IsHaid,
        //            ProsedurOperasi = vm.ProsedurOperasi,
        //            TanggalOperasi = TryParseTanggalToUtc(vm.TanggalOperasi),
        //            PerawatBedahId = vm.PerawatBedahId,
        //            Keterangan = vm.Keterangan,

        //            TTDPerawatRuanganId = vm.TTDPerawatRuanganId,
        //            TTDPerawatRuanganPath = ttd.Path,


        //            TglCatatan = DateTime.UtcNow,
        //            CreateBy = userId,
        //            CreateDateTime = DateTimeOffset.UtcNow
        //        };

        //        _applicationDbContext.PraOperasis.Add(praOperasi);
        //        await _applicationDbContext.SaveChangesAsync();

        //        return Created("", new
        //        {
        //            message = "Berhasil tambah PraOperasi",
        //            praOperasi.PraOperasiId,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        //[RequestSizeLimit(50_000_000)] // 50 MB
        //[RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> Edit(Guid id, [FromBody] PraOperasiViewModel vm)
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
        public async Task<IActionResult> UploadTTDPerawatBedah(Guid id, [FromBody] PraOperasiTTDPerawatBedahViewModel vm)
        {
            if (vm == null || vm.TTDPerawatBedah == null )
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

                // cek ttd
                var ttdPath = await _ttdService.CheckTTDAsync((Guid)vm.TTDPerawatBedah);

                // ✅ Update PraOperasi
                praOperasi.TTDPerawatBedahId = vm.TTDPerawatBedah;
                praOperasi.TTDPerawatBedahPath = ttdPath.Path;

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
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
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
                    var fileName = $"{praOperasi.PraOperasiId}_{prefix}_{safeTime}{ext}";

                    // 👉 Sesuaikan nama folder dengan kebutuhan kamu
                    var folderTarget = "TTDKeluargaPasien";
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

                    // ⚠ Di sini kita pakai pola yang sama seperti UpdatePenandaan:
                    //     tidak baca JSON dari Flask, tapi pakai path lokal yang sudah dibentuk
                    return filePath;
                }


                // Upload file → folder TTDUser
                var ttdPath = await UploadToFlaskAsync(vm.TTDKeluarga, "TTDKeluargaPasien");

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
            if (vm == null || vm.TTDPerawatPrimer == null  )
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

                // Upload file → folder TTDUser
                var ttdPath = await _ttdService.CheckTTDAsync((Guid)vm.TTDPerawatPrimer);


                // ✅ Update PraOperasi
                praOperasi.TTDPerawatPrimerId = vm.TTDPerawatPrimer;
                praOperasi.TTDPerawatPrimerPath = ttdPath.Path;

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
            if (vm == null || vm.TTDDokter == null)
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

                // cek ttdPath
                var ttdPath = await _ttdService.CheckTTDAsync((Guid)vm.TTDDokter);
                
                // ✅ Update PraOperasi
                praOperasi.TTDDokterId = vm.TTDDokter;
                praOperasi.TglPernyataanDokter = TryParseTanggalToUtc(vm.TglPernyataanDokter);
                praOperasi.TTDDokterPath = ttdPath.Path;

                _applicationDbContext.PraOperasis.Update(praOperasi);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "TTD Dokter Bedah berhasil diupload", ttdPath, praOperasiId = praOperasi.PraOperasiId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("update-penandaan/{id}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> UpdatePenandaan(Guid id,[FromForm] PraOperasiPenandaanUploadVM vm)
        {
            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ♻ Ambil data PraOperasi target
                var entity = await _applicationDbContext.PraOperasis
                    .FirstOrDefaultAsync(p => p.PraOperasiId == id);

                if (entity == null)
                    return NotFound(new { message = "Data PraOperasi tidak ditemukan." });

                // =====================================================================
                // 🔹 Helper Upload ke Flask (dipakai oleh Bag1 & Bag2)
                // =====================================================================
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
                    var folderTarget = "PenandaanOperasi";
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

                // =====================================================================
                // 🔹 Upload Bag1 & Bag2 secara paralel
                // =====================================================================
                var taskBag1 = UploadToFlaskAsync(vm.FilePenandaanOperasiBag1, "Bag1");
                var taskBag2 = UploadToFlaskAsync(vm.FilePenandaanOperasiBag2, "Bag2");

                await Task.WhenAll(taskBag1, taskBag2);

                var pathBag1 = taskBag1.Result;
                var pathBag2 = taskBag2.Result;

                // =====================================================================
                // 🔹 Update database (hanya yang di-upload)
                // =====================================================================
                if (pathBag1 != null)
                    entity.PenandaanOperasiBag1 = pathBag1;

                if (pathBag2 != null)
                    entity.PenandaanOperasiBag2 = pathBag2;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Berhasil update penandaan operasi",
                    praOperasiId = entity.PraOperasiId,
                    bag1 = entity.PenandaanOperasiBag1,
                    bag2 = entity.PenandaanOperasiBag2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan: " + ex.Message });
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
