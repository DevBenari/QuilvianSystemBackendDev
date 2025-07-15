using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using System.Security.Claims;
using SkiaSharp;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class JadwalPraktekController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<JadwalPraktekController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JadwalPraktekController
            (
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                ILogger<JadwalPraktekController> logger,
                IWebHostEnvironment webHostEnvironment
            )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

        }

        // GET: api/DokterPraktek
        [HttpGet]
        public async Task<IActionResult> GetAllDokterPraktek(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from a in _applicationDbContext.JadwalPrakteks
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId

                        join dp in _applicationDbContext.DokterPolis
                            on a.DokterPoliId equals dp.DokterPoliId

                        join d in _applicationDbContext.Dokters
                            on dp.DokterId equals d.DokterId

                        where a.IsDelete == false
                        select new
                        {
                            JadwalPraktekId = a.JadwalPraktekId,
                            CreateDateTime = a.CreateDateTime,
                            CreateByName = a.CreateBy,
                            UpdateDateTime = a.UpdateDateTime,
                            UpdateBy = a.UpdateBy,
                            DokterPoliId = a.DokterPoliId,
                            WaktuPraktek = a.WaktuPraktek,
                            HariPraktek = a.HariPraktek,
                            JamMulai = a.JamMulai,
                            JamBerakhir = a.JamBerakhir,
                            KodeJadwalPraktek = a.KodeJadwalPraktek,

                            // nama dokter
                            NamaDokter = d.NmDokter
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

        [HttpGet("JadwalByDokter/{DokterId}")]
        public IActionResult GetJadwalPraktekByDokterId(Guid DokterId)
        {
            var data = from jp in _applicationDbContext.JadwalPrakteks
                       join dp in _applicationDbContext.DokterPolis on jp.DokterPoliId equals dp.DokterPoliId
                       join d in _applicationDbContext.Dokters on dp.DokterId equals d.DokterId
                       join p in _applicationDbContext.Polikliniks on dp.PoliId equals p.PoliklinikId
                       where jp.IsDelete == false && dp.IsDelete == false && d.DokterId == DokterId
                       select new
                       {
                           jp.JadwalPraktekId,
                           jp.KodeJadwalPraktek,
                           jp.HariPraktek,
                           jp.WaktuPraktek,
                           jp.JamMulai,
                           jp.JamBerakhir,
                           NamaPoliklinik = p.NamaPoliklinik,
                           d.NmDokter,
                           d.KdDokter,
                           d.Email
                       };

            var result = data.ToList();
            if (!result.Any())
                return NotFound(new { message = "Jadwal tidak ditemukan untuk dokter ini." });

            return Ok(new
            {
                message = "Berhasil mengambil data jadwal",
                data = result
            });
        }

        // GET: api/JadwalPrakteks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.JadwalPrakteks.Find(id);
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

        // POST: api/JadwalPrakteks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JadwalPraktekViewModel vm)
        {

            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.JadwalPrakteks
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeJadwalPraktek)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"JDW{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeJadwalPraktek.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"JDW{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"JDW{setDateNow}" + (Convert.ToInt32(lastCode.KodeJadwalPraktek.Substring(9)) + 1).ToString("D4");
                    }
                }
                // cek duplikasi
                var isDuplicate = _applicationDbContext.JadwalPrakteks
                    .Any(c => c.KodeJadwalPraktek == kode);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var data = new JadwalPraktek
                    {
                        DokterPoliId = vm.DokterPoliId,
                        KodeJadwalPraktek = kode,
                        WaktuPraktek = vm.WaktuPraktek,
                        HariPraktek = vm.HariPraktek,
                        JamMulai = vm.JamMulai,
                        JamBerakhir = vm.JamBerakhir,
                        CreateBy = UserActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow

                    };

                    _applicationDbContext.JadwalPrakteks.Add(data);
                    _applicationDbContext.SaveChanges();
                    return Created("", new
                    {
                        message = "Data berhasil ditambahkan. || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // PUT: api/JadwalPraktek/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] JadwalPraktekViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }
            var data = _applicationDbContext.JadwalPrakteks.Find(id);
            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    data.DokterPoliId = vm.DokterPoliId;
                    data.WaktuPraktek = vm.WaktuPraktek;
                    data.HariPraktek = vm.HariPraktek;
                    data.JamMulai = vm.JamMulai;
                    data.JamBerakhir = vm.JamBerakhir;
                    data.UpdateBy = UserActiveId;
                    data.UpdateDateTime = DateTimeOffset.UtcNow;

                    _applicationDbContext.JadwalPrakteks.Update(data);
                    _applicationDbContext.SaveChanges();
                    return Ok(new
                    {
                        message = "Data berhasil diubah. || 200 OK",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch
            (Exception ex)
            {
                return BadRequest(new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/JadwalPrakteks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = _applicationDbContext.JadwalPrakteks.Find(id);
            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;
                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                data.IsDelete = true;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.DeleteBy = UserActiveId;
                _applicationDbContext.JadwalPrakteks.Update(data);
                _applicationDbContext.SaveChanges();
                return Ok(new
                {
                    message = "Data berhasil dihapus. || 200 OK",
                });
            }
            catch
            (Exception ex)
            {
                return BadRequest(new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedJadwalPraktek(
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
            var query = from a in _applicationDbContext.JadwalPrakteks
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId

                        join dp in _applicationDbContext.DokterPolis
                            on a.DokterPoliId equals dp.DokterPoliId

                        join p in _applicationDbContext.Polikliniks
                            on dp.PoliId equals p.PoliklinikId

                        join d in _applicationDbContext.Dokters
                            on dp.DokterId equals d.DokterId

                        where a.IsDelete == false
                        select new
                        {
                            JadwalPraktekId = a.JadwalPraktekId,
                            CreateDateTime = a.CreateDateTime,
                            CreateByName = a.CreateBy,
                            UpdateDateTime = a.UpdateDateTime,
                            UpdateBy = a.UpdateBy,
                            DokterPoliId = a.DokterPoliId,
                            PoliId = dp.PoliId,
                            NamaPoliklinik = p.NamaPoliklinik,
                            WaktuPraktek = a.WaktuPraktek,
                            HariPraktek = a.HariPraktek,
                            JamMulai = a.JamMulai,
                            JamBerakhir = a.JamBerakhir,
                            KodeJadwalPraktek = a.KodeJadwalPraktek,

                            // nama dokter
                            NamaDokter = d.NmDokter
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeJadwalPraktek, search) ||
                    EF.Functions.ILike(u.HariPraktek, search) ||
                    EF.Functions.ILike(u.NamaPoliklinik, search)
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
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
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
                    "KodeJadwalPraktek" => query.OrderByDescending(u => u.KodeJadwalPraktek),
                    "HariPraktek" => query.OrderByDescending(u => u.HariPraktek),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeJadwalPraktek" => query.OrderByDescending(u => u.KodeJadwalPraktek),
                    "HariPraktek" => query.OrderByDescending(u => u.HariPraktek),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
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