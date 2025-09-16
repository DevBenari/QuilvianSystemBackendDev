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
    public class RiwayatSertifikatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RiwayatSertifikatController> _logger;

        public RiwayatSertifikatController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RiwayatSertifikatController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // ✅ GET ALL with pagination
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _context.RiwayatSertifikats.AsNoTracking();

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var data = await query
                .OrderByDescending(p => p.TglTerbit)
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

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.RiwayatSertifikats.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RiwayatSertifikat model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            model.SertifikasiId = Guid.NewGuid();

            _context.RiwayatSertifikats.Add(model);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Created("", new { message = "Data berhasil ditambahkan || 201 Created" });

            return StatusCode(500, new { message = "Data tidak berhasil disimpan." });
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RiwayatSertifikat model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var data = await _context.RiwayatSertifikats.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            data.UserActiveId = model.UserActiveId;
            data.NamaSertifikasi = model.NamaSertifikasi;
            data.NamaInstitusi = model.NamaInstitusi;
            data.Penyelenggara = model.Penyelenggara;
            data.NoSertifikasi = model.NoSertifikasi;
            data.TglTerbit = model.TglTerbit;
            data.TglKadaluarsa = model.TglKadaluarsa;
            data.AsalPartisipasi = model.AsalPartisipasi;
            data.FilePath = model.FilePath;

            _context.RiwayatSertifikats.Update(data);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Update data berhasil || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.RiwayatSertifikats.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _context.RiwayatSertifikats.Remove(data);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
