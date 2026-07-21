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
    public class MasterCoaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MasterCoaController> _logger;

        public MasterCoaController(
            ApplicationDbContext context,
            ILogger<MasterCoaController> logger)
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

            var query = from c in _context.MasterCoas
                        join g in _context.GrupCoas
                        on c.GrupCOAId equals g.GrupCOAId into grupJoin
                        from g in grupJoin.DefaultIfEmpty()
                        join u in _context.UserActives
                        on c.CreateBy equals u.UserActiveId
                        where c.IsDelete == false
                        select new
                        {
                            c.COAId,
                            c.GrupCOAId,
                            NamaGrupCOA = g != null ? g.NamaGrupCOA : null,
                            c.NamaCOA,
                            c.KodeCOA,
                            c.IsPostable,
                            c.IsValid,
                            c.IsPLACC,
                            c.NomalBalance,

                            c.TipeAkunCOAId,
                            c.TipeTransaksi,

                            c.Keterangan,
                            c.CreateDateTime,
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
            var data = await _context.MasterCoas
                .FirstOrDefaultAsync(x => x.COAId == id && x.IsDelete == false);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new { message = "success", data });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MasterCoa model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            model.COAId = Guid.NewGuid();
            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.MasterCoas.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "created" });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterCoa model)
        {
            var data = await _context.MasterCoas
                .FirstOrDefaultAsync(x => x.COAId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            //data.GrupCOAId = model.GrupCOAId;
            data.NamaCOA = model.NamaCOA;
            data.KodeCOA = model.KodeCOA;
            data.IsPostable = model.IsPostable;
            data.IsValid = model.IsValid;
            data.IsPLACC = model.IsPLACC;
            data.NomalBalance = model.NomalBalance;
            data.TipeAkunCOAId = model.TipeAkunCOAId;
            data.TipeTransaksi = model.TipeTransaksi;
            data.GrupCOAId = model.GrupCOAId;
            data.Keterangan = model.Keterangan;

            data.UpdateBy = user?.UserActiveId ?? data.UpdateBy;
            data.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "updated" });
        }

        // ================= DELETE (SOFT) =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.MasterCoas
                .FirstOrDefaultAsync(x => x.COAId == id && x.IsDelete == false);

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

            return Ok(new { message = "deleted" });
        }

        // ================= PAGED =================
        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null)
        {
            var query = from c in _context.MasterCoas
                        join g in _context.GrupCoas
                        on c.GrupCOAId equals g.GrupCOAId into grupJoin
                        from g in grupJoin.DefaultIfEmpty()
                        join u in _context.UserActives
                        on c.CreateBy equals u.UserActiveId
                        where c.IsDelete == false
                        select new
                        {
                            c.COAId,
                            c.GrupCOAId,
                            NamaGrupCOA = g != null ? g.NamaGrupCOA : null,
                            c.NamaCOA,
                            c.KodeCOA,
                            c.IsPostable,
                            c.IsValid,
                            c.IsPLACC,
                            c.NomalBalance,

                            c.TipeAkunCOAId,
                            c.TipeTransaksi,

                            c.Keterangan,
                            c.CreateDateTime,
                            CreateByName = u.FullName
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NamaCOA, search) ||
                    EF.Functions.ILike(x.KodeCOA, search));
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
