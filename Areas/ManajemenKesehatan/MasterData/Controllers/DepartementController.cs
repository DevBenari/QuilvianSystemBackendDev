using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DepartementController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DepartementController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public DepartementController
            (ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DepartementController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartement(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Departements
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DepartementId = a.DepartementId,
                            KodeDepartement = a.KodeDepartement,
                            NamaDepartement = a.NamaDepartement,
                            KepalaDepartement = a.KepalaDepartement,
                            Lokasi = a.Lokasi,
                            Telepon = a.Telepon,
                            Email = a.Email,
                            JamBuka = a.JamBuka,
                            JamTutup = a.JamTutup,
                            Layanan = a.Layanan,
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
        public async Task<IActionResult> GetDepartementById(Guid id)
        {
            var listdata = _applicationDbContext.Departements.Find(id);
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
        public async Task<IActionResult> CreateDepartement([FromBody] DepartementViewModel vm)
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

                var dateNow = DateTimeOffset.Now;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Departements
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeDepartement)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"DPT{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeDepartement.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"DPT{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"DPT{setDateNow}" + (Convert.ToInt32(lastCode.KodeDepartement.Substring(9)) + 1).ToString("D4");
                    }
                }

                // cek duplikasi
                var isDuplicate = _applicationDbContext.Departements
                    .Any(c => c.KodeDepartement == kode && c.NamaDepartement == vm.NamaDepartement);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var data = new Departement
                    {
                        DepartementId = Guid.NewGuid(),
                        KodeDepartement = kode,
                        NamaDepartement = vm.NamaDepartement,
                        KepalaDepartement = vm.KepalaDepartement,
                        Lokasi = vm.Lokasi,
                        Telepon = vm.Telepon,
                        Email = vm.Email,
                        JamBuka = vm.JamBuka,
                        JamTutup = vm.JamTutup,
                        Layanan = vm.Layanan,
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy = UserActiveId,
                        UpdateDateTime = DateTimeOffset.Now,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.Now,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.Departements.Add(data);
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartement(Guid id, [FromBody] DepartementViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }

            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "Anda tidak memiliki akses. || 401 Unauthorized" });
                }

                // **Cari Data**
                var data = _applicationDbContext.Departements.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.NamaDepartement = vm.NamaDepartement;
                data.KepalaDepartement = vm.KepalaDepartement;
                data.Telepon = vm.Telepon;
                data.Email = vm.Email;
                data.JamBuka = vm.JamBuka;
                data.JamTutup = vm.JamTutup;
                data.Layanan = vm.Layanan;
                data.UpdateDateTime = DateTimeOffset.Now;
                data.UpdateBy = UserActiveId;

                _applicationDbContext.Departements.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                });
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });

            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartement(Guid id)
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

                // **Cari Data**
                var data = _applicationDbContext.Departements.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.Now;
                data.IsDelete = true;

                _applicationDbContext.Departements.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedDepartement(
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
            var query = from a in _applicationDbContext.Departements
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DepartementId = a.DepartementId,
                            KodeDepartement = a.KodeDepartement,
                            NamaDepartement = a.NamaDepartement,
                            KepalaDepartement = a.KepalaDepartement,
                            Lokasi = a.Lokasi,
                            Telepon = a.Telepon,
                            Email = a.Email,
                            JamBuka = a.JamBuka,
                            JamTutup = a.JamTutup,
                            Layanan = a.Layanan,

                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.KodeDepartement.Contains(search) || u.NamaDepartement.Contains(search) || u.Layanan.Contains(search)
                    || u.Lokasi.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u =>
                    u.CreateDateTime.Date >= startDate.Value.Date &&
                    u.CreateDateTime.Date <= endDate.Value.Date
                );
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
                    "KodeDepartement" => query.OrderByDescending(u => u.KodeDepartement),
                    "NamaDepartement" => query.OrderByDescending(u => u.NamaDepartement),
                    "LayananDepartement" => query.OrderByDescending(u => u.Layanan),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeDepartement" => query.OrderByDescending(u => u.KodeDepartement),
                    "NamaDepartement" => query.OrderByDescending(u => u.NamaDepartement),
                    "LayananDepartement" => query.OrderByDescending(u => u.Layanan),
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
