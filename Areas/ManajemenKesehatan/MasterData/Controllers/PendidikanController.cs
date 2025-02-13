using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PendidikanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendidikanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendidikanController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<PendidikanController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPendidikan()
        {
            var listdata = _applicationDbContext.Pendidikans.Where(a => a.IsDelete == false).ToList();
            if (listdata == null || !listdata.Any())
            {
                return NotFound(new { message = "Belum ada data. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPendidikanById(Guid id)
        {
            var listdata = _applicationDbContext.Pendidikans.Find(id);
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
        public async Task<IActionResult> CreatePendidikan([FromBody] PendidikanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
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

                var dateNow = DateTimeOffset.Now;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Pendidikans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodePendidikan)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"PDD{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodePendidikan.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"PDD{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"PDD{setDateNow}" + (Convert.ToInt32(lastCode.KodePendidikan.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.Pendidikans
                    .Any(c => c.KodePendidikan == kode && c.NamaPendidikan == vm.NamaPendidikan);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var data = new Pendidikan
                    {
                        PendidikanId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy = UserActiveId,
                        KodePendidikan = kode,
                        NamaPendidikan = vm.NamaPendidikan
                    };

                    _applicationDbContext.Pendidikans.Add(data);
                    _applicationDbContext.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePendidikan(Guid id, [FromBody] PendidikanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Pasien**
                var data = _applicationDbContext.Pendidikans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data Pasien**
                data.NamaPendidikan = vm.NamaPendidikan;

                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.Now;

                _applicationDbContext.Pendidikans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePendidikan(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Pasien**
                var data = _applicationDbContext.Pendidikans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.Now;
                data.IsDelete = true;

                _applicationDbContext.Pendidikans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PegedPendidikan(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "asc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "StartDate tidak boleh lebih besar dari EndDate." });
            }

            // Jika tidak menggunakan daterange, gunakan periode filter
            if (!startDate.HasValue && !endDate.HasValue && periode == null)
            {
                return BadRequest(new { message = "Harap pilih periode atau masukkan rentang tanggal yang valid." });
            }

            var query = _applicationDbContext.Pendidikans.AsQueryable();

            // 🔍 Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.KodePendidikan.Contains(search) ||
                                         u.NamaPendidikan.Contains(search));
            }

            // 📅 Filter berdasarkan daterange
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u => u.CreateDateTime.Date >= startDate.Value.Date &&
                                         u.CreateDateTime.Date <= endDate.Value.Date);
            }

            // 📆 Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u => u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                                                 u.CreateDateTime.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u => u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                                 u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek)));
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u => u.CreateDateTime.Month == today.Month &&
                                                 u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u => u.CreateDateTime.Month == today.Month - 1 &&
                                                 u.CreateDateTime.Year == today.Year);
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

            // Sorting Data
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, orderBy))
                    : query.OrderBy(e => EF.Property<object>(e, orderBy));
            }

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
