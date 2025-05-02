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
    public class BentukObatController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public BentukObatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _applicationDbContext = context;
            _userManager = userManager;
        }

        // GET: api/BentukObat
        [HttpGet]
        public async Task<IActionResult> GetAllBentukObat(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from b in _applicationDbContext.BentukObats
                        select new
                        {
                            BentukObatId = b.BentukObatId,
                            KodeBentukObat = b.KodeBentukObat,
                            NamaBentukObat = b.NamaBentukObat
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

        // GET: api/BentukObat/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBentukObatById(Guid id)
        {
            var bentukObat = await _applicationDbContext.BentukObats
                .FirstOrDefaultAsync(b => b.BentukObatId == id);

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

        // POST: api/BentukObat
        [HttpPost]
        public async Task<IActionResult> CreateBentukObat([FromBody] BentukObat bentukObat)
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

                // Cek jika sudah ada data yang sama berdasarkan KodeBentukObat
                var isDuplicate = await _applicationDbContext.BentukObats
                    .AnyAsync(b => b.KodeBentukObat == bentukObat.KodeBentukObat);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data dengan kode bentuk obat yang sama sudah ada || 409 Conflict Data" });
                }

                // Insert data baru
                bentukObat.BentukObatId = Guid.NewGuid();
                _applicationDbContext.BentukObats.Add(bentukObat);
                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // PUT: api/BentukObat/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBentukObat(Guid id, [FromBody] BentukObat bentukObat)
        {
            if (bentukObat == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cari data yang ingin diupdate
                var data = await _applicationDbContext.BentukObats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Cek duplikasi berdasarkan NamaBentukObat
                bool isDuplicate = await _applicationDbContext.BentukObats
                    .AnyAsync(b => b.NamaBentukObat.ToLower() == bentukObat.NamaBentukObat.ToLower() && b.BentukObatId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // Update data
                data.KodeBentukObat = bentukObat.KodeBentukObat;
                data.NamaBentukObat = bentukObat.NamaBentukObat;

                _applicationDbContext.BentukObats.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/BentukObat/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBentukObat(Guid id)
        {
            try
            {
                var data = await _applicationDbContext.BentukObats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                _applicationDbContext.BentukObats.Remove(data);
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
