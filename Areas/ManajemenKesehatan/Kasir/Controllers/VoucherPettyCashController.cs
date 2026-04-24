using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class VoucherPettyCashController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VoucherPettyCashController(ApplicationDbContext db) => _db = db;


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var listdata = await _db.VoucherPettyCashes
                .Where(x => x.IsDelete == false || x.IsDelete == null)
                .ToListAsync();

            return Ok(new { message = "Data berhasil diambil", data = listdata });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherPettyCashViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            var emailLogin = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var getUserActive = await _db.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
            if (getUserActive == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var data = new VoucherPettyCash
            {
                VoucherPettyCashId = Guid.NewGuid(),
                KodeVoucherPC = vm.KodeVoucherPC,
                LayananId = vm.LayananId,
                KasirId = vm.KasirId,
                ShiftSesi = vm.ShiftSesi,
                NamaPenerima = vm.NamaPenerima,
                TanggalPengajuan = vm.TanggalPengajuan,
                KategoriVoucher = vm.KategoriVoucher,
                NominalVoucher = vm.NominalVoucher,
                BuktiNota = vm.BuktiNota,
                StatusVoucher = vm.StatusVoucher,
                Keterangan = vm.Keterangan,
                CreateBy = getUserActive.UserActiveId,
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _db.VoucherPettyCashes.Add(data);
            await _db.SaveChangesAsync();

            return Created("", new { message = "Tambah Data Voucher Petty Cash Berhasil || 201 Created" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VoucherPettyCashViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Data tidak valid." });

            var data = await _db.VoucherPettyCashes.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan." });

            // Update field
            data.KodeVoucherPC = vm.KodeVoucherPC;
            data.LayananId = vm.LayananId;
            data.KasirId = vm.KasirId;
            data.ShiftSesi = vm.ShiftSesi;
            data.NamaPenerima = vm.NamaPenerima;
            data.TanggalPengajuan = vm.TanggalPengajuan;
            data.KategoriVoucher = vm.KategoriVoucher;
            data.NominalVoucher = vm.NominalVoucher;
            data.BuktiNota = vm.BuktiNota;
            data.StatusVoucher = vm.StatusVoucher;
            data.Keterangan = vm.Keterangan;
            data.UpdateDateTime = DateTimeOffset.UtcNow;

            _db.VoucherPettyCashes.Update(data);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Update Data Voucher Petty Cash Berhasil" });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            Guid? kasirId = null,
            Guid? layananId = null,
            string? orderBy = "TanggalPengajuan",
            string? sortDirection = "desc")
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _db.VoucherPettyCashes.AsQueryable();

            // Filter Kasir & Layanan
            if (kasirId.HasValue)
                query = query.Where(x => x.KasirId == kasirId.Value);

            if (layananId.HasValue)
                query = query.Where(x => x.LayananId == layananId.Value);

            // Sorting
            bool desc = sortDirection?.ToLower() == "desc";
            query = orderBy?.ToLower() switch
            {
                "kodevoucherpc" => desc ? query.OrderByDescending(x => x.KodeVoucherPC) : query.OrderBy(x => x.KodeVoucherPC),
                "tanggalpengajuan" => desc ? query.OrderByDescending(x => x.TanggalPengajuan) : query.OrderBy(x => x.TanggalPengajuan),
                "nominalvoucher" => desc ? query.OrderByDescending(x => x.NominalVoucher) : query.OrderBy(x => x.NominalVoucher),
                _ => desc ? query.OrderByDescending(x => x.TanggalPengajuan) : query.OrderBy(x => x.TanggalPengajuan)
            };

            // Pagination
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows.Select(x => new
                    {
                        x.VoucherPettyCashId,
                        x.KodeVoucherPC,
                        x.LayananId,
                        x.KasirId,
                        x.ShiftSesi,
                        x.NamaPenerima,
                        x.TanggalPengajuan,
                        x.KategoriVoucher,
                        x.NominalVoucher,
                        x.BuktiNota,
                        x.StatusVoucher,
                        x.Keterangan,
                        x.CreateBy,
                        x.CreateDateTime,
                        x.UpdateDateTime
                    }),
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}