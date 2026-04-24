using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]

    public class SatuanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<SatuanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SatuanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SatuanController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Satuan
        [HttpGet]
        public async Task<IActionResult> GetAllSatuan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from b in _applicationDbContext.Satuans
                        select new
                        {
                            SatuanId = b.SatuanId,
                            KodeSatuan = b.KodeSatuan,
                            NamaSatuan = b.NamaSatuan,
                            b.SingkatanSatuan,
                            b.EnSatuan,
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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

        // GET: api/Satuan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSatuanById(Guid id)
        {
            var bentukObat = await _applicationDbContext.Satuans
                .FirstOrDefaultAsync(b => b.SatuanId == id);

            if (bentukObat == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = bentukObat
            });
        }

        // POST: api/Satuan
        [HttpPost]
        public async Task<IActionResult> CreateSatuan([FromBody] SatuanViewModel satuanViewModel)
        {
            if (satuanViewModel == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Ambil User ID dari JWT Claims
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

                // Mendapatkan tanggal sekarang
                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Menentukan KodeSatuan berdasarkan tanggal dan urutan
                var lastCode = await _applicationDbContext.Satuans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(s => s.KodeSatuan)
                    .FirstOrDefaultAsync();

                string KodeSatuan;
                if (lastCode == null || lastCode.KodeSatuan.Substring(3, 6) != setDateNow)
                {
                    KodeSatuan = $"SNG{setDateNow}0001"; // Format kode satuan baru
                }
                else
                {
                    int lastNumber = Convert.ToInt32(lastCode.KodeSatuan.Substring(9));
                    KodeSatuan = $"SNG{setDateNow}{(lastNumber + 1).ToString("D4")}";
                }

                // Cek jika sudah ada data yang sama berdasarkan KodeSatuan
                var isDuplicate = await _applicationDbContext.Satuans
                    .AnyAsync(s => s.NamaSatuan.ToLower().Trim() == satuanViewModel.NamaSatuan.ToLower().Trim() && !s.IsDelete );

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data dengan kode satuan yang sama sudah ada || 409 Conflict Data" });
                }

                // Convert ViewModel ke Entity Satuan
                var satuan = new Satuan
                {
                    SatuanId = Guid.NewGuid(),
                    KodeSatuan = KodeSatuan,  // Gunakan kode yang sudah dihasilkan
                    NamaSatuan = satuanViewModel.NamaSatuan,
                    SingkatanSatuan = satuanViewModel.SingkatanSatuan,
                    EnSatuan = satuanViewModel.EnSatuan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Insert data baru ke database
                _applicationDbContext.Satuans.Add(satuan);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // PUT: api/Satuan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSatuan(Guid id, [FromBody] Satuan bentukObat)
        {
            if (bentukObat == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Ambil User ID dari JWT Claims
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

                // Cari data yang ingin diupdate
                var data = await _applicationDbContext.Satuans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Cek duplikasi berdasarkan NamaSatuan
                bool isDuplicate = await _applicationDbContext.Satuans
                    .AnyAsync(b => b.NamaSatuan.ToLower() == bentukObat.NamaSatuan.ToLower() && b.SatuanId != id && !b.IsDelete);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Update data
                data.KodeSatuan = bentukObat.KodeSatuan;
                data.NamaSatuan = bentukObat.NamaSatuan;
                data.SingkatanSatuan = bentukObat.SingkatanSatuan;
                data.EnSatuan = bentukObat.EnSatuan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Satuans.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/Satuan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSatuan(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.Satuans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                _applicationDbContext.Satuans.Remove(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedSatuan(
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
            var query = from b in _applicationDbContext.Satuans
                        select new
                        {
                            CreateDateTime = b.CreateDateTime,
                            SatuanId = b.SatuanId,
                            KodeSatuan = b.KodeSatuan,
                            NamaSatuan = b.NamaSatuan,
                            b.SingkatanSatuan,
                            b.EnSatuan
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeSatuan, search) ||
                    EF.Functions.ILike(u.NamaSatuan, search)
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
                    "NamaSatuan" => query.OrderByDescending(u => u.NamaSatuan),
                    "KodeSatuan" => query.OrderByDescending(u => u.KodeSatuan),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "NamaSatuan" => query.OrderByDescending(u => u.NamaSatuan),
                    "KodeSatuan" => query.OrderByDescending(u => u.KodeSatuan),
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
