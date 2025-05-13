using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class CoveranAsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<CoveranAsuransiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CoveranAsuransiController(ApplicationDbContext
            applicationDbContext, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CoveranAsuransiController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCoveranAsuransi(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.CoveranAsuransis
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            CoveranAsuransiId = a.CoveranAsuransiId,
                            KodeCoveranAsuransi = a.KodeCoveranAsuransi,
                            NamaAsuransi = a.NamaAsuransi,
                            ServiceCode = a.ServiceCode,
                            ServiceDesc = a.ServiceDesc,
                            ServiceCodeClass = a.ServiceCodeClass,
                            Class = a.Class,
                            IsSurgery = a.IsSurgery,
                            Tarif = a.Tarif,
                            TglBerlaku = a.TglBerlaku,
                            TglBerakhir = a.TglBerakhir,
                            IsPKS = a.IsPKS,
                            AsuransiId = a.AsuransiId
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
        public IActionResult GetCoveranAsuransi(Guid id)
        {
            var listdata = _applicationDbContext.CoveranAsuransis.Find(id);
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
        public async Task<IActionResult> CreateCoveranAsuransi([FromBody] CoveranAsuransiViewModel vm)
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

                var dateNow = DateTime.UtcNow; ;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.CoveranAsuransis
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeCoveranAsuransi)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"CVA{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeCoveranAsuransi.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"CVA{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"CVA{setDateNow}" + (Convert.ToInt32(lastCode.KodeCoveranAsuransi.Substring(9)) + 1).ToString("D4");
                    }
                }

                // cek duplikasi
                var isDuplicate = _applicationDbContext.CoveranAsuransis
                    .Any(c => c.KodeCoveranAsuransi == kode);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var data = new CoveranAsuransi
                    {
                        CoveranAsuransiId = Guid.NewGuid(),
                        KodeCoveranAsuransi = kode,
                        NamaAsuransi = vm.NamaAsuransi,
                        ServiceCode = vm.ServiceCode,
                        ServiceDesc = vm.ServiceDesc,
                        ServiceCodeClass = vm.ServiceCodeClass,
                        Class = vm.Class,
                        IsSurgery = vm.IsSurgery,
                        Tarif = vm.Tarif,
                        TglBerlaku = vm.TglBerlaku,
                        TglBerakhir = vm.TglBerakhir,
                        IsPKS = vm.IsPKS,
                        AsuransiId = vm.AsuransiId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,



                        IsDelete = false
                    };

                    _applicationDbContext.CoveranAsuransis.Add(data);
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
        public async Task<IActionResult> UpdateCoveranAsuransi(Guid id, [FromBody] CoveranAsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }
            var data = _applicationDbContext.CoveranAsuransis.Find(id);
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
                    data.NamaAsuransi = vm.NamaAsuransi;
                    data.ServiceCode = vm.ServiceCode;
                    data.ServiceDesc = vm.ServiceDesc;
                    data.ServiceCodeClass = vm.ServiceCodeClass;
                    data.Class = vm.Class;
                    data.IsSurgery = vm.IsSurgery;
                    data.Tarif = vm.Tarif;
                    data.TglBerlaku = vm.TglBerlaku;
                    data.TglBerakhir = vm.TglBerakhir;
                    data.IsPKS = vm.IsPKS;
                    data.AsuransiId = vm.AsuransiId;
                    data.UpdateDateTime = DateTimeOffset.UtcNow;
                    data.UpdateBy = UserActiveId;
                    _applicationDbContext.CoveranAsuransis.Update(data);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoveranAsuransi(Guid id)
        {
            var data = _applicationDbContext.CoveranAsuransis.Find(id);
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

                _applicationDbContext.CoveranAsuransis.Update(data);
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
        public IActionResult PagedCoveranAsuransi(
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
            // Query data
            var query = from a in _applicationDbContext.CoveranAsuransis
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            CoveranAsuransiId = a.CoveranAsuransiId,
                            KodeCoveranAsuransi = a.KodeCoveranAsuransi,
                            NamaAsuransi = a.NamaAsuransi,
                            ServiceCode = a.ServiceCode,
                            ServiceDesc = a.ServiceDesc,
                            ServiceCodeClass = a.ServiceCodeClass,
                            Class = a.Class,
                            IsSurgery = a.IsSurgery,
                            Tarif = a.Tarif,
                            TglBerlaku = a.TglBerlaku,
                            TglBerakhir = a.TglBerakhir,
                            IsPKS = a.IsPKS,
                            AsuransiId = a.AsuransiId
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeCoveranAsuransi, search) ||
                    EF.Functions.ILike(u.NamaAsuransi, search)
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
                    "KodeCoveranAsuransi" => query.OrderByDescending(u => u.KodeCoveranAsuransi),
                    "NamaAsuransi" => query.OrderByDescending(u => u.NamaAsuransi),
                    "ServiceDesc" => query.OrderByDescending(u => u.ServiceDesc),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodeCoveranAsuransi" => query.OrderByDescending(u => u.KodeCoveranAsuransi),
                    "NamaAsuransi" => query.OrderByDescending(u => u.NamaAsuransi),
                    "ServiceDesc" => query.OrderByDescending(u => u.ServiceDesc),
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
