using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class RecallController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<RecallController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RecallController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RecallController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecallViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User tidak ditemukan!" });

                // --- INSERT MASTER RECALL ---
                var recallId = Guid.NewGuid();

                var recall = new Recall
                {
                    RecallId = recallId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    SikapPasienDiet = vm.SikapPasienDiet,
                    AnjuranDiet = vm.AnjuranDiet,
                    TglRecall = vm.TglRecall,
                    DietesienId = user.UserActiveId,
                    CatatanPerawat = vm.CatatanPerawat,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.Recalls.Add(recall);

                // --- INSERT DETAIL RECALL ---
                if (vm.Details != null)
                {
                    foreach (var d in vm.Details)
                    {
                        var detail = new RecallDetail
                        {
                            DetailRecallId = Guid.NewGuid(),
                            RecallId = recallId,
                            MakananSelingan = d.MakananSelingan,
                            WaktuMakanan = d.WaktuMakanan,
                            BanyakGR = d.BanyakGR,
                            BanyakUTR = d.BanyakUTR,
                            IsSelingan = d.IsSelingan,
                            KAL = d.KAL,
                            Protein = d.Protein,
                            Lemak = d.Lemak,
                            CHO = d.CHO,
                            CA = d.CA,
                            FE = d.FE,
                            VitA = d.VitA,
                            VitB1 = d.VitB1,
                            VitC = d.VitC,
                            IsRataRataHarian = d.IsRataRataHarian,
                            IsRDA = d.IsRDA,
                            Keterangan = d.Keterangan,
                            CreateBy = user.UserActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.RecallDetails.Add(detail);
                    }
                }

                await _applicationDbContext.SaveChangesAsync();

                return Created("", new { message = "Data Recall berhasil ditambahkan" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RecallViewModel vm)
        {
            var recall = await _applicationDbContext.Recalls
                .FirstOrDefaultAsync(x => x.RecallId == id);

            if (recall == null)
                return NotFound(new { message = "Recall tidak ditemukan" });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(x => x.Email == emailLogin);

            recall.KunjunganId = vm.KunjunganId;
            recall.PasienId = vm.PasienId;
            recall.SikapPasienDiet = vm.SikapPasienDiet;
            recall.AnjuranDiet = vm.AnjuranDiet;
            recall.TglRecall = vm.TglRecall;
            recall.CatatanPerawat = vm.CatatanPerawat;
            recall.UpdateBy = user.UserActiveId;
            recall.UpdateDateTime = DateTimeOffset.UtcNow;

            // Hapus detail lama
            var oldDetails = _applicationDbContext.RecallDetails.Where(x => x.RecallId == id);
            _applicationDbContext.RecallDetails.RemoveRange(oldDetails);

            // Insert detail baru
            if (vm.Details != null)
            {
                foreach (var d in vm.Details)
                {
                    _applicationDbContext.RecallDetails.Add(new RecallDetail
                    {
                        DetailRecallId = Guid.NewGuid(),
                        RecallId = id,
                        MakananSelingan = d.MakananSelingan,
                        WaktuMakanan = d.WaktuMakanan,
                        BanyakGR = d.BanyakGR,
                        BanyakUTR = d.BanyakUTR,
                        IsSelingan = d.IsSelingan,
                        KAL = d.KAL,
                        Protein = d.Protein,
                        Lemak = d.Lemak,
                        CHO = d.CHO,
                        CA = d.CA,
                        FE = d.FE,
                        VitA = d.VitA,
                        VitB1 = d.VitB1,
                        VitC = d.VitC,
                        IsRataRataHarian = d.IsRataRataHarian,
                        IsRDA = d.IsRDA,
                        Keterangan = d.Keterangan,
                        CreateBy = user.UserActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });
                }
            }

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Update Recall berhasil" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _applicationDbContext.Recalls
                .FirstOrDefaultAsync(x => x.RecallId == id);

            if (data == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            var details = _applicationDbContext.RecallDetails.Where(x => x.RecallId == id);

            _applicationDbContext.RecallDetails.RemoveRange(details);
            _applicationDbContext.Recalls.Remove(data);

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Recall berhasil dihapus" });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> PagedAsync(
            int page = 1,
            int perPage = 10,
            string? search = null,
            Guid? kunjunganId = null,
            Guid? pasienId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // --- BASE QUERY JOIN TANPA N+1 ---
            var baseQuery =
                from r in _applicationDbContext.Recalls
                join u in _applicationDbContext.UserActives on r.DietesienId equals u.UserActiveId into uJoin
                from u in uJoin.DefaultIfEmpty()

                join d in _applicationDbContext.RecallDetails
                    on r.RecallId equals d.RecallId into dGroup
                from d in dGroup.DefaultIfEmpty()

                where r.IsDelete == false || r.IsDelete == null
                select new
                {
                    // Parent Recall
                    r.RecallId,
                    r.KunjunganId,
                    r.PasienId,
                    r.SikapPasienDiet,
                    r.AnjuranDiet,
                    r.TglRecall,
                    r.DietesienId,
                    DietesienName = u.FullName,
                    r.CatatanPerawat,
                    r.CreateDateTime,

                    // Detail
                    DetailRecallId = d.DetailRecallId,
                    d.MakananSelingan,
                    d.WaktuMakanan,
                    d.BanyakGR,
                    d.BanyakUTR,
                    d.IsSelingan,
                    d.KAL,
                    d.Protein,
                    d.Lemak,
                    d.CHO,
                    d.CA,
                    d.FE,
                    d.VitA,
                    d.VitB1,
                    d.VitC,
                    d.IsRataRataHarian,
                    d.IsRDA,
                    DetailKeterangan = d.Keterangan
                };


            // --- FILTER SEARCH ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                baseQuery = baseQuery.Where(r =>
                    (r.SikapPasienDiet ?? "").ToLower().Contains(s) ||
                    (r.AnjuranDiet ?? "").ToLower().Contains(s)
                );
            }

            // --- FILTER KUNJUNGAN ---
            if (kunjunganId.HasValue)
            {
                baseQuery = baseQuery.Where(r => r.KunjunganId == kunjunganId.Value);
            }

            // --- FILTER PASIEN ---
            if (pasienId.HasValue)
            {
                baseQuery = baseQuery.Where(r => r.PasienId == pasienId.Value);
            }

            // --- FILTER TANGGAL ---
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date.ToUniversalTime();
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                baseQuery = baseQuery.Where(r => r.CreateDateTime >= start && r.CreateDateTime <= end);
            }

            // --- FILTER PERIODE (Hari Ini, Minggu Ini, dst) ---
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        baseQuery = baseQuery.Where(r => r.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        baseQuery = baseQuery.Where(r =>
                            r.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            r.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        baseQuery = baseQuery.Where(r =>
                            r.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            r.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        baseQuery = baseQuery.Where(r =>
                            r.CreateDateTime.Month == today.Month &&
                            r.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        baseQuery = baseQuery.Where(r =>
                            r.CreateDateTime.Month == today.Month - 1 &&
                            r.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        baseQuery = baseQuery.Where(r => r.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        baseQuery = baseQuery.Where(r => r.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        baseQuery = baseQuery.Where(r => r.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        baseQuery = baseQuery.Where(r => r.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // --- SORTING ---
            baseQuery = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => baseQuery.OrderByDescending(r => r.CreateDateTime),
                    "DietesienName" => baseQuery.OrderByDescending(r => r.DietesienName),
                    _ => baseQuery.OrderByDescending(r => r.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => baseQuery.OrderBy(r => r.CreateDateTime),
                    "DietesienName" => baseQuery.OrderBy(r => r.DietesienName),
                    _ => baseQuery.OrderBy(r => r.CreateDateTime)
                };

            // --- EXECUTE QUERY (ASYNC) ---
            var rawList = await baseQuery.ToListAsync();

            // --- GROUPING PER RECALL ---
            var grouped = rawList
                .GroupBy(r => new
                {
                    r.RecallId,
                    r.KunjunganId,
                    r.PasienId,
                    r.SikapPasienDiet,
                    r.AnjuranDiet,
                    r.TglRecall,
                    r.DietesienId,
                    r.DietesienName,
                    r.CatatanPerawat,
                    r.CreateDateTime
                })
                .Select(g => new
                {
                    g.Key.RecallId,
                    g.Key.KunjunganId,
                    g.Key.PasienId,
                    g.Key.SikapPasienDiet,
                    g.Key.AnjuranDiet,
                    g.Key.TglRecall,
                    g.Key.DietesienId,
                    g.Key.DietesienName,
                    g.Key.CatatanPerawat,
                    g.Key.CreateDateTime,

                    Details = g
                        .Where(x => x.DetailRecallId != Guid.Empty)
                        .Select(x => new
                        {
                            x.DetailRecallId,
                            x.MakananSelingan,
                            x.WaktuMakanan,
                            x.BanyakGR,
                            x.BanyakUTR,
                            x.IsSelingan,
                            x.KAL,
                            x.Protein,
                            x.Lemak,
                            x.CHO,
                            x.CA,
                            x.FE,
                            x.VitA,
                            x.VitB1,
                            x.VitC,
                            x.IsRataRataHarian,
                            x.IsRDA,
                            x.DetailKeterangan
                        }).ToList()
                })
                .ToList();

            // --- PAGINATION ---
            var totalRows = grouped.Count;
            var paged = grouped.Skip((page - 1) * perPage).Take(perPage).ToList();

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = paged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }

    }
}
