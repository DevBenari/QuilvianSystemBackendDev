using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class SisaCutiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SisaCutiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet()]
        // ✅ Ambil semua user + sisa cuti per jenis cuti
        public async Task<IActionResult> Index()
        {
            var data = await (from pengajuan in _context.PengajuanCutis
                              join jenis in _context.JenisCutis
                                  on pengajuan.JenisCutiId equals jenis.JenisCutiId
                              group new { pengajuan, jenis } by new { pengajuan.UserActiveId, pengajuan.JenisCutiId, jenis.NamaCuti, jenis.KuotaTahunan } into g
                              select new
                              {
                                  UserActiveId = g.Key.UserActiveId,
                                  JenisCutiId = g.Key.JenisCutiId,
                                  NamaCuti = g.Key.NamaCuti,
                                  KuotaTahunan = string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan),

                                  // ✅ hitung status
                                  TotalCutiDisetujui = g.Where(x => x.pengajuan.Status == "Disetujui")
                                                        .Sum(x => x.pengajuan.JumlahCutiDiambil),

                                  TotalCutiPending = g.Where(x => string.IsNullOrEmpty(x.pengajuan.Status)
                                                               || x.pengajuan.Status == "Kosong")
                                                      .Sum(x => x.pengajuan.JumlahCutiDiambil),

                                  TotalCutiDitolak = g.Where(x => x.pengajuan.Status == "Ditolak")
                                                      .Sum(x => x.pengajuan.JumlahCutiDiambil),

                                  // ✅ Sisa cuti = kuota - cuti disetujui
                                  SisaCuti = (string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan))
                                             - g.Where(x => x.pengajuan.Status == "Disetujui")
                                                .Sum(x => x.pengajuan.JumlahCutiDiambil)
                              })
                              .ToListAsync();

            if (data == null || !data.Any())
            {
                return NotFound(new { Message = "Belum ada pengajuan cuti" });
            }

            return Ok(data);
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSisaCuti(Guid userId)
        {
            var data = await (from pengajuan in _context.PengajuanCutis
                              join jenis in _context.JenisCutis
                                  on pengajuan.JenisCutiId equals jenis.JenisCutiId
                              where pengajuan.UserActiveId == userId
                              group new { pengajuan, jenis } by new { pengajuan.UserActiveId, pengajuan.JenisCutiId, jenis.NamaCuti, jenis.KuotaTahunan } into g
                              select new
                              {
                                  UserActiveId = g.Key.UserActiveId,
                                  JenisCutiId = g.Key.JenisCutiId,
                                  NamaCuti = g.Key.NamaCuti,
                                  KuotaTahunan = string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan),

                                  // Hanya hitung cuti dengan status "Disetujui"
                                  TotalCutiDisetujui = g.Where(x => x.pengajuan.Status == "Disetujui")
                                                        .Sum(x => x.pengajuan.JumlahCutiDiambil),

                                  // Hitung juga pending & ditolak
                                  TotalCutiPending = g.Where(x => string.IsNullOrEmpty(x.pengajuan.Status) || x.pengajuan.Status == "Kosong")
                                                      .Sum(x => x.pengajuan.JumlahCutiDiambil),

                                  TotalCutiDitolak = g.Where(x => x.pengajuan.Status == "Ditolak")
                                                      .Sum(x => x.pengajuan.JumlahCutiDiambil),

                                  // Sisa cuti hanya dikurangi cuti yang disetujui
                                  SisaCuti = (string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan))
                                             - g.Where(x => x.pengajuan.Status == "Disetujui")
                                                .Sum(x => x.pengajuan.JumlahCutiDiambil)
                              })
                              .ToListAsync();

            if (data == null || !data.Any())
            {
                return NotFound(new { Message = "Belum ada pengajuan cuti untuk user ini" });
            }

            return Ok(data);
        }

    }
}
