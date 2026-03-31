using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.ViewModels;
using Swashbuckle.AspNetCore.Annotations;
using NewtonsoftJson = Newtonsoft.Json;
using SystemTextJson = System.Text.Json.Serialization;

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
                LoketKasirId = vm.LoketKasirId,
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
            data.LoketKasirId = vm.LoketKasirId;
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

        public enum PergantianShiftOrderBy
        {
            KodeRegistrasi,
            KasirId,
            LayananId,
            WaktuMulai,
            WaktuAkhir,
            TanggalPergantian
        }

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            Guid? kasirId = null,
            Guid? layananId = null,
            string? waktuMulai = null,      // filter jam mulai
            string? waktuAkhir = null,      // filter jam akhir
            string? orderBy = "TanggalPergantian",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = _db.PergantianShifts.AsQueryable();

            // =========================
            // Filter Kasir & Layanan
            // =========================
            if (kasirId.HasValue)
                query = query.Where(x => x.KasirId == kasirId.Value);

            if (layananId.HasValue)
                query = query.Where(x => x.LayananId == layananId.Value);

            // =========================
            // Filter WaktuMulai
            // =========================
            if (!string.IsNullOrWhiteSpace(waktuMulai))
            {
                var jamMulaiTerms = waktuMulai.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(x =>
                    jamMulaiTerms.Any(term => x.WaktuMulai.ToString("HH:mm").Contains(term))
                );
            }

            // =========================
            // Filter WaktuAkhir
            // =========================
            if (!string.IsNullOrWhiteSpace(waktuAkhir))
            {
                var jamAkhirTerms = waktuAkhir.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(x =>
                    jamAkhirTerms.Any(term => x.WaktuAkhir.ToString("HH:mm").Contains(term))
                );
            }

            // =========================
            // Filter tanggal
            // =========================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.TanggalPergantian >= start && x.TanggalPergantian <= end);
            }

            // =========================
            // Filter periode
            // =========================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.TanggalPergantian.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(x => x.TanggalPergantian.Date >= startWeek && x.TanggalPergantian.Date <= today);
                        break;
                    case PeriodeFilter.LastWeek:
                        var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
                        var lastWeekEnd = lastWeekStart.AddDays(6);
                        query = query.Where(x => x.TanggalPergantian.Date >= lastWeekStart && x.TanggalPergantian.Date <= lastWeekEnd);
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x => x.TanggalPergantian.Month == today.Month && x.TanggalPergantian.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(x => x.TanggalPergantian.Month == lastMonth.Month && x.TanggalPergantian.Year == lastMonth.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.TanggalPergantian.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.TanggalPergantian.Year == today.Year - 1);
                        break;
                }
            }

            // =========================
            // Sorting
            // =========================
            bool desc = sortDirection?.ToLower() == "desc";
            query = orderBy?.ToLower() switch
            {
                "layananid" => desc ? query.OrderByDescending(x => x.LayananId) : query.OrderBy(x => x.LayananId),
                "waktumulai" => desc ? query.OrderByDescending(x => x.WaktuMulai) : query.OrderBy(x => x.WaktuMulai),
                "waktuakhir" => desc ? query.OrderByDescending(x => x.WaktuAkhir) : query.OrderBy(x => x.WaktuAkhir),
                "kasirid" => desc ? query.OrderByDescending(x => x.KasirId) : query.OrderBy(x => x.KasirId),
                "tanggalpergantian" => desc ? query.OrderByDescending(x => x.TanggalPergantian) : query.OrderBy(x => x.TanggalPergantian),
                _ => desc ? query.OrderByDescending(x => x.TanggalPergantian) : query.OrderBy(x => x.TanggalPergantian)
            };

            // =========================
            // Pagination
            // =========================
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
                        x.PergantianShiftId,
                        x.KodeRegistrasi,
                        x.LayananId,
                        x.KasirId,
                        x.LoketKasirId,
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