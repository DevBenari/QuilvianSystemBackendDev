using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ObatAsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ObatAsuransiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _applicationDbContext = context;
            _userManager = userManager;
        }

        // GET: api/ObatAsuransi
        [HttpGet]
        public async Task<IActionResult> GetAllObatAsuransi(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from oa in _applicationDbContext.ObatAsuransis
                         join o in _applicationDbContext.Obats on oa.ObatId equals o.ObatId
                         join a in _applicationDbContext.Asuransis on oa.AsuransiId equals a.AsuransiId
                         select new
                         {
                             oa.CreateDateTime,
                             oa.ObatAsuransiId,
                             oa.ObatId,
                             o.ObatName,
                             oa.AsuransiId,
                             AsuransiName = a.NamaAsuransi,
                             // ============================
                             // MARKUP
                             // ============================
                             oa.MarkupDokter,
                             oa.MarkupRs,
                             oa.MarkupJp,
                             oa.MarkupBahp,
                             oa.MarkupLainnya,
                             oa.MarkupTotal,
                             oa.IsMarkupBerlaku,
                             oa.MarkupDari,
                             oa.MarkupSampai,

                             // ============================
                             // DISKON
                             // ============================
                             oa.DiskonDokter,
                             oa.DiskonRs,
                             oa.DiskonJp,
                             oa.DiskonBahp,
                             oa.DiskonTotal,
                             oa.IsDiskonBerlaku,
                             oa.DiskonDari,
                             oa.DiskonSampai,
                             
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

        // GET: api/ObatAsuransi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetObatAsuransiById(Guid id)
        {
            var obatAsuransi = await _applicationDbContext.ObatAsuransis
                .FirstOrDefaultAsync(oa => oa.ObatAsuransiId == id);

            if (obatAsuransi == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = obatAsuransi
            });
        }

        // POST: api/ObatAsuransi
        [HttpPost]
        public async Task<IActionResult> CreateObatAsuransi([FromBody] ObatAsuransiViewModel obatAsuransi)
        {
            if (obatAsuransi == null)
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

                // Cek jika sudah ada hubungan antara Obat dan Asuransi yang sama
                var isDuplicate = await _applicationDbContext.ObatAsuransis
                    .AnyAsync(oa => oa.ObatId == obatAsuransi.ObatId && oa.AsuransiId == obatAsuransi.AsuransiId && oa.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data sudah ada || 409 Conflict Data" });
                }

                // Convert to entity model
                var obatAsuransiEntity = new ObatAsuransi
                {
                    ObatAsuransiId = Guid.NewGuid(),
                    ObatId = obatAsuransi.ObatId,
                    AsuransiId = obatAsuransi.AsuransiId,
                    // ============================
                    // MARKUP
                    // ============================
                    MarkupDokter = obatAsuransi.MarkupDokter,
                    MarkupRs = obatAsuransi.MarkupRs,
                    MarkupJp = obatAsuransi.MarkupJp,
                    MarkupBahp = obatAsuransi.MarkupBahp,
                    MarkupLainnya = obatAsuransi.MarkupLainnya,
                    MarkupTotal = obatAsuransi.MarkupTotal,

                    IsMarkupBerlaku = obatAsuransi.IsMarkupBerlaku ,
                    MarkupDari = obatAsuransi.MarkupDari,
                    MarkupSampai = obatAsuransi.MarkupSampai,

                    // ============================
                    // DISKON
                    // ============================
                    DiskonDokter = obatAsuransi.DiskonDokter,
                    DiskonRs = obatAsuransi.DiskonRs,
                    DiskonJp = obatAsuransi.DiskonJp,
                    DiskonBahp = obatAsuransi.DiskonBahp,
                    DiskonTotal = obatAsuransi.DiskonTotal,

                    IsDiskonBerlaku = obatAsuransi.IsDiskonBerlaku,
                    DiskonDari = obatAsuransi.DiskonDari,
                    DiskonSampai = obatAsuransi.DiskonSampai,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // Insert data baru ke database
                _applicationDbContext.ObatAsuransis.Add(obatAsuransiEntity);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // PUT: api/ObatAsuransi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObatAsuransi(Guid id, [FromBody] ObatAsuransi obatAsuransi)
        {
            if (obatAsuransi == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cari data yang ingin diupdate
                var data = await _applicationDbContext.ObatAsuransis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Cek jika sudah ada hubungan antara Obat dan Asuransi yang sama
                var isDuplicate = await _applicationDbContext.ObatAsuransis
                    .AnyAsync(oa => oa.ObatId == obatAsuransi.ObatId && oa.AsuransiId == obatAsuransi.AsuransiId && oa.IsDelete == false
                    && oa.ObatAsuransiId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data sudah ada || 409 Conflict Data" });
                }
                // Update data
                data.ObatId = obatAsuransi.ObatId;
                data.AsuransiId = obatAsuransi.AsuransiId;
                // ============================
                // MARKUP
                // ============================
                data.MarkupDokter = obatAsuransi.MarkupDokter;
                data.MarkupRs = obatAsuransi.MarkupRs;
                data.MarkupJp = obatAsuransi.MarkupJp;
                data.MarkupBahp = obatAsuransi.MarkupBahp;
                data.MarkupLainnya = obatAsuransi.MarkupLainnya;
                data.MarkupTotal = obatAsuransi.MarkupTotal;

                data.IsMarkupBerlaku = obatAsuransi.IsMarkupBerlaku;
                data.MarkupDari = obatAsuransi.MarkupDari;
                data.MarkupSampai = obatAsuransi.MarkupSampai;

                // ============================
                // DISKON
                // ============================
                data.DiskonDokter = obatAsuransi.DiskonDokter;
                data.DiskonRs = obatAsuransi.DiskonRs;
                data.DiskonJp = obatAsuransi.DiskonJp;
                data.DiskonBahp = obatAsuransi.DiskonBahp;
                data.DiskonTotal = obatAsuransi.DiskonTotal;

                data.IsDiskonBerlaku = obatAsuransi.IsDiskonBerlaku;
                data.DiskonDari = obatAsuransi.DiskonDari;
                data.DiskonSampai = obatAsuransi.DiskonSampai;


                _applicationDbContext.ObatAsuransis.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/ObatAsuransi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObatAsuransi(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.ObatAsuransis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                _applicationDbContext.ObatAsuransis.Remove(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
