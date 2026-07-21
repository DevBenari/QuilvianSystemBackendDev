using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.COA.Models;
using QuilvianSystemBackendDev.Areas.Finance.COA.ViewModels;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.COA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class GrupCoaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GrupCoaController> _logger;

        public GrupCoaController(
            ApplicationDbContext context,
            ILogger<GrupCoaController> logger)
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

            var query = from g in _context.GrupCoas
                        join u in _context.UserActives
                        on g.CreateBy equals u.UserActiveId
                        where g.IsDelete == false
                        select new
                        {
                            g.GrupCOAId,
                            g.NamaGrupCOA,
                            g.KodeGrupCOA,
                            g.Keterangan,
                            g.CreateDateTime,
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
            var data = await _context.GrupCoas
                .FirstOrDefaultAsync(x => x.GrupCOAId == id && x.IsDelete == false);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new { message = "success", data });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GrupCoaViewModel model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            var grupCoa = new GrupCoa
            {
                GrupCOAId = Guid.NewGuid(),
                NamaGrupCOA = model.NamaGrupCOA,
                CreateBy = user.UserActiveId,
                CreateDateTime = DateTime.UtcNow,
                IsDelete = false
            };

            _context.GrupCoas.Add(grupCoa);
            await _context.SaveChangesAsync();

            return Ok(new { message = "created" });
        }
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] GrupCoa model)
        //{
        //    var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var user = await _context.UserActives
        //        .FirstOrDefaultAsync(x => x.Email == email);

        //    if (user == null)
        //        return Unauthorized();

        //    model.GrupCOAId = Guid.NewGuid();
        //    model.CreateBy = user.UserActiveId;
        //    model.CreateDateTime = DateTime.UtcNow;
        //    model.IsDelete = false;

        //    _context.GrupCoas.Add(model);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "created" });
        //}

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GrupCoa model)
        {
            var data = await _context.GrupCoas
                .FirstOrDefaultAsync(x => x.GrupCOAId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            data.NamaGrupCOA = model.NamaGrupCOA;
            data.KodeGrupCOA = model.KodeGrupCOA;
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
            var data = await _context.GrupCoas
                .FirstOrDefaultAsync(x => x.GrupCOAId == id && x.IsDelete == false);

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
            var query = from g in _context.GrupCoas
                        join u in _context.UserActives
                        on g.CreateBy equals u.UserActiveId
                        where g.IsDelete == false
                        select new
                        {
                            g.GrupCOAId,
                            g.NamaGrupCOA,
                            g.KodeGrupCOA,
                            g.Keterangan,
                            g.CreateDateTime,
                            CreateByName = u.FullName
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NamaGrupCOA, search) ||
                    EF.Functions.ILike(x.KodeGrupCOA, search));
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
