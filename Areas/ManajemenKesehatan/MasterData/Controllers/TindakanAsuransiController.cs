using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class TindakanAsuransiController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TindakanAsuransiController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TindakanAsuransiController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TindakanAsuransiController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        // Tambah getsdfd
        [HttpGet]
        public async Task<IActionResult> GetAlL(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.TindakanAsuransis
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.TindakanAsuransiId,
                            a.AsuransiId,
                            a.TindakanId,
                            // ============================
                            // MARKUP
                            // ============================
                            a.MarkupDokter,
                            a.MarkupRs,
                            a.MarkupJp,
                            a.MarkupBahp,
                            a.MarkupLainnya,
                            a.MarkupTotal,
                            a.IsMarkupBerlaku,
                            a.MarkupDari,
                            a.MarkupSampai,

                            // ============================
                            // DISKON
                            // ============================
                            a.DiskonDokter,
                            a.DiskonRs,
                            a.DiskonJp,
                            a.DiskonBahp,
                            a.DiskonTotal,
                            a.IsDiskonBerlaku,
                            a.DiskonDari,
                            a.DiskonSampai
                        };

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }
        // POST: api/tindakanasuransi
        [HttpPost]
        public async Task<IActionResult> CreateTindakanAsuransi([FromBody] TindakanAsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // **Cek Duplikasi**
                bool isDuplicate = _applicationDbContext.TindakanAsuransis
                    .Any(c => c.TindakanId == vm.TindakanId && c.AsuransiId == vm.AsuransiId && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Buat Data Baru**
                var data = new TindakanAsuransi
                {
                    TindakanAsuransiId = Guid.NewGuid(),
                    TindakanId = vm.TindakanId,
                    AsuransiId = vm.AsuransiId,
                    // ============================
                    // MARKUP
                    // ============================
                    MarkupDokter = vm.MarkupDokter,
                    MarkupRs = vm.MarkupRs,
                    MarkupJp = vm.MarkupJp,
                    MarkupBahp = vm.MarkupBahp,
                    MarkupLainnya = vm.MarkupLainnya,
                    MarkupTotal = vm.MarkupTotal,

                    IsMarkupBerlaku = vm.IsMarkupBerlaku,
                    MarkupDari = vm.MarkupDari,
                    MarkupSampai = vm.MarkupSampai,

                    // ============================
                    // DISKON
                    // ============================
                    DiskonDokter = vm.DiskonDokter,
                    DiskonRs = vm.DiskonRs,
                    DiskonJp = vm.DiskonJp,
                    DiskonBahp = vm.DiskonBahp,
                    DiskonTotal = vm.DiskonTotal,

                    IsDiskonBerlaku = vm.IsDiskonBerlaku,
                    DiskonDari = vm.DiskonDari,
                    DiskonSampai = vm.DiskonSampai,

                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userActiveId
                };

                // **Simpan ke Database**
                _applicationDbContext.TindakanAsuransis.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Relasi Berhasil || 201 Created" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // GET: api/tindakanasuransi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTindakanAsuransiById(Guid id)
        {
            var data = await _applicationDbContext.TindakanAsuransis
                .Where(t => t.TindakanId == id && !t.IsDelete)
                .ToListAsync();  // Mengambil semua data yang sesuai dalam bentuk list

            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new { message = "Data ditemukan || 200 OK", data });
        }

        // DELETE: api/tindakanasuransi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTindakanAsuransi(Guid id)
        {
            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data Relasi**
                var data = await _applicationDbContext.TindakanAsuransis.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TindakanAsuransis.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // PUT: api/TindakanAsuransi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTindakanAsuransi(Guid id, [FromBody] TindakanAsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                // ======================================================
                // 1) Ambil data lama yang mau diupdate
                // ======================================================
                var data = await _applicationDbContext.TindakanAsuransis
                    .FirstOrDefaultAsync(x => x.TindakanAsuransiId == id && x.IsDelete == false);

                if (data == null)
                {
                    return NotFound(new { message = $"Data relasi dengan ID {id} tidak ditemukan || 404 Not Found" });
                }

                // ======================================================
                // 2) Cek duplikasi (kecuali data yang sedang diupdate)
                // ======================================================
                bool isDuplicate = await _applicationDbContext.TindakanAsuransis
                    .AnyAsync(c =>
                        c.TindakanAsuransiId != id &&
                        c.TindakanId == vm.TindakanId &&
                        c.AsuransiId == vm.AsuransiId &&
                        c.IsDelete == false
                    );

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // ======================================================
                // 3) Update data
                // ======================================================
                data.TindakanId = vm.TindakanId;
                data.AsuransiId = vm.AsuransiId;
                // ============================
                // MARKUP
                // ============================
                data.MarkupDokter = vm.MarkupDokter;
                data.MarkupRs = vm.MarkupRs;
                data.MarkupJp = vm.MarkupJp;
                data.MarkupBahp = vm.MarkupBahp;
                data.MarkupLainnya = vm.MarkupLainnya;
                data.MarkupTotal = vm.MarkupTotal;

                data.IsMarkupBerlaku = vm.IsMarkupBerlaku ;
                data.MarkupDari = vm.MarkupDari;
                data.MarkupSampai = vm.MarkupSampai;

                // ============================
                // DISKON
                // ============================
                data.DiskonDokter = vm.DiskonDokter;
                data.DiskonRs = vm.DiskonRs;
                data.DiskonJp = vm.DiskonJp;
                data.DiskonBahp = vm.DiskonBahp;
                data.DiskonTotal = vm.DiskonTotal;

                data.IsDiskonBerlaku = vm.IsDiskonBerlaku;
                data.DiskonDari = vm.DiskonDari;
                data.DiskonSampai = vm.DiskonSampai;


                // audit update (jika ada fieldnya)
                data.UpdateDateTime = DateTime.UtcNow;
                data.UpdateBy = userActiveId;

                // ======================================================
                // 4) Save
                // ======================================================
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Relasi Berhasil || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diupdate." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged-all")]
        public async Task<IActionResult> GetPagedAll(
            int page = 1,
            int perPage = 10,

            Guid? tindakanId = null,
            Guid? kelasId = null,
            Guid? poliId = null,

            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",

            DateTime? startDate = null,
            DateTime? endDate = null,

            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,

            bool? isRawatInap = null,
            string? poliNama = null
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;
                if (perPage > 50) perPage = 50; // biar swagger tidak berat

                // ======================================================
                // 0) Resolve tanggal dari "periode" (kalau start/end tidak dikirim)
                //    Implementasi aman pakai ToString() supaya compile walau enum member beda.
                // ======================================================
                if ((!startDate.HasValue || !endDate.HasValue) && periode.HasValue)
                {
                    var now = DateTime.Now; // server time; kalau mau Jakarta fixed, pakai timezone
                    var p = periode.Value.ToString();

                    // Silakan sesuaikan string ini dengan enum kamu
                    if (p.Equals("HariIni", StringComparison.OrdinalIgnoreCase) || p.Equals("Today", StringComparison.OrdinalIgnoreCase))
                    {
                        startDate = now.Date;
                        endDate = now.Date;
                    }
                    else if (p.Equals("MingguIni", StringComparison.OrdinalIgnoreCase) || p.Equals("ThisWeek", StringComparison.OrdinalIgnoreCase))
                    {
                        // asumsi minggu mulai Senin
                        int diff = ((int)now.DayOfWeek + 6) % 7; // Senin=0
                        startDate = now.Date.AddDays(-diff);
                        endDate = startDate.Value.AddDays(6);
                    }
                    else if (p.Equals("BulanIni", StringComparison.OrdinalIgnoreCase) || p.Equals("ThisMonth", StringComparison.OrdinalIgnoreCase))
                    {
                        startDate = new DateTime(now.Year, now.Month, 1);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                    }
                    else if (p.Equals("TahunIni", StringComparison.OrdinalIgnoreCase) || p.Equals("ThisYear", StringComparison.OrdinalIgnoreCase))
                    {
                        startDate = new DateTime(now.Year, 1, 1);
                        endDate = new DateTime(now.Year, 12, 31);
                    }
                }

                // ======================================================
                // 1) BASE QUERY (tanpa join besar)
                // ======================================================
                var baseQuery = _applicationDbContext.TindakanAsuransis
                    .AsNoTracking()
                    .Where(a => a.IsDelete == false || a.IsDelete == null);

                // Filter direct by tindakanId (di tabel TindakanAsuransi ada TindakanId)
                if (tindakanId.HasValue)
                    baseQuery = baseQuery.Where(a => a.TindakanId == tindakanId.Value);

                // Filter date
                if (startDate.HasValue && endDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    var endExclusive = endDate.Value.Date.AddDays(1);
                    baseQuery = baseQuery.Where(a => a.CreateDateTime >= start && a.CreateDateTime < endExclusive);
                }

                // ======================================================
                // 2) FILTER RELASI via EXISTS/Any (lebih ringan)
                // ======================================================

                // Filter isRawatInap (di tabel Tindakan)
                if (isRawatInap.HasValue)
                {
                    var rawat = isRawatInap.Value;
                    baseQuery = baseQuery.Where(a =>
                        _applicationDbContext.Tindakans.Any(t =>
                            t.TindakanId == a.TindakanId && t.IsRawatInap == rawat));
                }

                // Filter kelasId (via TarifKelass -> Kelas)
                if (kelasId.HasValue)
                {
                    var kid = kelasId.Value;
                    baseQuery = baseQuery.Where(a =>
                        _applicationDbContext.TarifKelass.Any(tk =>
                            tk.TindakanId == a.TindakanId && tk.KelasId == kid));
                }

                // Filter poliId (via TindakanPolis)
                if (poliId.HasValue)
                {
                    var pid = poliId.Value;
                    baseQuery = baseQuery.Where(a =>
                        _applicationDbContext.TindakanPolis.Any(tp =>
                            tp.TindakanId == a.TindakanId && tp.PoliklinikId == pid));
                }

                // Filter poliNama (via TindakanPolis join Poliklinik) -> EXISTS, tidak bikin duplikasi
                if (!string.IsNullOrWhiteSpace(poliNama))
                {
                    var pattern = $"%{poliNama.Trim().ToLower()}%";
                    baseQuery = baseQuery.Where(a =>
                        (from tp in _applicationDbContext.TindakanPolis
                         join p in _applicationDbContext.Polikliniks on tp.PoliklinikId equals p.PoliklinikId
                         where tp.TindakanId == a.TindakanId
                               && p.NamaPoliklinik != null
                               && EF.Functions.ILike(p.NamaPoliklinik.ToLower(), pattern)
                         select 1).Any()
                    );
                }

                // ======================================================
                // 3) SORTING (hanya kolom yang ada di TindakanAsuransi agar ringan)
                // ======================================================
                bool desc = (sortDirection ?? "desc").Equals("desc", StringComparison.OrdinalIgnoreCase);

                var sorted = orderBy?.Trim() switch
                {
                    "MarkupTotal" => desc ? baseQuery.OrderByDescending(a => a.MarkupTotal).ThenByDescending(a => a.TindakanAsuransiId)
                                         : baseQuery.OrderBy(a => a.MarkupTotal).ThenBy(a => a.TindakanAsuransiId),

                    "DiskonTotal" => desc ? baseQuery.OrderByDescending(a => a.DiskonTotal).ThenByDescending(a => a.TindakanAsuransiId)
                                         : baseQuery.OrderBy(a => a.DiskonTotal).ThenBy(a => a.TindakanAsuransiId),

                    _ => desc ? baseQuery.OrderByDescending(a => a.CreateDateTime).ThenByDescending(a => a.TindakanAsuransiId)
                              : baseQuery.OrderBy(a => a.CreateDateTime).ThenBy(a => a.TindakanAsuransiId),
                };

                // ======================================================
                // 4) TOTAL ROWS (tanpa join besar)
                // ======================================================
                var totalRows = await sorted.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                // ======================================================
                // 5) PAGING ID dulu (ringan)
                // ======================================================
                var pagedIds = await sorted
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(a => a.TindakanAsuransiId)
                    .ToListAsync();

                if (pagedIds.Count == 0)
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

                var idSet = pagedIds.ToHashSet();

                // ======================================================
                // 6) LOAD CORE rows (tanpa join)
                // ======================================================
                var core = await _applicationDbContext.TindakanAsuransis
                    .AsNoTracking()
                    .Where(a => idSet.Contains(a.TindakanAsuransiId))
                    .Select(a => new
                    {
                        a.TindakanAsuransiId,
                        a.CreateDateTime,
                        a.CreateBy,
                        a.AsuransiId,
                        a.TindakanId,

                        // MARKUP
                        a.MarkupDokter,
                        a.MarkupRs,
                        a.MarkupJp,
                        a.MarkupBahp,
                        a.MarkupLainnya,
                        a.MarkupTotal,
                        a.IsMarkupBerlaku,
                        a.MarkupDari,
                        a.MarkupSampai,

                        // DISKON
                        a.DiskonDokter,
                        a.DiskonRs,
                        a.DiskonJp,
                        a.DiskonBahp,
                        a.DiskonTotal,
                        a.IsDiskonBerlaku,
                        a.DiskonDari,
                        a.DiskonSampai
                    })
                    .ToListAsync();

                var coreDict = core.ToDictionary(x => x.TindakanAsuransiId, x => x);

                // ======================================================
                // 7) BUILD IDS untuk LOOKUP
                // ======================================================
                var userIds = core.Select(x => x.CreateBy).Where(x => x != Guid.Empty).Distinct().ToList();
                var asuransiIds = core.Select(x => x.AsuransiId).Where(x => x != Guid.Empty).Distinct().ToList();
                var tindakanIds = core.Select(x => x.TindakanId).Where(x => x != Guid.Empty).Distinct().ToList();

                // ======================================================
                // 8) LOOKUP: User, Asuransi, Tindakan
                // ======================================================
                var userData = await _applicationDbContext.UserActives
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.UserActiveId))
                    .Select(u => new { u.UserActiveId, u.FullName })
                    .ToListAsync();
                var userDict = userData.ToDictionary(x => x.UserActiveId, x => x.FullName);

                var asuData = await _applicationDbContext.Asuransis
                    .AsNoTracking()
                    .Where(a => asuransiIds.Contains(a.AsuransiId))
                    .Select(a => new { a.AsuransiId, a.NamaAsuransi })
                    .ToListAsync();
                var asuDict = asuData.ToDictionary(x => x.AsuransiId, x => x.NamaAsuransi);

                var tindakanData = await _applicationDbContext.Tindakans
                    .AsNoTracking()
                    .Where(t => tindakanIds.Contains(t.TindakanId))
                    .Select(t => new { t.TindakanId, t.KodeTindakan, t.NamaTindakan, t.IsRawatInap, t.UnitAsal })
                    .ToListAsync();
                var tindakanDict = tindakanData.ToDictionary(x => x.TindakanId, x => x);

                // ======================================================
                // 9) LOOKUP: Poliklinik (via TindakanPolis)
                // ======================================================
                var poliData = await (
                    from tp in _applicationDbContext.TindakanPolis.AsNoTracking()
                    join p in _applicationDbContext.Polikliniks.AsNoTracking()
                        on tp.PoliklinikId equals p.PoliklinikId
                    where tindakanIds.Contains(tp.TindakanId)
                    select new { tp.TindakanId, p.PoliklinikId, p.NamaPoliklinik }
                ).ToListAsync();
                var poliLookup = poliData.ToLookup(x => x.TindakanId);

                // ======================================================
                // 10) LOOKUP: KELAS (via TarifKelass join Kelass)
                //     -> ini memenuhi request "Joinkan juga ke tabel kelas id"
                // ======================================================
                var tarifQuery =
                    from tk in _applicationDbContext.TarifKelass.AsNoTracking()
                    join k in _applicationDbContext.Kelass.AsNoTracking()
                        on tk.KelasId equals k.KelasId
                    where tindakanIds.Contains((Guid)tk.TindakanId)
                    select new
                    {
                        tk.TindakanId,
                        tk.KelasId,
                        k.NamaKelas,
                        tk.TarifKelasId,
                        tk.TarifDokter,
                        tk.TarifRs,
                        tk.TarifJp,
                        tk.TarifTotal
                    };

                if (kelasId.HasValue)
                    tarifQuery = tarifQuery.Where(x => x.KelasId == kelasId.Value);

                var tarifData = await tarifQuery.ToListAsync();
                var tarifLookup = tarifData.ToLookup(x => x.TindakanId);

                // ======================================================
                // 11) BUILD RESULT sesuai urutan paging
                // ======================================================
                var result = pagedIds
                    .Where(coreDict.ContainsKey)
                    .Select(id =>
                    {
                        var r = coreDict[id];

                        userDict.TryGetValue(r.CreateBy, out var createByName);
                        asuDict.TryGetValue(r.AsuransiId, out var asuransiNama);
                        tindakanDict.TryGetValue(r.TindakanId, out var tind);

                        return new
                        {
                            r.TindakanAsuransiId,
                            r.CreateDateTime,
                            r.CreateBy,
                            CreateByName = createByName,

                            r.AsuransiId,
                            AsuransiNama = asuransiNama,

                            r.TindakanId,
                            KodeTindakan = tind?.KodeTindakan,
                            NamaTindakan = tind?.NamaTindakan,
                            IsRawatInap = tind?.IsRawatInap,
                            UnitAsal = tind?.UnitAsal,

                            PoliNames = poliLookup[r.TindakanId].ToList(),

                            // JOIN KELAS DI SINI
                            KelasTarif = tarifLookup[r.TindakanId].ToList(),

                            // MARKUP
                            r.MarkupDokter,
                            r.MarkupRs,
                            r.MarkupJp,
                            r.MarkupBahp,
                            r.MarkupLainnya,
                            r.MarkupTotal,
                            r.IsMarkupBerlaku,
                            r.MarkupDari,
                            r.MarkupSampai,

                            // DISKON
                            r.DiskonDokter,
                            r.DiskonRs,
                            r.DiskonJp,
                            r.DiskonBahp,
                            r.DiskonTotal,
                            r.IsDiskonBerlaku,
                            r.DiskonDari,
                            r.DiskonSampai
                        };
                    })
                    .ToList();

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = result,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}",
                    inner = ex.InnerException?.Message
                });
            }
        }


    }
}
