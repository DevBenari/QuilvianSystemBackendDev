using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class PengajuanRekrutmenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengajuanRekrutmenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PengajuanRekrutmen?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPagedPengajuanRekrutmen(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var totalItems = await _context.PengajuanRekrutmens.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)perPage);

            var data = await _context.PengajuanRekrutmens
                .OrderBy(p => p.TglPengajuan)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                currentPage = page,
                perPage,
                totalItems,
                totalPages,
                data
            });
        }

        // GET: api/PengajuanRekrutmen/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPengajuanRekrutmen(Guid id)
        {
            var item = await _context.PengajuanRekrutmens.FindAsync(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        // POST: api/PengajuanRekrutmen
        [HttpPost]
        public async Task<IActionResult> CreatePengajuanRekrutmen(PengajuanRekrutmen pengajuan)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            pengajuan.PengajuanRekrutmenId = Guid.NewGuid();
            _context.PengajuanRekrutmens.Add(pengajuan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPengajuanRekrutmen), new { id = pengajuan.PengajuanRekrutmenId }, pengajuan);
        }

        // PUT: api/PengajuanRekrutmen/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePengajuanRekrutmen(Guid id, PengajuanRekrutmen pengajuan)
        {
            if (id != pengajuan.PengajuanRekrutmenId)
                return BadRequest();

            _context.Entry(pengajuan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PengajuanRekrutmens.Any(e => e.PengajuanRekrutmenId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/PengajuanRekrutmen/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanRekrutmen(Guid id)
        {
            var item = await _context.PengajuanRekrutmens.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.PengajuanRekrutmens.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
