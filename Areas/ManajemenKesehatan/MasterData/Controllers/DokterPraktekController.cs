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
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DokterPraktekController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DokterPraktekController
            (
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                ILogger<PendaftaranPasienBaruController> logger,
                IWebHostEnvironment webHostEnvironment
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

        }

        // GET: api/DokterPraktek
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.DokterPrakteks
                .Include(dp => dp.Dokters)  // Include relasi dengan tabel Dokters
                .ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/DokterPraktek/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _context.DokterPrakteks
                .Include(dp => dp.Dokters)
                .FirstOrDefaultAsync(dp => dp.DokterPraktekId == id);

            if (result == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }

            return Ok(result);
        }

        // POST: api/DokterPraktek
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DokterPraktekViewModel model)
        {
            //validate modelstate
            if (ModelState.IsValid)
            {
                var dokterPraktek = new DokterPraktek
                {
                    DokterPraktekId = Guid.NewGuid(),
                    Dokter = model.Dokter,
                    Layanan = model.Layanan,
                    JamPraktek = model.JamPraktek,
                    Hari = model.Hari,
                    JamMasuk = model.JamMasuk,
                    JamKeluar = model.JamKeluar,
                    DokterId = model.DokterId,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };
                var checkDuplicate = _context.DokterPrakteks.Where(c => c.DokterId == model.DokterId && c.Dokter == model.Dokter).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.DokterPrakteks.Where(c => c.DokterId == model.DokterId && c.Dokter == model.Dokter).FirstOrDefault();
                    if (result == null)
                    {
                        _context.DokterPrakteks.Add(dokterPraktek);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
                    }
                    else
                    {
                        return BadRequest(new { message = "Data tidak dapat di input !!! || 400 Bad Request" });
                    }
                }
                else
                {
                    return Conflict(new { message = "Terdapat duplikasi data !!! || 409 Conflict Data" });
                }
            }
            else
            {
                return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });

            }
        }

        // PUT: api/DokterPraktek/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DokterPraktekViewModel model)
        {

            //cek apakah data ada di database
            var existingDokterPraktek = await _context.DokterPrakteks.FindAsync(id);
            if (existingDokterPraktek == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.DokterPrakteks.Where
                (c => c.DokterId == model.DokterId && c.Dokter == model.Dokter
                && c.DokterPraktekId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingDokterPraktek.Dokter = model.Dokter;
            existingDokterPraktek.Layanan = model.Layanan;
            existingDokterPraktek.JamPraktek = model.JamPraktek;
            existingDokterPraktek.Hari = model.Hari;
            existingDokterPraktek.JamMasuk = model.JamMasuk;
            existingDokterPraktek.JamKeluar = model.JamKeluar;
            existingDokterPraktek.UpdateDateTime = DateTimeOffset.Now;
            existingDokterPraktek.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.DokterPrakteks.Update(existingDokterPraktek);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
            //if (model == null || id != model.DokterPraktekId)
            //{
            //    return BadRequest(new { message = "Data tidak valid." });
            //}
            //var existingRecord = await _context.DokterPrakteks.FindAsync(id);
            //if (existingRecord == null)
            //{
            //    return NotFound(new { message = "Data tidak ditemukan." });
            //}
            //// Update properties
            //foreach (var prop in model.GetType().GetProperties())
            //{
            //    var value = prop.GetValue(model);
            //    if (value != null)
            //    {
            //        prop.SetValue(existingRecord, value);
            //    }
            //}

            //_context.DokterPrakteks.Update(existingRecord);
            //await _context.SaveChangesAsync();

            //return Ok(new { message = "Data berhasil diperbarui." });
        }

        // DELETE: api/DokterPraktek/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.DokterPrakteks.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.DokterPrakteks.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }

        [HttpGet("paged")]
        public IActionResult PagedDokterPraktek(
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

            var query = _context.DokterPrakteks.AsQueryable();

            // 🔍 Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Dokter.Contains(search) ||
                                         u.Layanan.Contains(search) ||
                                         u.Hari.Contains(search));
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
