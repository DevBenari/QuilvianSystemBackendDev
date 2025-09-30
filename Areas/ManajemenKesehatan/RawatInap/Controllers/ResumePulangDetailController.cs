using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers;
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
    public class ResumePulangDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResumePulangDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResumePulangDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResumePulangDetailController> logger,
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
            var query = (from a in _applicationDbContext.ResumePulangDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetResumePulangId,
                             a.ResumePulangId,
                             a.Is65th,
                             a.IsPercobaanBunuhDiri,
                             a.IsKorbanKriminal,
                             a.IsKeterbatasanMobilitas,
                             a.IsPerawatanLanjutan,
                             a.IsBantuanADL,
                             a.TransportasiPulang,
                             a.IsPasienTinggalSendiri,
                             a.NamaWali,
                             a.LetakKamarPasien,
                             a.KondisiPenerangan,
                             a.JarakKamarMandi,
                             a.PerawatanYangDibantu,
                             a.IsDibantuAlatMedis,
                             a.IsAlatBantu,
                             a.IsPerluBantuanKhusus,
                             a.Keterangan,
                             a.TglDetailResumePulang,
                             a.TTId,
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
            var listdata = _applicationDbContext.ResumePulangDetails.Find(id);
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
        public async Task<IActionResult> Create([FromForm] ResumePulangDetailViewModel vm)
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

                // ✅ Ambil user aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ==================================================
                // ✅ PROSES UPLOAD TTD (sama seperti CttPemberianObat)
                // ==================================================
                string ttdPath = null;
                Guid ttdId;

                if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                {
                    var maxSize = 1 * 1024 * 1024; // max 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                    if (vm.TTDFile.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file TTD terlalu besar! Maksimal 1MB." });

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Format TTD tidak valid! Gunakan JPG atau JPEG." });

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var ttdFileName = $"{getUserActive.FullName}_{safeTime}_ResumePulang{fileExtension}";

                    // 📤 Upload ke Flask
                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.TTDFile.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                        }, "file", ttdFileName },

                        { new StringContent("TTDUser"), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    ttdPath = jsonResp.fileUrl;

                    // Simpan ke MasterTTD
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
                    ttdId = newTTD.TTDId;
                }
                else
                {
                    return BadRequest(new { message = "TTD harus diisi." });
                }

                // ==================================================
                // ✅ BUAT DATA RESUME PULANG DETAIL
                // ==================================================
                var data = new ResumePulangDetail
                {
                    DetResumePulangId = Guid.NewGuid(),
                    ResumePulangId = vm.ResumePulangId,
                    Is65th = vm.Is65th,
                    IsPercobaanBunuhDiri = vm.IsPercobaanBunuhDiri,
                    IsKorbanKriminal = vm.IsKorbanKriminal,
                    IsKeterbatasanMobilitas = vm.IsKeterbatasanMobilitas,
                    IsPerawatanLanjutan = vm.IsPerawatanLanjutan,
                    IsBantuanADL = vm.IsBantuanADL,
                    TransportasiPulang = vm.TransportasiPulang,
                    IsPasienTinggalSendiri = vm.IsPasienTinggalSendiri,
                    NamaWali = vm.NamaWali,
                    LetakKamarPasien = vm.LetakKamarPasien,
                    KondisiPenerangan = vm.KondisiPenerangan,
                    JarakKamarMandi = vm.JarakKamarMandi,
                    PerawatanYangDibantu = vm.PerawatanYangDibantu,
                    IsDibantuAlatMedis = vm.IsDibantuAlatMedis,
                    IsAlatBantu = vm.IsAlatBantu,
                    IsPerluBantuanKhusus = vm.IsPerluBantuanKhusus,
                    Keterangan = vm.Keterangan,
                    TglDetailResumePulang = vm.TglDetailResumePulang,
                    TTId = ttdId,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.ResumePulangDetails.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });

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
        public async Task<IActionResult> Update(Guid id, [FromForm] ResumePulangDetailViewModel vm)
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

                // ✅ Ambil user aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari data yang akan diupdate
                var data = await _applicationDbContext.ResumePulangDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data ResumePulangDetail tidak ditemukan." });
                }

                // ==================================================
                // ✅ PROSES UPLOAD TTD (opsional, hanya jika ada file baru)
                // ==================================================
                if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                {
                    var maxSize = 1 * 1024 * 1024; // max 1MB
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                    if (vm.TTDFile.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file TTD terlalu besar! Maksimal 1MB." });

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Format TTD tidak valid! Gunakan JPG atau JPEG." });

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    var ttdFileName = $"{getUserActive.FullName}_{safeTime}_ResumePulang{fileExtension}";

                    // 📤 Upload ke Flask
                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.TTDFile.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                        }, "file", ttdFileName },

                        { new StringContent("TTDUser"), "folderTarget" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                        return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                    var ttdPath = jsonResp.fileUrl;

                    // Simpan ke MasterTTD
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
                    data.TTId = newTTD.TTDId;
                }

                // ==================================================
                // ✅ UPDATE FIELD RESUME PULANG DETAIL
                // ==================================================
                data.ResumePulangId = vm.ResumePulangId;
                data.Is65th = vm.Is65th;
                data.IsPercobaanBunuhDiri = vm.IsPercobaanBunuhDiri;
                data.IsKorbanKriminal = vm.IsKorbanKriminal;
                data.IsKeterbatasanMobilitas = vm.IsKeterbatasanMobilitas;
                data.IsPerawatanLanjutan = vm.IsPerawatanLanjutan;
                data.IsBantuanADL = vm.IsBantuanADL;
                data.TransportasiPulang = vm.TransportasiPulang;
                data.IsPasienTinggalSendiri = vm.IsPasienTinggalSendiri;
                data.NamaWali = vm.NamaWali;
                data.LetakKamarPasien = vm.LetakKamarPasien;
                data.KondisiPenerangan = vm.KondisiPenerangan;
                data.JarakKamarMandi = vm.JarakKamarMandi;
                data.PerawatanYangDibantu = vm.PerawatanYangDibantu;
                data.IsDibantuAlatMedis = vm.IsDibantuAlatMedis;
                data.IsAlatBantu = vm.IsAlatBantu;
                data.IsPerluBantuanKhusus = vm.IsPerluBantuanKhusus;
                data.Keterangan = vm.Keterangan;
                data.TglDetailResumePulang = vm.TglDetailResumePulang;

                // Audit
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui ke database." });
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

        [HttpGet("paged")]
        public IActionResult Paged(
        int page = 1,
        int perPage = 10,
        //string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.ResumePulangDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetResumePulangId,
                             a.ResumePulangId,
                             a.Is65th,
                             a.IsPercobaanBunuhDiri,
                             a.IsKorbanKriminal,
                             a.IsKeterbatasanMobilitas,
                             a.IsPerawatanLanjutan,
                             a.IsBantuanADL,
                             a.TransportasiPulang,
                             a.IsPasienTinggalSendiri,
                             a.NamaWali,
                             a.LetakKamarPasien,
                             a.KondisiPenerangan,
                             a.JarakKamarMandi,
                             a.PerawatanYangDibantu,
                             a.IsDibantuAlatMedis,
                             a.IsAlatBantu,
                             a.IsPerluBantuanKhusus,
                             a.Keterangan,
                             a.TglDetailResumePulang,
                             a.TTId,
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
