using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Observasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ObservasiCairanWsdController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<DiskonController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObservasiCairanWsdController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DiskonController> logger,
            IWebHostEnvironment webHostEnvironment,
            ITTDService ttdService
        )
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _ttdService = ttdService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from o in _db.ObservasiCairanWsds
                        where o.IsDelete == false
                        select new
                        {
                            o.ObservasiCairanWSDId,
                            o.KunjunganId,
                            o.PasienId,
                            o.UserActiveId,
                            o.TglAwalObservasiWSD,
                            o.TglAkhirObservasiWSD,
                            o.CairanSisaWSDSebelumnya,
                            o.CairanWSDBertambah,
                            o.CairanSisaWSDTabung,
                            o.TtdId,
                            o.PathTtd,
                            o.Keterangan,
                            o.CreateDateTime
                        };

            var totalRows = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreateDateTime)
                                  .Skip((page - 1) * perPage)
                                  .Take(perPage)
                                  .ToListAsync();

            return Ok(new
            {
                message = "Data retrieved successfully",
                data,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _db.ObservasiCairanWsds.FindAsync(id);
            if (item == null || item.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            return Ok(new { message = "Ditemukan", data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObservasiCairanWsdViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserActives.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            // cek ttd 
            var ttd = await _ttdService.CheckTTDAsync(vm.TtdId ?? Guid.Empty);

            var entity = new ObservasiCairanWsd
            {
                ObservasiCairanWSDId = Guid.NewGuid(),
                KunjunganId = vm.KunjunganId,
                PasienId = vm.PasienId,
                UserActiveId = user.UserActiveId,
                TglAwalObservasiWSD = vm.TglAwalObservasiWSD,
                TglAkhirObservasiWSD = vm.TglAkhirObservasiWSD,
                CairanSisaWSDSebelumnya = vm.CairanSisaWSDSebelumnya,
                CairanWSDBertambah = vm.CairanWSDBertambah,
                CairanSisaWSDTabung = vm.CairanSisaWSDTabung,
                TtdId = vm.TtdId,
                PathTtd = ttd.Path,
                Keterangan = vm.Keterangan,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = user.UserActiveId
            };

            _db.ObservasiCairanWsds.Add(entity);
            await _db.SaveChangesAsync();

            return Created("", new { message = "Data berhasil ditambahkan.", ttdPetugasId = ttd.TTDId});
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ObservasiCairanWsdViewModel vm)
        {
            var item = await _db.ObservasiCairanWsds.FindAsync(id);
            if (item == null || item.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserActives.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            // cek ttd
            var ttd = await _ttdService.CheckTTDAsync(vm.TtdId ?? Guid.Empty);

            item.KunjunganId = vm.KunjunganId;
            item.PasienId = vm.PasienId;
            item.TglAwalObservasiWSD = vm.TglAwalObservasiWSD;
            item.TglAkhirObservasiWSD = vm.TglAkhirObservasiWSD;
            item.CairanSisaWSDSebelumnya = vm.CairanSisaWSDSebelumnya;
            item.CairanWSDBertambah = vm.CairanWSDBertambah;
            item.CairanSisaWSDTabung = vm.CairanSisaWSDTabung;
            item.TtdId = vm.TtdId;
            item.PathTtd = ttd.Path;
            item.Keterangan = vm.Keterangan;
            item.UpdateBy = user.UserActiveId;
            item.UpdateDateTime = DateTime.UtcNow;

            _db.ObservasiCairanWsds.Update(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui.", ttdPetugasId = ttd.TTDId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var item = await _db.ObservasiCairanWsds.FindAsync(id);
            if (item == null || item.IsDelete)
                return NotFound(new { message = "Data tidak ditemukan." });

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserActives.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            item.IsDelete = true;
            item.DeleteBy = user.UserActiveId;
            item.DeleteDateTime = DateTime.UtcNow;

            _db.ObservasiCairanWsds.Update(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (soft delete)." });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null)
            {
                try
                {
                    if (page < 1) page = 1;
                    if (perPage < 1) perPage = 10;

                    // Query dasar
                    var query = from o in _db.ObservasiCairanWsds
                                join u in _db.UserActives on o.UserActiveId equals u.UserActiveId
                                where o.IsDelete == false
                                select new
                                {
                                    o.ObservasiCairanWSDId,
                                    o.KunjunganId,
                                    o.PasienId,
                                    o.UserActiveId,
                                    UserFullName = u.FullName,
                                    o.TglAwalObservasiWSD,
                                    o.TglAkhirObservasiWSD,
                                    o.CairanSisaWSDSebelumnya,
                                    o.CairanWSDBertambah,
                                    o.CairanSisaWSDTabung,
                                    o.TtdId,
                                    o.PathTtd,
                                    o.Keterangan,
                                    o.CreateDateTime
                                };

                    // filter based on kunjungan id
                    if (kunjunganId.HasValue )
                    {
                        query = query.Where(u=>u.KunjunganId==kunjunganId.Value);
                    }

                    // filter based on pasien id
                    if (pasienId.HasValue)
                    {
                        query = query.Where(u => u.PasienId == pasienId.Value);
                    }

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                    {
                        search = $"%{search.ToLower()}%";
                        query = query.Where(x =>
                            EF.Functions.ILike(x.Keterangan ?? "", search) ||
                            EF.Functions.ILike(x.UserFullName ?? "", search)
                        );
                    }

                    // Filter tanggal
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        var start = startDate.Value.Date.ToUniversalTime();
                        var end = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime <= end);
                    }

                    // Sorting
                    var sort = orderBy?.ToLower() ?? "createdatetime";
                    var desc = sortDirection?.ToLower() == "desc";

                    query = sort switch
                    {
                        "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                        "userfullname" => desc ? query.OrderByDescending(x => x.UserFullName) : query.OrderBy(x => x.UserFullName),
                        _ => query.OrderByDescending(x => x.CreateDateTime)
                    };

                    // Pagination
                    int totalRows = await query.CountAsync();
                    int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                    var data = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

                    if (data.Count == 0 && page > totalPages)
                    {
                        return NotFound(new { message = "Page not found." });
                    }

                    return Ok(new
                    {
                        status = "success",
                        message = "Data retrieved successfully",
                        data = new
                        {
                            Rows = data,
                            TotalRows = totalRows,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = totalPages
                        }
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

    }
}
