using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using QuilvianSystemBackendDev.Models;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KandunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public KandunganController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _applicationDbContext = context;
            _userManager = userManager;
        }

        // GET: api/Kandungan
        [HttpGet]
        public async Task<IActionResult> GetAllKandungan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from k in _applicationDbContext.Kandungans
                        select new
                        {
                            KandunganId = k.KandunganId,
                            KodeKandungan = k.KodeKandungan,
                            NamaKandungan = k.NamaKandungan
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

        // GET: api/Kandungan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKandunganById(Guid id)
        {
            var kandungan = await _applicationDbContext.Kandungans
                .FirstOrDefaultAsync(k => k.KandunganId == id);

            if (kandungan == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = kandungan
            });
        }

        // POST: api/Kandungan
        [HttpPost]
        public async Task<IActionResult> CreateKandungan([FromBody] KandunganViewModel kandunganViewModel)
        {
            if (kandunganViewModel == null)
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

                // Menentukan KodeKandungan berdasarkan tanggal dan urutan
                var lastCode = await _applicationDbContext.Kandungans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeKandungan)
                    .FirstOrDefaultAsync();

                string KodeKandungan;
                if (lastCode == null || lastCode.KodeKandungan.Substring(3, 6) != setDateNow)
                {
                    KodeKandungan = $"KDG{setDateNow}0001";
                }
                else
                {
                    int lastNumber = Convert.ToInt32(lastCode.KodeKandungan.Substring(9));
                    KodeKandungan = $"KDG{setDateNow}{(lastNumber + 1).ToString("D4")}";
                }

                // Cek jika sudah ada data yang sama berdasarkan KodeKandungan
                var isDuplicate = await _applicationDbContext.Kandungans
                    .AnyAsync(k => k.KodeKandungan == KodeKandungan && k.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data dengan kode kandungan yang sama sudah ada || 409 Conflict Data" });
                }

                // Convert ViewModel ke Entity Kandungan
                var kandungan = new Kandungan
                {
                    KandunganId = Guid.NewGuid(),
                    KodeKandungan = KodeKandungan,  // Gunakan kode yang sudah dihasilkan
                    NamaKandungan = kandunganViewModel.NamaKandungan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Insert data baru ke database
                _applicationDbContext.Kandungans.Add(kandungan);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // PUT: api/Kandungan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKandungan(Guid id, [FromBody] Kandungan kandungan)
        {
            if (kandungan == null)
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

                // Cari data yang ingin diupdate
                var data = await _applicationDbContext.Kandungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Cek duplikasi berdasarkan NamaKandungan
                bool isDuplicate = await _applicationDbContext.Kandungans
                    .AnyAsync(k => k.NamaKandungan.ToLower() == kandungan.NamaKandungan.ToLower() 
                    && k.KandunganId != id && k.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Update data
                data.KodeKandungan = kandungan.KodeKandungan;
                data.NamaKandungan = kandungan.NamaKandungan;
                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Kandungans.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/Kandungan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKandungan(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.Kandungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                _applicationDbContext.Kandungans.Remove(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedKandungan(
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
            var query = from k in _applicationDbContext.Kandungans
                        select new
                        {
                            KandunganId = k.KandunganId,
                            KodeKandungan = k.KodeKandungan,
                            NamaKandungan = k.NamaKandungan,
                            CreateDateTime = k.CreateDateTime,
                            k.CreateBy
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeKandungan, search) ||
                    EF.Functions.ILike(u.NamaKandungan, search)
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
                    "CreateByName" => query.OrderByDescending(u => u.CreateBy),
                    "NamaKandungan" => query.OrderByDescending(u => u.NamaKandungan),
                    "KodeKandungan" => query.OrderByDescending(u => u.KodeKandungan),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateBy),
                    "NamaKandungan" => query.OrderByDescending(u => u.NamaKandungan),
                    "KodeKandungan" => query.OrderByDescending(u => u.KodeKandungan),
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
