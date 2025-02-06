using System.Runtime.ConstrainedExecution;
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
    //[Authorize] 
    public class KeanggotaanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KeanggotaanController(ApplicationDbContext context)
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
        public async Task<IActionResult> Create([FromBody] KeanggotaanViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Keangotaans
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KeangotaanKode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KeangotaanKode = "AGT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KeangotaanKode.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.KeangotaanKode = "AGT" + setDateNow + "0001";
                }
                else
                {
                    model.KeangotaanKode = "AGT" + setDateNow +
                        (Convert.ToInt32(lastCode.KeangotaanKode.Substring(9)) + 1).ToString("D4");
                }
            }

            //Validate ModelState
            if (ModelState.IsValid)
            {
                var keanggotaan = new Keangotaan
                {
                    KeangotaanId = Guid.NewGuid(),
                    KeangotaanKode = model.KeangotaanKode,
                    JenisKeangotaan = model.JenisKeangotaan,
                    JenisPromo = model.JenisPromo,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };


                var checkDuplicate = _context.Keangotaans.Where(c => c.KeangotaanKode == model.KeangotaanKode && c.JenisKeangotaan == model.JenisKeangotaan
                                     && c.JenisPromo == model.JenisPromo).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Keangotaans.Where(c => c.KeangotaanKode == model.KeangotaanKode && c.JenisKeangotaan == model.JenisKeangotaan
                                     && c.JenisPromo == model.JenisPromo).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Keangotaans.Add(keanggotaan);
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
                return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
            }
        }

        // PUT: api/Keangotaan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] KeanggotaanViewModel model)
        {
            // cek apakah data ada di database
            var existingKeanggotaan = await _context.Keangotaans.FindAsync(id);
            if (existingKeanggotaan == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Keangotaans.Where
                (c => c.KeangotaanKode == model.KeangotaanKode && c.JenisKeangotaan == model.JenisKeangotaan
                                     && c.JenisPromo == model.JenisPromo).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingKeanggotaan.JenisKeangotaan = model.JenisKeangotaan;
            existingKeanggotaan.JenisPromo = model.JenisPromo;


            //existingKeanggotaan.KeangotaanKode = model.KeangotaanKode;

            existingKeanggotaan.UpdateDateTime = DateTimeOffset.Now;
            existingKeanggotaan.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Keangotaans.Update(existingKeanggotaan);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
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
