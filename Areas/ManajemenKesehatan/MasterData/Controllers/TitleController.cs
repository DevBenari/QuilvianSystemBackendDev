using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;
using static QRCoder.PayloadGenerator;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class TitleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TitleController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Title
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.Titles.ToListAsync();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // GET: api/Title/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.Titles.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = record });
        }

        // POST: api/Title
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Title model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            model.TitleId = Guid.NewGuid();
            _context.Titles.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.TitleId }, model);
        }

        // PUT: api/Title/{id}
        [HttpPut("{id}")]
        //public async Task<IActionResult> Update(Guid id, [FromBody] TitleViewModel model)
        //{
        //    var existingRecord = await _context.Titles.FindAsync(id);
        //    if (existingRecord == null)
        //    {
        //        return NotFound(new { message = "Data tidak ditemukan." });
        //    }
        //    // Update properties
        //    foreach (var prop in model.GetType().GetProperties())
        //    {
        //        var value = prop.GetValue(model);
        //        if (value != null)
        //        {
        //            prop.SetValue(existingRecord, value);
        //        }
        //    }

        //    _context.Titles.Update(existingRecord);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Data berhasil diperbarui." });
        //}
        public async Task<IActionResult> Update(Guid id, [FromBody] TitleViewModel update)
        {
            // Mendapatkan email dari claim yang terautentikasi
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email);

            if (update == null)
            {
                return BadRequest("Data user tidak boleh kosong. || 400 Bad Request");
            }

            // Cari data berdasarkan ID
            var vm = await _context.Titles.FindAsync(id);
            if (vm == null)
            {
                return NotFound($"User dengan ID {id} tidak ditemukan. || 404 Not Found");
            }

            try
            {
                // Perbarui data user
                vm.UpdateDateTime = DateTime.Now;
                vm.UpdateBy = Guid.Parse(user.Id); // Menyimpan userId sebagai GUID
                vm.NamaTitle = update.NamaTitle;

                // Tandai data sebagai telah diubah
                _context.Titles.Update(vm);

                // Simpan perubahan ke database
                await _context.SaveChangesAsync(); // Pastikan menggunakan SaveChangesAsync untuk operasi asinkron

                return Ok(new { message = "Berhasil Update || 200 OK" });
            }
            catch (Exception ex)
            {
                // Tangani error jika terjadi masalah
                return StatusCode(500, $"Terjadi kesalahan saat memperbarui data: {ex.Message}");
            }
        }


        // DELETE: api/Title/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var record = await _context.Titles.FindAsync(id);
            if (record == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            _context.Titles.Remove(record);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus." });
        }
    }
}
