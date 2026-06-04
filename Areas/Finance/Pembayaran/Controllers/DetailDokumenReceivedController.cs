using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class DetailDokumenReceivedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailDokumenReceivedController> _logger;

        public DetailDokumenReceivedController(
            ApplicationDbContext context,
            ILogger<DetailDokumenReceivedController> logger)
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

            var query = from d in _context.DetailDokumenReceiveds
                        join u in _context.UserActives
                        on d.CreateBy equals u.UserActiveId
                        where d.IsDelete == false
                        select new
                        {
                            d.DetailDokReceivedId,
                            d.DetailReceivedPaymentId,
                            d.KunjunganId,
                            d.PasienId,
                            d.NoBilling,
                            d.SuratPengantar,
                            d.Kwitansi,
                            d.RekapitulasiTagihan,
                            d.Invoice,
                            d.TandaTerima,
                            d.TglTerima,
                            d.TglKirim,
                            d.TglTagihan,
                            d.TotalPiutang,
                            d.TglJaatuhTempo,
                            d.IsTerbayar,
                            d.Keterangan,
                            d.CreateDateTime,
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
            var data = await _context.DetailDokumenReceiveds
                .FirstOrDefaultAsync(x =>
                    x.DetailDokReceivedId == id &&
                    x.IsDelete == false);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new { message = "success", data });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DetailDokumenReceived model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            model.DetailDokReceivedId = Guid.NewGuid();
            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.DetailDokumenReceiveds.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "created" });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DetailDokumenReceived model)
        {
            var data = await _context.DetailDokumenReceiveds
                .FirstOrDefaultAsync(x =>
                    x.DetailDokReceivedId == id &&
                    x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            data.DetailReceivedPaymentId = model.DetailReceivedPaymentId;
            data.KunjunganId = model.KunjunganId;
            data.PasienId = model.PasienId;
            data.NoBilling = model.NoBilling;
            data.SuratPengantar = model.SuratPengantar;
            data.Kwitansi = model.Kwitansi;
            data.RekapitulasiTagihan = model.RekapitulasiTagihan;
            data.Invoice = model.Invoice;
            data.TandaTerima = model.TandaTerima;
            data.TglTerima = model.TglTerima;
            data.TglKirim = model.TglKirim;
            data.TglTagihan = model.TglTagihan;
            data.TotalPiutang = model.TotalPiutang;
            data.TglJaatuhTempo = model.TglJaatuhTempo;
            data.IsTerbayar = model.IsTerbayar;
            data.Keterangan = model.Keterangan;

            data.UpdateBy = user?.UserActiveId ?? Guid.Empty;
            data.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "updated" });
        }

        // ================= DELETE (SOFT) =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _context.DetailDokumenReceiveds
                .FirstOrDefaultAsync(x =>
                    x.DetailDokReceivedId == id &&
                    x.IsDelete == false);

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
            var query = from d in _context.DetailDokumenReceiveds
                        join u in _context.UserActives
                        on d.CreateBy equals u.UserActiveId
                        where d.IsDelete == false
                        select new
                        {
                            d.DetailDokReceivedId,
                            d.NoBilling,
                            d.PasienId,
                            d.TotalPiutang,
                            d.IsTerbayar,
                            d.TglJaatuhTempo,
                            d.CreateDateTime,
                            CreateByName = u.FullName
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NoBilling, search));
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
