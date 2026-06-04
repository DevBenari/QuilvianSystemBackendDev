using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class StockDarahController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<StockDarahController> _logger;
        private readonly IWebHostEnvironment _env;

        public StockDarahController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<StockDarahController> logger,
            IWebHostEnvironment env
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from s in _context.StockDarahs
                        join g in _context.UserActives on s.CreateBy equals g.UserActiveId
                        where s.IsDelete == false
                        orderby s.CreateDateTime descending
                        select new
                        {
                            s.StockDarahId,
                            s.DarahDetailId,
                            s.TipeKomponenId,
                            s.Rhesus,
                            s.Golongan,
                            s.Wacc,
                            s.JumlahKantong,
                            s.Amount,
                            s.JumlahExpired,
                            s.TglExpired,
                            s.SisaStock,
                            s.MinStock,
                            s.StatusStock,
                            s.Keterangan,
                            s.CreateDateTime,
                            CreateByName = g.FullName
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
            var data = await _context.StockDarahs.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan || 200 OK", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockDarahViewModel vm)
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

                var data = new StockDarah
                {
                    StockDarahId = Guid.NewGuid(),
                    DarahDetailId = vm.DarahDetailId,
                    TipeKomponenId = vm.TipeKomponenId,
                    Rhesus = vm.Rhesus,
                    Golongan = vm.Golongan,
                    Wacc = vm.Wacc,
                    JumlahKantong = vm.JumlahKantong,
                    Amount = vm.Amount,
                    JumlahExpired = vm.JumlahExpired,
                    TglExpired = vm.TglExpired,
                    SisaStock = vm.SisaStock,
                    MinStock = vm.MinStock,
                    StatusStock = vm.StatusStock,
                    Keterangan = vm.Keterangan,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.StockDarahs.Add(data);
                await _context.SaveChangesAsync();

                return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StockDarahViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                var data = await _context.StockDarahs.FindAsync(id);
                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                data.DarahDetailId = vm.DarahDetailId;
                data.TipeKomponenId = vm.TipeKomponenId;
                data.Rhesus = vm.Rhesus;
                data.Golongan = vm.Golongan;
                data.Wacc = vm.Wacc;
                data.JumlahKantong = vm.JumlahKantong;
                data.Amount = vm.Amount;
                data.JumlahExpired = vm.JumlahExpired;
                data.TglExpired = vm.TglExpired;
                data.SisaStock = vm.SisaStock;
                data.MinStock = vm.MinStock;
                data.StatusStock = vm.StatusStock;
                data.Keterangan = vm.Keterangan;
                data.UpdateBy = user?.UserActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _context.StockDarahs.Update(data);
                await _context.SaveChangesAsync();

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
                var data = await _context.StockDarahs.FindAsync(id);
                if (data == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.UserActives.FirstOrDefault(u => u.Email == emailLogin);

                data.IsDelete = true;
                data.DeleteBy = user?.UserActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                _context.StockDarahs.Update(data);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
        // GET /paged
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

                var query = from s in _context.StockDarahs
                            join u in _context.UserActives on s.CreateBy equals u.UserActiveId
                            where s.IsDelete == false
                            select new
                            {
                                s.StockDarahId,
                                s.DarahDetailId,
                                s.TipeKomponenId,
                                s.Rhesus,
                                s.Golongan,
                                s.Wacc,
                                s.JumlahKantong,
                                s.Amount,
                                s.JumlahExpired,
                                s.TglExpired,
                                s.SisaStock,
                                s.MinStock,
                                s.StatusStock,
                                s.Keterangan,
                                s.CreateDateTime,
                                CreateByName = u.FullName
                            };

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%";
                    query = query.Where(s =>
                        EF.Functions.ILike(s.Golongan, search) ||
                        EF.Functions.ILike(s.Rhesus, search) ||
                        EF.Functions.ILike(s.StatusStock, search) ||
                        EF.Functions.ILike(s.Keterangan, search)
                    );
                }

                // Filter tanggal
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(s => s.CreateDateTime >= startUtc && s.CreateDateTime <= endUtc);
                }

                // Sorting dinamis
                var sortCol = orderBy?.ToLower() ?? "createdatetime";
                bool isDesc = sortDirection?.ToLower() == "desc";

                query = sortCol switch
                {
                    "golongan" => isDesc ? query.OrderByDescending(x => x.Golongan) : query.OrderBy(x => x.Golongan),
                    "rhesus" => isDesc ? query.OrderByDescending(x => x.Rhesus) : query.OrderBy(x => x.Rhesus),
                    "jumlahkantong" => isDesc ? query.OrderByDescending(x => x.JumlahKantong) : query.OrderBy(x => x.JumlahKantong),
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
                _logger.LogError(ex, "Error in GetPaged StockDarah");
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}
