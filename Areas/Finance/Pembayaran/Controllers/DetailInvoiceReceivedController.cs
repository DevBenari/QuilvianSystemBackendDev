using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Finance.Pembayaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DetailInvoiceReceivedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DetailInvoiceReceivedController> _logger;
        private readonly IWebHostEnvironment _env;

        public DetailInvoiceReceivedController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DetailInvoiceReceivedController> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _env = env;
        }

        // ================= GET ALL (PAGING SIMPLE) =================
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from d in _context.DetailInvoiceReceiveds
                        join u in _context.UserActives
                        on d.CreateBy equals u.UserActiveId
                        where d.IsDelete == false
                        select new
                        {
                            d.DetailInvoicePaymentId,
                            d.DetailReceivedPaymentId,
                            d.KunjunganId,
                            d.PasiemId,
                            d.NoRM,
                            d.NamaPasien,
                            d.NoBilling,
                            d.TglTerima,
                            d.TglKirim,
                            d.TglTagihan,
                            d.PiutangTerbayar,
                            d.PembayaranKe,
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
        [HttpGet("DetailReceivedPayment/{id}")]
        public async Task<IActionResult> detailReceivedPaymentId(Guid id)
        {
            var data = await _context.DetailInvoiceReceiveds
                .Where(x => x.DetailReceivedPaymentId == id && x.IsDelete == false)
                .ToListAsync();

            if (data == null || !data.Any())
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new { message = "success", data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.DetailInvoiceReceiveds
                .FirstOrDefaultAsync(x => x.DetailInvoicePaymentId == id && x.IsDelete == false);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(new { message = "success", data });
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DetailInvoiceReceived model)
        {
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return Unauthorized();

            model.DetailInvoicePaymentId = Guid.NewGuid();
            model.CreateBy = user.UserActiveId;
            model.CreateDateTime = DateTime.UtcNow;
            model.IsDelete = false;

            _context.DetailInvoiceReceiveds.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "created",
                data = model
            });
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DetailInvoiceReceived model)
        {
            var data = await _context.DetailInvoiceReceiveds
                .FirstOrDefaultAsync(x => x.DetailInvoicePaymentId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            data.DetailReceivedPaymentId = model.DetailReceivedPaymentId;
            data.KunjunganId = model.KunjunganId;
            data.PasiemId = model.PasiemId;
            data.NoRM = model.NoRM;
            data.NamaPasien = model.NamaPasien;
            data.NoBilling = model.NoBilling;
            data.TglTerima = model.TglTerima;
            data.TglKirim = model.TglKirim;
            data.TglTagihan = model.TglTagihan;
            data.TotalPiutang = model.TotalPiutang;
            data.PiutangTerbayar = model.PiutangTerbayar;
            data.PembayaranKe = model.PembayaranKe;
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
            var data = await _context.DetailInvoiceReceiveds
                .FirstOrDefaultAsync(x => x.DetailInvoicePaymentId == id && x.IsDelete == false);

            if (data == null)
                return NotFound();

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.UserActives
                .FirstOrDefaultAsync(x => x.Email == email);

            data.IsDelete = true;
            data.DeleteBy = user?.UserActiveId ?? Guid.Empty;
            data.DeleteDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "deleted" });
        }

        // ================= PAGED (SEARCH + FILTER) =================
        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null)
        {
            var query = from d in _context.DetailInvoiceReceiveds
                        join u in _context.UserActives
                        on d.CreateBy equals u.UserActiveId
                        where d.IsDelete == false
                        select new
                        {
                            d.DetailInvoicePaymentId,
                            d.DetailReceivedPaymentId,
                            d.KunjunganId,
                            d.PasiemId,
                            d.NoRM,
                            d.NamaPasien,
                            d.NoBilling,
                            d.TglTerima,
                            d.TglKirim,
                            d.TglTagihan,
                            d.PiutangTerbayar,
                            d.PembayaranKe,
                            d.TotalPiutang,
                            d.TglJaatuhTempo,
                            d.IsTerbayar,
                            d.Keterangan,
                            d.CreateDateTime,
                            CreateByName = u.FullName
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NoRM, search) ||
                    EF.Functions.ILike(x.NamaPasien, search) ||
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
