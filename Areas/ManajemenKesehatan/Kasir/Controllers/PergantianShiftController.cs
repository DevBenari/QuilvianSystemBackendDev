using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.ViewModels;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PergantianShiftController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PergantianShiftController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _db.PergantianShifts
                .OrderByDescending(x => x.TanggalPergantian)
                .Select(x => new {
                    x.PergantianShiftId,
                    x.KodeRegistrasi,
                    x.LayananId,
                    x.KasirId,
                    x.ShiftPergantian,
                    x.WaktuMulai,
                    x.WaktuAkhir,
                    x.TanggalPergantian,
                    x.SaldoAwal,
                    x.PendapatanTunai,
                    x.KasFisik,
                    x.SelisihPendapatanTunai,
                    x.TotalPendapatan,
                    x.PendapatanNonTunai,
                    x.Keterangan
                });

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            return Ok(new { message = "Berhasil", data = rows, pagination = new { CurrentPage = page, PerPage = perPage, TotalRows = totalRows, TotalPages = totalPages } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _db.PergantianShifts.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });
            return Ok(new { message = "Ditemukan", data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PergantianShiftViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Data tidak valid" });

            var data = new PergantianShift
            {
                PergantianShiftId = Guid.NewGuid(),
                KodeRegistrasi = vm.KodeRegistrasi,
                LayananId = vm.LayananId,
                KasirId = vm.KasirId,
                ShiftPergantian = vm.ShiftPergantian,
                WaktuMulai = vm.WaktuMulai,
                WaktuAkhir = vm.WaktuAkhir,
                TanggalPergantian = vm.TanggalPergantian,
                SaldoAwal = vm.SaldoAwal,
                PendapatanTunai = vm.PendapatanTunai,
                KasFisik = vm.KasFisik,
                SelisihPendapatanTunai = vm.SelisihPendapatanTunai,
                TotalPendapatan = vm.TotalPendapatan,
                PendapatanNonTunai = vm.PendapatanNonTunai,
                Keterangan = vm.Keterangan
            };

            _db.PergantianShifts.Add(data);
            await _db.SaveChangesAsync();
            return Created("", new { message = "Tambah Data Berhasil" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PergantianShiftViewModel vm)
        {
            var data = await _db.PergantianShifts.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            data.KodeRegistrasi = vm.KodeRegistrasi;
            data.LayananId = vm.LayananId;
            data.KasirId = vm.KasirId;
            data.ShiftPergantian = vm.ShiftPergantian;
            data.WaktuMulai = vm.WaktuMulai;
            data.WaktuAkhir = vm.WaktuAkhir;
            data.TanggalPergantian = vm.TanggalPergantian;
            data.SaldoAwal = vm.SaldoAwal;
            data.PendapatanTunai = vm.PendapatanTunai;
            data.KasFisik = vm.KasFisik;
            data.SelisihPendapatanTunai = vm.SelisihPendapatanTunai;
            data.TotalPendapatan = vm.TotalPendapatan;
            data.PendapatanNonTunai = vm.PendapatanNonTunai;
            data.Keterangan = vm.Keterangan;

            _db.PergantianShifts.Update(data);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Update Data Berhasil" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _db.PergantianShifts.FindAsync(id);
            if (data == null) return NotFound(new { message = "Data tidak ditemukan" });

            _db.PergantianShifts.Remove(data);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Data berhasil dihapus" });
        }
    }
}