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
    //[EnableCors("FrontendCorsPolicy")]
    public class GradePayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradePayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GradePay?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPagedGradePay(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var totalItems = await _context.GradePays.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)perPage);

            var data = await _context.GradePays
                .OrderBy(g => g.KodeGrade)
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

        // GET: api/GradePay/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGradePay(Guid id)
        {
            var gradePay = await _context.GradePays.FindAsync(id);
            if (gradePay == null)
                return NotFound();
            return Ok(gradePay);
        }

        // POST: api/GradePay
        [HttpPost]
        public async Task<IActionResult> CreateGradePay(GradePay gradePay)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            gradePay.GradePayId = Guid.NewGuid();
            _context.GradePays.Add(gradePay);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGradePay), new { id = gradePay.GradePayId }, gradePay);
        }

        // PUT: api/GradePay/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGradePay(Guid id, GradePay gradePay)
        {
            if (id != gradePay.GradePayId)
                return BadRequest();

            _context.Entry(gradePay).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.GradePays.Any(e => e.GradePayId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/GradePay/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGradePay(Guid id)
        {
            var gradePay = await _context.GradePays.FindAsync(id);
            if (gradePay == null)
                return NotFound();

            _context.GradePays.Remove(gradePay);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
