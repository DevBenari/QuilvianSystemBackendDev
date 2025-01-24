using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianBackendDev.Repositories;
using QuilvianBackendDev.Areas.ManajemenKesehatan.MasterData.Models;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    public class AsuransiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsuransiController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/Asuransi
        [HttpGet]
        public IActionResult GetAllAsuransi()
        {
            var asuransiList = _context.Asuransis.ToList();
            if (!asuransiList.Any())
            {
                return NotFound(new { message = "Data asuransi tidak ditemukan." });
            }

            return Ok(asuransiList);
        }

        // GET: api/Asuransi/{id}
        [HttpGet("{id}")]
        public IActionResult GetAsuransiById(Guid id)
        {
            var asuransi = _context.Asuransis.Find(id);
            if (asuransi == null)
            {
                return NotFound(new { message = "Asuransi tidak ditemukan." });
            }

            return Ok(asuransi);
        }

        // POST: api/Asuransi
        [HttpPost]
        public IActionResult AddAsuransi([FromBody] Asuransi newAsuransi)
        {
            if (newAsuransi == null)
            {
                return BadRequest(new { message = "Data asuransi tidak valid." });
            }

            newAsuransi.AsuransiId = Guid.NewGuid();
            newAsuransi.CreateDateTime = DateTimeOffset.Now;

            _context.Asuransis.Add(newAsuransi);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAsuransiById), new { id = newAsuransi.AsuransiId }, newAsuransi);
        }

        // PUT: api/Asuransi/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAsuransi(Guid id, [FromBody] Asuransi updatedAsuransi)
        {
            var existingAsuransi = _context.Asuransis.Find(id);
            if (existingAsuransi == null)
            {
                return NotFound(new { message = "Asuransi tidak ditemukan." });
            }

            existingAsuransi.NamaAsuransi = updatedAsuransi.NamaAsuransi;
            existingAsuransi.KodeAsuransi = updatedAsuransi.KodeAsuransi;
            existingAsuransi.TipePerusahaan = updatedAsuransi.TipePerusahaan;
            existingAsuransi.Status = updatedAsuransi.Status;

            _context.SaveChanges();
            return NoContent();
        }

        // DELETE: api/Asuransi/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAsuransi(Guid id)
        {
            var asuransi = _context.Asuransis.Find(id);
            if (asuransi == null)
            {
                return NotFound(new { message = "Asuransi tidak ditemukan." });
            }

            _context.Asuransis.Remove(asuransi);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
