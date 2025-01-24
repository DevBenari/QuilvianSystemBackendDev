using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianBackendDev.Repositories;

namespace QuilvianBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WilayahController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WilayahController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GeoData/Provinsi
        [HttpGet("Provinsi")]
        public async Task<IActionResult> GetAllProvinsi()
        {
            var records = await _context.Provinsis.ToListAsync();
            return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
        }

        // GET: api/GeoData/Kabupaten
        [HttpGet("Kabupaten")]
        public async Task<IActionResult> GetAllKabupaten()
        {
            var records = await _context.Kabupatens.Include(k => k.Provinsi).ToListAsync();
            return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
        }

        // GET: api/GeoData/Kecamatan
        [HttpGet("Kecamatan")]
        public async Task<IActionResult> GetAllKecamatan()
        {
            var records = await _context.Kecamatans.Include(k => k.Kabupaten).ToListAsync();
            return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
        }

        // GET: api/GeoData/Kelurahan
        [HttpGet("Kelurahan")]
        public async Task<IActionResult> GetAllKelurahan()
        {
            var records = await _context.Kelurahans.Include(k => k.Kecamatan).ToListAsync();
            return records.Any() ? Ok(new { message = "Data ditemukan.", data = records }) : NotFound(new { message = "Tidak ada data ditemukan." });
        }

        // GET: api/GeoData/{model}/{id}
        [HttpGet("{model}/{id}")]
        public async Task<IActionResult> GetById(string model, Guid id)
        {
            if (model == "Provinsi")
            {
                var record = await _context.Provinsis.FindAsync(id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Provinsi dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kabupaten")
            {
                var record = await _context.Kabupatens.Include(k => k.Provinsi).FirstOrDefaultAsync(k => k.KabupatenId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kabupaten dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kecamatan")
            {
                var record = await _context.Kecamatans.Include(k => k.Kabupaten).FirstOrDefaultAsync(k => k.KecamatanId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kecamatan dengan ID {id} tidak ditemukan." });
            }
            else if (model == "Kelurahan")
            {
                var record = await _context.Kelurahans.Include(k => k.Kecamatan).FirstOrDefaultAsync(k => k.KelurahanId == id);
                return record != null ? Ok(new { message = "Data ditemukan.", data = record }) : NotFound(new { message = $"Kelurahan dengan ID {id} tidak ditemukan." });
            }

            return BadRequest(new { message = "Model tidak valid." });
        }

        // POST: api/GeoData/Provinsi
        [HttpPost("Provinsi")]
        public async Task<IActionResult> CreateProvinsi([FromBody] Provinsi model)
        {
            if (model == null) return BadRequest(new { message = "Data tidak valid." });

            model.ProvinsiId = Guid.NewGuid();
            _context.Provinsis.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { model = "Provinsi", id = model.ProvinsiId }, model);
        }

        // POST: api/GeoData/Kabupaten
        [HttpPost("Kabupaten")]
        public async Task<IActionResult> CreateKabupaten([FromBody] Kabupaten model)
        {
            if (model == null) return BadRequest(new { message = "Data tidak valid." });

            model.KabupatenId = Guid.NewGuid();
            _context.Kabupatens.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { model = "Kabupaten", id = model.KabupatenId }, model);
        }

        // POST: api/GeoData/Kecamatan
        [HttpPost("Kecamatan")]
        public async Task<IActionResult> CreateKecamatan([FromBody] Kecamatan model)
        {
            if (model == null) return BadRequest(new { message = "Data tidak valid." });

            model.KecamatanId = Guid.NewGuid();
            _context.Kecamatans.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { model = "Kecamatan", id = model.KecamatanId }, model);
        }

        // POST: api/GeoData/Kelurahan
        [HttpPost("Kelurahan")]
        public async Task<IActionResult> CreateKelurahan([FromBody] Kelurahan model)
        {
            if (model == null) return BadRequest(new { message = "Data tidak valid." });

            model.KelurahanId = Guid.NewGuid();
            _context.Kelurahans.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { model = "Kelurahan", id = model.KelurahanId }, model);
        }

        // PUT: api/GeoData/{model}/{id}
        [HttpPut("{model}/{id}")]
        public async Task<IActionResult> Update(string model, Guid id, [FromBody] dynamic requestBody)
        {
            if (model == "Provinsi")
            {
                var existingRecord = await _context.Provinsis.FindAsync(id);
                if (existingRecord == null) return NotFound(new { message = $"Provinsi dengan ID {id} tidak ditemukan." });

                // Update model properties (manually or via a generic method)
                existingRecord.ProvinsiCode = requestBody.ProvinsiCode ?? existingRecord.ProvinsiCode;
                existingRecord.ProvinsiName = requestBody.ProvinsiName ?? existingRecord.ProvinsiName;

                _context.Provinsis.Update(existingRecord);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Data berhasil diperbarui." });
            }

            // Similar logic for Kabupaten, Kecamatan, and Kelurahan...

            return BadRequest(new { message = "Model tidak valid." });
        }

        // DELETE: api/GeoData/{model}/{id}
        [HttpDelete("{model}/{id}")]
        public async Task<IActionResult> Delete(string model, Guid id)
        {
            if (model == "Provinsi")
            {
                var record = await _context.Provinsis.FindAsync(id);
                if (record == null) return NotFound(new { message = $"Provinsi dengan ID {id} tidak ditemukan." });

                _context.Provinsis.Remove(record);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Data berhasil dihapus." });
            }

            // Similar logic for Kabupaten, Kecamatan, and Kelurahan...

            return BadRequest(new { message = "Model tidak valid." });
        }
    }
}
