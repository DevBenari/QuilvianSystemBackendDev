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
    public class GolonganDarahController : Controller
    {

        private readonly ApplicationDbContext _context;

        public GolonganDarahController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GolonganDarah
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.GolonganDarahs.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/GolonganDarah/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.GolonganDarahs.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/GolonganDarah
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GolonganDarahViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.GolonganDarahs
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KodeGolonganDarah)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KodeGolonganDarah = "GDR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KodeGolonganDarah.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.KodeGolonganDarah = "GDR" + setDateNow + "0001";
                }
                else
                {
                    model.KodeGolonganDarah = "GDR" + setDateNow +
                        (Convert.ToInt32(lastCode.KodeGolonganDarah.Substring(9)) + 1).ToString("D4");
                }
            }

            //Validate ModelState
            if (ModelState.IsValid)
            {
                var goldar = new GolonganDarah
                {
                    GolonganDarahId = Guid.NewGuid(),
                    KodeGolonganDarah = model.KodeGolonganDarah,
                    NamaGolonganDarah = model.NamaGolonganDarah,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };


                var checkDuplicate = _context.GolonganDarahs.Where(c => c.KodeGolonganDarah == model.KodeGolonganDarah && c.NamaGolonganDarah == model.KodeGolonganDarah).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.GolonganDarahs.Where(c => c.KodeGolonganDarah == model.KodeGolonganDarah && c.NamaGolonganDarah == model.KodeGolonganDarah).FirstOrDefault();
                    if (result == null)
                    {
                        _context.GolonganDarahs.Add(goldar);
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

        // PUT: api/GolonganDarah/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GolonganDarahViewModel model)
        {
            //cek apakah data ada di database
            var existingGoldar = await _context.GolonganDarahs.FindAsync(id);
            if (existingGoldar == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.GolonganDarahs.Where
                (c => c.KodeGolonganDarah == model.KodeGolonganDarah && c.NamaGolonganDarah == model.NamaGolonganDarah
                && c.GolonganDarahId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingGoldar.NamaGolonganDarah = model.NamaGolonganDarah;

            // kode negara tidak diupdate hanya diganti nama negaranya saja
            //existingGoldar.KodeGolonganDarah = model.KodeGolonganDarah;

            existingGoldar.UpdateDateTime = DateTimeOffset.Now;
            existingGoldar.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.GolonganDarahs.Update(existingGoldar);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }

        }

        // DELETE: api/GolonganDarah/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.GolonganDarahs.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.GolonganDarahs.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }

        //fungsi search
        // GET: api/Negara/Search?keyword={keyword}
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            // Validasi input keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword tidak boleh kosong. || 400 Bad Request" });
            }

            // Lakukan pencarian di database (case-insensitive)
            var searchResults = await _context.GolonganDarahs
                .Where(n => EF.Functions.Like(n.NamaGolonganDarah, $"%{keyword}%"))
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
