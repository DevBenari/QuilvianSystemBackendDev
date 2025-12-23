using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    [EnableCors("AllowSpecific")]
    public class TindakanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<TindakanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TindakanController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TindakanController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/tindakan
        [HttpGet]
        public async Task<IActionResult> GetAllTindakan(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            try
            {
                // =========================
                // 1️⃣ Query utama tindakan
                // =========================
                var query = from t in _applicationDbContext.Tindakans
                            join u in _applicationDbContext.UserActives on t.CreateBy equals u.UserActiveId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where t.IsDelete == false || t.IsDelete == null
                            orderby t.CreateDateTime descending
                            select new
                            {
                                t.TindakanId,
                                t.KodeTindakan,
                                t.NamaTindakan,
                                t.IsRawatInap,
                                t.UnitAsal,
                                CreateByName = u != null ? u.FullName : null,
                                t.CreateDateTime
                            };

                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var tindakanList = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                if (!tindakanList.Any())
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan." });

                // =========================
                // 2️⃣ Ambil semua ID Tindakan untuk batch query relasi
                // =========================
                var tindakanIds = tindakanList.Select(t => t.TindakanId).ToList();

                // ---- Relasi Asuransi
                var asuransiData = await (
                    from ta in _applicationDbContext.TindakanAsuransis
                    join asu in _applicationDbContext.Asuransis on ta.AsuransiId equals asu.AsuransiId
                    where tindakanIds.Contains(ta.TindakanId)
                    select new
                    {
                        ta.TindakanId,
                        asu.AsuransiId,
                        asu.NamaAsuransi
                    }
                ).ToListAsync();

                // ---- Relasi Poli
                var poliData = await (
                    from tp in _applicationDbContext.TindakanPolis
                    join poli in _applicationDbContext.Polikliniks on tp.PoliId equals poli.PoliklinikId
                    where tindakanIds.Contains(tp.TindakanId)
                    select new
                    {
                        tp.TindakanId,
                        PoliId = poli.PoliklinikId,
                        NamaPoliklinik = poli.NamaPoliklinik
                    }
                ).ToListAsync();

                // ---- Relasi Tarif Kelas
                var tarifData = await (
                    from tk in _applicationDbContext.TarifKelass
                    join k in _applicationDbContext.Kelass on tk.KelasId equals k.KelasId
                    where tindakanIds.Contains((Guid)tk.TindakanId)
                    select new
                    {
                        tk.TindakanId,
                        tk.KelasId,
                        tk.TarifKelasId,
                        tk.TarifDokter,
                        tk.TarifRs,
                        tk.TarifJp,
                        tk.TarifBahp,
                        tk.TarifLain,
                        tk.TarifTotal,
                        tk.KSO,
                        NamaKelas = k.NamaKelas
                    }
                ).ToListAsync();

                // =========================
                // 3️⃣ Buat lookup agar efisien di memory
                // =========================
                var asuransiLookup = asuransiData.ToLookup(x => x.TindakanId);
                var poliLookup = poliData.ToLookup(x => x.TindakanId);
                var tarifLookup = tarifData.ToLookup(x => x.TindakanId);

                // =========================
                // 4️⃣ Gabungkan hasil ke struktur akhir
                // =========================
                var listdata = tindakanList.Select(t => new
                {
                    t.TindakanId,
                    t.KodeTindakan,
                    t.NamaTindakan,
                    t.IsRawatInap,
                    t.UnitAsal,
                    t.CreateByName,
                    t.CreateDateTime,

                    AsuransiNames = asuransiLookup[t.TindakanId]
                        .Select(a => new
                        {
                            a.AsuransiId,
                            a.NamaAsuransi
                        }).Distinct().ToList(),

                    PoliNames = poliLookup[t.TindakanId]
                        .Select(p => new
                        {
                            p.PoliId,
                            p.NamaPoliklinik
                        }).Distinct().ToList(),

                    TarifKelas = tarifLookup[t.TindakanId]
                        .Select(k => new
                        {
                            k.KelasId,
                            k.TarifKelasId,
                            k.TarifDokter,
                            k.TarifRs,
                            k.TarifJp,
                            k.TarifBahp,
                            k.TarifLain,
                            k.TarifTotal,
                            k.KSO,
                            k.NamaKelas
                        }).Distinct().ToList()
                }).ToList();

                // =========================
                // 5️⃣ Return hasil
                // =========================
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // GET: api/tindakan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTindakanById(Guid id)
        {
            try
            {
                // 1️⃣ Ambil tindakan utama
                var tindakan = await (
                    from t in _applicationDbContext.Tindakans
                    join u in _applicationDbContext.UserActives on t.CreateBy equals u.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()
                    where t.TindakanId == id && (t.IsDelete == false || t.IsDelete == null)
                    select new
                    {
                        t.TindakanId,
                        t.KodeTindakan,
                        t.NamaTindakan,
                        t.IsRawatInap,
                        t.UnitAsal,
                        t.CreateDateTime,
                        t.CreateBy,
                        CreateByName = u != null ? u.FullName : null
                    }
                ).FirstOrDefaultAsync();

                if (tindakan == null)
                    return NotFound(new { message = "Data tindakan tidak ditemukan." });

                // 2️⃣ Ambil semua relasi sekaligus
                var asuransiData = await (
                    from ta in _applicationDbContext.TindakanAsuransis
                    join asu in _applicationDbContext.Asuransis on ta.AsuransiId equals asu.AsuransiId
                    where ta.TindakanId == id
                    select new
                    {
                        asu.AsuransiId,
                        asu.NamaAsuransi
                    }
                ).ToListAsync();

                var poliData = await (
                    from tp in _applicationDbContext.TindakanPolis
                    join poli in _applicationDbContext.Polikliniks on tp.PoliId equals poli.PoliklinikId
                    where tp.TindakanId == id
                    select new
                    {
                        poli.PoliklinikId,
                        poli.NamaPoliklinik
                    }
                ).ToListAsync();

                var tarifData = await (
                    from tk in _applicationDbContext.TarifKelass
                    join k in _applicationDbContext.Kelass on tk.KelasId equals k.KelasId
                    where tk.TindakanId == id
                    select new
                    {
                        tk.KelasId,
                        tk.TarifKelasId,
                        tk.TarifDokter,
                        tk.TarifRs,
                        tk.TarifJp,
                        tk.TarifBahp,
                        tk.TarifLain,
                        tk.TarifTotal,
                        tk.KSO,
                        NamaKelas = k.NamaKelas
                    }
                ).ToListAsync();

                // 3️⃣ Satukan semua hasil
                var result = new
                {
                    tindakan.TindakanId,
                    tindakan.KodeTindakan,
                    tindakan.NamaTindakan,
                    tindakan.IsRawatInap,
                    tindakan.UnitAsal,
                    tindakan.CreateDateTime,
                    tindakan.CreateBy,
                    tindakan.CreateByName,

                    AsuransiNames = asuransiData.Select(a => new
                    {
                        a.AsuransiId,
                        a.NamaAsuransi
                    }).Distinct().ToList(),

                    PoliNames = poliData.Select(p => new
                    {
                        PoliId = p.PoliklinikId,
                        p.NamaPoliklinik
                    }).Distinct().ToList(),

                    TarifKelas = tarifData.Select(t => new
                    {
                        t.KelasId,
                        t.TarifKelasId,
                        t.TarifDokter,
                        t.TarifRs,
                        t.TarifJp,
                        t.TarifBahp,
                        t.TarifLain,
                        t.TarifTotal,
                        t.KSO,
                        t.NamaKelas
                    }).Distinct().ToList()
                };

                // 4️⃣ Return hasil
                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // POST: api/tindakan
        [HttpPost]
        public async Task<IActionResult> CreateTindakan([FromBody] TindakanViewModel vm)
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
                var dateNow = DateTime.UtcNow;

                var lastCode = _applicationDbContext.Tindakans
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.CreateDateTime)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"TDK{dateNow.ToString("yyMMdd")}0001";
                }
                else
                {
                    var lastNumber = int.Parse(lastCode.KodeTindakan.Substring(9));
                    kode = $"TDK{dateNow.ToString("yyMMdd")}{(lastNumber + 1).ToString("D4")}";
                }

                bool isDuplicate = _applicationDbContext.Tindakans
                    .Any(c => c.NamaTindakan.ToLower().Trim() == vm.NamaTindakan.ToLower().Trim() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                var data = new Models.Tindakan
                {
                    TindakanId = Guid.NewGuid(),
                    CreateDateTime = dateNow,
                    CreateBy = userActiveId,
                    KodeTindakan = kode,
                    UnitAsal = vm.UnitAsal,
                    NamaTindakan = vm.NamaTindakan,
                    IsRawatInap = vm.IsRawatInap,
                };

                _applicationDbContext.Tindakans.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // PUT: api/tindakan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTindakan(Guid id, [FromBody] TindakanViewModel vm)
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
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                var data = await _applicationDbContext.Tindakans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                bool isDuplicate = await _applicationDbContext.Tindakans
                    .AnyAsync(c => c.NamaTindakan.ToLower().Trim() == vm.NamaTindakan.ToLower().Trim() && c.TindakanId != id
                    && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                data.NamaTindakan = vm.NamaTindakan;
                data.IsRawatInap = vm.IsRawatInap;
                data.UnitAsal = vm.UnitAsal;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.Tindakans.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/tindakan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTindakan(Guid id)
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

                var data = await _applicationDbContext.Tindakans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Tindakans.Update(data);
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

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            string? search = null,
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

                // ======================================================
                // 1. BASE QUERY
                // ======================================================
                var query = _applicationDbContext.Tindakans
                    .AsNoTracking()
                    .Where(t => t.IsDelete == false || t.IsDelete == null);

                if (isRawatInap.HasValue)
                    query = query.Where(t => t.IsRawatInap == isRawatInap.Value);

                if (tindakanId.HasValue)
                    query = query.Where(t => t.TindakanId == tindakanId.Value);

                if (kelasId.HasValue)
                {
                    var kid = kelasId.Value;
                    query = query.Where(t =>
                        _applicationDbContext.TarifKelass
                            .Any(tk => tk.TindakanId == t.TindakanId && tk.KelasId == kid));
                }

                if (poliId.HasValue)
                {
                    var pid = poliId.Value;
                    query = query.Where(t =>
                        _applicationDbContext.TindakanPolis
                            .Any(tp => tp.TindakanId == t.TindakanId && tp.PoliId == pid));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = $"%{search.ToLower()}%";
                    query = query.Where(t =>
                        EF.Functions.ILike(t.KodeTindakan, s) ||
                        EF.Functions.ILike(t.NamaTindakan, s));
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(t => t.CreateDateTime >= start && t.CreateDateTime <= end);
                }

                // ======================================================
                // 2. FILTER POLI (HANYA TINDAKAN DENGAN POLI TERTENTU)
                // ======================================================
                if (!string.IsNullOrWhiteSpace(poliNama))
                {
                    var pattern = $"%{poliNama.ToLower()}%";

                    query =
                        from t in query
                        join tp in _applicationDbContext.TindakanPolis on t.TindakanId equals tp.TindakanId
                        join p in _applicationDbContext.Polikliniks on tp.PoliId equals p.PoliklinikId
                        where EF.Functions.ILike(p.NamaPoliklinik, pattern)
                        select t;
                }

                // ======================================================
                // 3. SORTING
                // ======================================================
                query = sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "KodeTindakan" => query.OrderByDescending(t => t.KodeTindakan),
                        "NamaTindakan" => query.OrderByDescending(t => t.NamaTindakan),
                        _ => query.OrderByDescending(t => t.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "KodeTindakan" => query.OrderBy(t => t.KodeTindakan),
                        "NamaTindakan" => query.OrderBy(t => t.NamaTindakan),
                        _ => query.OrderBy(t => t.CreateDateTime)
                    };

                // ======================================================
                // 4. PAGING
                // ======================================================
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var tindakanList = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(t => new
                    {
                        t.TindakanId,
                        t.KodeTindakan,
                        t.NamaTindakan,
                        t.IsRawatInap,
                        t.UnitAsal,
                        t.CreateDateTime,
                        t.CreateBy,
                        CreateByName = _applicationDbContext.UserActives
                            .Where(u => u.UserActiveId == t.CreateBy)
                            .Select(u => u.FullName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!tindakanList.Any())
                    return NotFound(new { message = "Data tidak ditemukan." });

                // ======================================================
                // 5. LOAD RELATIONS (ASURANSI, POLI, TARIF)
                // ======================================================
                var tindakanIds = tindakanList.Select(t => t.TindakanId).ToList();

                var asuransiData = await (
                    from ta in _applicationDbContext.TindakanAsuransis
                    join asu in _applicationDbContext.Asuransis on ta.AsuransiId equals asu.AsuransiId
                    where tindakanIds.Contains((Guid)ta.TindakanId)
                    select new { ta.TindakanId, asu.AsuransiId, asu.NamaAsuransi }
                ).ToListAsync();

                var poliData = await (
                    from tp in _applicationDbContext.TindakanPolis
                    join p in _applicationDbContext.Polikliniks on tp.PoliId equals p.PoliklinikId
                    where tindakanIds.Contains((Guid)tp.TindakanId)
                    select new { tp.TindakanId, p.PoliklinikId, p.NamaPoliklinik }
                ).ToListAsync();

                var tarifDataQuery = from tk in _applicationDbContext.TarifKelass
                                     join k in _applicationDbContext.Kelass on tk.KelasId equals k.KelasId
                                     where tindakanIds.Contains((Guid)tk.TindakanId)
                                     select new
                                     {
                                         tk.TindakanId,
                                         tk.KelasId,
                                         tk.TarifKelasId,
                                         tk.TarifDokter,
                                         tk.TarifRs,
                                         tk.TarifJp,
                                         tk.TarifTotal,
                                         k.NamaKelas
                                     };

                if (kelasId.HasValue)
                    tarifDataQuery = tarifDataQuery.Where(t => t.KelasId == kelasId.Value);

                var tarifData = await tarifDataQuery.ToListAsync();

                // ======================================================
                // 6. BUILD LOOKUP
                // ======================================================
                var asuransiLookup = asuransiData.ToLookup(x => x.TindakanId);
                var poliLookup = poliData.ToLookup(x => x.TindakanId);
                var tarifLookup = tarifData.ToLookup(x => x.TindakanId);

                // ======================================================
                // 7. BUILD RESULT (FILTER PoliNames SECARA IN-MEMORY)
                // ======================================================
                var result = tindakanList.Select(t =>
                {
                    var poliItems = poliLookup[t.TindakanId].ToList();

                    if (!string.IsNullOrWhiteSpace(poliNama))
                    {
                        // filter in-memory, pakai StringComparison, BUKAN EF.Functions.ILike
                        poliItems = poliItems
                            .Where(p => p.NamaPoliklinik != null &&
                                        p.NamaPoliklinik.Contains(
                                            poliNama,
                                            StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    return new
                    {
                        t.TindakanId,
                        t.KodeTindakan,
                        t.NamaTindakan,
                        t.IsRawatInap,
                        t.UnitAsal,
                        t.CreateDateTime,
                        t.CreateBy,
                        t.CreateByName,
                        AsuransiNames = asuransiLookup[t.TindakanId].ToList(),
                        PoliNames = poliItems,
                        TarifKelas = tarifLookup[t.TindakanId].ToList()
                    };
                });

                // ======================================================
                // 8. RETURN RESPONSE
                // ======================================================
                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    },
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }






        //[HttpGet("paged")]
        //public async Task<IActionResult> Paged(
        //    int page = 1,
        //    int perPage = 10,
        //    string? search = null,
        //    Guid? kelasId = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        //    DateTime? startDate = null,
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        //    DateTime? endDate = null,
        //    [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
        //    // Tambahan filter boolean IsRawatInap
        //    bool? isRawatInap = null)
        //{
        //    try
        //    {
        //        // 1. Validasi Paging
        //        if (page < 1) page = 1;
        //        if (perPage < 1) perPage = 10;

        //        // 2. Query data (Sama seperti GetAll, hanya diubah menjadi IQueryable)
        //        var query = from t in _applicationDbContext.Tindakans
        //                        // Left Join UserActives untuk menghindari error jika CreateBy null
        //                    join u_inner in _applicationDbContext.UserActives on t.CreateBy equals u_inner.UserActiveId into userGroup
        //                    from u in userGroup.DefaultIfEmpty()
        //                    where t.IsDelete == false
        //                    select new
        //                    {
        //                        t.TindakanId,
        //                        t.KodeTindakan,
        //                        t.NamaTindakan,
        //                        t.IsRawatInap,
        //                        CreateByName = u != null ? u.FullName : null,
        //                        t.CreateDateTime,

        //                        // Mengambil Asuransi terkait (Subquery)
        //                        AsuransiNames = (from ta in _applicationDbContext.TindakanAsuransis
        //                                         join asu in _applicationDbContext.Asuransis on ta.AsuransiId equals asu.AsuransiId
        //                                         where ta.TindakanId == t.TindakanId
        //                                         select new
        //                                         {
        //                                             AsuransiId = asu.AsuransiId,
        //                                             NamaAsuransi = asu.NamaAsuransi
        //                                         }).Distinct().ToList(),

        //                        // Mengambil Poli terkait (Subquery)
        //                        PoliNames = (from tp in _applicationDbContext.TindakanPolis
        //                                     join poli in _applicationDbContext.Polikliniks on tp.PoliId equals poli.PoliklinikId
        //                                     where tp.TindakanId == t.TindakanId
        //                                     select new
        //                                     {
        //                                         PoliId = poli.PoliklinikId,
        //                                         NamaPoliklinik = poli.NamaPoliklinik
        //                                     }).Distinct().ToList(),

        //                        // Mengambil Tarif Kelas terkait (Subquery)
        //                        TarifKelas = (from tk in _applicationDbContext.TarifKelass
        //                                      where tk.TindakanId == t.TindakanId
        //                                      join k in _applicationDbContext.Kelass on tk.KelasId equals k.KelasId
        //                                      select new
        //                                      {
        //                                          tk.KelasId,
        //                                          tk.TarifKelasId,
        //                                          tk.TarifDokter,
        //                                          tk.TarifRs,
        //                                          tk.TarifJp,
        //                                          tk.TarifBahp,
        //                                          tk.TarifLain,
        //                                          tk.TarifTotal,
        //                                          tk.KSO,
        //                                          NamaKelas = k.NamaKelas
        //                                      }).ToList()
        //                    };

        //        // 3. Filter data berdasarkan input user

        //        // Filter: IsRawatInap (Boolean)
        //        if (isRawatInap.HasValue)
        //        {
        //            query = query.Where(t => t.IsRawatInap == isRawatInap.Value);
        //        }

        //        // filter berdasarkan kelas id
        //        if (kelasId.HasValue) 
        //        {
        //            query = query.Where(u=> u.Kelas)
        //        }

        //        // Filter: Search (Pencarian pada Kode atau Nama Tindakan)
        //        if (!string.IsNullOrWhiteSpace(search))
        //        {
        //            string searchLower = $"%{search.ToLower()}%";
        //            query = query.Where(t =>
        //                EF.Functions.ILike(t.KodeTindakan, searchLower) ||
        //                EF.Functions.ILike(t.NamaTindakan, searchLower)
        //            );
        //        }

        //        // Filter: Rentang Tanggal (StartDate & EndDate)
        //        if (startDate.HasValue && endDate.HasValue)
        //        {
        //            // Pastikan perbandingan tanggal yang akurat (awal hari StartDate hingga akhir hari EndDate)
        //            DateTimeOffset startUtc = startDate.Value.Date;
        //            DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1);

        //            query = query.Where(u =>
        //                u.CreateDateTime >= startUtc &&
        //                u.CreateDateTime <= endUtc);
        //        }

        //        // Filter: Periode (Today, ThisWeek, dll.)
        //        if (periode.HasValue)
        //        {
        //            // Menggunakan DateTime.Now atau DateTime.UtcNow tergantung konfigurasi database/server
        //            DateTime today = DateTime.UtcNow.Date;

        //            switch (periode)
        //            {
        //                case PeriodeFilter.Today:
        //                    query = query.Where(u => u.CreateDateTime.Date == today);
        //                    break;
        //                case PeriodeFilter.ThisWeek:
        //                    DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        //                    query = query.Where(u => u.CreateDateTime.Date >= startOfWeek);
        //                    break;
        //                case PeriodeFilter.LastWeek:
        //                    DateTime startOfLastWeek = today.AddDays(-7 - (int)today.DayOfWeek);
        //                    DateTime endOfLastWeek = today.AddDays(-(int)today.DayOfWeek).AddTicks(-1);
        //                    query = query.Where(u => u.CreateDateTime.Date >= startOfLastWeek && u.CreateDateTime.Date <= endOfLastWeek);
        //                    break;
        //                case PeriodeFilter.ThisMonth:
        //                    query = query.Where(u => u.CreateDateTime.Month == today.Month && u.CreateDateTime.Year == today.Year);
        //                    break;
        //                case PeriodeFilter.LastMonth:
        //                    DateTime lastMonth = today.AddMonths(-1);
        //                    query = query.Where(u => u.CreateDateTime.Month == lastMonth.Month && u.CreateDateTime.Year == lastMonth.Year);
        //                    break;
        //                case PeriodeFilter.ThisYear:
        //                    query = query.Where(u => u.CreateDateTime.Year == today.Year);
        //                    break;
        //                case PeriodeFilter.LastYear:
        //                    query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
        //                    break;
        //                case PeriodeFilter.Last3Months:
        //                    query = query.Where(u => u.CreateDateTime.Date >= today.AddMonths(-3));
        //                    break;
        //                case PeriodeFilter.Last6Months:
        //                    query = query.Where(u => u.CreateDateTime.Date >= today.AddMonths(-6));
        //                    break;
        //            }
        //        }

        //        // 4. Sorting Data (Dynamic Sorting)
        //        query = sortDirection?.ToLower() == "desc"
        //            ? orderBy switch
        //            {
        //                "CreateDateTime" => query.OrderByDescending(t => t.CreateDateTime),
        //                "KodeTindakan" => query.OrderByDescending(t => t.KodeTindakan),
        //                "NamaTindakan" => query.OrderByDescending(t => t.NamaTindakan),
        //                "CreateByName" => query.OrderByDescending(t => t.CreateByName),
        //                _ => query.OrderByDescending(t => t.CreateDateTime) // Default
        //            }
        //            : orderBy switch
        //            {
        //                "CreateDateTime" => query.OrderBy(t => t.CreateDateTime),
        //                "KodeTindakan" => query.OrderBy(t => t.KodeTindakan),
        //                "NamaTindakan" => query.OrderBy(t => t.NamaTindakan),
        //                "CreateByName" => query.OrderBy(t => t.CreateByName),
        //                _ => query.OrderBy(t => t.CreateDateTime) // Default
        //            };

        //        // 5. Eksekusi Paging dan Hitung Total
        //        var totalRows = await query.CountAsync();
        //        var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //        var listdata = await query
        //            .Skip((page - 1) * perPage)
        //            .Take(perPage)
        //            .ToListAsync(); // Eksekusi query

        //        // 6. Response
        //        if (!listdata.Any())
        //        {
        //            if (page > totalPages && totalRows > 0)
        //            {
        //                return NotFound(new { message = "Halaman tidak ditemukan." });
        //            }
        //            return NotFound(new { message = "Belum ada data tindakan yang sesuai dengan filter." });
        //        }

        //        return Ok(new
        //        {
        //            status = "success",
        //            message = "Data retrieved successfully",
        //            data = new
        //            {
        //                Rows = listdata,
        //                TotalRows = totalRows,
        //                CurrentPage = page,
        //                PerPage = perPage,
        //                TotalPages = totalPages
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Penanganan error internal server
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}
    }
}
