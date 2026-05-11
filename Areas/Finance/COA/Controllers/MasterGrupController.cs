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
    [EnableCors("AllowSpecific")]
    public class MasterGrupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MasterGrupController> _logger;

        public MasterGrupController(
            ApplicationDbContext context,
            ILogger<MasterGrupController> logger)
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

            var query = from g in _context.MasterGrups
                        join u in _context.UserActives
                        on g.CreateBy equals u.UserActiveId
                        join t in _context.TipeAkuns
                        on g.TipeAkunCOAId equals t.TipeAkunCOAId into tipeJoin
                        from t in tipeJoin.DefaultIfEmpty()
                        where g.IsDelete == false
                        select new
                        {
                            g.GrupCOAId,
                            g.TipeAkunCOAId,
                            NamaTipeAkunCOA = t != null ? t.NamaTipeAkunCOA : null,
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
            var data = await _context.MasterGrups
                .FirstOrDefaultAsync(x => x.GrupCOAId == id && x.IsDelete == false);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new { message = "success", data });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MasterGrup model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            model.GrupCOAId = Guid.NewGuid();
            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.MasterGrups.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "created" });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterGrup model)
        {
            var data = await _context.MasterGrups
                .FirstOrDefaultAsync(x => x.GrupCOAId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            data.TipeAkunCOAId = model.TipeAkunCOAId;
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
            var data = await _context.MasterGrups
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
            var query = from g in _context.MasterGrups
                        join u in _context.UserActives
                        on g.CreateBy equals u.UserActiveId
                        join t in _context.TipeAkuns
                        on g.TipeAkunCOAId equals t.TipeAkunCOAId into tipeJoin
                        from t in tipeJoin.DefaultIfEmpty()
                        where g.IsDelete == false
                        select new
                        {
                            g.GrupCOAId,
                            g.NamaGrupCOA,
                            g.KodeGrupCOA,
                            NamaTipeAkunCOA = t != null ? t.NamaTipeAkunCOA : null,
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
