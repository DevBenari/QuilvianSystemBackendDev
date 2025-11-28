using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Hemodialisa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class HemodialisaHasilController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<HemodialisaHasilController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HemodialisaHasilController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<HemodialisaHasilController> logger,
            IWebHostEnvironment webHostEnvironment,
            ITTDService ttdService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _ttdService = ttdService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // ================================
                // 1. Ambil PARENT
                // ================================
                var parent = await (
                    from h in _applicationDbContext.HemodialisaHasils
                    join u in _applicationDbContext.UserActives
                        on h.CreateBy equals u.UserActiveId into uGroup
                    from u in uGroup.DefaultIfEmpty()
                    where h.HasilHemodialisaId == id && (h.IsDelete == null || h.IsDelete == false)
                    select new
                    {
                        h.HasilHemodialisaId,
                        h.KunjunganId,
                        h.PasienId,
                        h.AsuransiId,
                        h.NamaAsuransi,
                        h.NoMesin,
                        h.HemodialisaKe,
                        h.TipeDializer,
                        h.JamMulai,
                        h.JamAkhir,

                        h.AksesVaskuler,
                        h.JenisHemodialisa,
                        h.Dialisat,

                        h.SirkulasiHeparin,
                        h.HeparinAwal,
                        h.HeparinMaintenance,
                        h.HeparinContinue,
                        h.HeparinIntermitten,

                        h.PenyulitHD,

                        h.AksesVaskulerId,
                        h.TTDAksesVaskuler,

                        h.DPPJAId,
                        h.TTDPPJA,

                        h.VerifikatorId,
                        h.ScoreTotalGizi,
                        h.StatusGizi,
                        h.Keterangan,

                        h.UF,
                        h.LaporanNaCl,

                        h.CreateDateTime,
                        CreateByName = u.FullName
                    }
                ).FirstOrDefaultAsync();

                if (parent == null)
                    return NotFound(new { message = "Data Hemodialisa tidak ditemukan." });

                // ================================
                // 2. Ambil DETAIL: MonitoringHD (NO N+1)
                // ================================
                var details = await (
                    from m in _applicationDbContext.MonitoringHDs
                    where m.HasilHemodialisaId == id && (m.IsDelete == null || m.IsDelete == false)
                    orderby m.JamMonitoring
                    select new
                    {
                        m.MonitoringHDId,
                        m.HasilHemodialisaId,
                        m.NoDx,
                        m.JamMonitoring,
                        m.Tensi,
                        m.Nadi,
                        m.TD,
                        m.VP,
                        m.AP,
                        m.QB,
                        m.QD,
                        m.TMP,
                        m.DP,
                        m.UF,
                        m.Keluhan,
                        m.Terapi,
                        m.Keterangan,
                        m.CreateDateTime
                    }
                ).ToListAsync();

                // ================================
                // 3. RESPONSE
                // ================================
                return Ok(new
                {
                    status = "success",
                    message = "Data Hemodialisa ditemukan.",
                    data = new
                    {
                        Parent = parent,
                        Details = details
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HemodialisaHasilViewModel vm)
        {
            try
            {
                if (vm == null || !ModelState.IsValid)
                    return BadRequest(new { message = "Data tidak valid." });

                // ================================
                // Ambil User Login
                // ================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var userActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (userActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                // cek ttd
                var av = await _ttdService.CheckTTDAsync((Guid)vm.AksesVaskulerId);
                var ppja = await _ttdService.CheckTTDAsync((Guid)vm.DPPJAId);

                // ================================
                // Insert PARENT
                // ================================
                var parent = new HemodialisaHasil
                {
                    HasilHemodialisaId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    AsuransiId = vm.AsuransiId,
                    NamaAsuransi = vm.NamaAsuransi,
                    NoMesin = vm.NoMesin,
                    HemodialisaKe = vm.HemodialisaKe,
                    TipeDializer = vm.TipeDializer,

                    JamMulai = vm.JamMulai,
                    JamAkhir = vm.JamAkhir,

                    AksesVaskuler = vm.AksesVaskuler,
                    JenisHemodialisa = vm.JenisHemodialisa,
                    Dialisat = vm.Dialisat,

                    SirkulasiHeparin = vm.SirkulasiHeparin,
                    HeparinAwal = vm.HeparinAwal,
                    HeparinMaintenance = vm.HeparinMaintenance,
                    HeparinContinue = vm.HeparinContinue,
                    HeparinIntermitten = vm.HeparinIntermitten,
                    PenyulitHD = vm.PenyulitHD,

                    AksesVaskulerId = vm.AksesVaskulerId,
                    TTDAksesVaskuler = av.Path,

                    DPPJAId = vm.DPPJAId,
                    TTDPPJA = ppja.Path,

                    VerifikatorId = vm.VerifikatorId,
                    ScoreTotalGizi = vm.ScoreTotalGizi,
                    StatusGizi = vm.StatusGizi,
                    Keterangan = vm.Keterangan,

                    // Dictionary
                    UF = vm.UF ?? new Dictionary<string, decimal>(),
                    LaporanNaCl = vm.LaporanNaCl ?? new(),

                    CreateBy = userActive.UserActiveId,
                    CreateDateTime = DateTime.UtcNow
                };

                await _applicationDbContext.HemodialisaHasils.AddAsync(parent);
                await _applicationDbContext.SaveChangesAsync();

                // ================================
                // Insert DETAIL: MonitoringHD
                // ================================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        var detail = new MonitoringHD
                        {
                            MonitoringHDId = Guid.NewGuid(),
                            HasilHemodialisaId = parent.HasilHemodialisaId,

                            NoDx = d.NoDx,
                            JamMonitoring = d.JamMonitoring,
                            Tensi = d.Tensi,
                            Nadi = d.Nadi,
                            TD = d.TD,
                            VP = d.VP,
                            AP = d.AP,
                            QB = d.QB,
                            QD = d.QD,
                            TMP = d.TMP,
                            DP = d.DP,
                            UF = d.UF,
                            Keluhan = d.Keluhan,
                            Terapi = d.Terapi,
                            Keterangan = d.Keterangan,

                            CreateBy = userActive.UserActiveId,
                            CreateDateTime = DateTime.UtcNow
                        };

                        await _applicationDbContext.MonitoringHDs.AddAsync(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }

                return Ok(new
                {
                    status = "success",
                    message = "Data Hemodialisa + Monitoring berhasil disimpan",
                    id = parent.HasilHemodialisaId,
                    ttdAksesVaskulerID = av.TTDId,
                    ttdPPJA = ppja.TTDId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("Add-UF-Hemodialisa/{id}")]
        public async Task<IActionResult> AppendUF(Guid id, [FromBody] Dictionary<string, decimal> newUF)
        {
            if (newUF == null || !newUF.Any())
                return BadRequest(new { message = "UF baru tidak boleh kosong." });

            var parent = await _applicationDbContext.HemodialisaHasils
                .FirstOrDefaultAsync(x => x.HasilHemodialisaId == id && x.IsDelete != true);

            if (parent == null)
                return NotFound(new { message = "Data Hemodialisa tidak ditemukan." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x => x.Email == emailLogin);

            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            if (parent.UF == null)
                parent.UF = new Dictionary<string, decimal>();

            foreach (var item in newUF)
            {
                if (parent.UF.ContainsKey(item.Key))
                    parent.UF[item.Key] = item.Value;
                else
                    parent.UF.Add(item.Key, item.Value);
            }

            parent.UpdateBy = user.UserActiveId;
            parent.UpdateDateTime = DateTime.UtcNow;

            // ⬇️ KUNCI: paksa EF menganggap properti UF berubah
            _applicationDbContext.Entry(parent).Property(p => p.UF).IsModified = true;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                message = "Data UF berhasil ditambahkan / diupdate",
                updatedUF = parent.UF
            });
        }

        [HttpPut("Add-LaporanNaCL-Hemodialisa/{id}")]
        public async Task<IActionResult> AppendLaporanNaCL(Guid id, [FromBody] Dictionary<string, LaporanNaCLEntry>? req )
        {
            if (req == null || !req.Any())
                return BadRequest(new { message = "Laporan NaCL baru tidak boleh kosong." });

            var parent = await _applicationDbContext.HemodialisaHasils
                .FirstOrDefaultAsync(x => x.HasilHemodialisaId == id && x.IsDelete != true);

            if (parent == null)
                return NotFound(new { message = "Data Hemodialisa tidak ditemukan." });

            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationDbContext.UserActives
                .FirstOrDefaultAsync(x => x.Email == emailLogin);

            if (user == null)
                return Unauthorized(new { message = "User tidak ditemukan." });

            if (parent.LaporanNaCl == null)
                parent.LaporanNaCl = new Dictionary<string, LaporanNaCLEntry>();

            foreach (var item in req)
            {
                if (parent.LaporanNaCl.ContainsKey(item.Key))
                    parent.LaporanNaCl[item.Key] = item.Value;
                else
                    parent.LaporanNaCl.Add(item.Key, item.Value);
            }

            parent.UpdateBy = user.UserActiveId;
            parent.UpdateDateTime = DateTime.UtcNow;

            // ⬇️ KUNCI: paksa EF menganggap properti UF berubah
            _applicationDbContext.Entry(parent).Property(p => p.LaporanNaCl).IsModified = true;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                message = "Data LaporanNaCL berhasil ditambahkan / diupdate",
                updatedUF = parent.LaporanNaCl
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HemodialisaHasilViewModel vm)
        {
            try
            {
                if (vm == null || !ModelState.IsValid)
                    return BadRequest(new { message = "Data tidak valid." });

                var parent = await _applicationDbContext.HemodialisaHasils
                    .FirstOrDefaultAsync(x => x.HasilHemodialisaId == id && x.IsDelete != true);

                if (parent == null)
                    return NotFound(new { message = "Data tidak ditemukan." });

                // ==== Ambil User Login ====
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (userActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                // cek ttd
                var av = await _ttdService.CheckTTDAsync((Guid)vm.AksesVaskulerId);
                var ppja = await _ttdService.CheckTTDAsync((Guid)vm.DPPJAId);

                // ==== Update Field Parent ====
                parent.KunjunganId = vm.KunjunganId;
                parent.PasienId = vm.PasienId;
                parent.AsuransiId = vm.AsuransiId;
                parent.NamaAsuransi = vm.NamaAsuransi;
                parent.NoMesin = vm.NoMesin;
                parent.HemodialisaKe = vm.HemodialisaKe;
                parent.TipeDializer = vm.TipeDializer;

                parent.JamMulai = vm.JamMulai;
                parent.JamAkhir = vm.JamAkhir;

                parent.AksesVaskuler = vm.AksesVaskuler;
                parent.JenisHemodialisa = vm.JenisHemodialisa;
                parent.Dialisat = vm.Dialisat;

                parent.SirkulasiHeparin = vm.SirkulasiHeparin;
                parent.HeparinAwal = vm.HeparinAwal;
                parent.HeparinMaintenance = vm.HeparinMaintenance;
                parent.HeparinContinue = vm.HeparinContinue;
                parent.HeparinIntermitten = vm.HeparinIntermitten;

                parent.PenyulitHD = vm.PenyulitHD;

                parent.AksesVaskulerId = vm.AksesVaskulerId;
                parent.TTDAksesVaskuler = av.Path;

                parent.DPPJAId = vm.DPPJAId;
                parent.TTDPPJA = ppja.Path;

                parent.VerifikatorId = vm.VerifikatorId;

                parent.ScoreTotalGizi = vm.ScoreTotalGizi;
                parent.StatusGizi = vm.StatusGizi;
                parent.Keterangan = vm.Keterangan;

                // Dictionary update
                parent.UF = vm.UF ?? parent.UF;
                parent.LaporanNaCl = vm.LaporanNaCl ?? parent.LaporanNaCl;

                parent.UpdateBy = userActive.UserActiveId;
                parent.UpdateDateTime = DateTime.UtcNow;

                // Simpan perubahan parent
                await _applicationDbContext.SaveChangesAsync();

                // ======================
                // Handle DETAIL Monitoring
                // ======================
                if (vm.Details != null)
                {
                    // Hapus detail lama
                    var oldDetails = _applicationDbContext.MonitoringHDs
                        .Where(x => x.HasilHemodialisaId == id);

                    _applicationDbContext.MonitoringHDs.RemoveRange(oldDetails);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert ulang detail baru
                    foreach (var d in vm.Details)
                    {
                        var detail = new MonitoringHD
                        {
                            MonitoringHDId = Guid.NewGuid(),
                            HasilHemodialisaId = id,

                            NoDx = d.NoDx,
                            JamMonitoring = d.JamMonitoring,
                            Tensi = d.Tensi,
                            Nadi = d.Nadi,
                            TD = d.TD,
                            VP = d.VP,
                            AP = d.AP,
                            QB = d.QB,
                            QD = d.QD,
                            TMP = d.TMP,
                            DP = d.DP,
                            UF = d.UF,
                            Keluhan = d.Keluhan,
                            Terapi = d.Terapi,
                            Keterangan = d.Keterangan,

                            CreateBy = userActive.UserActiveId,
                            CreateDateTime = DateTime.UtcNow
                        };

                        await _applicationDbContext.MonitoringHDs.AddAsync(detail);
                    }
                    await _applicationDbContext.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "Update data berhasil",
                    id = id,
                    ttdAksesVaskulerID = av.TTDId,
                    ttdPPJA = ppja.TTDId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                var parent = await _applicationDbContext.HemodialisaHasils
                    .FirstOrDefaultAsync(x => x.HasilHemodialisaId == id && (x.IsDelete == null || x.IsDelete == false));

                if (parent == null)
                    return NotFound(new { message = "Data Hemodialisa tidak ditemukan." });

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User tidak ditemukan" });

                // Tandai parent
                parent.IsDelete = true;
                parent.UpdateBy = user.UserActiveId;
                parent.UpdateDateTime = DateTime.UtcNow;

                // Ambil child MonitoringHD
                var children = await _applicationDbContext.MonitoringHDs
                    .Where(x => x.HasilHemodialisaId == id && (x.IsDelete == null || x.IsDelete == false))
                    .ToListAsync();

                // Tandai anak
                foreach (var child in children)
                {
                    child.IsDelete = true;
                    child.UpdateBy = user.UserActiveId;
                    child.UpdateDateTime = DateTime.UtcNow;
                }

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Soft delete berhasil. Parent & MonitoringHD ditandai sebagai terhapus." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        Guid? kunjunganId = null,
        string? search = null,
        string? orderBy = "CreateDateTime",
        DateTime? startDate = null,
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))]
        PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // ==========================================================
            // 1) BASE QUERY PARENT
            // ==========================================================
            var query = _applicationDbContext.HemodialisaHasils
                .Where(x => x.IsDelete != true)
                .AsQueryable();

            // ============================
            // FILTER BY KUNJUNGAN ID
            // ============================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            // ============================
            // SEARCH
            // ============================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.NamaAsuransi ?? "", search) ||
                    EF.Functions.ILike(x.Keterangan ?? "", search)
                );
            }

            // ============================
            // RANGE TANGGAL
            // ============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc
                );
            }

            // ============================
            // FILTER PERIODE
            // ============================
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        var startWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(x => x.CreateDateTime.Date >= startWeek);
                        break;

                    case PeriodeFilter.LastWeek:
                        var startLastWeek = today.AddDays(-7 - (int)today.DayOfWeek);
                        var endLastWeek = today.AddDays(-(int)today.DayOfWeek);
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= startLastWeek &&
                            x.CreateDateTime.Date < endLastWeek);
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        var lastMonth = today.AddMonths(-1);
                        query = query.Where(x =>
                            x.CreateDateTime.Month == lastMonth.Month &&
                            x.CreateDateTime.Year == lastMonth.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // ============================
            // TOTAL ROWS BEFORE PAGING
            // ============================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // ============================
            // APPLY PAGING
            // ============================
            var parentPage = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!parentPage.Any())
                return Ok(new { status = "success", data = new { Rows = new List<object>(), totalRows, page, perPage, totalPages } });

            var parentIds = parentPage.Select(x => x.HasilHemodialisaId).ToList();

            // ==========================================================
            // 2) LOAD MonitoringHD BY PARENT IDS (NO N+1)
            // ==========================================================
            var monitoring = await _applicationDbContext.MonitoringHDs
                .Where(m => parentIds.Contains(m.HasilHemodialisaId ?? Guid.Empty))
                .OrderBy(m => m.CreateDateTime)
                .ToListAsync();

            // MERGE
            var merged = parentPage.Select(p => new
            {
                Parent = p,
                Monitoring = monitoring.Where(m => m.HasilHemodialisaId == p.HasilHemodialisaId).ToList()
            });

            return Ok(new
            {
                status = "success",
                data = merged,
                totalRows,
                currentPage = page,
                perPage,
                totalPages
            });
        }





    }
}
