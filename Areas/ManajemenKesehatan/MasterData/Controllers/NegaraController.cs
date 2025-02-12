using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class NegaraController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public NegaraController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get : api/Negara
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Negaras.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // Get : api/Negara/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Negaras.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // Post : api/Negara
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NegaraViewModel model)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;

            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            // Generate UserActiveCode
            var lastCode = _context.Negaras
                .Where(d => d.CreateDateTime.Day == day && d.CreateDateTime.Month == month && d.CreateDateTime.Year == year)
                .OrderByDescending(k => k.KodeNegara)
                .FirstOrDefault();

            if (lastCode == null)
            {
                model.KodeNegara = "NGR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.KodeNegara.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.KodeNegara = "NGR" + setDateNow + "0001";
                }
                else
                {
                    model.KodeNegara = "NGR" + setDateNow +
                        (Convert.ToInt32(lastCode.KodeNegara.Substring(9)) + 1).ToString("D4");
                }
            }

            //Validate ModelState
            if (ModelState.IsValid)
            {
                var negara = new Negara
                {
                    NegaraId = Guid.NewGuid(),
                    KodeNegara = model.KodeNegara,
                    NamaNegara = model.NamaNegara,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = Guid.NewGuid(),
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = Guid.NewGuid(),
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = Guid.NewGuid(),
                    IsDelete = false
                };
                
               
                var checkDuplicate = _context.Negaras.Where(c => c.KodeNegara == model.KodeNegara && c.NamaNegara == model.NamaNegara).ToList();

                if (checkDuplicate.Count == 0)
                {
                    var result = _context.Negaras.Where(c => c.KodeNegara == model.KodeNegara && c.NamaNegara == model.NamaNegara).FirstOrDefault();
                    if (result == null)
                    {
                        _context.Negaras.Add(negara);
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

        // update negara
        // Put : api/Negara/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] NegaraViewModel model)
        {
            //cek apakah data ada di database
            var existingNegara = await _context.Negaras.FindAsync(id);
            if (existingNegara == null)
            {
                return NotFound(new { message = "Data tidak ditemukan. || 404 Not Found " });
            }

            //cek duplikat data
            var checkDuplicate = _context.Negaras.Where
                (c => c.KodeNegara == model.KodeNegara && c.NamaNegara == model.NamaNegara
                && c.NegaraId != id).FirstOrDefault();
            if (checkDuplicate != null)
            {
                return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
            }

            // Update properti dari data yang ada dengan nilai dari model
            existingNegara.NamaNegara = model.NamaNegara;
            
            // kode negara tidak diupdate hanya diganti nama negaranya saja
            //existingNegara.KodeNegara = model.KodeNegara;

            existingNegara.UpdateDateTime = DateTimeOffset.Now;
            existingNegara.UpdateBy = Guid.NewGuid();  // Sesuaikan dengan ID pengguna yang mengupdate

            // Simpan perubahan ke database
            try
            {
                _context.Negaras.Update(existingNegara);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAll), new { message = "Tambah Data Berhasil || 201 Created" }, model);
            }
            catch (Exception ex)
            {
                // Tangani kesalahan jika ada
                return StatusCode(500, new { message = "Terjadi kesalahan di server.", error = ex.Message });
            }

        }

        // Delete : api/Negara/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Negaras.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Negaras.Remove(record);
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
            var searchResults = await _context.Negaras
                .Where(n => EF.Functions.Like(n.NamaNegara, $"%{keyword}%"))
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
