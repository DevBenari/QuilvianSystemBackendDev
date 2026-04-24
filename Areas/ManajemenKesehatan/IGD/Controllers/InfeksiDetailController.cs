using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class InfeksiDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<InfeksiDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InfeksiDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<InfeksiDetailController> logger,
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
            var query =
                from d in _applicationDbContext.InfeksiDetails
                join u in _applicationDbContext.UserActives
                    on d.CreateBy equals u.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus
                    on d.PasienId equals p.PendaftaranPasienBaruId into pasienGroup
                from p in pasienGroup.DefaultIfEmpty()

                    // 🔹 Ambil data vital sign terbaru per kunjungan
                let latestVital = (
                    from v in _applicationDbContext.VitalSigns
                    where v.KunjunganId == d.KunjunganId
                    orderby v.CreateDateTime descending
                    select v
                ).FirstOrDefault()

                where d.IsDelete == false || d.IsDelete == null
                orderby d.CreateDateTime descending

                select new
                {
                    d.DetailInfeksiId,
                    d.InfeksiId,
                    d.KunjunganId,
                    d.PasienId,
                    NamaPasien = p.NamaLengkap ?? null,
                    d.HariKe,
                    d.LokasiReaksi,
                    d.TglMulaiReaksi,
                    d.TglAkhirReaksi,
                    d.Nyeri,
                    d.Merah,
                    d.Bengkak,
                    d.PUS,
                    d.Menggigil,
                    // ✅ Ambil suhu dari vital sign terbaru
                    Suhu = latestVital != null ? latestVital.Suhu : null,
                    d.IsDemam,
                    d.Drainase,
                    d.Perforasi,
                    d.Fistula,
                    d.NyeriSupraPublik,
                    d.NyeriSaatBerkemih,
                    d.PasangDCKe,
                    d.AnyangAnyangan,
                    d.Gatal,
                    d.Keterangan,
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // 🔹 Cari data utama berdasarkan ID
                var detail = await (
                    from d in _applicationDbContext.InfeksiDetails
                    join u in _applicationDbContext.UserActives
                        on d.CreateBy equals u.UserActiveId into userGroup
                    from u in userGroup.DefaultIfEmpty()

                    join p in _applicationDbContext.PendaftaranPasienBarus
                        on d.PasienId equals p.PendaftaranPasienBaruId into pasienGroup
                    from p in pasienGroup.DefaultIfEmpty()

                        // 🔹 Ambil vital sign terbaru berdasarkan KunjunganId
                    let latestVital = (
                        from v in _applicationDbContext.VitalSigns
                        where v.KunjunganId == d.KunjunganId
                        orderby v.CreateDateTime descending
                        select v
                    ).FirstOrDefault()

                    where d.DetailInfeksiId == id && (d.IsDelete == false || d.IsDelete == null)

                    select new
                    {
                        d.DetailInfeksiId,
                        d.InfeksiId,
                        d.KunjunganId,
                        d.PasienId,
                        NamaPasien = p.NamaLengkap ?? "-",
                        d.HariKe,
                        d.LokasiReaksi,
                        d.TglMulaiReaksi,
                        d.TglAkhirReaksi,
                        d.Nyeri,
                        d.Merah,
                        d.Bengkak,
                        d.PUS,
                        d.Menggigil,
                        Suhu = latestVital != null ? latestVital.Suhu : null, // 🔥 suhu terbaru
                        d.IsDemam,
                        d.Drainase,
                        d.Perforasi,
                        d.Fistula,
                        d.NyeriSupraPublik,
                        d.NyeriSaatBerkemih,
                        d.PasangDCKe,
                        d.AnyangAnyangan,
                        d.Gatal,
                        d.Keterangan,
                        d.CreateDateTime,
                        d.CreateBy,
                        CreateByName = u.FullName
                    }
                ).FirstOrDefaultAsync();

                if (detail == null)
                {
                    return NotFound(new { message = "Data Detail Infeksi tidak ditemukan. || 404 Not Found" });
                }

                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    data = detail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InfeksiDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif dari JWT
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

                // ======================================================
                // 🔹 Hitung HariKe berdasarkan KunjunganId & InfeksiId
                // ======================================================
                var hariKe = await _applicationDbContext.InfeksiDetails
                    .CountAsync(x => x.KunjunganId == vm.KunjunganId && 
                    x.InfeksiId == vm.InfeksiId) + 1;

                // ======================================================
                // 🔹 Hitung PasangDCKe berdasarkan PasienId
                // ======================================================
                var pasangDCKe = await _applicationDbContext.InfeksiDetails
                    .CountAsync(x => x.PasienId == vm.PasienId) + 1;

                // ======================================================
                // ✅ Buat data baru
                // ======================================================
                var data = new InfeksiDetail
                {
                    DetailInfeksiId = Guid.NewGuid(),
                    InfeksiId = vm.InfeksiId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    HariKe = hariKe,
                    LokasiReaksi = vm.LokasiReaksi,
                    TglMulaiReaksi = vm.TglMulaiReaksi,
                    TglAkhirReaksi = vm.TglAkhirReaksi,
                    Nyeri = vm.Nyeri,
                    Merah = vm.Merah,
                    Bengkak = vm.Bengkak,
                    PUS = vm.PUS,
                    Menggigil = vm.Menggigil,
                    IsDemam = vm.IsDemam,
                    Drainase = vm.Drainase,
                    Perforasi = vm.Perforasi,
                    Fistula = vm.Fistula,
                    NyeriSupraPublik = vm.NyeriSupraPublik,
                    NyeriSaatBerkemih = vm.NyeriSaatBerkemih,
                    PasangDCKe = $"DC-{pasangDCKe}", // otomatis format DC-1, DC-2, dst
                    AnyangAnyangan = vm.AnyangAnyangan,
                    Gatal = vm.Gatal,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.InfeksiDetails.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Detail Infeksi Berhasil || 201 Created",
                        data = new
                        {
                            data.DetailInfeksiId,
                            data.KunjunganId,
                            data.PasienId,
                            data.HariKe,
                            data.PasangDCKe,
                        }
                    });
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InfeksiDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif dari JWT
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

                // ✅ Cari data yang akan diperbarui
                var existingData = await _applicationDbContext.InfeksiDetails
                    .FirstOrDefaultAsync(x => x.DetailInfeksiId == id && (x.IsDelete == false || x.IsDelete == null));

                if (existingData == null)
                {
                    return NotFound(new { message = "Data Detail Infeksi tidak ditemukan. || 404 Not Found" });
                }


                // ======================================================
                // ✅ Update data
                // ======================================================
                existingData.InfeksiId = vm.InfeksiId;
                existingData.KunjunganId = vm.KunjunganId;
                existingData.PasienId = vm.PasienId;
                existingData.LokasiReaksi = vm.LokasiReaksi;
                existingData.TglMulaiReaksi = vm.TglMulaiReaksi;
                existingData.TglAkhirReaksi = vm.TglAkhirReaksi;
                existingData.Nyeri = vm.Nyeri;
                existingData.Merah = vm.Merah;
                existingData.Bengkak = vm.Bengkak;
                existingData.PUS = vm.PUS;
                existingData.Menggigil = vm.Menggigil;
                existingData.IsDemam = vm.IsDemam;
                existingData.Drainase = vm.Drainase;
                existingData.Perforasi = vm.Perforasi;
                existingData.Fistula = vm.Fistula;
                existingData.NyeriSupraPublik = vm.NyeriSupraPublik;
                existingData.NyeriSaatBerkemih = vm.NyeriSaatBerkemih;
                existingData.AnyangAnyangan = vm.AnyangAnyangan;
                existingData.Gatal = vm.Gatal;
                existingData.Keterangan = vm.Keterangan;

                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.InfeksiDetails.Update(existingData);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Detail Infeksi Berhasil || 200 OK",
                        data = new
                        {
                            existingData.DetailInfeksiId,
                            existingData.KunjunganId,
                            existingData.PasienId,
                            existingData.UpdateDateTime
                        }
                    });
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
                var data = await _applicationDbContext.InfeksiDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.InfeksiDetails.Update(data);
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
        public IActionResult Paged(
        int page = 1,
        int perPage = 10,
        Guid? kunjunganId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            // Query data
            var query =
                from d in _applicationDbContext.InfeksiDetails
                join u in _applicationDbContext.UserActives
                    on d.CreateBy equals u.UserActiveId into userGroup
                from u in userGroup.DefaultIfEmpty()

                join p in _applicationDbContext.PendaftaranPasienBarus
                    on d.PasienId equals p.PendaftaranPasienBaruId into pasienGroup
                from p in pasienGroup.DefaultIfEmpty()

                    // 🔹 Ambil data vital sign terbaru per kunjungan
                let latestVital = (
                    from v in _applicationDbContext.VitalSigns
                    where v.KunjunganId == d.KunjunganId
                    orderby v.CreateDateTime descending
                    select v
                ).FirstOrDefault()

                where d.IsDelete == false || d.IsDelete == null
                orderby d.CreateDateTime descending

                select new
                {
                    d.DetailInfeksiId,
                    d.InfeksiId,
                    d.KunjunganId,
                    d.PasienId,
                    NamaPasien = p.NamaLengkap ?? null,
                    d.HariKe,
                    d.LokasiReaksi,
                    d.TglMulaiReaksi,
                    d.TglAkhirReaksi,
                    d.Nyeri,
                    d.Merah,
                    d.Bengkak,
                    d.PUS,
                    d.Menggigil,
                    // ✅ Ambil suhu dari vital sign terbaru
                    Suhu = latestVital != null ? latestVital.Suhu : null,
                    d.IsDemam,
                    d.Drainase,
                    d.Perforasi,
                    d.Fistula,
                    d.NyeriSupraPublik,
                    d.NyeriSaatBerkemih,
                    d.PasangDCKe,
                    d.AnyangAnyangan,
                    d.Gatal,
                    d.Keterangan,
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName
                };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            //filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
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
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
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

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }

    }
}
