using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TarifRadiologiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TarifRadiologiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TarifRadiologiController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TarifRadiologiController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = (from a in _applicationDbContext.TarifRadiologis.AsNoTracking()
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                            on a.CreateBy equals u.UserActiveId

                            join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                            on a.LabPemeriksaanId equals lp.PemeriksaanLabId into lpG
                            from lp in lpG.DefaultIfEmpty()

                            join k in _applicationDbContext.Kelass.AsNoTracking()
                            on a.KelasId equals k.KelasId into kG
                            from k in kG.DefaultIfEmpty()



                            where a.IsDelete == false && a.TarifRadId == id
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.TarifRadId,
                                a.LabPemeriksaanId,
                                NamaPemeriksaan = lp.NamaPemeriksaan ?? null,
                                a.KelasId,
                                NamaKelas = k.NamaKelas ?? null,
                                a.TarifDokter,
                                a.TarifRs,
                                a.TarifJp,
                                a.TarifBahp,
                                a.TarifLain,
                                a.TarifTotal,
                                a.KSO,
                                a.Keterangan,
                            });
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

    }
}
