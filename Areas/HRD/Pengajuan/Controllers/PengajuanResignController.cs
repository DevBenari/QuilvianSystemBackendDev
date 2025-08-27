using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class PengajuanResignController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<PengajuanResignController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PengajuanResignController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PengajuanResignController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPengajuanResign(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _applicationDbContext.PengajuanResigns.AsNoTracking();

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var listdata = await query
                .OrderByDescending(r => r.TglEfektifResign)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPengajuanResignById(Guid id)
        {
            var data = await _applicationDbContext.PengajuanResigns.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePengajuanResign([FromBody] PengajuanResign vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = new PengajuanResign
            {
                ResignId = Guid.NewGuid(),
                UserActiveId = vm.UserActiveId,
                DepartementId = vm.DepartementId,
                PositionId = vm.PositionId,
                TglEfektifResign = vm.TglEfektifResign,
                NoticePeriod = vm.NoticePeriod,
                AlasanUtama = vm.AlasanUtama,
                AlasanTambahan = vm.AlasanTambahan,
                Approved1 = vm.Approved1,
                Approved2 = vm.Approved2,
                isTerimaPenawaran = vm.isTerimaPenawaran,
                StatusResign = vm.StatusResign
            };

            _applicationDbContext.PengajuanResigns.Add(data);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Created("", new { message = "Data berhasil ditambahkan || 201 Created" });

            return StatusCode(500, new { message = "Data tidak berhasil disimpan." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePengajuanResign(Guid id, [FromBody] PengajuanResign vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var data = await _applicationDbContext.PengajuanResigns.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            data.UserActiveId = vm.UserActiveId;
            data.DepartementId = vm.DepartementId;
            data.PositionId = vm.PositionId;
            data.TglEfektifResign = vm.TglEfektifResign;
            data.NoticePeriod = vm.NoticePeriod;
            data.AlasanUtama = vm.AlasanUtama;
            data.AlasanTambahan = vm.AlasanTambahan;
            data.Approved1 = vm.Approved1;
            data.Approved2 = vm.Approved2;
            data.isTerimaPenawaran = vm.isTerimaPenawaran;
            data.StatusResign = vm.StatusResign;

            _applicationDbContext.PengajuanResigns.Update(data);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Update data berhasil || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanResign(Guid id)
        {
            var data = await _applicationDbContext.PengajuanResigns.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _applicationDbContext.PengajuanResigns.Remove(data);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
