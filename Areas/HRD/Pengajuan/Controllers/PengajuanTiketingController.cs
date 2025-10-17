using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Models; // Untuk UserActivity & ApplicationDbContext
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Controllers
{
    [Route("api/HRD/[controller]")]
    [ApiController]
    public class PengajuanTiketingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengajuanTiketingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/HRD/PengajuanTiketing?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> GetPengajuanTiketing(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from t in _context.PengajuanTiketings
                        join u1 in _context.UserActives on t.ApprovedBy1 equals u1.UserActiveId into approvedBy1Join
                        from u1 in approvedBy1Join.DefaultIfEmpty()
                        join u2 in _context.UserActives on t.ApprovedBy2 equals u2.UserActiveId into approvedBy2Join
                        from u2 in approvedBy2Join.DefaultIfEmpty()
                        join uc in _context.UserActives on t.CreateBy equals uc.UserActiveId into createdByJoin
                        from uc in createdByJoin.DefaultIfEmpty()
                        orderby t.CreateDateTime descending
                        select new
                        {
                            t.TicketId,
                            t.UserActiveId,
                            t.DepartementId,
                            t.JenisTicketId,
                            t.NoAntrian,
                            t.JudulTicketing,
                            t.Deskripsi,
                            t.Prioritas,
                            t.Ruangan,
                            t.TglDibutuhkan,
                            t.EstimasiBudget,
                            t.Lampiran,
                            t.Status,

                            t.ApprovedBy1,
                            ApprovedBy1Name = u1 != null ? u1.FullName : null,
                            t.ApprovedBy2,
                            ApprovedBy2Name = u2 != null ? u2.FullName : null,

                            t.CreateDateTime,
                            t.CreateBy,
                            CreateByName = uc != null ? uc.FullName : null,
                            t.UpdateDateTime,
                            t.UpdateBy,
                            t.DeleteDateTime,
                            t.DeleteBy,
                            t.IsDelete
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return Ok(new
            {
                message = listData.Any() ? "Berhasil || 200 OK" : "Belum ada data",
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

        // ✅ GET by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PengajuanTiketing>> GetPengajuanTiketing(Guid id)
        {
            var tiketing = await _context.PengajuanTiketings.FindAsync(id);

            if (tiketing == null)
                return NotFound();

            return tiketing;
        }

        // ✅ POST
        [HttpPost]
        public async Task<ActionResult<PengajuanTiketing>> PostPengajuanTiketing(PengajuanTiketing tiketing)
        {
            tiketing.TicketId = Guid.NewGuid();
            tiketing.CreateDateTime = DateTimeOffset.UtcNow;
            // tiketing.CreateBy = userId dari token, jika ada

            _context.PengajuanTiketings.Add(tiketing);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPengajuanTiketing), new { id = tiketing.TicketId }, tiketing);
        }

        // ✅ PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPengajuanTiketing(Guid id, PengajuanTiketing tiketing)
        {
            if (id != tiketing.TicketId)
                return BadRequest();

            tiketing.UpdateDateTime = DateTimeOffset.UtcNow;
            _context.Entry(tiketing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PengajuanTiketings.Any(e => e.TicketId == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanTiketing(Guid id)
        {
            var tiketing = await _context.PengajuanTiketings.FindAsync(id);
            if (tiketing == null)
                return NotFound(new { message = "Data tidak ditemukan." });

            _context.PengajuanTiketings.Remove(tiketing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus (hard delete) || 200 OK" });
        }
    }
}
