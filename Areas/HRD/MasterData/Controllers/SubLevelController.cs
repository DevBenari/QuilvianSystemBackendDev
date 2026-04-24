using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SubLevelController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<SubLevelController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SubLevelController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SubLevelController> logger,
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
        public async Task<IActionResult> GetAllSubLevels(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _applicationDbContext.SubLevels.AsNoTracking();

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = await query
                .OrderByDescending(a => a.SubLevelNum)
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
        public async Task<IActionResult> GetSubLevelById(Guid id)
        {
            var data = await _applicationDbContext.SubLevels.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data = data });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubLevel([FromBody] SubLevel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var isDuplicate = _applicationDbContext.SubLevels.Any(s =>
                s.SubLevelNum == vm.SubLevelNum && s.LevelId == vm.LevelId);

            if (isDuplicate)
                return Conflict(new { message = "Data SubLevel sudah ada! || 409 Conflict" });

            var data = new SubLevel
            {
                SubLevelId = Guid.NewGuid(),
                LevelId = vm.LevelId,
                SubLevelNum = vm.SubLevelNum,
                PayGrade = vm.PayGrade,
                BasicSalary = vm.BasicSalary,
                AdditionalSalary = vm.AdditionalSalary,
                Subsidy = vm.Subsidy,
                Compensation = vm.Compensation,
                Reimbursement = vm.Reimbursement,
                DailyTransport = vm.DailyTransport,
                MealAllowance = vm.MealAllowance,
                MealOutsideOffice = vm.MealOutsideOffice,
                DiligentFee = vm.DiligentFee,
                isOvertime = vm.isOvertime,
                isAbsent = vm.isAbsent,
                isInsentif = vm.isInsentif,
                isBonus = vm.isBonus,
                isLeaveCompansation = vm.isLeaveCompansation,
                isPositionAllowance = vm.isPositionAllowance,
                Keterangan = vm.Keterangan
            };

            _applicationDbContext.SubLevels.Add(data);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Created("", new { message = "Data berhasil ditambahkan || 201 Created" });

            return StatusCode(500, new { message = "Data tidak berhasil disimpan." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubLevel(Guid id, [FromBody] SubLevel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _applicationDbContext.SubLevels.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            data.SubLevelNum = vm.SubLevelNum;
            data.LevelId = vm.LevelId;
            data.PayGrade = vm.PayGrade;
            data.BasicSalary = vm.BasicSalary;
            data.AdditionalSalary = vm.AdditionalSalary;
            data.Subsidy = vm.Subsidy;
            data.Compensation = vm.Compensation;
            data.Reimbursement = vm.Reimbursement;
            data.DailyTransport = vm.DailyTransport;
            data.MealAllowance = vm.MealAllowance;
            data.MealOutsideOffice = vm.MealOutsideOffice;
            data.DiligentFee = vm.DiligentFee;
            data.isOvertime = vm.isOvertime;
            data.isAbsent = vm.isAbsent;
            data.isInsentif = vm.isInsentif;
            data.isBonus = vm.isBonus;
            data.isLeaveCompansation = vm.isLeaveCompansation;
            data.isPositionAllowance = vm.isPositionAllowance;
            data.Keterangan = vm.Keterangan;

            _applicationDbContext.SubLevels.Update(data);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Update data berhasil || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubLevel(Guid id)
        {
            if (!_applicationDbContext.Database.CanConnect())
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            var data = await _applicationDbContext.SubLevels.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _applicationDbContext.SubLevels.Remove(data);
            var result = await _applicationDbContext.SaveChangesAsync();

            if (result > 0)
                return Ok(new { message = "Data berhasil dihapus || 200 OK" });

            return StatusCode(500, new { message = "Data tidak berhasil dihapus." });
        }
    }
}
