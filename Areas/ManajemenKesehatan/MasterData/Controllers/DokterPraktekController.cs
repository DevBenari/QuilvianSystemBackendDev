using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DokterPraktekController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DokterPraktekController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DokterPraktek
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.DokterPrakteks
                .Include(dp => dp.Dokters)  // Include relasi dengan tabel Dokters
                .ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/DokterPraktek/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _context.DokterPrakteks
                .Include(dp => dp.Dokters)
                .FirstOrDefaultAsync(dp => dp.DokterPraktekId == id);

            if (result == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }

            return Ok(result);
        }

        // POST: api/DokterPraktek
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DokterPraktekViewModel model)
        {
            //validate modelstate
            if (ModelState.IsValid)
            {
                var dokterPraktek = new DokterPraktek
                {
                    DokterPraktekId = Guid.NewGuid(),
                    Dokter = model.Dokter,
                    Layanan = model.Layanan,
                    JamPraktek = model.JamPraktek,
                    Hari = model.Hari,
                    JamMasuk = model.JamMasuk,
                    JamKeluar = model.JamKeluar,
                    DokterId = model.DokterId,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };
                var checkDuplicate = _context.DokterPrakteks.Where(c => c.DokterId == model.DokterId && c.Dokter == model.Dokter).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.DokterPrakteks.Where(c => c.DokterId == model.DokterId && c.Dokter == model.Dokter).FirstOrDefault();
                    if (result == null)
                    {
                        _context.DokterPrakteks.Add(dokterPraktek);
                        _context.SaveChanges();
                        return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
                    }
                    else
                    {
                        return BadRequest(new { message = "Data tidak dapat di input !!! || 400 Bad Request" });
                    }
                }
                else
                {
                    return Conflict(new { message = "Terdapat duplikasi data !!! || 409 Conflict Data" });
                }
            }
            else
            {
                return BadRequest(new { message = "Data tidak valid !!!! || 400 Bad Request" });

            }
        }

        // PUT: api/DokterPraktek/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DokterPraktekViewModel model)
        {

            //cek apakah data ada di database
            var existingDokterPraktek = await _context.DokterPrakteks.FindAsync(id);
            if (existingDokterPraktek == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.DokterPrakteks.Where
                (c => c.DokterId == model.DokterId && c.Dokter == model.Dokter
                && c.DokterPraktekId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingDokterPraktek.Dokter = model.Dokter;
            existingDokterPraktek.Layanan = model.Layanan;
            existingDokterPraktek.JamPraktek = model.JamPraktek;
            existingDokterPraktek.Hari = model.Hari;
            existingDokterPraktek.JamMasuk = model.JamMasuk;
            existingDokterPraktek.JamKeluar = model.JamKeluar;
            existingDokterPraktek.UpdateDateTime = DateTimeOffset.Now;
            existingDokterPraktek.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.DokterPrakteks.Update(existingDokterPraktek);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
            //if (model == null || id != model.DokterPraktekId)
            //{
            //    return BadRequest(new { message = "Data tidak valid." });
            //}
            //var existingRecord = await _context.DokterPrakteks.FindAsync(id);
            //if (existingRecord == null)
            //{
            //    return NotFound(new { message = "Data tidak ditemukan." });
            //}
            //// Update properties
            //foreach (var prop in model.GetType().GetProperties())
            //{
            //    var value = prop.GetValue(model);
            //    if (value != null)
            //    {
            //        prop.SetValue(existingRecord, value);
            //    }
            //}

            //_context.DokterPrakteks.Update(existingRecord);
            //await _context.SaveChangesAsync();

            //return Ok(new { message = "Data berhasil diperbarui." });
        }

        // DELETE: api/DokterPraktek/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.DokterPrakteks.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.DokterPrakteks.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }
    }
}
