using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JenisUserRoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JenisUserRoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // CRUD JENIS USER
        // ================================
        [HttpGet("JenisUser")]
        public async Task<ActionResult<IEnumerable<JenisUser>>> GetJenisUsers()
        {
            var data = await _context.JenisUsers
                .OrderBy(j => j.No) // Urutkan berdasarkan kode
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("JenisUser/{id}")]
        public async Task<ActionResult<JenisUser>> GetJenisUser(Guid id)
        {
            var entity = await _context.JenisUsers.FindAsync(id);
            if (entity == null) return NotFound();
            return entity;
        }

        [HttpPost("JenisUser")]
        public async Task<ActionResult<JenisUser>> PostJenisUser(JenisUser jenisUser)
        {
            jenisUser.JenisUserId = Guid.NewGuid();
            _context.JenisUsers.Add(jenisUser);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetJenisUser), new { id = jenisUser.JenisUserId }, jenisUser);
        }

        [HttpPut("JenisUser/{id}")]
        public async Task<IActionResult> PutJenisUser(Guid id, JenisUser jenisUser)
        {
            if (id != jenisUser.JenisUserId) return BadRequest();
            _context.Entry(jenisUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("JenisUser/{id}")]
        public async Task<IActionResult> DeleteJenisUser(Guid id)
        {
            var entity = await _context.JenisUsers.FindAsync(id);
            if (entity == null) return NotFound();
            _context.JenisUsers.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================================
        // CRUD JENIS PEMBAYARAN
        // ================================
        [HttpGet("JenisPembayaran")]
        public async Task<ActionResult<IEnumerable<JenisPembayaran>>> GetJenisPembayaran()
        {
            return await _context.JenisPembayarans.ToListAsync();
        }

        [HttpGet("JenisPembayaran/{id}")]
        public async Task<ActionResult<JenisPembayaran>> GetJenisPembayaran(Guid id)
        {
            var entity = await _context.JenisPembayarans.FindAsync(id);
            if (entity == null) return NotFound();
            return entity;
        }

        [HttpPost("JenisPembayaran")]
        public async Task<ActionResult<JenisPembayaran>> PostJenisPembayaran(JenisPembayaran jenisPembayaran)
        {
            jenisPembayaran.JenisPembayaranId = Guid.NewGuid();
            _context.JenisPembayarans.Add(jenisPembayaran);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetJenisPembayaran), new { id = jenisPembayaran.JenisPembayaranId }, jenisPembayaran);
        }

        [HttpPut("JenisPembayaran/{id}")]
        public async Task<IActionResult> PutJenisPembayaran(Guid id, JenisPembayaran jenisPembayaran)
        {
            if (id != jenisPembayaran.JenisPembayaranId) return BadRequest();
            _context.Entry(jenisPembayaran).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("JenisPembayaran/{id}")]
        public async Task<IActionResult> DeleteJenisPembayaran(Guid id)
        {
            var entity = await _context.JenisPembayarans.FindAsync(id);
            if (entity == null) return NotFound();
            _context.JenisPembayarans.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================================
        // CRUD PEMBAYARAN
        // ================================
        [HttpGet("Pembayaran")]
        public async Task<ActionResult<IEnumerable<Pembayaran>>> GetPembayaran()
        {
            return await _context.Pembayarans.ToListAsync();
        }

        [HttpGet("Pembayaran/{id}")]
        public async Task<ActionResult<Pembayaran>> GetPembayaran(Guid id)
        {
            var entity = await _context.Pembayarans.FindAsync(id);
            if (entity == null) return NotFound();
            return entity;
        }

        [HttpPost("Pembayaran")]
        public async Task<ActionResult<Pembayaran>> PostPembayaran(Pembayaran pembayaran)
        {
            pembayaran.PembayaranId = Guid.NewGuid();

            // Isi nama otomatis dari relasi
            var jenisUser = await _context.JenisUsers.FindAsync(pembayaran.JenisUserId);
            var jenisPembayaran = await _context.JenisPembayarans.FindAsync(pembayaran.JenisPembayaranId);

            if (jenisUser == null || jenisPembayaran == null) return BadRequest("JenisUser atau JenisPembayaran tidak valid.");

            pembayaran.NamaJenisUser = jenisUser.NamaJenisUser;
            pembayaran.NamaPembayaran = jenisPembayaran.NamaPembayaran;

            _context.Pembayarans.Add(pembayaran);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPembayaran), new { id = pembayaran.PembayaranId }, pembayaran);
        }

        [HttpPut("Pembayaran/{id}")]
        public async Task<IActionResult> PutPembayaran(Guid id, Pembayaran pembayaran)
        {
            if (id != pembayaran.PembayaranId) return BadRequest();

            // Update nama otomatis kalau id berubah
            var jenisUser = await _context.JenisUsers.FindAsync(pembayaran.JenisUserId);
            var jenisPembayaran = await _context.JenisPembayarans.FindAsync(pembayaran.JenisPembayaranId);

            if (jenisUser == null || jenisPembayaran == null) return BadRequest("JenisUser atau JenisPembayaran tidak valid.");

            pembayaran.NamaJenisUser = jenisUser.NamaJenisUser;
            pembayaran.NamaPembayaran = jenisPembayaran.NamaPembayaran;

            _context.Entry(pembayaran).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Pembayaran/{id}")]
        public async Task<IActionResult> DeletePembayaran(Guid id)
        {
            var entity = await _context.Pembayarans.FindAsync(id);
            if (entity == null) return NotFound();
            _context.Pembayarans.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
