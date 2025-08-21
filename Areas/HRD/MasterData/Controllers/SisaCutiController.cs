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
        // ✅ Index tampilkan semua sisa cuti user per jenis cuti
        public async Task<IActionResult> Index()
        {
            var data = await (from pengajuan in _context.PengajuanCutis
                              join jenis in _context.JenisCutis
                                  on pengajuan.JenisCutiId equals jenis.JenisCutiId
                              group new { pengajuan, jenis } by new { pengajuan.UserActiveId, pengajuan.JenisCutiId, jenis.NamaCuti, jenis.KuotaTahunan } into g
                              select new
                              {
                                  g.Key.UserActiveId,
                                  g.Key.JenisCutiId,
                                  NamaCuti = g.Key.NamaCuti,
                                  KuotaTahunan = string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan),
                                  TotalCutiDiambil = g.Sum(x => x.pengajuan.JumlahCutiDiambil),
                                  SisaCuti = (string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan))
                                             - g.Sum(x => x.pengajuan.JumlahCutiDiambil)
                              })
                              .ToListAsync();

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
                                  TotalCutiDiambil = g.Sum(x => x.pengajuan.JumlahCutiDiambil),
                                  SisaCuti = (string.IsNullOrEmpty(g.Key.KuotaTahunan) ? 0 : int.Parse(g.Key.KuotaTahunan))
                                             - g.Sum(x => x.pengajuan.JumlahCutiDiambil)
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
