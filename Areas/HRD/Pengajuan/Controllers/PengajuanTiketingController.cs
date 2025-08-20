using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Controllers
{
    [Area("HRD")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class PengajuanTiketingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengajuanTiketingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HRD/PengajuanTiketing
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PengajuanTiketing>>> GetPengajuanTiketing()
        {
            return await _context.PengajuanTiketings.ToListAsync();
        }

        // GET: api/HRD/PengajuanTiketing/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PengajuanTiketing>> GetPengajuanTiketing(Guid id)
        {
            var tiket = await _context.PengajuanTiketings.FindAsync(id);

            if (tiket == null)
            {
                return NotFound();
            }

            return tiket;
        }

        // POST: api/HRD/PengajuanTiketing
        [HttpPost]
        public async Task<ActionResult<PengajuanTiketing>> PostPengajuanTiketing(PengajuanTiketing pengajuan)
        {
            pengajuan.TicketId = Guid.NewGuid();
            pengajuan.CreateDateTime = DateTimeOffset.UtcNow;

            _context.PengajuanTiketings.Add(pengajuan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPengajuanTiketing), new { id = pengajuan.TicketId }, pengajuan);
        }

        // PUT: api/HRD/PengajuanTiketing/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPengajuanTiketing(Guid id, PengajuanTiketing pengajuan)
        {
            if (id != pengajuan.TicketId)
            {
                return BadRequest();
            }

            pengajuan.UpdateDateTime = DateTimeOffset.UtcNow;
            _context.Entry(pengajuan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PengajuanTiketings.Any(e => e.TicketId == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/HRD/PengajuanTiketing/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanTiketing(Guid id)
        {
            var pengajuan = await _context.PengajuanTiketings.FindAsync(id);
            if (pengajuan == null)
            {
                return NotFound();
            }

            _context.PengajuanTiketings.Remove(pengajuan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
