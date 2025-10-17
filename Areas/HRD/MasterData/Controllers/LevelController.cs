using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
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
    public class LevelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LevelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Level?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPagedLevel(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var totalItems = await _context.Levels.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)perPage);

            var data = await _context.Levels
                .OrderBy(l => l.KodeLevel)
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

        // GET: api/Level/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLevel(Guid id)
        {
            var level = await _context.Levels.FindAsync(id);
            if (level == null)
                return NotFound();
            return Ok(level);
        }

        // POST: api/Level
        [HttpPost]
        public async Task<IActionResult> CreateLevel(Level level)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            level.LevelId = Guid.NewGuid();
            _context.Levels.Add(level);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLevel), new { id = level.LevelId }, level);
        }

        // PUT: api/Level/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLevel(Guid id, Level level)
        {
            if (id != level.LevelId)
                return BadRequest();

            _context.Entry(level).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Levels.Any(e => e.LevelId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Level/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLevel(Guid id)
        {
            var level = await _context.Levels.FindAsync(id);
            if (level == null)
                return NotFound();

            _context.Levels.Remove(level);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
