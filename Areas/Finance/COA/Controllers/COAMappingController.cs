using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.COA.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class COAMappingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<COAMappingController> _logger;

        public COAMappingController(
            ApplicationDbContext context,
            ILogger<COAMappingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query =
                from m in _context.COAMappings
                join coa in _context.MasterCoas
                    on m.COAId equals coa.COAId
                join u in _context.UserActives
                    on m.CreateBy equals u.UserActiveId
                where m.IsDelete == false
                select new
                {
                    m.COAMappingId,
                    m.TransaksiId,
                    m.NamaTransaksi,
                    m.COAId,
                    NamaCOA = coa.NamaCOA,
                    m.Keterangan,
                    m.CreateDateTime,
                    CreateByName = u.FullName
                };

            var totalRows = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = "success",
                data,
                pagination = new
                {
                    page,
                    perPage,
                    totalRows,
                    totalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data =
                await (from m in _context.COAMappings
                       join coa in _context.MasterCoas
                            on m.COAId equals coa.COAId
                       where m.COAMappingId == id && m.IsDelete == false
                       select new
                       {
                           m.COAMappingId,
                           m.TransaksiId,
                           m.NamaTransaksi,
                           m.COAId,
                           NamaCOA = coa.NamaCOA,
                           m.Keterangan
                       }).FirstOrDefaultAsync();

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new
            {
                message = "success",
                data
            });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] COAMapping model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            var coa = await _context.MasterCoas
                .FirstOrDefaultAsync(x => x.COAId == model.COAId && x.IsDelete == false);

            if (coa == null)
                return BadRequest(new
                {
                    message = "COA tidak ditemukan"
                });

            model.COAMappingId = Guid.NewGuid();
            model.NamaCOA = coa.NamaCOA;

            // Ambil NamaTransaksi dari tabel transaksi sesuai kebutuhan Anda
            // model.NamaTransaksi = ...

            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.COAMappings.Add(model);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "created"
            });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] COAMapping model)
        {
            var data = await _context.COAMappings
                .FirstOrDefaultAsync(x => x.COAMappingId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            var coa = await _context.MasterCoas
                .FirstOrDefaultAsync(x => x.COAId == model.COAId && x.IsDelete == false);

            if (coa == null)
                return BadRequest(new
                {
                    message = "COA tidak ditemukan"
                });

            data.TransaksiId = model.TransaksiId;
            data.NamaTransaksi = model.NamaTransaksi;
            data.COAId = model.COAId;
            data.NamaCOA = coa.NamaCOA;
            data.Keterangan = model.Keterangan;

            data.UpdateBy = user?.UserActiveId ?? data.UpdateBy;
            data.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "updated"
            });
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.COAMappings
                .FirstOrDefaultAsync(x => x.COAMappingId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            data.IsDelete = true;
            data.DeleteBy = user.UserActiveId;
            data.DeleteDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "deleted"
            });
        }

        // ================= PAGED =================
        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null)
        {
            var query =
                from m in _context.COAMappings
                join coa in _context.MasterCoas
                    on m.COAId equals coa.COAId
                join u in _context.UserActives
                    on m.CreateBy equals u.UserActiveId
                where m.IsDelete == false
                select new
                {
                    m.COAMappingId,
                    m.TransaksiId,
                    m.NamaTransaksi,
                    m.COAId,
                    NamaCOA = coa.NamaCOA,
                    m.Keterangan,
                    m.CreateDateTime,
                    CreateByName = u.FullName
                };

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NamaTransaksi!, search) ||
                    EF.Functions.ILike(x.NamaCOA!, search));
            }

            var totalRows = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = "success",
                data,
                pagination = new
                {
                    page,
                    perPage,
                    totalRows,
                    totalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }
    }
}
