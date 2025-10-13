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
                // 🔹 1️⃣ Ambil semua data utama (Lab Pemeriksaan + Kategori + Lab + User)
                var mainData = await (
                    from a in _applicationDbContext.LabPemeriksaans
                    join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    join k in _applicationDbContext.LabKategoriPemeriksaans on a.KategoriPemeriksaanId equals k.KategoriPemeriksaanId into kategoriGroup
                    from k in kategoriGroup.DefaultIfEmpty()
                    join l in _applicationDbContext.Labs on k.LabId equals l.LabId into labGroup
                    from l in labGroup.DefaultIfEmpty()
                    where a.IsDelete == false || a.IsDelete == null
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
                ).OrderByDescending(x => x.CreateDateTime)
                 .ToListAsync();

                if (!mainData.Any())
                {
                    return NotFound(new { message = "Belum ada data Lab Pemeriksaan yang tersedia. || 404 Not Found" });
                }

                // 🔹 2️⃣ Ambil semua TarifKelas sekaligus (hanya satu query)
                var allTarifKelas = await (
                    from tk in _applicationDbContext.TarifKelass
                    join kl in _applicationDbContext.Kelass on tk.KelasId equals kl.KelasId
                    select new
                    {
                        tk.PemeriksaanLabId,
                        tk.KelasId,
                        tk.TarifKelasId,
                        tk.TarifDokter,
                        tk.TarifRs,
                        tk.TarifJp,
                        tk.TarifBahp,
                        tk.TarifLain,
                        tk.TarifTotal,
                        tk.KSO,
                        NamaKelas = kl.NamaKelas
                    }
                ).ToListAsync();

                // 🔹 3️⃣ Gabungkan TarifKelas dengan Pemeriksaan berdasarkan PemeriksaanLabId
                var result = mainData.Select(r => new
                {
                    r.PemeriksaanLabId,
                    r.NamaPemeriksaan,
                    r.HargaPemeriksaan,
                    r.KodePemeriksaan,
                    r.Keterangan,
                    r.CreateDateTime,
                    r.CreateBy,
                    r.CreateByName,
                    r.KategoriPemeriksaanId,
                    r.NamaKategori,
                    r.KodeKategoriPemeriksaan,
                    r.LabId,
                    r.NamaLab,
                    r.KodeLab,

                    // 🔹 Ambil semua tarif kelas yang cocok
                    TarifKelas = allTarifKelas
                        .Where(t => t.PemeriksaanLabId == r.PemeriksaanLabId)
                        .ToList()
                }).ToList();

                // ✅ Return hasil akhir
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    total = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
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
                var tarifKelasList = await (
                    from tk in _applicationDbContext.TarifKelass
                    join kl in _applicationDbContext.Kelass on tk.KelasId equals kl.KelasId
                    where tk.PemeriksaanLabId == id
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
                        NamaKelas = kl.NamaKelas
                    }
                ).ToListAsync();

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
                    TarifKelas = tarifKelasList
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
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

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
        string? search = null,
        string? kodePemeriksaan = null,
        string? namaLab = null,
        string? namaKategori = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            try
            {
                // ⚡ Ambil semua tarif kelas hanya sekali
                var allTarifKelas = await (
                    from tk in _applicationDbContext.TarifKelass
                    join kl in _applicationDbContext.Kelass on tk.KelasId equals kl.KelasId
                    select new
                    {
                        tk.PemeriksaanLabId,
                        tk.KelasId,
                        tk.TarifKelasId,
                        tk.TarifDokter,
                        tk.TarifRs,
                        tk.TarifJp,
                        tk.TarifBahp,
                        tk.TarifLain,
                        tk.TarifTotal,
                        tk.KSO,
                        NamaKelas = kl.NamaKelas
                    }
                ).ToListAsync();

                // 🔹 Query utama
                var query = from a in _applicationDbContext.LabPemeriksaans
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                                on a.CreateBy equals u.UserActiveId
                            join k in _applicationDbContext.LabKategoriPemeriksaans
                                on a.KategoriPemeriksaanId equals k.KategoriPemeriksaanId into kategoriGroup
                            from k in kategoriGroup.DefaultIfEmpty()
                            join l in _applicationDbContext.Labs
                                on k.LabId equals l.LabId into labGroup
                            from l in labGroup.DefaultIfEmpty()
                            where a.IsDelete == false || a.IsDelete == null
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.PemeriksaanLabId,
                                a.NamaPemeriksaan,
                                a.HargaPemeriksaan,
                                a.KodePemeriksaan,
                                a.KategoriPemeriksaanId,
                                k.NamaKategori,
                                KodeKategoriPemeriksaan = k.KodeKategori,
                                l.LabId,
                                l.NamaLab,
                                KodeLab = l.KodeKategori,
                                a.Keterangan
                            };

                // 🔍 Filter pencarian
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search.ToLower()}%";
                    query = query.Where(u =>
                        EF.Functions.ILike(u.NamaPemeriksaan, pattern) ||
                        EF.Functions.ILike(u.KodeLab, pattern) ||
                        EF.Functions.ILike(u.KodeKategoriPemeriksaan, pattern));
                }

                if (!string.IsNullOrWhiteSpace(kodePemeriksaan))
                {
                    string pattern = $"%{kodePemeriksaan.ToLower()}%";
                    query = query.Where(u => EF.Functions.ILike(u.KodePemeriksaan, pattern));
                }

                if (!string.IsNullOrWhiteSpace(namaLab))
                {
                    string pattern = $"%{namaLab.ToLower()}%";
                    query = query.Where(u => EF.Functions.ILike(u.NamaLab, pattern));
                }

                if (!string.IsNullOrWhiteSpace(namaKategori))
                {
                    string pattern = $"%{namaKategori.ToLower()}%";
                    query = query.Where(u => EF.Functions.ILike(u.NamaKategori, pattern));
                }

                if (KategoriPemeriksaanId.HasValue)
                    query = query.Where(u => u.KategoriPemeriksaanId == KategoriPemeriksaanId);

                if (Labid.HasValue)
                    query = query.Where(u => u.LabId == Labid);

                // 🔹 Filter tanggal
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                    DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    query = query.Where(u =>
                        u.CreateDateTime >= startUtc &&
                        u.CreateDateTime <= endUtc);
                }

                // 🔹 Filter periode
                if (periode.HasValue)
                {
                    DateTime today = DateTime.UtcNow.Date;

                    switch (periode)
                    {
                        case PeriodeFilter.Today:
                            query = query.Where(u => u.CreateDateTime.Date == today);
                            break;
                        case PeriodeFilter.ThisWeek:
                            query = query.Where(u =>
                                u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                                u.CreateDateTime.Date <= today);
                            break;
                        case PeriodeFilter.LastWeek:
                            query = query.Where(u =>
                                u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                            break;
                        case PeriodeFilter.ThisMonth:
                            query = query.Where(u =>
                                u.CreateDateTime.Month == today.Month &&
                                u.CreateDateTime.Year == today.Year);
                            break;
                        case PeriodeFilter.LastMonth:
                            var lastMonth = today.AddMonths(-1);
                            query = query.Where(u =>
                                u.CreateDateTime.Month == lastMonth.Month &&
                                u.CreateDateTime.Year == lastMonth.Year);
                            break;
                        case PeriodeFilter.ThisYear:
                            query = query.Where(u => u.CreateDateTime.Year == today.Year);
                            break;
                        case PeriodeFilter.LastYear:
                            query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                            break;
                        case PeriodeFilter.Last3Months:
                            query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                            break;
                        case PeriodeFilter.Last6Months:
                            query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                            break;
                    }
                }

                // 🔹 Sorting
                query = sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                        "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                        "NamaKategori" => query.OrderByDescending(u => u.NamaKategori),
                        "NamaLab" => query.OrderByDescending(u => u.NamaLab),
                        "NamaPemeriksaan" => query.OrderByDescending(u => u.NamaPemeriksaan),
                        _ => query.OrderByDescending(u => u.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                        "CreateByName" => query.OrderBy(u => u.CreateByName),
                        "NamaKategori" => query.OrderBy(u => u.NamaKategori),
                        "NamaLab" => query.OrderBy(u => u.NamaLab),
                        "NamaPemeriksaan" => query.OrderBy(u => u.NamaPemeriksaan),
                        _ => query.OrderBy(u => u.CreateDateTime)
                    };

                // 🔹 Eksekusi dan pagination
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

                if (rows.Count == 0 && page > totalPages)
                    return NotFound(new { message = "Page not found." });

                // ✅ Gabungkan data tarif kelas di memory (tanpa N+1)
                var result = rows.Select(r => new
                {
                    r.PemeriksaanLabId,
                    r.NamaPemeriksaan,
                    r.NamaLab,
                    r.NamaKategori,
                    r.KodePemeriksaan,
                    r.HargaPemeriksaan,
                    r.CreateDateTime,
                    r.CreateByName,
                    r.Keterangan,
                    TarifKelas = allTarifKelas
                        .Where(t => t.PemeriksaanLabId == r.PemeriksaanLabId)
                        .ToList()
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
