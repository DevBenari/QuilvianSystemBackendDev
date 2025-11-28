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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
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
    [EnableCors("AllowSpecific")]
    public class CttPemberianObatController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly string _uploadUrl;
        private readonly ITTDService _ttdService;
        private readonly ILogger<CttPemberianObatController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CttPemberianObatController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CttPemberianObatController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ITTDService ttdService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            //_uploadUrl = configuration["FileStorage:UploadUrl"];
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

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query =
                   from a in _applicationDbContext.CttPemberianObats
                       // LEFT JOIN UserActive (creator)
                   join u0 in _applicationDbContext.UserActives on a.CreateBy equals u0.UserActiveId into gu0
                   from u in gu0.DefaultIfEmpty()
                       // LEFT JOIN Obat
                   join o0 in _applicationDbContext.Obats on a.ObatId equals o0.ObatId into go0
                   from o in go0.DefaultIfEmpty()

                   where a.IsDelete == false || a.IsDelete == null
                   orderby a.CreateDateTime descending
                   select new
                   {
                       a.CreateDateTime,
                       a.CreateBy,
                       CreateByName = u.FullName,

                       a.CttPemberianObatId,
                       a.ObatId,
                       a.KunjunganId,
                       a.RacikanId,
                       a.TglPemberian,
                       a.WaktuPemberian,
                       a.StatusPemberian,
                       a.StatusCttEso,
                       a.CaraPemberianObat,
                       a.UserActiveIdPerawat,
                       a.TTDPerawatPath,
                       a.Keterangan,

                       // >>> Informasi Obat (berdasarkan ObatId)
                       NamaObat = o != null ? o.ObatName : null,
                       DosisObat = o != null ? o.TakaranDosis : null, 

                   };


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
            var data =
                await (from a in _applicationDbContext.CttPemberianObats.AsNoTracking()
                       where a.CttPemberianObatId == id
                             && (a.IsDelete == false || a.IsDelete == null)

                       // LEFT JOIN creator
                       join u0 in _applicationDbContext.UserActives
                            on a.CreateBy equals u0.UserActiveId into gu0
                       from u in gu0.DefaultIfEmpty()

                           // LEFT JOIN perawat pemberi obat
                       join p0 in _applicationDbContext.UserActives
                            on a.UserActiveIdPerawat equals p0.UserActiveId into gp0
                       from perawat in gp0.DefaultIfEmpty()

                           // LEFT JOIN obat
                       join o0 in _applicationDbContext.Obats
                            on a.ObatId equals o0.ObatId into go0
                       from o in go0.DefaultIfEmpty()

                           // Left Join Racikan
                           join r0 in _applicationDbContext.Racikans
                            on a.RacikanId equals r0.RacikanId into gr0
                       from r in gr0.DefaultIfEmpty()

                       select new
                       {
                           a.CttPemberianObatId,
                           a.CreateDateTime,
                           a.CreateBy,
                           CreateByName = u != null ? u.FullName : null,
                           a.KunjunganId,
                           a.ObatId,
                           ObatInfo = new
                           {
                               // === sesuaikan nama kolom entity Obat kamu ===
                               NamaObat = o != null ? o.ObatName : null,
                               DosisObat = o != null ? o.TakaranDosis : null,

                           },

                           a.RacikanId,
                           RacikanInfo = new
                           {
                               // === sesuaikan nama kolom entity Racikan kamu ===
                               NamaRacikan = r != null ? r.NamaRacikan : null,
                               KeteranganRacikan = r != null ? r.Keterangan : null,
                           },

                           a.TglPemberian,
                           a.WaktuPemberian,
                           a.StatusPemberian,
                           a.StatusCttEso,
                           a.CaraPemberianObat,

                           a.UserActiveIdPerawat,
                           PerawatName = perawat != null ? perawat.FullName : null,
                           a.TTDPerawatPath,
                           a.Keterangan
                       })
                       .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CttPemberianObatViewModel vm)
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

                // **Ambil User aktif dari JWT**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // **Cek duplikasi**
                bool isDuplicate = _applicationDbContext.CttPemberianObats
                    .Any(c => c.KunjunganId == vm.KunjunganId && (c.RacikanId == vm.RacikanId || c.ObatId == vm.ObatId) && c.IsDelete == false);

                if (isDuplicate)
                    return Conflict(new { message = "Catatan pemberian obat ini sudah ada." });

                // ==================================================
                // ✅ PROSES UPLOAD TTD (langsung ke server Flask)
                // ==================================================
                //string ttdPath = null;
                //Guid ttdId;

                //if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                //{
                //    var maxSize = 1 * 1024 * 1024; // max 1MB
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                //    var fileExtension = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                //    if (vm.TTDFile.Length > maxSize)
                //        return BadRequest(new { message = "Ukuran file TTD terlalu besar! Maksimal 1MB." });

                //    if (!allowedExtensions.Contains(fileExtension))
                //        return BadRequest(new { message = "Format TTD tidak valid! Gunakan JPG atau JPEG." });

                //    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                //    var ttdFileName = $"{getUserActive.FullName}_{safeTime}_CttObat{fileExtension}";

                //    // 📤 Upload ke Flask
                //    using var client = new HttpClient();
                //    using var ms = new MemoryStream();
                //    await vm.TTDFile.CopyToAsync(ms);
                //    ms.Position = 0;

                //    var content = new MultipartFormDataContent {
                //        { new StreamContent(ms) {
                //            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                //        }, "file", ttdFileName },

                //        { new StringContent("TTDUser"), "folderTarget" }
                //    };

                //    var flaskResponse = await client.PostAsync(_uploadUrl, content);
                //    if (!flaskResponse.IsSuccessStatusCode)
                //        return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                //    // Ambil URL/path hasil upload dari response Flask
                //    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                //    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                //    ttdPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? ""; // Pastikan Flask balikin {"fileUrl": "http://.../TTDUser/nama_file.jpg"}

                //    // Simpan ke MasterTTD
                //    var newTTD = new MasterTTD
                //    {
                //        TTDId = Guid.NewGuid(),
                //        UserActiveId = userActiveId,
                //        TTDPath = ttdPath,
                //        CreateDateTime = DateTimeOffset.UtcNow,
                //        CreateBy = userActiveId
                //    };

                //    _applicationDbContext.MasterTTDs.Add(newTTD);
                //    await _applicationDbContext.SaveChangesAsync();
                //    ttdId = newTTD.TTDId;
                //}
                //else
                //{
                //    return BadRequest(new { message = "TTD harus diisi." });
                //}

                // ==================================================
                // ✅ BUAT DATA CATATAN PEMBERIAN OBAT
                // ==================================================

                // cek ttd 
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.UserActiveIdPerawat);
                var data = new CttPemberianObat
                {
                    CttPemberianObatId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    ObatId = vm.ObatId,
                    RacikanId = vm.RacikanId,
                    TglPemberian = TryParseTanggalToUtc(vm.TglPemberian),
                    WaktuPemberian = TimeOnly.ParseExact(vm.WaktuPemberian, "HH:mm:ss"),
                    StatusPemberian = vm.StatusPemberian,
                    StatusCttEso = vm.StatusCttEso,
                    CaraPemberianObat = vm.CaraPemberianObat,
                    UserActiveIdPerawat = vm.UserActiveIdPerawat,
                    TTDPerawatPath = ttd.Path,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                _applicationDbContext.CttPemberianObats.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created",
                        ttdPerawatId = ttd.TTDId
                    });

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


        [HttpPut("StatusCatatanESO/{id}")]
        public async Task<IActionResult> UpdateStatusCatatanESO(Guid id, [FromBody] StatusCttEsoViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

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
                var data = await _applicationDbContext.CttPemberianObats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.StatusCttEso = vm.Status;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.CttPemberianObats.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
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
        public async Task<IActionResult> Update(Guid id, [FromForm] CttPemberianObatViewModel vm)
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

                // **Ambil user aktif dari JWT**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // **Cari data yang mau diedit**
                var existing = await _applicationDbContext.CttPemberianObats.FindAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                //string ttdPath;
                //Guid ttdId = (Guid)existing.TTDId;

                // ==================================================
                // ✅ PROSES UPDATE TTD (jika ada file baru)
                // ==================================================
                //if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                //{
                //    var maxSize = 1 * 1024 * 1024; // max 1MB
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                //    var fileExtension = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                //    if (vm.TTDFile.Length > maxSize)
                //        return BadRequest(new { message = "Ukuran file TTD terlalu besar! Maksimal 1MB." });

                //    if (!allowedExtensions.Contains(fileExtension))
                //        return BadRequest(new { message = "Format TTD tidak valid! Gunakan JPG atau JPEG." });

                //    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                //    var ttdFileName = $"{getUserActive.FullName}_{safeTime}_CttObat{fileExtension}";

                //    var masterTTD = _applicationDbContext.MasterTTDs.FirstOrDefault(t => t.TTDId == existing.TTDId);
                //    ttdPath = masterTTD.TTDPath;
                //    // 📤 Upload ke Flask
                //    using var client = new HttpClient();
                //    using var ms = new MemoryStream();
                //    await vm.TTDFile.CopyToAsync(ms);
                //    ms.Position = 0;

                //    var content = new MultipartFormDataContent {
                //        { new StreamContent(ms) {
                //            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                //        }, "file", ttdFileName },

                //        { new StringContent("TTDUser"), "folderTarget" }
                //    };

                //    var flaskResponse = await client.PostAsync(_uploadUrl, content);
                //    if (!flaskResponse.IsSuccessStatusCode)
                //        return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                //    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                //    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                //    ttdPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? ""; // ambil URL dari Flask

                //    // Update MasterTTD
                //    if (masterTTD != null)
                //    {
                //        masterTTD.TTDPath = ttdPath;
                //        masterTTD.UpdateDateTime = DateTimeOffset.UtcNow;
                //        masterTTD.UpdateBy = userActiveId;
                //        _applicationDbContext.MasterTTDs.Update(masterTTD);
                //        ttdId = masterTTD.TTDId;
                //    }
                //    else
                //    {
                //        var newTTD = new MasterTTD
                //        {
                //            TTDId = Guid.NewGuid(),
                //            UserActiveId = userActiveId,
                //            TTDPath = ttdPath,
                //            CreateDateTime = DateTimeOffset.UtcNow,
                //            CreateBy = userActiveId
                //        };
                //        _applicationDbContext.MasterTTDs.Add(newTTD);
                //        await _applicationDbContext.SaveChangesAsync();
                //        ttdId = newTTD.TTDId;
                //    }
                //}

                // ==================================================
                // ✅ UPDATE FIELD CATATAN PEMBERIAN OBAT
                // ==================================================

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.UserActiveIdPerawat);


                existing.KunjunganId = vm.KunjunganId;
                existing.ObatId = vm.ObatId;
                existing.RacikanId = vm.RacikanId;
                existing.TglPemberian = TryParseTanggalToUtc(vm.TglPemberian);
                existing.WaktuPemberian = TimeOnly.ParseExact(vm.WaktuPemberian, "HH:mm:ss");
                existing.StatusPemberian = vm.StatusPemberian;
                existing.StatusCttEso = vm.StatusCttEso;
                existing.CaraPemberianObat = vm.CaraPemberianObat;
                existing.UserActiveIdPerawat = vm.UserActiveIdPerawat;
                existing.TTDPerawatPath = ttd.Path;
                existing.Keterangan = vm.Keterangan;
                existing.UpdateBy = userActiveId;
                existing.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.CttPemberianObats.Update(existing);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });

                return StatusCode(500, new 
                { message = "Data tidak berhasil diperbarui di database.",
                  ttdPerawatId = ttd.TTDId
                
                });
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
                var data = await _applicationDbContext.CttPemberianObats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.CttPemberianObats.Update(data);
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
        string? search = null,
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
            var query =
                   from a in _applicationDbContext.CttPemberianObats
                       // LEFT JOIN UserActive (creator)
                   join u0 in _applicationDbContext.UserActives on a.CreateBy equals u0.UserActiveId into gu0
                   from u in gu0.DefaultIfEmpty()
                       // LEFT JOIN Obat
                   join o0 in _applicationDbContext.Obats on a.ObatId equals o0.ObatId into go0
                   from o in go0.DefaultIfEmpty()

                   // left join racikan
                   join r0 in _applicationDbContext.Racikans on a.RacikanId equals r0.RacikanId into gr0
                   from r in gr0.DefaultIfEmpty()


                   where a.IsDelete == false || a.IsDelete == null
                   orderby a.CreateDateTime descending
                   select new
                   {
                       a.CreateDateTime,
                       a.CreateBy,
                       CreateByName = u.FullName,

                       a.CttPemberianObatId,
                       a.KunjunganId,
                       a.ObatId,
                       a.RacikanId,
                       a.TglPemberian,
                       a.WaktuPemberian,
                       a.StatusPemberian,
                       a.CaraPemberianObat,
                       a.UserActiveIdPerawat,
                       a.TTDPerawatPath,
                       a.Keterangan,
                       a.StatusCttEso,


                       // >>> Informasi Obat (berdasarkan ObatId)
                       NamaObat = o != null ? o.ObatName : null,
                       DosisObat = o != null ? o.TakaranDosis : null,

                       // >>> Informasi Racikan (berdasarkan RacikanId)
                       NamaRacikan = r != null ? r.NamaRacikan : null,
                       KeteranganRacikan = r != null ? r.Keterangan : null,

                   };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            //**Filter berdasarkan KunjunganId**
            if (kunjunganid.HasValue)
            {
                query = query.Where(u => u.KunjunganId == kunjunganid.Value);
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
