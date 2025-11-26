using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PenerimaDarahPasienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<PenerimaDarahPasienController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<PenerimaanDarahPasienHub> _hubContext;

        public PenerimaDarahPasienController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PenerimaDarahPasienController> logger,
            IWebHostEnvironment env,
            IHubContext<PenerimaanDarahPasienHub> hubContext
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _env = env;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from p in _context.PenerimaDarahPasiens
                        join u in _context.UserActives on p.CreateBy equals u.UserActiveId
                        where p.IsDelete == false
                        orderby p.CreateDateTime descending
                        select new
                        {
                            p.PenerimaanDarahPasienId,
                            p.PasienId,
                            p.GolonganDarahId,
                            p.Rhesus,
                            p.JumlahKantong,
                            p.Sumber,
                            p.TglMasuk,
                            p.TglExpired,
                            p.Keterangan,
                            p.CreateDateTime,
                            CreateByName = u.FullName
                        };

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (!listdata.Any())
                return NotFound(new { message = "Belum ada data || 404 Not Found" });

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
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.PenerimaDarahPasiens.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PenerimaDarahPasienViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                if (!_context.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var data = new PenerimaDarahPasien
                {
                    PenerimaanDarahPasienId = Guid.NewGuid(),
                    PasienId = vm.PasienId,
                    GolonganDarahId = vm.GolonganDarahId,
                    Rhesus = vm.Rhesus,
                    JumlahKantong = vm.JumlahKantong,
                    Sumber = vm.Sumber,
                    TglMasuk = vm.TglMasuk,
                    TglExpired = vm.TglExpired,
                    Keterangan = vm.Keterangan,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.PenerimaDarahPasiens.Add(data);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("Penerimaan Darah pasien Created", new
                {
                    Action = "create",
                    id = data.PenerimaanDarahPasienId
                });

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PenerimaDarahPasienViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                var data = await _context.PenerimaDarahPasiens.FindAsync(id);
                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                data.PasienId = vm.PasienId;
                data.GolonganDarahId = vm.GolonganDarahId;
                data.Rhesus = vm.Rhesus;
                data.JumlahKantong = vm.JumlahKantong;
                data.Sumber = vm.Sumber;
                data.TglMasuk = vm.TglMasuk;
                data.TglExpired = vm.TglExpired;
                data.Keterangan = vm.Keterangan;
                data.UpdateBy = (Guid)(user?.UserActiveId);
                data.UpdateDateTime = DateTime.UtcNow;

                _context.PenerimaDarahPasiens.Update(data);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("Penerimaan Darah pasien Changed", new
                {
                    Action = "changed",
                    id = data.PenerimaanDarahPasienId
                });

                return Ok(new { message = "Update Data Berhasil || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var data = await _context.PenerimaDarahPasiens.FindAsync(id);
                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                data.IsDelete = true;
                data.DeleteBy = (Guid)(user?.UserActiveId);
                data.DeleteDateTime = DateTime.UtcNow;

                _context.PenerimaDarahPasiens.Update(data);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            DateTime? startDate = null,
            DateTime? endDate = null
)
        {
            try
            {
                if (!await _context.Database.CanConnectAsync())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                var query = from p in _context.PenerimaDarahPasiens
                            join u in _context.UserActives on p.CreateBy equals u.UserActiveId
                            where p.IsDelete == false
                            select new
                            {
                                p.PenerimaanDarahPasienId,
                                p.PasienId,
                                p.GolonganDarahId,
                                p.Rhesus,
                                p.JumlahKantong,
                                p.Sumber,
                                p.TglMasuk,
                                p.TglExpired,
                                p.Keterangan,
                                p.CreateDateTime,
                                CreateByName = u.FullName
                            };

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%";
                    query = query.Where(p =>
                        EF.Functions.ILike(p.Rhesus, search) ||
                        EF.Functions.ILike(p.Sumber, search) ||
                        EF.Functions.ILike(p.Keterangan, search)
                    );
                }

                // Filter tanggal
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(p => p.CreateDateTime >= startUtc && p.CreateDateTime <= endUtc);
                }

                // Sorting
                var sortCol = orderBy?.ToLower() ?? "createdatetime";
                bool isDesc = sortDirection?.ToLower() == "desc";

                query = sortCol switch
                {
                    "rhesus" => isDesc ? query.OrderByDescending(x => x.Rhesus) : query.OrderBy(x => x.Rhesus),
                    "sumber" => isDesc ? query.OrderByDescending(x => x.Sumber) : query.OrderBy(x => x.Sumber),
                    "tglmasuk" => isDesc ? query.OrderByDescending(x => x.TglMasuk) : query.OrderBy(x => x.TglMasuk),
                    _ => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
                };

                int totalRows = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

                if (!rows.Any())
                    return NotFound(new { message = "Page not found or no data available." });

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = rows,
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaged PenerimaanDarah");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


    }
}
