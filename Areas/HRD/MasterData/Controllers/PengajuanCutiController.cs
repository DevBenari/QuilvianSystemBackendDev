using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class PengajuanCutiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengajuanCutiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HRD/PengajuanCuti
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PengajuanCuti>>> GetPengajuanCuti()
        {
            return await _context.PengajuanCutis.ToListAsync();
        }

        // GET: api/HRD/PengajuanCuti/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PengajuanCuti>> GetPengajuanCuti(Guid id)
        {
            var pengajuanCuti = await _context.PengajuanCutis.FindAsync(id);

            if (pengajuanCuti == null)
                return NotFound();

            return pengajuanCuti;
        }

        // POST: api/HRD/PengajuanCuti
        [HttpPost]
        public async Task<ActionResult<PengajuanCuti>> PostPengajuanCuti(PengajuanCuti pengajuanCuti)
        {
            pengajuanCuti.PengajuanCutiId = Guid.NewGuid();
            _context.PengajuanCutis.Add(pengajuanCuti);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPengajuanCuti), new { id = pengajuanCuti.PengajuanCutiId }, pengajuanCuti);
        }

        // PUT: api/HRD/PengajuanCuti/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPengajuanCuti(Guid id, PengajuanCuti pengajuanCuti)
        {
            if (id != pengajuanCuti.PengajuanCutiId)
                return BadRequest();

            _context.Entry(pengajuanCuti).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PengajuanCutiExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/HRD/PengajuanCuti/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanCuti(Guid id)
        {
            var pengajuanCuti = await _context.PengajuanCutis.FindAsync(id);
            if (pengajuanCuti == null)
                return NotFound();

            _context.PengajuanCutis.Remove(pengajuanCuti);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PengajuanCutiExists(Guid id)
        {
            return _context.PengajuanCutis.Any(e => e.PengajuanCutiId == id);
        }
    }
}
