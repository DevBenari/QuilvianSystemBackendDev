using System.Linq;
using System.Runtime.ConstrainedExecution;
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
using static OpenCvSharp.Stitcher;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
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
                             a.SatuanPemeriksaan,
                             a.InfoNReff,
                             a.DetailDiagnosaKlinis,
                             a.ReseptorEstrogenER,
                             a.ReseptorProgesteronPR,
                             a.HER,
                             a.Ki67,
                             a.StatusER,
                             a.StatusPR,
                             a.HERImunohistokimia,
                             a.LainLain,
                             a.StatusHasil,
                             a.HasilPemeriksaan,
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
            if (id == Guid.Empty)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Id tidak valid."
                });
            }

            var data = await (
                from a in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u.UserActiveId

                join b in _applicationDbContext.LabHasils.AsNoTracking()
                    on a.HasilLabId equals b.HasilLabId into bGroup
                from b in bGroup.DefaultIfEmpty()

                join c in _applicationDbContext.Labs.AsNoTracking()
                    on b.LabId equals c.LabId into cGroup
                from c in cGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && a.DetailHasilLabId == id

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
                    a.SatuanPemeriksaan,
                    a.InfoNReff,
                    a.Kondisi,
                    a.KategoriGC,
                    a.Rincian,
                    a.Anjuran,
                    a.DiagnosisPA,
                    a.DetailDiagnosaKlinis,
                    a.ReseptorEstrogenER,
                    a.ReseptorProgesteronPR,
                    a.HER,
                    a.Ki67,
                    a.StatusER,
                    a.StatusPR,
                    a.HERImunohistokimia,
                    a.LainLain,
                    a.StatusHasil,
                    a.HasilPemeriksaan,
                    a.Keterangan,

                    // Lab Hasil
                    b.LabId,
                    NamaLab = c.NamaLab,
                    b.LabBookingId,
                    IsCito = b.LabBooking != null ? b.LabBooking.IsCito : null,
                    b.UserActiveId,
                    b.PenanggungJawabAnalisId,
                    b.PenanggungJawabId,
                    b.TanggalPemeriksaan,
                    b.DokterPerujukId,
                    DokterPerujukNama = b.DokterPerujuk.NmDokter,
                    b.DokterKonfirmatorId,
                    DokterKonfirmatorNama = b.DokterKonfirmator.NmDokter,
                    b.NoPhoneKonfirmator,
                    b.IsKonfirmatorDPJP,
                    KeteranganLabHasil = b.Keterangan
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Data tidak ditemukan."
                });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data
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

                var folderTarget = "HasilLabPhoto";

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
                            { new StringContent(folderTarget), "folderTarget" }
                        };

                        var flaskResponse = await client.PostAsync(_uploadUrl, content);
                        if (!flaskResponse.IsSuccessStatusCode)
                            return StatusCode(500, new { message = $"Gagal upload foto ({file.FileName}) ke server Flask." });

                        // ✅ SIMPAN RELATIVE PATH TANPA /uploads (PASTI konsisten)
                        photoPaths.Add($"/{folderTarget}/{fileName}");
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
                    SatuanPemeriksaan = vm.SatuanPemeriksaan,
                    DetailDiagnosaKlinis = vm.DetailDiagnosaKlinis,
                    ReseptorProgesteronPR = vm.ReseptorProgesteronPR,
                    ReseptorEstrogenER = vm.ReseptorEstrogenER,
                    HER = vm.HER,
                    Ki67 = vm.Ki67,
                    StatusER = vm.StatusER,
                    StatusPR = vm.StatusPR,
                    HERImunohistokimia = vm.HERImunohistokimia,
                    LainLain = vm.LainLain,
                    InfoNReff = vm.InfoNReff,
                    Kondisi = vm.Kondisi,
                    KategoriGC = vm.KategoriGC,
                    Rincian = vm.Rincian,
                    Anjuran = vm.Anjuran,
                    DiagnosisPA = vm.DiagnosisPA,
                    HasilImunoHistokimia = vm.HasilImunoHistokimia ?? new List<HasilImunoHistokimiaItem>(), 
                    StatusHasil = vm.StatusHasil,
                    HasilPemeriksaan = vm.HasilPemeriksaan,
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
        public async Task<IActionResult> Update(Guid id, [FromForm] LabHasilDetailViewModel vm, CancellationToken ct)
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

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Ambil data lama
                var data = await _applicationDbContext.LabHasilDetails
                    .FirstOrDefaultAsync(x => x.DetailHasilLabId == id && (x.IsDelete == false || x.IsDelete == null), ct);

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
                data.SatuanPemeriksaan = vm.SatuanPemeriksaan;
                data.DetailDiagnosaKlinis = vm.DetailDiagnosaKlinis;
                data.ReseptorProgesteronPR = vm.ReseptorProgesteronPR;
                data.ReseptorEstrogenER = vm.ReseptorEstrogenER;
                data.HER = vm.HER;
                data.Ki67 = vm.Ki67;
                data.StatusER = vm.StatusER;
                data.StatusPR = vm.StatusPR;
                data.HERImunohistokimia = vm.HERImunohistokimia;
                data.LainLain = vm.LainLain;
                data.InfoNReff = vm.InfoNReff;
                data.Kondisi = vm.Kondisi;
                data.KategoriGC = vm.KategoriGC;
                data.Rincian = vm.Rincian;
                data.Anjuran = vm.Anjuran;
                data.DiagnosisPA = vm.DiagnosisPA;
                data.HasilImunoHistokimia = vm.HasilImunoHistokimia ?? new List<HasilImunoHistokimiaItem>();
                data.StatusHasil = vm.StatusHasil;
                data.HasilPemeriksaan = vm.HasilPemeriksaan;
                data.Keterangan = vm.Keterangan;

                // ======================================================
                // ✅ Upload MULTI FILE (opsional)
                // - kalau ada foto baru => replace PhotoLabPath
                // - kalau tidak ada => keep PhotoLabPath lama
                // - simpan path final: "/HasilLabPhoto/{fileName}" (tanpa "/uploads")
                // ======================================================
                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
                var maxSize = 5 * 1024 * 1024; // 5MB
                var folderTarget = "HasilLabPhoto";

                if (vm.PhotoLab != null && vm.PhotoLab.Any(f => f != null && f.Length > 0))
                {
                    // Pakai NoPhotoLab lama, kalau kosong fallback
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
                        await file.CopyToAsync(ms, ct);
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
                    { new StringContent(folderTarget), "folderTarget" }
                };

                        var flaskResponse = await client.PostAsync(_uploadUrl, content, ct);
                        if (!flaskResponse.IsSuccessStatusCode)
                            return StatusCode(500, new { message = $"Gagal upload foto ({file.FileName}) ke server Flask." });

                        // ✅ SIMPAN PATH STANDAR (tanpa /uploads)
                        photoPaths.Add($"/{folderTarget}/{fileName}");
                    }

                    // ✅ replace path lama (JSON array)
                    data.PhotoLabPath = photoPaths.Any() ? JsonConvert.SerializeObject(photoPaths) : null;
                }

                // ======================================================
                // ✅ Audit update
                // ======================================================
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                var result = await _applicationDbContext.SaveChangesAsync(ct);

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
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        Guid? kunjunganId = null,
        Guid? labbookingid = null,
        bool? isCito = null,
        string? namaLab =null,
        string? namaDokter =null,
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
                             a.SatuanPemeriksaan,
                             a.InfoNReff,
                             a.DetailDiagnosaKlinis,
                             a.ReseptorEstrogenER,
                             a.ReseptorProgesteronPR,
                             a.HER,
                             a.Ki67,
                             a.StatusER,
                             a.StatusPR,
                             a.HERImunohistokimia,
                             a.LainLain,
                             a.Kondisi,
                             a.KategoriGC,
                             a.Rincian,
                             a.Anjuran,
                             a.DiagnosisPA,
                             a.StatusHasil,
                             a.HasilPemeriksaan,
                             a.Keterangan,

                             // ttg Lab hasil
                             b.LabId,
                             NamaLab = c.NamaLab,
                             b.LabBookingId,
                             IsCito = b.LabBooking != null ? b.LabBooking.IsCito:null,
                             b.DokterPerujukId,
                             DokterPerujukNama = b.DokterPerujuk.NmDokter,
                             b.DokterKonfirmatorId,
                             DokterKonfirmatorNama = b.DokterKonfirmator.NmDokter,
                             b.NoPhoneKonfirmator,
                             b.IsKonfirmatorDPJP,
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

            if (!string.IsNullOrWhiteSpace(namaDokter))
            {
                namaDokter = $"%{namaDokter.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.DokterKonfirmatorNama, namaDokter) ||
                    EF.Functions.ILike(u.DokterPerujukNama, namaDokter)
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
            
            // filter is cito
            if (isCito.HasValue)
            {
                query = query.Where(u => u.IsCito == isCito);
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
