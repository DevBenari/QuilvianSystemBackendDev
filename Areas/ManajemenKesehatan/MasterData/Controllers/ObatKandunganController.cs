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

            var query = from ok in _applicationDbContext.ObatKandungans
                        join o in _applicationDbContext.Obats on ok.ObatId equals o.ObatId
                        join k in _applicationDbContext.Kandungans on ok.KandunganId equals k.KandunganId
                        select new
                        {
                            ObatKandunganId = ok.ObatKandunganId,
                            ObatId = ok.ObatId,
                            ObatName = o.ObatName,
                            KandunganId = ok.KandunganId,
                            KandunganName = k.NamaKandungan
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
        public async Task<IActionResult> CreateObatKandungan([FromBody] ObatKandungan obatKandungan)
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

                // Insert data baru
                obatKandungan.ObatKandunganId = Guid.NewGuid();
                _applicationDbContext.ObatKandungans.Add(obatKandungan);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
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
                // Cari data yang ingin diupdate
                var data = await _applicationDbContext.ObatKandungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Update data
                data.ObatId = obatKandungan.ObatId;
                data.KandunganId = obatKandungan.KandunganId;

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
    }
}
