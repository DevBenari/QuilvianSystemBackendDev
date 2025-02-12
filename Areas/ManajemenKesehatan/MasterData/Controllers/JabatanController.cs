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
    public class JabatanController : Controller
    {

        private readonly ApplicationDbContext _context;

        public JabatanController(ApplicationDbContext context)
        {
            _context = context;
        }

        //get : api/Jabatan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Jabatans.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        //get : api/jabatan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Jabatans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        //post : api/jabatan
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JabatanViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Jabatans
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.JabatanKode)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.JabatanKode = "JBT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.JabatanKode.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.JabatanKode = "JBT" + setDateNow + "0001";
                }
                else
                {
                    model.JabatanKode = "JBT" + setDateNow +
                        (Convert.ToInt32(lastCode.JabatanKode.Substring(9)) + 1).ToString("D4");
                }
            }

            // validate modelstate
            if (ModelState.IsValid)
            {
                var jabatan = new Jabatan
                {
                    JabatanId = Guid.NewGuid(),
                    JabatanKode = model.JabatanKode,
                    JenisJabatan = model.JenisJabatan,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };

                var checkDuplicate = _context.Jabatans.Where(c => c.JabatanKode == model.JabatanKode && c.JenisJabatan == model.JenisJabatan).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Jabatans.Where(c => c.JabatanKode == model.JabatanKode && c.JenisJabatan == model.JenisJabatan).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Jabatans.Add(jabatan);
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

        //update jabatan
        //Put : api/Jabatan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] JabatanViewModel model)
        {
            //cek apakah data ada di database
            var existingJabatan = await _context.Jabatans.FindAsync(id);
            if (existingJabatan == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Jabatans.Where
                (c => c.JabatanKode == model.JabatanKode && c.JenisJabatan == model.JenisJabatan
                && c.JabatanId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            existingJabatan.JenisJabatan = model.JenisJabatan;



            existingJabatan.UpdateDateTime = DateTimeOffset.Now;
            existingJabatan.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Jabatans.Update(existingJabatan);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }

        }

        //delete jabatan
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Jabatans.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Jabatans.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }

        //fungsi search
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            // Validasi input keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
            }

            // Lakukan pencarian di database (case-insensitive)
            var searchResults = await _context.Jabatans
                .Where(n => EF.Functions.Like(n.JenisJabatan, $"%{keyword}%"))
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
