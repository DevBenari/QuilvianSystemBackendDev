using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class RiwayatPendidikanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RiwayatPendidikanController> _logger;

        public RiwayatPendidikanController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RiwayatPendidikanController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _context.RiwayatPendidikans.AsNoTracking();

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var data = await query
                .OrderByDescending(p => p.TahunLulus)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!data.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data,
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.RiwayatPendidikans.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RiwayatPendidikan model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            model.PendidikanId = Guid.NewGuid();

            _context.RiwayatPendidikans.Add(model);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Created("", new { message = "Data berhasil ditambahkan || 201 Created" });

            return StatusCode(500, new { message = "Data tidak berhasil disimpan." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RiwayatPendidikan model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var data = await _context.RiwayatPendidikans.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            data.UserActiveId = model.UserActiveId;
            data.JenjangPendidikan = model.JenjangPendidikan;
            data.NamaInstitusi = model.NamaInstitusi;
            data.Jurusan = model.Jurusan;
            data.TahunMasuk = model.TahunMasuk;
            data.TahunLulus = model.TahunLulus;
            data.NilaiIpk = model.NilaiIpk;
            data.ProvinsiId = model.ProvinsiId;

            _context.RiwayatPendidikans.Update(data);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Update data berhasil || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.RiwayatPendidikans.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _context.RiwayatPendidikans.Remove(data);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
