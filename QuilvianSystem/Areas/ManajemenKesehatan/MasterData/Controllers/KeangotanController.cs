using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    public class KeangotaanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeangotaanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Keangotaan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Keangotaans.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Keangotaan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Keangotaans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Keangotaan
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Keangotaan model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            model.KeangotaanId = Guid.NewGuid();
            _context.Keangotaans.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.KeangotaanId }, model);
        }

        // PUT: api/Keangotaan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Keangotaan model)
        {
            if (model == null || id != model.KeangotaanId)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            var existingRecord = await _context.Keangotaans.FindAsync(id);
            if (existingRecord == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }
            // Update properties
            foreach (var prop in model.GetType().GetProperties())
            {
                var value = prop.GetValue(model);
                if (value != null)
                {
                    prop.SetValue(existingRecord, value);
                }
            }

            _context.Keangotaans.Update(existingRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui." });
        }

        // DELETE: api/Keangotaan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Keangotaans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Keangotaans.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }
    }
}
