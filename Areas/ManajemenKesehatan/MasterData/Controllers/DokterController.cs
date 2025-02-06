using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Migrations;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    public class DokterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DokterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Dokter
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Dokters.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Dokter/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Dokters.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Dokter
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DokterViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Dokters
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KdDokter)
                .FirstOrDefault();

            if (lastCode != null)
            {
                model.KdDokter = "DR" + setDateNow + "0001";
            }
            else
            { 
                var lastCodeTrim = lastCode.KdDokter.Substring(3,6);
                if (lastCodeTrim != setDateNow)
                {
                    model.KdDokter = "DR" + setDateNow + "0001";
                }
                else
                {
                    model.KdDokter = "DR" + setDateNow + (Convert.ToInt32(lastCode.KdDokter.Substring(9)) + 1).ToString("D4");
                }
            }

            // validate model state
            if (ModelState.IsValid)
            {
                var dokter = new Dokter
                {
                    DokterId = Guid.NewGuid(),
                    KdDokter = model.KdDokter,
                    NmDokter = model.NmDokter,
                    Sip = model.Sip,
                    Str = model.Str,
                    TglSip = model.TglSip,
                    TglStr = model.TglStr,
                    PanggilDokter = model.PanggilDokter,
                    Nik = model.Nik,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };

                var checkDuplicate = _context.Dokters.Where(c => c.KdDokter == model.KdDokter && c.NmDokter == model.NmDokter).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Dokters.Where(c => c.KdDokter == model.KdDokter && c.NmDokter == model.NmDokter).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Dokters.Add(dokter);
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

        // PUT: api/Dokter/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DokterViewModel model)
        {
            //cek apakah data ada ditabase
            var existingDokter = await _context.Dokters.FindAsync(id);
            if (existingDokter == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Dokters.Where
                (c => c.KdDokter == model.KdDokter && c.NmDokter == model.NmDokter
                && c.DokterId != id).FirstOrDefault();
            if (checkDuplicate == null)
            {
                return Conflict(new { message = "Terdapat duplikasi data !!! || 409 Conflict Data" });
            }

            //existingDokter.KdDokter = model.KdDokter;
            existingDokter.NmDokter = model.NmDokter;
            existingDokter.Sip = model.Sip;
            existingDokter.Str = model.Str;
            existingDokter.TglSip = model.TglSip;
            existingDokter.TglStr = model.TglStr;
            existingDokter.PanggilDokter = model.PanggilDokter;
            existingDokter.Nik = model.Nik;
            existingDokter.UpdateDateTime = DateTimeOffset.Now;
            existingDokter.UpdateBy = Guid.NewGuid();

            try
            {
                _context.Dokters.Update(existingDokter);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }
        }

        // DELETE: api/Dokter/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Dokters.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Dokters.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }

        //fungsi search
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            // Validasi input keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
            }

            // Lakukan pencarian di database (case-insensitive)
            var searchResults = await _context.Dokters
                .Where(n => EF.Functions.Like(n.NmDokter, $"%{keyword}%"))
                .ToListAsync();

            // Jika tidak ada data ditemukan
            if (!searchResults.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found" });
            }

            // Mengembalikan hasil pencarian
            return Ok(new { message = "Data ditemukan.", data = searchResults });
        }
    }
}
