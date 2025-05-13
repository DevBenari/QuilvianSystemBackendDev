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
using System.Data;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ObatKandunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ObatKandunganController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _applicationDbContext = context;
            _userManager = userManager;
        }

        // GET: api/ObatKandungan
        [HttpGet]
        public async Task<IActionResult> GetAllObatKandungan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from ok in _applicationDbContext.ObatKandungans
                        join o in _applicationDbContext.Obats on ok.ObatId equals o.ObatId
                        join k in _applicationDbContext.Kandungans on ok.KandunganId equals k.KandunganId
                        select new
                        {
                            CreateDateTime = ok.CreateDateTime,
                            ObatKandunganId = ok.ObatKandunganId,
                            ObatId = ok.ObatId,
                            ObatName = o.ObatName,
                            KandunganId = ok.KandunganId,
                            KandunganName = k.NamaKandungan
                        }).OrderByDescending(a => a.CreateDateTime);

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

        // GET: api/ObatKandungan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetObatKandunganById(Guid id)
        {
            var obatKandungan = await _applicationDbContext.ObatKandungans
                .FirstOrDefaultAsync(ok => ok.ObatKandunganId == id);

            if (obatKandungan == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = obatKandungan
            });
        }

        // POST: api/ObatKandungan
        [HttpPost]
        public async Task<IActionResult> CreateObatKandungan([FromBody] ObatKandunganViewModel obatKandungan)
        {
            if (obatKandungan == null)
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

                // Cek jika sudah ada hubungan antara Obat dan Kandungan yang sama
                var isDuplicate = await _applicationDbContext.ObatKandungans
                    .AnyAsync(ok => ok.ObatId == obatKandungan.ObatId && ok.KandunganId == obatKandungan.KandunganId);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data sudah ada || 409 Conflict Data" });
                }

                // Perbaiki penamaan variabel untuk membuat ObatKandungan baru
                var obatKandunganEntity = new ObatKandungan
                {
                    ObatKandunganId = Guid.NewGuid(),
                    ObatId = obatKandungan.ObatId,
                    KandunganId = obatKandungan.KandunganId,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Menambahkan entitas obat kandungan ke database
                _applicationDbContext.ObatKandungans.Add(obatKandunganEntity);
                await _applicationDbContext.SaveChangesAsync();

                // Kembalikan respons bahwa data berhasil dibuat
                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                // Tangani kesalahan internal dengan memberikan respons yang sesuai
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // PUT: api/ObatKandungan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObatKandungan(Guid id, [FromBody] ObatKandungan obatKandungan)
        {
            if (obatKandungan == null)
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
                var data = await _applicationDbContext.ObatKandungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Update data
                data.ObatId = obatKandungan.ObatId;
                data.KandunganId = obatKandungan.KandunganId;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.ObatKandungans.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/ObatKandungan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObatKandungan(Guid id)
        {
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

                var data = await _applicationDbContext.ObatKandungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                _applicationDbContext.ObatKandungans.Remove(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
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
            var query = from ok in _applicationDbContext.ObatKandungans
                        join o in _applicationDbContext.Obats on ok.ObatId equals o.ObatId
                        join k in _applicationDbContext.Kandungans on ok.KandunganId equals k.KandunganId
                        select new
                        {
                            ObatKandunganId = ok.ObatKandunganId,
                            ObatId = ok.ObatId,
                            ObatName = o.ObatName,
                            KandunganId = ok.KandunganId,
                            KandunganName = k.NamaKandungan,
                            o.CreateDateTime
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.ObatName, search) ||
                    EF.Functions.ILike(u.KandunganName, search)
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
                    "ObatName" => query.OrderByDescending(u => u.ObatName),
                    "KandunganName" => query.OrderByDescending(u => u.KandunganName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "ObatName" => query.OrderByDescending(u => u.ObatName),
                    "KandunganName" => query.OrderByDescending(u => u.KandunganName),
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
