using System.Linq;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class LabPemeriksaanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LabPemeriksaanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabPemeriksaanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabPemeriksaanController> logger,
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
            try
            {
                // 🔹 Hitung offset (paging)
                int skip = (page - 1) * perPage;

                // 🔹 Query data utama pemeriksaan
                var mainData = await (
                    from a in _applicationDbContext.LabPemeriksaans
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join k in _applicationDbContext.LabKategoriPemeriksaans on a.KategoriPemeriksaanId equals k.KategoriPemeriksaanId into kategoriGroup
                    from k in kategoriGroup.DefaultIfEmpty()

                    join l in _applicationDbContext.Labs on k.LabId equals l.LabId into labGroup
                    from l in labGroup.DefaultIfEmpty()

                    where a.IsDelete == false || a.IsDelete == null

                    orderby a.CreateDateTime descending

                    select new 
                    {
                        PemeriksaanLabId = a.PemeriksaanLabId,
                        NamaPemeriksaan = a.NamaPemeriksaan,
                        HargaPemeriksaan = a.HargaPemeriksaan,
                        KodePemeriksaan = a.KodePemeriksaan,
                        Keterangan = a.Keterangan,
                        CreateDateTime = a.CreateDateTime,
                        CreateBy = a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,

                        KategoriPemeriksaanId = a.KategoriPemeriksaanId,
                        NamaKategori = k != null ? k.NamaKategori : null,
                        KodeKategoriPemeriksaan = k != null ? k.KodeKategori : null,

                        LabId = l != null ? l.LabId : null,
                        NamaLab = l != null ? l.NamaLab : null,
                        KodeLab = l != null ? l.KodeKategori : null,
                        TarifKelas = new List<object>()

                    }
                )
                .Skip(skip)
                .Take(perPage)
                .ToListAsync();

                if (!mainData.Any())
                {
                    return NotFound(new { message = "Belum ada data Lab Pemeriksaan. || 404 Not Found" });
                }

                // 🔹 Ambil list PemeriksaanLabId
                var pemeriksaanIds = mainData.Select(x => x.PemeriksaanLabId).ToList();

                // 🔹 Ambil semua tarif kelas sekaligus (1 query)
                //var tarifKelas = await (
                //    from tk in _applicationDbContext.TarifKelass
                //    join kl in _applicationDbContext.Kelass on tk.KelasId equals kl.KelasId
                //    where pemeriksaanIds.Contains((Guid)tk.PemeriksaanLabId)
                //    select new 
                //    {
                //        TarifKelasId = tk.TarifKelasId,
                //        KelasId = tk.KelasId,
                //        NamaKelas = kl.NamaKelas,

                //        TarifDokter = tk.TarifDokter,
                //        TarifRs = tk.TarifRs,
                //        TarifJp = tk.TarifJp,
                //        TarifBahp = tk.TarifBahp,
                //        TarifLain = tk.TarifLain,
                //        TarifTotal = tk.TarifTotal,
                //        KSO = tk.KSO,

                //        // mapping PM Lab Id
                //        PemeriksaanLabId = tk.PemeriksaanLabId
                //    }
                //).ToListAsync();

                // 🔹 Gabungkan tarif kelas ke pemeriksaan
                //foreach (var item in mainData)
                //{
                //    var tk = tarifKelas
                //        .Where(x => x.PemeriksaanLabId == item.PemeriksaanLabId)
                //        .Cast<object>()
                //        .ToList();

                //    item.TarifKelas.AddRange(tk);
                //}

                // 🔹 Total data (tanpa paging)
                int totalData = await _applicationDbContext.LabPemeriksaans
                    .CountAsync(a => a.IsDelete == false || a.IsDelete == null);

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    page,
                    perPage,
                    totalData,
                    totalFiltered = mainData.Count,
                    data = mainData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLabPemeriksaanById(Guid id)
        {
            try
            {
                // 🔹 1️⃣ Ambil data utama pemeriksaan lab
                var mainData = await (
                    from a in _applicationDbContext.LabPemeriksaans
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    join k in _applicationDbContext.LabKategoriPemeriksaans on a.KategoriPemeriksaanId equals k.KategoriPemeriksaanId into kategoriGroup
                    from k in kategoriGroup.DefaultIfEmpty()
                    join l in _applicationDbContext.Labs on k.LabId equals l.LabId into labGroup
                    from l in labGroup.DefaultIfEmpty()
                    where a.PemeriksaanLabId == id && (a.IsDelete == false || a.IsDelete == null)
                    select new
                    {
                        a.PemeriksaanLabId,
                        a.NamaPemeriksaan,
                        a.HargaPemeriksaan,
                        a.KodePemeriksaan,
                        a.Keterangan,
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,
                        KategoriPemeriksaanId = a.KategoriPemeriksaanId,
                        NamaKategori = k != null ? k.NamaKategori : null,
                        KodeKategoriPemeriksaan = k != null ? k.KodeKategori : null,
                        LabId = l != null ? l.LabId : (Guid?)null,
                        NamaLab = l != null ? l.NamaLab : null,
                        KodeLab = l != null ? l.KodeKategori : null
                    }
                ).FirstOrDefaultAsync();

                if (mainData == null)
                {
                    return NotFound(new { message = "Pemeriksaan Lab tidak ditemukan." });
                }

                // 🔹 2️⃣ Ambil semua tarif kelas terkait sekali query (tanpa N+1)
                //var tarifKelasList = await (
                //    from tk in _applicationDbContext.TarifKelass
                //    join kl in _applicationDbContext.Kelass on tk.KelasId equals kl.KelasId
                //    where tk.PemeriksaanLabId == id
                //    select new
                //    {
                //        tk.KelasId,
                //        tk.TarifKelasId,
                //        tk.TarifDokter,
                //        tk.TarifRs,
                //        tk.TarifJp,
                //        tk.TarifBahp,
                //        tk.TarifLain,
                //        tk.TarifTotal,
                //        tk.KSO,
                //        NamaKelas = kl.NamaKelas
                //    }
                //).ToListAsync();

                // 🔹 3️⃣ Gabungkan hasil ke satu objek
                var result = new
                {
                    mainData.PemeriksaanLabId,
                    mainData.NamaPemeriksaan,
                    mainData.HargaPemeriksaan,
                    mainData.KodePemeriksaan,
                    mainData.Keterangan,
                    mainData.CreateDateTime,
                    mainData.CreateBy,
                    mainData.CreateByName,
                    mainData.KategoriPemeriksaanId,
                    mainData.NamaKategori,
                    mainData.KodeKategoriPemeriksaan,
                    mainData.LabId,
                    mainData.NamaLab,
                    mainData.KodeLab,
                    //TarifKelas = tarifKelasList
                };

                // ✅ Return hasil
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LabPemeriksaaanViewModel vm)
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
                bool isDuplicate = await _applicationDbContext.LabPemeriksaans
                                    .AnyAsync(c => c.NamaPemeriksaan.ToLower().Trim()
                                    == vm.NamaPemeriksaan.ToLower().Trim() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                }

                // **Buat Data Baru**
                var data = new LabPemeriksaan
                {
                    PemeriksaanLabId = Guid.NewGuid(),
                    NamaPemeriksaan = vm.NamaPemeriksaan,
                    KodePemeriksaan = vm.KodePemeriksaan,
                    HargaPemeriksaan = vm.HargaPemeriksaan,
                    KategoriPemeriksaanId = vm.KategoriPemeriksaanId,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.LabPemeriksaans.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] LabPemeriksaaanViewModel vm)
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

                // **Cek apakah data ada**
                var existingData = await _applicationDbContext.LabPemeriksaans.FindAsync(id);
                if (existingData == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Cek Duplikasi (Nama Pemeriksaan case-insensitive dan berbeda ID)**
                bool isDuplicate = await _applicationDbContext.LabPemeriksaans
                    .AnyAsync(c => c.NamaPemeriksaan.ToLower() == vm.NamaPemeriksaan.ToLower() && c.PemeriksaanLabId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Nama pemeriksaan ini sudah tersedia." });
                }

                // **Update Data**
                existingData.NamaPemeriksaan = vm.NamaPemeriksaan;
                existingData.KodePemeriksaan = vm.KodePemeriksaan; // tetap diperbolehkan diubah jika memang boleh
                existingData.HargaPemeriksaan = vm.HargaPemeriksaan;
                existingData.KategoriPemeriksaanId = vm.KategoriPemeriksaanId;
                existingData.Keterangan = vm.Keterangan;
                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabPemeriksaans.Update(existingData);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui di database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}" });
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
                var data = await _applicationDbContext.LabPemeriksaans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabPemeriksaans.Update(data);
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

        //[HttpGet("FiterLabdanKategori")]
        //public IActionResult GetDropdowns()
        //{
        //    try
        //    {
        //        // Ambil semua nama lab aktif
        //        var labs = _applicationDbContext.Labs
        //            .Where(l => l.IsDelete == false || l.IsDelete == null)
        //            .Select(l => l.NamaLab)
        //            .OrderBy(l => l)
        //            .ToList();

        //        // Ambil semua nama kategori aktif
        //        var kategoris = _applicationDbContext.LabKategoriPemeriksaans
        //            .Where(k => k.IsDelete == false || k.IsDelete == null)
        //            .Select(k => k.NamaKategori)
        //            .OrderBy(k => k)
        //            .ToList();

        //        return Ok(new
        //        {
        //            status = "success",
        //            message = "Dropdown data retrieved successfully",
        //            data = new
        //            {
        //                Labs = labs,
        //                Kategoris = kategoris
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpGet("paged")]
        public async Task<IActionResult> Paged(
            int page = 1,
            int perPage = 10,
            Guid? KategoriPemeriksaanId = null,
            Guid? Labid = null,
            Guid? kelasId = null,
            Guid? asuransiId = null,
            string? search = null,
            string? kodePemeriksaan = null,
            string? namaLab = null,
            string? namaKategori = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;
                if (perPage > 100) perPage = 100;

                // ======================================================
                // 1) BASE QUERY
                // ======================================================
                var query =
                    from a in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    where (a.IsDelete == false || a.IsDelete == null)

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on a.CreateBy equals u0.UserActiveId into userJoin
                    from u in userJoin.DefaultIfEmpty()

                    join k0 in _applicationDbContext.LabKategoriPemeriksaans.AsNoTracking()
                        on a.KategoriPemeriksaanId equals k0.KategoriPemeriksaanId into kategoriGroup
                    from k in kategoriGroup.DefaultIfEmpty()

                    join l0 in _applicationDbContext.Labs.AsNoTracking()
                        on k.LabId equals l0.LabId into labGroup
                    from l in labGroup.DefaultIfEmpty()

                    select new
                    {
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u != null ? u.FullName : null,

                        a.PemeriksaanLabId,
                        a.NamaPemeriksaan,
                        a.HargaPemeriksaan,
                        a.KodePemeriksaan,
                        a.KategoriPemeriksaanId,

                        NamaKategori = k != null ? k.NamaKategori : null,
                        KodeKategoriPemeriksaan = k != null ? k.KodeKategori : null,

                        LabId = l != null ? (Guid?)l.LabId : null,
                        NamaLab = l != null ? l.NamaLab : null,
                        KodeLab = l != null ? l.KodeKategori : null,

                        a.Keterangan
                    };

                // ======================================================
                // 2) FILTERS
                // ======================================================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.Trim()}%";
                    query = query.Where(x =>
                        (x.NamaPemeriksaan != null && EF.Functions.ILike(x.NamaPemeriksaan, pattern)) ||
                        (x.KodePemeriksaan != null && EF.Functions.ILike(x.KodePemeriksaan, pattern)) ||
                        (x.KodeLab != null && EF.Functions.ILike(x.KodeLab, pattern)) ||
                        (x.KodeKategoriPemeriksaan != null && EF.Functions.ILike(x.KodeKategoriPemeriksaan, pattern)) ||
                        (x.NamaLab != null && EF.Functions.ILike(x.NamaLab, pattern)) ||
                        (x.NamaKategori != null && EF.Functions.ILike(x.NamaKategori, pattern))
                    );
                }

                if (!string.IsNullOrWhiteSpace(kodePemeriksaan))
                {
                    var pattern = $"%{kodePemeriksaan.Trim()}%";
                    query = query.Where(x => x.KodePemeriksaan != null && EF.Functions.ILike(x.KodePemeriksaan, pattern));
                }

                if (!string.IsNullOrWhiteSpace(namaLab))
                {
                    var pattern = $"%{namaLab.Trim()}%";
                    query = query.Where(x => x.NamaLab != null && EF.Functions.ILike(x.NamaLab, pattern));
                }

                if (!string.IsNullOrWhiteSpace(namaKategori))
                {
                    var pattern = $"%{namaKategori.Trim()}%";
                    query = query.Where(x => x.NamaKategori != null && EF.Functions.ILike(x.NamaKategori, pattern));
                }

                if (KategoriPemeriksaanId.HasValue)
                    query = query.Where(x => x.KategoriPemeriksaanId == KategoriPemeriksaanId.Value);

                if (Labid.HasValue)
                    query = query.Where(x => x.LabId == Labid.Value);

                // ✅ FILTER BY ASURANSI: hanya pemeriksaan lab yang dicover asuransi ini
                if (asuransiId.HasValue)
                {
                    var aid = asuransiId.Value;
                    query = query.Where(x =>
                        _applicationDbContext.PemeriksaanLabAsuransis.Any(pa =>
                            (pa.IsDelete == false || pa.IsDelete == null)
                            && pa.AsuransiId == aid
                            && pa.PemeriksaanLabId == x.PemeriksaanLabId
                        )
                    );
                }

                // Date range (lebih sargable: < endExclusive)
                if (startDate.HasValue && endDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    var endExclusive = endDate.Value.Date.AddDays(1);
                    query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                }

                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    switch (periode)
                    {
                        case PeriodeFilter.Today:
                            query = query.Where(x => x.CreateDateTime.Date == today);
                            break;
                        case PeriodeFilter.ThisWeek:
                            query = query.Where(x =>
                                x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                                x.CreateDateTime.Date <= today);
                            break;
                        case PeriodeFilter.LastWeek:
                            query = query.Where(x =>
                                x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                            break;
                        case PeriodeFilter.ThisMonth:
                            query = query.Where(x => x.CreateDateTime.Month == today.Month && x.CreateDateTime.Year == today.Year);
                            break;
                        case PeriodeFilter.LastMonth:
                            var lastMonth = today.AddMonths(-1);
                            query = query.Where(x => x.CreateDateTime.Month == lastMonth.Month && x.CreateDateTime.Year == lastMonth.Year);
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

                // ======================================================
                // 3) SORTING
                // ======================================================
                bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

                query = desc
                    ? orderBy switch
                    {
                        "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                        "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                        "NamaKategori" => query.OrderByDescending(x => x.NamaKategori),
                        "NamaLab" => query.OrderByDescending(x => x.NamaLab),
                        "NamaPemeriksaan" => query.OrderByDescending(x => x.NamaPemeriksaan),
                        _ => query.OrderByDescending(x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                        "CreateByName" => query.OrderBy(x => x.CreateByName),
                        "NamaKategori" => query.OrderBy(x => x.NamaKategori),
                        "NamaLab" => query.OrderBy(x => x.NamaLab),
                        "NamaPemeriksaan" => query.OrderBy(x => x.NamaPemeriksaan),
                        _ => query.OrderBy(x => x.CreateDateTime)
                    };

                // ======================================================
                // 4) PAGING
                // ======================================================
                var totalRows = await query.CountAsync(ct);
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                if (totalRows == 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "No data found",
                        data = new
                        {
                            Rows = Array.Empty<object>(),
                            TotalRows = 0,
                            CurrentPage = page,
                            PerPage = perPage,
                            TotalPages = 0
                        }
                    });
                }

                if (page > totalPages)
                    return NotFound(new { message = "Page not found." });

                var rows = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync(ct);

                // ======================================================
                // 5) COVERAGE ASURANSI (BATCH) + LOOKUP (tanpa Max(uuid))
                // ======================================================
                var pemeriksaanIds = rows.Select(r => r.PemeriksaanLabId).Distinct().ToList();

                var baseCover = _applicationDbContext.PemeriksaanLabAsuransis.AsNoTracking()
                    .Where(x => (x.IsDelete == false || x.IsDelete == null))
                    .Where(x => x.PemeriksaanLabId != null && pemeriksaanIds.Contains(x.PemeriksaanLabId.Value))
                    .Where(x => x.AsuransiId != null); // biar aman dari null

                if (asuransiId.HasValue)
                {
                    var aid = asuransiId.Value;
                    baseCover = baseCover.Where(x => x.AsuransiId == aid);
                }

                // 1) ambil CreateDateTime terbaru per (PemeriksaanLabId, AsuransiId)
                var latestCreateQ =
                    from x in baseCover
                    group x by new { LabId = x.PemeriksaanLabId!.Value, AsuId = x.AsuransiId!.Value } into g
                    select new
                    {
                        PemeriksaanLabId = g.Key.LabId,
                        AsuransiId = g.Key.AsuId,
                        MaxCreate = g.Max(z => z.CreateDateTime)
                    };

                // 2) ambil kandidat row yang timestamp-nya sama dengan MaxCreate (bisa lebih dari 1 kalau tie)
                var candidates = await (
                    from x in baseCover
                    join lc in latestCreateQ
                        on new
                        {
                            LabId = x.PemeriksaanLabId!.Value,
                            AsuId = x.AsuransiId!.Value,
                            Create = x.CreateDateTime
                        }
                        equals new
                        {
                            LabId = lc.PemeriksaanLabId,
                            AsuId = lc.AsuransiId,
                            Create = lc.MaxCreate
                        }
                    join a in _applicationDbContext.Asuransis.AsNoTracking()
                        on x.AsuransiId equals a.AsuransiId
                    select new
                    {
                        PemeriksaanLabId = x.PemeriksaanLabId!.Value,
                        AsuransiId = x.AsuransiId!.Value,
                        NamaAsuransi = a.NamaAsuransi,
                        MarkupTotal = (decimal?)x.MarkupTotal ?? 0m,

                        // PK untuk tie-break di memory
                        Id = x.PemeriksaanLabAsuransiId
                    }
                ).ToListAsync(ct);

                // 3) final coverRows: 1 row per (PemeriksaanLabId, AsuransiId)
                //    tie-break: pilih Id terbesar (hanya untuk kasus CreateDateTime sama)
                var coverRows = candidates
                    .GroupBy(x => new { x.PemeriksaanLabId, x.AsuransiId })
                    .Select(g => g.OrderByDescending(x => x.Id).First())
                    .ToList();

                // lookup per PemeriksaanLabId (untuk list cover)
                var coverByPemeriksaan = coverRows.ToLookup(x => x.PemeriksaanLabId);

                // lookup cepat untuk asuransi terpilih (kalau asuransiId dikirim)
                Dictionary<Guid, (Guid AsuransiId, string? NamaAsuransi, decimal MarkupTotal)> coverForSelectedAsuransi = new();

                if (asuransiId.HasValue)
                {
                    coverForSelectedAsuransi = coverRows.ToDictionary(
                        x => x.PemeriksaanLabId,
                        x => (x.AsuransiId, (string?)x.NamaAsuransi, x.MarkupTotal)
                    );
                }
                // ======================================================
                // 6) BUILD RESULT
                // ======================================================
                var result = rows.Select(r =>
                {
                    var coverList = coverByPemeriksaan[r.PemeriksaanLabId]
                        .Select(x => new { x.AsuransiId, x.NamaAsuransi, x.MarkupTotal })
                        .ToList();
                    return new
                    {
                        r.PemeriksaanLabId,
                        r.NamaPemeriksaan,
                        r.NamaLab,
                        r.NamaKategori,
                        r.KodePemeriksaan,
                        r.HargaPemeriksaan,
                        AsuransiCoverages = coverList
                    };
                });

                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = result,
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }



        //[HttpGet("paged")]
        //public IActionResult Paged(
        //    int page = 1,
        //    int perPage = 10,
        //    Guid? KategoriPemeriksaanId = null,
        //    Guid? Labid = null,
        //    string? search = null,
        //    string? kodePemeriksaan = null,
        //    string? namaLab = null,
        //    string? namaKategori = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
        //    [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
        //    [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        //{
        //    try
        //    {
        //        // 🔹 Query utama (JOIN antar tabel)
        //        var query = (from a in _applicationDbContext.LabPemeriksaans
        //                     join u in _applicationDbContext.UserActives.DefaultIfEmpty()
        //                         on a.CreateBy equals u.UserActiveId
        //                     join k in _applicationDbContext.LabKategoriPemeriksaans
        //                         on a.KategoriPemeriksaanId equals k.KategoriPemeriksaanId into kategoriGroup
        //                     from k in kategoriGroup.DefaultIfEmpty()
        //                     join l in _applicationDbContext.Labs
        //                         on k.LabId equals l.LabId into labGroup
        //                     from l in labGroup.DefaultIfEmpty()
        //                     where a.IsDelete == false || a.IsDelete == null
        //                     select new
        //                     {
        //                         a.CreateDateTime,
        //                         a.CreateBy,
        //                         CreateByName = u.FullName,
        //                         a.PemeriksaanLabId,
        //                         a.NamaPemeriksaan,
        //                         a.HargaPemeriksaan,
        //                         a.KodePemeriksaan,
        //                         a.KategoriPemeriksaanId,
        //                         k.NamaKategori,
        //                         KodeKategoriPemeriksaan= k.KodeKategori,
        //                         l.LabId,
        //                         l.NamaLab,
        //                         KodeLab = l.KodeKategori,
        //                         a.Keterangan
        //                     });

        //        // 🔍 Filter pencarian umum (bebas)
        //        if (!string.IsNullOrWhiteSpace(search))
        //        {
        //            search = $"%{search.ToLower()}%";
        //            query = query.Where(u =>
        //                EF.Functions.ILike(u.NamaPemeriksaan, search) ||
        //                EF.Functions.ILike(u.KodeLab, search) ||
        //                EF.Functions.ILike(u.KodeKategoriPemeriksaan, search)
        //            );
        //        }

        //        // 🔹 Filter berdasarkan dropdown kode pemeriksaan
        //        if (!string.IsNullOrWhiteSpace(kodePemeriksaan))
        //        {
        //            string pattern = $"%{kodePemeriksaan.ToLower()}%";
        //            query = query.Where(u => EF.Functions.ILike(u.KodePemeriksaan, pattern));
        //        }

        //        // 🔹 Filter berdasarkan dropdown Nama Lab
        //        if (!string.IsNullOrWhiteSpace(namaLab))
        //        {
        //            string pattern = $"%{namaLab.ToLower()}%";
        //            query = query.Where(u => EF.Functions.ILike(u.NamaLab, pattern));
        //        }

        //        // 🔹 Filter berdasarkan dropdown Nama Kategori
        //        if (!string.IsNullOrWhiteSpace(namaKategori))
        //        {
        //            string pattern = $"%%{namaKategori.ToLower()}";
        //            query = query.Where(u=> EF.Functions.ILike(u.NamaKategori, pattern));
        //        }

        //        // 🔹 Filter berdasarkan KategoriPemeriksaanId
        //        if (KategoriPemeriksaanId.HasValue)
        //        {
        //            query = query.Where(u => u.KategoriPemeriksaanId == KategoriPemeriksaanId);
        //        }

        //        // 🔹 Filter berdasarkan LabId
        //        if (Labid.HasValue)
        //        {
        //            query = query.Where(u => u.LabId == Labid);
        //        }


        //        // 🔹 Filter berdasarkan tanggal
        //        if (startDate.HasValue && endDate.HasValue)
        //        {
        //            DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
        //            DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

        //            query = query.Where(u =>
        //                u.CreateDateTime >= startUtc &&
        //                u.CreateDateTime <= endUtc);
        //        }

        //        // 🔹 Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
        //        if (periode.HasValue)
        //        {
        //            DateTime today = DateTime.UtcNow.Date;

        //            switch (periode)
        //            {
        //                case PeriodeFilter.Today:
        //                    query = query.Where(u => u.CreateDateTime.Date == today);
        //                    break;
        //                case PeriodeFilter.ThisWeek:
        //                    query = query.Where(u =>
        //                        u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
        //                        u.CreateDateTime.Date <= today);
        //                    break;
        //                case PeriodeFilter.LastWeek:
        //                    query = query.Where(u =>
        //                        u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
        //                        u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
        //                    break;
        //                case PeriodeFilter.ThisMonth:
        //                    query = query.Where(u =>
        //                        u.CreateDateTime.Month == today.Month &&
        //                        u.CreateDateTime.Year == today.Year);
        //                    break;
        //                case PeriodeFilter.LastMonth:
        //                    var lastMonth = today.AddMonths(-1);
        //                    query = query.Where(u =>
        //                        u.CreateDateTime.Month == lastMonth.Month &&
        //                        u.CreateDateTime.Year == lastMonth.Year);
        //                    break;
        //                case PeriodeFilter.ThisYear:
        //                    query = query.Where(u => u.CreateDateTime.Year == today.Year);
        //                    break;
        //                case PeriodeFilter.LastYear:
        //                    query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
        //                    break;
        //                case PeriodeFilter.Last3Months:
        //                    query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
        //                    break;
        //                case PeriodeFilter.Last6Months:
        //                    query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
        //                    break;
        //            }
        //        }

        //        // 🔹 Sorting
        //        query = sortDirection?.ToLower() == "desc"
        //            ? orderBy switch
        //            {
        //                "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
        //                "CreateByName" => query.OrderByDescending(u => u.CreateByName),
        //                "NamaKategori" => query.OrderByDescending(u => u.NamaKategori),
        //                "NamaLab" => query.OrderByDescending(u => u.NamaLab),
        //                "NamaPemeriksaan" => query.OrderByDescending(u => u.NamaPemeriksaan),
        //                _ => query.OrderByDescending(u => u.CreateDateTime)
        //            }
        //            : orderBy switch
        //            {
        //                "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
        //                "CreateByName" => query.OrderBy(u => u.CreateByName),
        //                "NamaKategori" => query.OrderBy(u => u.NamaKategori),
        //                "NamaLab" => query.OrderBy(u => u.NamaLab),
        //                "NamaPemeriksaan" => query.OrderBy(u => u.NamaPemeriksaan),
        //                _ => query.OrderBy(u => u.CreateDateTime)
        //            };

        //        // 🔹 Pagination
        //        var totalRows = query.Count();
        //        var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
        //        var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

        //        if (rows.Count == 0 && page > totalPages)
        //        {
        //            return NotFound(new { message = "Page not found." });
        //        }

        //        // ✅ Return hasil
        //        return Ok(new
        //        {
        //            status = "success",
        //            message = "Data retrieved successfully",
        //            data = new
        //            {
        //                Rows = rows,
        //                TotalRows = totalRows,
        //                CurrentPage = page,
        //                PerPage = perPage,
        //                TotalPages = totalPages
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

    }
}
