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
    //[EnableCors("FrontendCorsPolicy")]
    public class CounterOfferController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CounterOfferController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CounterOffer?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPagedCounterOffers(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var totalItems = await _context.CounterOffers.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)perPage);

            var data = await _context.CounterOffers
                .OrderBy(c => c.TglOffer)
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

        // GET: api/CounterOffer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCounterOffer(Guid id)
        {
            var item = await _context.CounterOffers.FindAsync(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        // POST: api/CounterOffer
        [HttpPost]
        public async Task<IActionResult> CreateCounterOffer(CounterOffer counterOffer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            counterOffer.CounterOfferId = Guid.NewGuid();
            _context.CounterOffers.Add(counterOffer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCounterOffer), new { id = counterOffer.CounterOfferId }, counterOffer);
        }

        // PUT: api/CounterOffer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCounterOffer(Guid id, CounterOffer counterOffer)
        {
            if (id != counterOffer.CounterOfferId)
                return BadRequest();

            _context.Entry(counterOffer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.CounterOffers.Any(e => e.CounterOfferId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/CounterOffer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCounterOffer(Guid id)
        {
            var data = await _context.CounterOffers.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _context.CounterOffers.Remove(data);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
