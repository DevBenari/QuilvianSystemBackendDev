using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JenisTiketingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JenisTiketingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/JenisTiketing
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JenisTiketing>>> GetJenisTiketing()
        {
            return await _context.Set<JenisTiketing>().ToListAsync();
        }

        // GET: api/JenisTiketing/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<JenisTiketing>> GetJenisTiketing(Guid id)
        {
            var jenisTiketing = await _context.Set<JenisTiketing>().FindAsync(id);

            if (jenisTiketing == null)
                return NotFound();

            return jenisTiketing;
        }

        // POST: api/JenisTiketing
        [HttpPost]
        public async Task<ActionResult<JenisTiketing>> PostJenisTiketing(JenisTiketing jenisTiketing)
        {
            _context.Set<JenisTiketing>().Add(jenisTiketing);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJenisTiketing), new { id = jenisTiketing.JenisTicketId }, jenisTiketing);
        }

        // PUT: api/JenisTiketing/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJenisTiketing(Guid id, JenisTiketing jenisTiketing)
        {
            if (id != jenisTiketing.JenisTicketId)
                return BadRequest();

            _context.Entry(jenisTiketing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Set<JenisTiketing>().Any(e => e.JenisTicketId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/JenisTiketing/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJenisTiketing(Guid id)
        {
            var jenisTiketing = await _context.Set<JenisTiketing>().FindAsync(id);
            if (jenisTiketing == null)
                return NotFound();

            _context.Set<JenisTiketing>().Remove(jenisTiketing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
