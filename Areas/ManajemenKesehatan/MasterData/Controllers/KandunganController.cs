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
        public async Task<IActionResult> CreateKandungan([FromBody] Kandungan kandungan)
        {
            if (kandungan == null)
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

                // Cek jika sudah ada data yang sama berdasarkan KodeKandungan
                var isDuplicate = await _applicationDbContext.Kandungans
                    .AnyAsync(k => k.KodeKandungan == kandungan.KodeKandungan);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data dengan kode kandungan yang sama sudah ada || 409 Conflict Data" });
                }

                // Insert data baru
                kandungan.KandunganId = Guid.NewGuid();
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
                // Cari data yang ingin diupdate
                var data = await _applicationDbContext.Kandungans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Cek duplikasi berdasarkan NamaKandungan
                bool isDuplicate = await _applicationDbContext.Kandungans
                    .AnyAsync(k => k.NamaKandungan.ToLower() == kandungan.NamaKandungan.ToLower() && k.KandunganId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Update data
                data.KodeKandungan = kandungan.KodeKandungan;
                data.NamaKandungan = kandungan.NamaKandungan;

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
    }
}
