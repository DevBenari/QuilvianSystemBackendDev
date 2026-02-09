using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class OperasiTindakanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<OperasiTindakanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OperasiTindakanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<OperasiTindakanController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.OperasiTindakans
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.TindakanOperasiId,
                             a.TindakanId,
                             a.JenisOperasiId,
                             a.TipeOperasiId,
                             a.Keterangan,
                         }).OrderByDescending(a => a.CreateDateTime);

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.OperasiTindakans.Find(id);
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OperasiTindakanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
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

                //// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.OperasiTindakans
                //                    .AnyAsync(c => c.NamaDiskon == vm.NamaDiskon && c.IsDelete == false);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new OperasiTindakan
                {
                    TindakanOperasiId = Guid.NewGuid(),
                    TindakanId = vm.TindakanId,
                    TipeOperasiId = vm.TipeOperasiId,
                    JenisOperasiId = vm.JenisOperasiId,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.OperasiTindakans.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OperasiTindakanViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
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

                // **Cari Data**
                var data = await _applicationDbContext.OperasiTindakans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.TindakanId = vm.TindakanId;
                data.JenisOperasiId = vm.JenisOperasiId;
                data.TipeOperasiId = vm.TipeOperasiId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.OperasiTindakans.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
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

                // **Cari Data**
                var data = await _applicationDbContext.OperasiTindakans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.OperasiTindakans.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
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
            Guid? jenisOPId = null,
            Guid? kelasId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            DateTime? startDate = null,
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ==========================================================
                // 1️⃣ BASE QUERY
                // ==========================================================
                var query = _applicationDbContext.OperasiTindakans
                    .AsNoTracking()
                    .Where(o => o.IsDelete == false || o.IsDelete == null);

                if (jenisOPId.HasValue)
                    query = query.Where(o => o.JenisOperasiId == jenisOPId.Value);

                // Search by keterangan
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = $"%{search.ToLower()}%";
                    query = query.Where(o => EF.Functions.ILike(o.Keterangan.ToLower(), s));
                }

                // Filter tanggal manual
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTimeOffset startUtc = startDate.Value.Date;
                    DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.CreateDateTime >= startUtc && o.CreateDateTime <= endUtc);
                }

                // Filter Periode
                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    switch (periode)
                    {
                        case PeriodeFilter.Today:
                            query = query.Where(o => o.CreateDateTime.Date == today);
                            break;

                        case PeriodeFilter.ThisWeek:
                            query = query.Where(o =>
                                o.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                                o.CreateDateTime.Date <= today
                            );
                            break;

                        case PeriodeFilter.LastWeek:
                            query = query.Where(o =>
                                o.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                o.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                            );
                            break;

                        case PeriodeFilter.ThisMonth:
                            query = query.Where(o =>
                                o.CreateDateTime.Month == today.Month &&
                                o.CreateDateTime.Year == today.Year
                            );
                            break;

                        case PeriodeFilter.LastMonth:
                            query = query.Where(o =>
                                o.CreateDateTime.Month == today.Month - 1 &&
                                o.CreateDateTime.Year == today.Year
                            );
                            break;
                    }
                }

                // ==========================================================
                // 2️⃣ SORTING
                // ==========================================================
                query = sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "Keterangan" => query.OrderByDescending(o => o.Keterangan),
                        _ => query.OrderByDescending(o => o.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "Keterangan" => query.OrderBy(o => o.Keterangan),
                        _ => query.OrderBy(o => o.CreateDateTime)
                    };

                // ==========================================================
                // 3️⃣ PAGING
                // ==========================================================
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var operasiList = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .Select(o => new
                    {
                        o.TindakanOperasiId,
                        o.TindakanId,
                        o.JenisOperasiId,
                        o.TipeOperasiId,
                        o.Keterangan,
                        o.CreateDateTime,
                        o.CreateBy,
                        CreateByName = _applicationDbContext.UserActives
                            .Where(u => u.UserActiveId == o.CreateBy)
                            .Select(u => u.FullName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!operasiList.Any())
                    return NotFound(new { message = "Data tidak ditemukan." });

                // ==========================================================
                // 4️⃣ LOAD ALL RELATIONS (BATCH QUERY — NO N+1)
                // ==========================================================
                var operasiIds = operasiList.Select(x => x.TindakanOperasiId).ToList();
                var tindakanIds = operasiList.Select(x => x.TindakanId).ToList();

                // JOIN: Tindakan
                var tindakanData = await (
                    from t in _applicationDbContext.Tindakans
                    where tindakanIds.Contains(t.TindakanId)
                    select new
                    {
                        t.TindakanId,
                        t.KodeTindakan,
                        t.NamaTindakan
                    }
                ).ToListAsync();

                var tindakanLookup = tindakanData.ToLookup(x => x.TindakanId);

                // JOIN: Asuransi
                var asuransiData = await (
                    from ta in _applicationDbContext.TindakanAsuransis
                    join asu in _applicationDbContext.Asuransis on ta.AsuransiId equals asu.AsuransiId
                    where tindakanIds.Contains((Guid)ta.TindakanId)
                    select new
                    {
                        ta.TindakanId,
                        asu.AsuransiId,
                        asu.NamaAsuransi
                    }
                ).ToListAsync();

                var asuransiLookup = asuransiData.ToLookup(x => x.TindakanId);

                // JOIN: Poli
                var poliData = await (
                    from tp in _applicationDbContext.TindakanPolis
                    join p in _applicationDbContext.Polikliniks on tp.PoliklinikId equals p.PoliklinikId
                    where tindakanIds.Contains((Guid)tp.TindakanId)
                    select new
                    {
                        tp.TindakanId,
                        p.PoliklinikId,
                        p.NamaPoliklinik
                    }
                ).ToListAsync();

                var poliLookup = poliData.ToLookup(x => x.TindakanId);

                // JOIN: Tarif Kelas
                var tarifDataQuery =
                    from tk in _applicationDbContext.TarifKelass
                    join k in _applicationDbContext.Kelass on tk.KelasId equals k.KelasId
                    where tindakanIds.Contains((Guid)tk.TindakanId)
                    select new
                    {
                        tk.TindakanId,
                        tk.TarifKelasId,
                        tk.KelasId,
                        k.NamaKelas,
                        tk.TarifDokter,
                        tk.TarifRs,
                        tk.TarifJp,
                        tk.TarifTotal
                    };

                if (kelasId.HasValue)
                    tarifDataQuery = tarifDataQuery.Where(t => t.KelasId == kelasId.Value);

                var tarifData = await tarifDataQuery.ToListAsync();
                var tarifLookup = tarifData.ToLookup(x => x.TindakanId);

                // JOIN: Jenis Operasi
                var jenisOperasiData = await (
                    from o in _applicationDbContext.OperasiTindakans
                    join jo in _applicationDbContext.OperasiJeniss on o.JenisOperasiId equals jo.JenisOperasiId
                    where operasiIds.Contains(o.TindakanOperasiId)
                    select new
                    {
                        o.TindakanOperasiId,
                        jo.JenisOperasiId,
                        jo.NamaJenisOperasi
                    }
                ).ToListAsync();

                var jenisOperasiLookup = jenisOperasiData.ToLookup(x => x.TindakanOperasiId);

                // JOIN: Tipe Operasi
                var tipeOperasiData = await (
                    from o in _applicationDbContext.OperasiTindakans
                    join toper in _applicationDbContext.OperasiTipes on o.TipeOperasiId equals toper.TipeOperasiId
                    where operasiIds.Contains(o.TindakanOperasiId)
                    select new
                    {
                        o.TindakanOperasiId,
                        toper.TipeOperasiId,
                        toper.NamaTipeOperasi
                    }
                ).ToListAsync();

                var tipeOperasiLookup = tipeOperasiData.ToLookup(x => x.TindakanOperasiId);

                // ==========================================================
                // 5️⃣ FINAL RESULT (SAMA FORMATNYA DENGAN Paged Tindakan)
                // ==========================================================
                var result = operasiList.Select(o => new
                {
                    o.TindakanOperasiId,
                    o.TindakanId,

                    KodeTindakan = tindakanLookup[(Guid)o.TindakanId].FirstOrDefault()?.KodeTindakan,
                    NamaTindakan = tindakanLookup[(Guid)o.TindakanId].FirstOrDefault()?.NamaTindakan,

                    o.JenisOperasiId,
                    JenisOperasi = jenisOperasiLookup[o.TindakanOperasiId].FirstOrDefault(),

                    o.TipeOperasiId,
                    TipeOperasi = tipeOperasiLookup[o.TindakanOperasiId].FirstOrDefault(),

                    o.Keterangan,
                    o.CreateDateTime,
                    o.CreateBy,
                    o.CreateByName,

                    AsuransiNames = asuransiLookup[(Guid)o.TindakanId].ToList(),
                    PoliNames = poliLookup[(Guid)o.TindakanId].ToList(),
                    TarifKelas = tarifLookup[(Guid)o.TindakanId].ToList()
                });

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

    }
}
