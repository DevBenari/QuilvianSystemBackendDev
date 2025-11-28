using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
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
    [EnableCors("AllowSpecific")]
    public class IGDAssessmentAwalController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<IGDAssessmentAwalController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly string _uploadUrl;
        private readonly ITTDService _ttdService;
        private readonly IHubContext<IGDAssessmentAwalHub> _hubContext;

        public IGDAssessmentAwalController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IGDAssessmentAwalController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IHubContext<IGDAssessmentAwalHub> hubContext,
            ITTDService ttdService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            //_uploadUrl = configuration["FileStorage:UploadUrl"];
            _hubContext = hubContext;
            _ttdService = ttdService;
        }

        // ====================== GET ALL ======================
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.IGDAssessmentAwals
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        where a.IsDelete == false || a.IsDelete == null
                        orderby a.CreateDateTime descending
                        select new
                        {
                            a.AssessmentAwalIGD,
                            a.KunjunganId,
                            a.IsSpritualPenting,
                            a.IsMenngikutiKegiatanSpritual,
                            a.DataSubjektif,
                            a.DataObjektif,
                            a.KebutuhanTransportasi,
                            a.StatusKehamilan,
                            a.TTDPerawatId,
                            a.TTDPath,
                            a.CreateDateTime,
                            CreateByName = u.FullName
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var list = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

            if (!list.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = list,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        // ====================== GET BY ID ======================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await (from a in _applicationDbContext.IGDAssessmentAwals
                              join u in _applicationDbContext.UserActives
                                  on a.CreateBy equals u.UserActiveId into userJoin
                              from u in userJoin.DefaultIfEmpty()
                              where a.AssessmentAwalIGD == id && (a.IsDelete == false || a.IsDelete == null)
                              select new
                              {
                                  a.AssessmentAwalIGD,
                                  a.KunjunganId,
                                  a.IsSpritualPenting,
                                  a.IsMenngikutiKegiatanSpritual,
                                  a.DataSubjektif,
                                  a.DataObjektif,
                                  a.KebutuhanTransportasi,
                                  a.StatusKehamilan,
                                  a.TTDPerawatId,
                                  a.TTDPath,
                                  a.CreateDateTime,
                                  CreateByName = u.FullName
                              }).FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan || 404 Not Found" });

            return Ok(new { message = "Berhasil || 200 OK", data });
        }

        // ====================== CREATE ======================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IGDAssessmentAwalViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.TTDPerawatId);

                // ✅ Upload TTD jika ada
                //if (vm.TTDFile != null && vm.TTDFile.Length > 0)
                //{
                //    var maxSize = 1 * 1024 * 1024; // max 1MB
                //    var allowedExt = new List<string> { ".jpg", ".jpeg" };
                //    var ext = Path.GetExtension(vm.TTDFile.FileName).ToLower();

                //    if (vm.TTDFile.Length > maxSize)
                //        return BadRequest(new { message = "Ukuran file terlalu besar (maksimal 1MB)." });

                //    if (!allowedExt.Contains(ext))
                //        return BadRequest(new { message = "Format file tidak valid (harus JPG atau JPEG)." });

                //    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                //    var fileName = $"{user.FullName}_{safeTime}_IGDAssessmentAwal{ext}";

                //    using var client = new HttpClient();
                //    using var ms = new MemoryStream();
                //    await vm.TTDFile.CopyToAsync(ms);
                //    ms.Position = 0;

                //    var content = new MultipartFormDataContent {
                //        { new StreamContent(ms) {
                //            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
                //        }, "file", fileName },
                //        { new StringContent("TTDIGD"), "folderTarget" }
                //    };

                //    var response = await client.PostAsync(_uploadUrl, content);
                //    if (!response.IsSuccessStatusCode)
                //        return StatusCode(500, new { message = "Gagal upload tanda tangan ke Flask." });

                //    var body = await response.Content.ReadAsStringAsync();
                //    dynamic json = JsonConvert.DeserializeObject(body);
                //    ttdPath = json?.url ?? json?.fileUrl ?? json?.path ?? "";
                //}

                // ✅ Simpan ke database
                var data = new IGDAssessmentAwal
                {
                    AssessmentAwalIGD = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    IsSpritualPenting = vm.IsSpritualPenting,
                    IsMenngikutiKegiatanSpritual = vm.IsMenngikutiKegiatanSpritual,
                    DataSubjektif = vm.DataSubjektif,
                    DataObjektif = vm.DataObjektif,
                    KebutuhanTransportasi = vm.KebutuhanTransportasi,
                    StatusKehamilan = vm.StatusKehamilan,
                    TTDPerawatId = vm.TTDPerawatId,
                    TTDPath = ttd.Path,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.IGDAssessmentAwals.Add(data);
                await _applicationDbContext.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("IGD Assessment awal Created", new
                {
                    Action = "create",
                    data = data.AssessmentAwalIGD,
                    ttdId = ttd.TTDId
                });

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // ====================== UPDATE ======================
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] IGDAssessmentAwalViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var existing = await _applicationDbContext.IGDAssessmentAwals.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
            if (user == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            // cek ttd
            var ttd = await _ttdService.CheckTTDAsync((Guid)vm.TTDPerawatId);

            // ✅ Upload baru jika ada file baru
            //if (vm.TTDFile != null && vm.TTDFile.Length > 0)
            //{
            //    var ext = Path.GetExtension(vm.TTDFile.FileName).ToLower();
            //    if (ext != ".jpg" && ext != ".jpeg")
            //        return BadRequest(new { message = "Format TTD tidak valid (harus JPG/JPEG)." });

            //    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            //    var fileName = $"{user.FullName}_{safeTime}_IGDAssessmentAwal{ext}";

            //    using var client = new HttpClient();
            //    using var ms = new MemoryStream();
            //    await vm.TTDFile.CopyToAsync(ms);
            //    ms.Position = 0;

            //    var content = new MultipartFormDataContent {
            //        { new StreamContent(ms) {
            //            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.TTDFile.ContentType) }
            //        }, "file", fileName },
            //        { new StringContent("TTDIGD"), "folderTarget" }
            //    };

            //    var response = await client.PostAsync(_uploadUrl, content);
            //    if (!response.IsSuccessStatusCode)
            //        return StatusCode(500, new { message = "Gagal upload tanda tangan ke Flask." });

            //    var body = await response.Content.ReadAsStringAsync();
            //    dynamic json = JsonConvert.DeserializeObject(body);
            //    ttdPath = json?.url ?? json?.fileUrl ?? json?.path ?? "";
            //}

            // ✅ Update field
            existing.KunjunganId = vm.KunjunganId;
            existing.IsSpritualPenting = vm.IsSpritualPenting;
            existing.IsMenngikutiKegiatanSpritual = vm.IsMenngikutiKegiatanSpritual;
            existing.DataSubjektif = vm.DataSubjektif;
            existing.DataObjektif = vm.DataObjektif;
            existing.KebutuhanTransportasi = vm.KebutuhanTransportasi;
            existing.StatusKehamilan = vm.StatusKehamilan;
            existing.TTDPerawatId = vm.TTDPerawatId;
            existing.TTDPath = ttd.Path;
            existing.UpdateBy = user.UserActiveId;
            existing.UpdateDateTime = DateTimeOffset.UtcNow;

            _applicationDbContext.IGDAssessmentAwals.Update(existing);
            await _applicationDbContext.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("IGD assessment awal changed", new
            {
                Action = "create",
                id = existing.AssessmentAwalIGD,
                ttdId = ttd.TTDId
            });

            return Ok(new { message = "Update Data Berhasil || 200 OK" });
        }

        // ====================== DELETE (SOFT DELETE) ======================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _applicationDbContext.IGDAssessmentAwals.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
            if (user == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            existing.IsDelete = true;
            existing.DeleteBy = user.UserActiveId;
            existing.DeleteDateTime = DateTimeOffset.UtcNow;

            _applicationDbContext.IGDAssessmentAwals.Update(existing);
            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
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
            var query = from a in _applicationDbContext.IGDAssessmentAwals
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        where a.IsDelete == false || a.IsDelete == null
                        orderby a.CreateDateTime descending
                        select new
                        {
                            a.AssessmentAwalIGD,
                            a.KunjunganId,
                            a.IsSpritualPenting,
                            a.IsMenngikutiKegiatanSpritual,
                            a.DataSubjektif,
                            a.DataObjektif,
                            a.KebutuhanTransportasi,
                            a.StatusKehamilan,
                            a.TTDPath,
                            a.CreateDateTime,
                            CreateByName = u.FullName
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            //kunjungan id
            if (kunjunganid.HasValue && kunjunganid != Guid.Empty)
            {
                query = query.Where(u => u.KunjunganId == kunjunganid);
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

