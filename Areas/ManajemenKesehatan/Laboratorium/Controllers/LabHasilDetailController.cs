using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Client.Http;
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

        private static List<string> ToPhotoLabPaths(string? photoLabPath)
        {
            if (string.IsNullOrWhiteSpace(photoLabPath))
                return new List<string>();

            var s = photoLabPath.Trim();

            // Jika JSON array string
            if (s.StartsWith("[") && s.EndsWith("]"))
            {
                try
                {
                    return (JsonConvert.DeserializeObject<List<string>>(s) ?? new List<string>())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim())
                        .ToList();
                }
                catch
                {
                    return new List<string>();
                }
            }

            // Fallback: format CSV "/a,/b,/c"
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
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
                             PhotoLab = ToPhotoLabPaths(a.PhotoLabPath),
                             JumlahFoto = ToPhotoLabPaths(a.PhotoLabPath).Count,
                             a.HasilLabManual,
                             a.HasilLabAI,
                             a.JumlahFilm,
                             a.KeadaanSpecimen,
                             a.AnalisId,
                             a.IsDefinitif,
                             a.IsDuplu,
                             a.HasilMakroskopik,
                             a.HasilMikroskopik,
                             a.KesimpulanHasil,
                             a.NilaiNormal,
                             a.BloodVolume,
                             a.SputumVolume,
                             a.UrineVolume,
                             a.PusVolume,
                             a.StoolVolume,
                             a.JaringanVolume,
                             a.BodyFluidVolume,
                             a.PetugasSpecimenId,
                             a.TanggalSpecimen,
                             a.JamSpecimen,
                             a.InfoNReff,
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
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _applicationDbContext.LabHasilDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DetailHasilLabId == id &&
                    (x.IsDelete == false || x.IsDelete == null), ct);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });


            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",

                // opsional: jangan kirim PhotoLabPath string biar FE tidak melihat \"
                data = new
                {
                    data.DetailHasilLabId,
                    data.HasilLabId,
                    data.PemeriksaanLabId,
                    data.KelasId,
                    data.TanggalSelesai,
                    data.NoPhotoLab,
                    PhotoLabPath = ToPhotoLabPaths(data.PhotoLabPath),
                    JumlahFoto = ToPhotoLabPaths(data.PhotoLabPath).Count,
                    data.HasilLabManual,
                    data.HasilLabAI,
                    data.JumlahFilm,
                    data.KeadaanSpecimen,
                    data.AnalisId,
                    data.IsDefinitif,
                    data.IsDuplu,
                    data.HasilMakroskopik,
                    data.HasilMikroskopik,
                    data.KesimpulanHasil,
                    data.NilaiNormal,
                    data.BloodVolume,
                    data.SputumVolume,
                    data.UrineVolume,
                    data.PusVolume,
                    data.StoolVolume,
                    data.JaringanVolume,
                    data.BodyFluidVolume,
                    data.PetugasSpecimenId,
                    data.TanggalSpecimen,
                    data.JamSpecimen,
                    data.InfoNReff,
                    data.Keterangan,
                    data.CreateBy,
                    data.CreateDateTime
                }
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

                // 🔐 Ambil User Aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Ambil prefix dari tabel MstLab lewat join ke HasilLab
                var labData = await (from hl in _applicationDbContext.LabHasils
                                     join ml in _applicationDbContext.Labs on hl.LabId equals ml.LabId
                                     where hl.HasilLabId == vm.HasilLabId
                                     select new { ml.KodeKategori }).FirstOrDefaultAsync();

                if (labData == null)
                    return BadRequest(new { message = "Data lab tidak ditemukan atau tidak valid!" });

                string prefix = labData.KodeKategori ?? "LAB";

                // ✅ Generate NoPhotoLab unik per jenis lab dan tanggal
                var today = DateTime.UtcNow.ToString("yyMMdd");

                int urutan = await _applicationDbContext.LabHasilDetails
                    .Where(a => a.NoPhotoLab != null && a.NoPhotoLab.StartsWith(prefix + today))
                    .CountAsync() + 1;

                string noPhotoLab = $"{prefix}{today}{urutan:0000}";

                // ✅ Upload MULTI FILE ke Flask
                var photoPaths = new List<string>();

                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
                var maxSize = 5 * 1024 * 1024; // 5 MB

                if (vm.PhotoLab != null && vm.PhotoLab.Any())
                {
                    using var client = new HttpClient();

                    int i = 0;
                    foreach (var file in vm.PhotoLab.Where(f => f != null && f.Length > 0))
                    {
                        i++;

                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedExtensions.Contains(ext))
                            return BadRequest(new { message = $"Format foto tidak valid ({file.FileName}). Gunakan JPG/PNG." });

                        if (file.Length > maxSize)
                            return BadRequest(new { message = $"Ukuran foto terlalu besar ({file.FileName})! Maks 5MB." });

                        var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
                        var fileName = $"{noPhotoLab}_{i:00}_{safeTime}{ext}";

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
                        "file", fileName
                    },
                    { new StringContent("HasilLabPhoto"), "folderTarget" }
                };

                        var flaskResponse = await client.PostAsync(_uploadUrl, content);
                        if (!flaskResponse.IsSuccessStatusCode)
                            return StatusCode(500, new { message = $"Gagal upload foto ({file.FileName}) ke server Flask." });

                        var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                        dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);

                        string path = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? "";
                        if (string.IsNullOrWhiteSpace(path))
                            return StatusCode(500, new { message = $"Upload berhasil tapi path kosong ({file.FileName})." });

                        photoPaths.Add(path);
                    }
                }

                // ✅ Simpan path multi-foto sebagai JSON array (atau null jika tidak ada foto)
                string? photoPathJson = photoPaths.Any() ? JsonConvert.SerializeObject(photoPaths) : null;

                // ✅ Simpan ke Database
                var data = new LabHasilDetail
                {
                    DetailHasilLabId = Guid.NewGuid(),
                    HasilLabId = vm.HasilLabId,
                    PemeriksaanLabId = vm.PemeriksaanLabId,
                    KelasId = vm.KelasId,
                    TanggalSelesai = vm.TanggalSelesai ?? DateTime.UtcNow,
                    NoPhotoLab = noPhotoLab,

                    // ✅ menampung banyak path
                    PhotoLabPath = photoPathJson,

                    HasilLabManual = vm.HasilLabManual,
                    HasilLabAI = vm.HasilLabAI,
                    JumlahFilm = vm.JumlahFilm,
                    KeadaanSpecimen = vm.KeadaanSpecimen,
                    AnalisId = vm.AnalisId,
                    IsDefinitif = vm.IsDefinitif,
                    IsDuplu = vm.IsDuplu,
                    HasilMakroskopik = vm.HasilMakroskopik,
                    HasilMikroskopik = vm.HasilMikroskopik,
                    KesimpulanHasil = vm.KesimpulanHasil,
                    NilaiNormal = vm.NilaiNormal,
                    BloodVolume = vm.BloodVolume,
                    SputumVolume = vm.SputumVolume,
                    UrineVolume = vm.UrineVolume,
                    PusVolume = vm.PusVolume,
                    StoolVolume = vm.StoolVolume,
                    JaringanVolume = vm.JaringanVolume,
                    BodyFluidVolume = vm.BodyFluidVolume,
                    PetugasSpecimenId = vm.PetugasSpecimenId,
                    TanggalSpecimen = vm.TanggalSpecimen,
                    JamSpecimen = vm.JamSpecimen,
                    InfoNReff = vm.InfoNReff,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTime.UtcNow,
                };

                _applicationDbContext.LabHasilDetails.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });

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
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // 🔐 Ambil User Aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Ambil data lama
                var data = await _applicationDbContext.LabHasilDetails
                    .FirstOrDefaultAsync(x => x.DetailHasilLabId == id && (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // ======================================================
                // ✅ Update field non-file
                // ======================================================
                data.HasilLabId = vm.HasilLabId;
                data.PemeriksaanLabId = vm.PemeriksaanLabId;
                data.KelasId = vm.KelasId;
                data.TanggalSelesai = vm.TanggalSelesai ?? data.TanggalSelesai;

                data.HasilLabManual = vm.HasilLabManual;
                data.HasilLabAI = vm.HasilLabAI;
                data.JumlahFilm = vm.JumlahFilm;
                data.KeadaanSpecimen = vm.KeadaanSpecimen;
                data.AnalisId = vm.AnalisId;
                data.IsDefinitif = vm.IsDefinitif;
                data.IsDuplu = vm.IsDuplu;
                data.HasilMakroskopik = vm.HasilMakroskopik;
                data.HasilMikroskopik = vm.HasilMikroskopik;
                data.KesimpulanHasil = vm.KesimpulanHasil;
                data.NilaiNormal = vm.NilaiNormal;
                data.BloodVolume = vm.BloodVolume;
                data.SputumVolume = vm.SputumVolume;
                data.UrineVolume = vm.UrineVolume;
                data.PusVolume = vm.PusVolume;
                data.StoolVolume = vm.StoolVolume;
                data.JaringanVolume = vm.JaringanVolume;
                data.BodyFluidVolume = vm.BodyFluidVolume;
                data.PetugasSpecimenId = vm.PetugasSpecimenId;
                data.TanggalSpecimen = vm.TanggalSpecimen;
                data.JamSpecimen = vm.JamSpecimen;
                data.InfoNReff = vm.InfoNReff;
                data.Keterangan = vm.Keterangan;

                // ======================================================
                // ✅ Upload MULTI FILE (opsional)
                // - kalau ada foto baru => replace PhotoLabPath
                // - kalau tidak ada => keep PhotoLabPath lama
                // ======================================================
                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
                var maxSize = 5 * 1024 * 1024; // 5 MB

                if (vm.PhotoLab != null && vm.PhotoLab.Any(f => f != null && f.Length > 0))
                {
                    // Pakai NoPhotoLab lama, kalau kosong fallback generate sederhana
                    var baseNoPhotoLab = !string.IsNullOrWhiteSpace(data.NoPhotoLab)
                        ? data.NoPhotoLab
                        : $"LAB{DateTime.UtcNow:yyMMdd}0001";

                    var photoPaths = new List<string>();

                    using var client = new HttpClient();

                    int i = 0;
                    foreach (var file in vm.PhotoLab.Where(f => f != null && f.Length > 0))
                    {
                        i++;

                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedExtensions.Contains(ext))
                            return BadRequest(new { message = $"Format foto tidak valid ({file.FileName}). Gunakan JPG/PNG." });

                        if (file.Length > maxSize)
                            return BadRequest(new { message = $"Ukuran foto terlalu besar ({file.FileName})! Maks 5MB." });

                        var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
                        var fileName = $"{baseNoPhotoLab}_{i:00}_{safeTime}{ext}";

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
                        "file", fileName
                    },
                    { new StringContent("HasilLabPhoto"), "folderTarget" }
                };

                        var flaskResponse = await client.PostAsync(_uploadUrl, content);
                        if (!flaskResponse.IsSuccessStatusCode)
                            return StatusCode(500, new { message = $"Gagal upload foto ({file.FileName}) ke server Flask." });

                        var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                        dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);

                        string path = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? "";
                        if (string.IsNullOrWhiteSpace(path))
                            return StatusCode(500, new { message = $"Upload berhasil tapi path kosong ({file.FileName})." });

                        photoPaths.Add(path);
                    }

                    // ✅ replace path lama
                    data.PhotoLabPath = photoPaths.Any() ? JsonConvert.SerializeObject(photoPaths) : null;
                }

                // ======================================================
                // ✅ Audit update
                // ======================================================
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Edit Data Berhasil || 200 OK" });

                return StatusCode(500, new { message = "Data tidak berhasil diupdate ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Kesalahan database: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat update DetailHasilLab");
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
        Guid? kunjunganId = null,
        Guid? labbookingid = null,
        string? namaLab =null,
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

                         join b in _applicationDbContext.LabHasils.AsNoTracking()
                         on a.HasilLabId equals b.HasilLabId into bGroup
                         from b in bGroup.DefaultIfEmpty()

                         join c in _applicationDbContext.Labs.AsNoTracking()
                         on b.LabId equals c.LabId into cGroup
                         from c in cGroup.DefaultIfEmpty()  

                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailHasilLabId,
                             a.HasilLabId,
                             b.KunjunganId,
                             a.PemeriksaanLabId,
                             a.KelasId,
                             a.TanggalSelesai,
                             a.NoPhotoLab,
                             PhotoLabPath = ToPhotoLabPaths(a.PhotoLabPath),
                             JumlahFotoLab = ToPhotoLabPaths(a.PhotoLabPath).Count,
                             a.HasilLabManual,
                             a.HasilLabAI,
                             a.JumlahFilm,
                             a.KeadaanSpecimen,
                             a.AnalisId,
                             a.IsDefinitif,
                             a.IsDuplu,
                             a.HasilMakroskopik,
                             a.HasilMikroskopik,
                             a.KesimpulanHasil,
                             a.NilaiNormal,
                             a.BloodVolume,
                             a.SputumVolume,
                             a.UrineVolume,
                             a.PusVolume,
                             a.StoolVolume,
                             a.JaringanVolume,
                             a.BodyFluidVolume,
                             a.PetugasSpecimenId,
                             a.TanggalSpecimen,
                             a.JamSpecimen,
                             a.InfoNReff,
                             a.Keterangan,

                             // ttg Lab hasil
                             b.LabId,
                             NamaLab = c.NamaLab,
                             b.LabBookingId,
                             b.UserActiveId,
                             b.PenanggungJawabAnalisId,
                             b.PenanggungJawabId,
                             b.TanggalPemeriksaan,
                             KeteranganLabHasil = b.Keterangan,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                namaLab = $"%{namaLab.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLab, namaLab)
                );
            }

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            // lab booking id
            if (labbookingid.HasValue)
            {
                query = query.Where(u => u.LabBookingId == labbookingid.Value);
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
