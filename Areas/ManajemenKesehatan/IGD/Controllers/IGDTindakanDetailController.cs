using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class IGDTindakanDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly string _uploadUrl;
        private readonly ITTDService _ttdService;
        private readonly ILogger<IGDTindakanDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IGDTindakanDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IGDTindakanDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ITTDService ttdService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _ttdService = ttdService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.IGDTindakanDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailTindakanIGDId,
                             a.KunjunganId,
                             a.TindakanId,
                             a.KategoriTindakan,
                             a.WaktuTindakan,
                             a.TTDPath,
                             a.HasilSkinTest,
                             a.HasilTetanusToxoid,
                             a.HasilMedikamentosa,
                             a.JumlahAntiTetanusSerum,
                             a.JalurMedikamentosa,
                             a.WaktuPengobatan,
                             a.PerawatId,
                             a.DokterId,
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
            var listdata = _applicationDbContext.IGDTindakanDetails.Find(id);
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
        public async Task<IActionResult> Create([FromBody] IGDTindakanDetailViewModel vm) 
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
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

                //Guid ttdId;
                //string ttdPath;

                //if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                //{
                //    var maxSize = 1 * 1024 * 1024; // max 1MB
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg" };
                //    var fileExtension = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                //    if (vm.TTDFile.Length > maxSize)
                //        return BadRequest(new { message = "Ukuran file TTD terlalu besar! Maksimal 1MB." });

                //    if (!allowedExtensions.Contains(fileExtension))
                //        return BadRequest(new { message = "Format TTD tidak valid! Gunakan JPG atau JPEG." });

                //    // Nama file unik
                //    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                //    var ttdFileName = $"{getUserActive.FullName}_{safeTime}_TindakanIGD{fileExtension}";

                //    // 📤 Upload ke Flask
                //    using var client = new HttpClient();
                //    using var ms = new MemoryStream();
                //    await vm.TTDFile.CopyToAsync(ms);
                //    ms.Position = 0;

                //    var content = new MultipartFormDataContent
                //    {
                //        {
                //            new StreamContent(ms)
                //            {
                //                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                //            },
                //            "file", ttdFileName
                //        },
                //        { new StringContent("TTDIGD"), "folderTarget" }
                //    };

                //    // Pastikan _uploadUrl diinisialisasi di controller (misal: "https://yourflaskserver/upload")
                //    var flaskResponse = await client.PostAsync(_uploadUrl, content);

                //    if (!flaskResponse.IsSuccessStatusCode)
                //        return StatusCode(500, new { message = "Gagal upload tanda tangan ke server Flask." });

                //    // Ambil URL/path hasil upload dari response Flask
                //    var responseBody = await flaskResponse.Content.ReadAsStringAsync();
                //    Console.WriteLine("[DEBUG] Flask response: " + responseBody);
                //    _logger.LogInformation($"[DEBUG] Flask response: {responseBody}");
                //    dynamic jsonResp = JsonConvert.DeserializeObject(responseBody);
                //    ttdPath = jsonResp?.url ?? jsonResp?.fileUrl ?? jsonResp?.path ?? "";

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

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync(userActiveId);


                // ==================================================
                // ✅ BUAT DATA IGD TINDAKAN DETAIL
                // ==================================================
                var data = new IGDTindakanDetail
                {
                    DetailTindakanIGDId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    TindakanId = vm.TindakanId,
                    Keterangan = vm.Keterangan,
                    KategoriTindakan = vm.KategoriTindakan,
                    WaktuTindakan = vm.WaktuTindakan,
                    TTDPath = ttd.Path,
                    HasilSkinTest = vm.HasilSkinTest,
                    HasilMedikamentosa = vm.HasilMedikamentosa,
                    HasilTetanusToxoid = vm.HasilTetanusToxoid,
                    JumlahAntiTetanusSerum = vm.JumlahAntiTetanusSerum,
                    JalurMedikamentosa = vm.JalurMedikamentosa,
                    WaktuPengobatan = vm.WaktuPengobatan,
                    PerawatId = vm.PerawatId,
                    DokterId = vm.DokterId,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Simpan ke database
                _applicationDbContext.IGDTindakanDetails.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created", ttdId = ttd.TTDId});

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
        public async Task<IActionResult> Update(Guid id, [FromBody] IGDTindakanDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // ✅ Cek koneksi ke database
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Ambil data lama dari database
                var existingData = await _applicationDbContext.IGDTindakanDetails
                    .FirstOrDefaultAsync(x => x.DetailTindakanIGDId == id);

                if (existingData == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync(userActiveId);

                // ==================================================
                // ✅ UPDATE DATA LAMA
                // ==================================================
                existingData.KunjunganId = vm.KunjunganId ?? existingData.KunjunganId;
                existingData.TindakanId = vm.TindakanId ?? existingData.TindakanId;
                existingData.KategoriTindakan = vm.KategoriTindakan ?? existingData.KategoriTindakan;
                existingData.WaktuTindakan = vm.WaktuTindakan ?? existingData.WaktuTindakan;
                existingData.Keterangan = vm.Keterangan ?? existingData.Keterangan;
                existingData.TTDPath = ttd.Path;
                existingData.HasilSkinTest = vm.HasilSkinTest;
                    existingData.HasilMedikamentosa = vm.HasilMedikamentosa;
                    existingData.HasilTetanusToxoid = vm.HasilTetanusToxoid;
                    existingData.JumlahAntiTetanusSerum = vm.JumlahAntiTetanusSerum;
                    existingData.JalurMedikamentosa = vm.JalurMedikamentosa;
                    existingData.WaktuPengobatan = vm.WaktuPengobatan;
                    existingData.PerawatId = vm.PerawatId;
                    existingData.DokterId = vm.DokterId;

                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.IGDTindakanDetails.Update(existingData);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Data berhasil diperbarui || 200 OK", ttdId = ttd.TTDId });

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}" });
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
                var data = await _applicationDbContext.IGDTindakanDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.IGDTindakanDetails.Update(data);
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
            var query = (from a in _applicationDbContext.IGDTindakanDetails
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailTindakanIGDId,
                             a.KunjunganId,
                             a.TindakanId,
                             a.KategoriTindakan,
                             a.WaktuTindakan,
                             a.TTDPath,
                             a.HasilSkinTest,
                             a.HasilTetanusToxoid,
                             a.HasilMedikamentosa,
                             a.JumlahAntiTetanusSerum,
                             a.JalurMedikamentosa,
                             a.WaktuPengobatan,
                             a.PerawatId,
                             a.DokterId,
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
