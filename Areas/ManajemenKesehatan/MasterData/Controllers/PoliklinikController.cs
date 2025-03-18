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
    public class PoliklinikController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PoliklinikController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PoliklinikController
            (ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PoliklinikController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPoliklinik(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Polikliniks
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PoliklinikId = a.PoliklinikId,
                            KodePoliklinik = a.KodePoliklinik,
                            NamaPoliklinik = a.NamaPoliklinik,
                            KepalaPoliklinik = a.KepalaPoliklinik,
                            Lokasi = a.Lokasi,
                            Telepon = a.Telepon,
                            Email = a.Email,
                            JamBuka = a.JamBuka,
                            JamTutup = a.JamTutup,
                            LayananPoliklinik = a.LayananPoliklinik,
                            Deskripsi = a.Deskripsi,
                            HariOperasional = a.HariOperasional,
                            JumlahMaxPasien = a.JumlahMaxPasien,

                            //SubPolis = a.SubPolis
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
        public async Task<IActionResult> GetPoliklinikById(Guid id)
        {
            var listdata = _applicationDbContext.Polikliniks.Find(id);
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
        public async Task<IActionResult> CreatePoliklinik([FromBody] PoliklinikViewModel vm)
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

                var dateNow = DateTimeOffset.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.Polikliniks
                    .Where(d => d.CreateDateTime.Date == dateNow.UtcDateTime.Date)
                    .OrderByDescending(k => k.KodePoliklinik)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"POL{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodePoliklinik.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"POL{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"POL{setDateNow}" + (Convert.ToInt32(lastCode.KodePoliklinik.Substring(9)) + 1).ToString("D4");
                    }
                }

                // cek duplikasi
                var isDuplicate = _applicationDbContext.Polikliniks
                    .Any(c => c.KodePoliklinik == kode && c.NamaPoliklinik == vm.NamaPoliklinik);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Validate ModelState
                if (ModelState.IsValid)
                {
                    var data = new Poliklinik
                    {
                        PoliklinikId = Guid.NewGuid(),
                        KodePoliklinik = kode,
                        NamaPoliklinik = vm.NamaPoliklinik,
                        KepalaPoliklinik = vm.KepalaPoliklinik,
                        Lokasi = vm.Lokasi,
                        Telepon = vm.Telepon,
                        Email = vm.Email,
                        HariOperasional = vm.HariOperasional,
                        JamBuka = vm.JamBuka,
                        JamTutup = vm.JamTutup,
                        JumlahMaxPasien = vm.JumlahMaxPasien,
                        LayananPoliklinik = vm.LayananPoliklinik,
                        Deskripsi = vm.Deskripsi,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        UpdateDateTime = DateTimeOffset.UtcNow,
                        UpdateBy = UserActiveId,
                        DeleteDateTime = DateTimeOffset.UtcNow,
                        DeleteBy = UserActiveId,
                        IsDelete = false
                    };

                    _applicationDbContext.Polikliniks.Add(data);
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
        public async Task<IActionResult> UpdatePoliklinik(Guid id, [FromBody] PoliklinikViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }
            var data = _applicationDbContext.Polikliniks.Find(id);
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
                // cek duplikasi
                var isDuplicate = _applicationDbContext.Polikliniks
                    .Any(c => c.PoliklinikId != id && c.NamaPoliklinik == vm.NamaPoliklinik);
                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }
                // Validate ModelState
                if (ModelState.IsValid)
                {
                    data.NamaPoliklinik = vm.NamaPoliklinik;
                    data.KepalaPoliklinik = vm.KepalaPoliklinik;
                    data.Lokasi = vm.Lokasi;
                    data.Telepon = vm.Telepon;
                    data.Email = vm.Email;
                    data.JamBuka = vm.JamBuka;
                    data.JamTutup = vm.JamTutup;
                    data.LayananPoliklinik = vm.LayananPoliklinik;
                    data.UpdateDateTime = DateTimeOffset.UtcNow;
                    data.UpdateBy = UserActiveId;
                    data.JumlahMaxPasien = vm.JumlahMaxPasien;
                    data.Deskripsi = vm.Deskripsi;

                    _applicationDbContext.Polikliniks.Update(data);
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
        public async Task<IActionResult> DeletePoliklinik(Guid id)
        {
            var data = _applicationDbContext.Polikliniks.Find(id);
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
                _applicationDbContext.Polikliniks.Update(data);
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
        public IActionResult PagedPoliklinik(
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
            var query = from a in _applicationDbContext.Polikliniks
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PoliklinikId = a.PoliklinikId,
                            KodePoliklinik = a.KodePoliklinik,
                            NamaPoliklinik = a.NamaPoliklinik,
                            KepalaPoliklinik = a.KepalaPoliklinik,
                            Lokasi = a.Lokasi,
                            Telepon = a.Telepon,
                            Email = a.Email,
                            JamBuka = a.JamBuka,
                            JamTutup = a.JamTutup,
                            LayananPoliklinik = a.LayananPoliklinik,
                            Deskripsi = a.Deskripsi,
                            HariOperasional = a.HariOperasional,
                            JumlahMaxPasien = a.JumlahMaxPasien,

                            //SubPolis = a.SubPolis
                        };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.KodePoliklinik.Contains(search) || u.NamaPoliklinik.Contains(search) || u.LayananPoliklinik.Contains(search)
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
                    "KodePoliklinik" => query.OrderByDescending(u => u.KodePoliklinik),
                    "NamaPoliklinik" => query.OrderByDescending(u => u.NamaPoliklinik),
                    "LayananPoliklinik" => query.OrderByDescending(u => u.LayananPoliklinik),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePoliklinik" => query.OrderByDescending(u => u.KodePoliklinik),
                    "NamaPoliklinik" => query.OrderByDescending(u => u.NamaPoliklinik),
                    "LayananPoliklinik" => query.OrderByDescending(u => u.LayananPoliklinik),
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
