using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Models;
using QuilvianSystemBackendDev.Areas.HRD.Pengajuan.ViewModels;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.Pengajuan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class PengajuanCutiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengajuanCutiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPengajuanCuti(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from c in _context.PengajuanCutis
                        join u1 in _context.UserActives on c.ApprovedBy equals u1.UserActiveId into approvedByJoin
                        from u1 in approvedByJoin.DefaultIfEmpty()
                        join u2 in _context.UserActives on c.Approved2By equals u2.UserActiveId into approved2ByJoin
                        from u2 in approved2ByJoin.DefaultIfEmpty()
                        join u3 in _context.UserActives on c.CreateBy equals u3.UserActiveId into createdByJoin
                        from u3 in createdByJoin.DefaultIfEmpty()
                        where c.IsDelete == false
                        orderby c.CreateDateTime descending
                        select new
                        {
                            c.PengajuanCutiId,
                            c.UserActiveId,
                            c.JenisCutiId,
                            c.MulaiCuti,
                            c.SelesaiCuti,
                            c.JumlahCutiDiambil,
                            c.SisaKuotaCuti,
                            c.AlasanCuti,
                            c.PICPengganti,

                            c.ApprovedBy,
                            ApprovedByName = u1 != null ? u1.FullName : null,
                            c.TglPersetujuan,
                            c.CatatanApprovedBy,

                            c.Approved2By,
                            Approved2ByName = u2 != null ? u2.FullName : null,
                            c.TglPersetujuan2,
                            c.CatatanApproved2By,

                            c.LampiranPendukung,
                            c.DepartemenId,

                            c.CreateDateTime,
                            c.CreateBy,
                            CreateByName = u3 != null ? u3.FullName : null,
                            c.UpdateDateTime,
                            c.UpdateBy,
                            c.DeleteDateTime,
                            c.DeleteBy,
                            c.IsDelete
                        };

            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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

        // GET: api/HRD/PengajuanCuti/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PengajuanCuti>> GetPengajuanCuti(Guid id)
        {
            var pengajuanCuti = await _context.PengajuanCutis.FindAsync(id);

            if (pengajuanCuti == null)
                return NotFound();

            return pengajuanCuti;
        }

        // POST: api/HRD/PengajuanCuti
        [HttpPost]
        public async Task<ActionResult<PengajuanCuti>> PostPengajuanCuti(PengajuanCuti pengajuanCuti)
        {
            pengajuanCuti.PengajuanCutiId = Guid.NewGuid();
            _context.PengajuanCutis.Add(pengajuanCuti);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPengajuanCuti), new { id = pengajuanCuti.PengajuanCutiId }, pengajuanCuti);
        }

        // PUT: api/HRD/PengajuanCuti/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPengajuanCuti(Guid id, PengajuanCuti pengajuanCuti)
        {
            if (id != pengajuanCuti.PengajuanCutiId)
                return BadRequest();

            _context.Entry(pengajuanCuti).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PengajuanCutiExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/HRD/PengajuanCuti/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengajuanCuti(Guid id)
        {
            var pengajuanCuti = await _context.PengajuanCutis.FindAsync(id);
            if (pengajuanCuti == null)
                return NotFound();

            _context.PengajuanCutis.Remove(pengajuanCuti);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PengajuanCutiExists(Guid id)
        {
            return _context.PengajuanCutis.Any(e => e.PengajuanCutiId == id);
        }
    }
}
