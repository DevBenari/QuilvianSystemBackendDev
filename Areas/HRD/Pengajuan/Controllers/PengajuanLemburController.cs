using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Models; // untuk UserActivity
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Controllers
{
    [Route("api/HRD/[controller]")]
    [ApiController]
    public class PengajuanLemburController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengajuanLemburController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/HRD/PengajuanLembur?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPengajuanLembur(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from l in _context.PengajuanLemburs
                        join u1 in _context.UserActives on l.ApprovedBy1 equals u1.UserActiveId into approvedBy1Join
                        from u1 in approvedBy1Join.DefaultIfEmpty()
                        join u2 in _context.UserActives on l.ApprovedBy2 equals u2.UserActiveId into approvedBy2Join
                        from u2 in approvedBy2Join.DefaultIfEmpty()
                        join uc in _context.UserActives on l.CreateBy equals uc.UserActiveId into createdByJoin
                        from uc in createdByJoin.DefaultIfEmpty()
                        where l.IsDelete == false
                        orderby l.CreateDateTime descending
                        select new
                        {
                            l.PengajuanLemburId,
                            l.UserActiveId,
                            l.DepartementId,
                            l.JenisLemburId,
                            l.TglLembur,
                            l.Keterangan,
                            l.LamaLembur,
                            l.Deskripsi,

                            l.ApprovedBy1,
                            ApprovedBy1Name = u1 != null ? u1.FullName : null,
                            l.ApprovedBy2,
                            ApprovedBy2Name = u2 != null ? u2.FullName : null,

                            l.CreateDateTime,
                            l.CreateBy,
                            CreateByName = uc != null ? uc.FullName : null,
                            l.UpdateDateTime,
                            l.UpdateBy,
                            l.DeleteDateTime,
                            l.DeleteBy,
                            l.IsDelete
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listData.Any())
                return NotFound(new { message = "Belum ada data || 404 Not Found" });

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listData,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        // ✅ GET: api/HRD/PengajuanLembur/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PengajuanLembur>> GetPengajuanLembur(Guid id)
        {
            var pengajuan = await _context.PengajuanLemburs.FindAsync(id);

            if (pengajuan == null)
                return NotFound();

            return pengajuan;
        }

        // ✅ POST: api/HRD/PengajuanLembur
        [HttpPost]
        public async Task<ActionResult<PengajuanLembur>> PostPengajuanLembur(PengajuanLembur pengajuan)
        {
            pengajuan.PengajuanLemburId = Guid.NewGuid();
            pengajuan.CreateDateTime = DateTimeOffset.UtcNow;
            // pengajuan.CreateBy bisa diisi dari User Login (contoh: HttpContext.User)

            _context.PengajuanLemburs.Add(pengajuan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPengajuanLembur), new { id = pengajuan.PengajuanLemburId }, pengajuan);
        }

        // ✅ PUT: api/HRD/PengajuanLembur/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPengajuanLembur(Guid id, PengajuanLembur pengajuan)
        {
            if (id != pengajuan.PengajuanLemburId)
                return BadRequest();

            pengajuan.UpdateDateTime = DateTimeOffset.UtcNow;

            _context.Entry(pengajuan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PengajuanLemburs.Any(e => e.PengajuanLemburId == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // ✅ DELETE (soft delete): api/HRD/PengajuanLembur/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanLembur(Guid id)
        {
            var pengajuan = await _context.PengajuanLemburs.FindAsync(id);
            if (pengajuan == null)
                return NotFound();

            pengajuan.IsDelete = true;
            pengajuan.DeleteDateTime = DateTimeOffset.UtcNow;
            // pengajuan.DeleteBy = user login id

            _context.PengajuanLemburs.Update(pengajuan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (soft delete)" });
        }
    }
}
