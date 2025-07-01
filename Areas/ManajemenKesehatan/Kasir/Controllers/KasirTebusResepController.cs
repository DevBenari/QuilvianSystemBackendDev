using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KasirTebusResepController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KasirTebusResepController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KasirTebusResepController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<KasirTebusResepController> logger,
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
            var query = (from a in _applicationDbContext.KasirTebusReseps
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.KasirTebusResepId,
                             a.NoRegistrasi,
                             a.NoAntrian,
                             a.PaymentMethodId,
                             a.NamaMetode,
                             a.StatusPembayaran,
                             a.TanggalBayar,
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

        [HttpGet("BillingResepTebus/{ResepTebusId}")]
        public async Task<IActionResult> GetBillingResepTebusById(Guid ResepTebusId)
        {
            var query =
                    from ktr in _applicationDbContext.KasirTebusReseps
                    join rt in _applicationDbContext.ResepTebuss on ktr.ResepTebusId equals rt.ResepTebusId
                    join rd in _applicationDbContext.ResepTebusDetails on rt.ResepTebusId equals rd.ResepTebusId
                    join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
                    join mp in _applicationDbContext.MetodePembayarans on ktr.PaymentMethodId equals mp.MetodePembayaranId into paymentMethodGroup
                    from mp in paymentMethodGroup.DefaultIfEmpty()
                    where rt.ResepTebusId == ResepTebusId
                    select new { ktr, rt, rd, o, mp };

            var result = await query.ToListAsync();

            var kasirData = result.GroupBy(x => x.ktr.KasirTebusResepId).Select(group =>
            {
                var firstItem = group.First();
                return new
                {
                    firstItem.ktr?.KasirTebusResepId,
                    firstItem.ktr?.NoRegistrasi,
                    firstItem.ktr?.NoAntrian,
                    firstItem.ktr?.TanggalBayar,
                    firstItem.ktr?.StatusPembayaran,
                    firstItem.ktr?.Keterangan,
                    PaymentMethod = new
                    {
                        firstItem.mp?.MetodePembayaranId,
                        firstItem.mp?.NamaMetode
                    },
                    Resep = new
                    {
                        firstItem.rt.ResepTebusId,
                        NamaPasien = firstItem.rt.NamaPenebus,
                    },
                    DetailObat = group.Select(x => new
                    {
                        x.o.ObatId,
                        x.o.ObatName,
                        x.rd.Qty,
                        x.rd.HargaObat,
                        Subtotal = x.rd.Qty * x.rd.HargaObat
                    }).Distinct().ToList(),
                    TotalTagihan = group.Sum(x => x.rd.Qty * x.rd.HargaObat)
                };
            }).ToList();

            if (!kasirData.Any())
            {
                return NotFound(new { message = "Data billing resep tebus ini tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new { status = "success", data = kasirData.FirstOrDefault() });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query =
                from ktr in _applicationDbContext.KasirTebusReseps
                join rt in _applicationDbContext.ResepTebuss on ktr.ResepTebusId equals rt.ResepTebusId
                join rd in _applicationDbContext.ResepTebusDetails on rt.ResepTebusId equals rd.ResepTebusId
                join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
                join mp in _applicationDbContext.MetodePembayarans on ktr.PaymentMethodId equals mp.MetodePembayaranId into paymentMethodGroup
                from mp in paymentMethodGroup.DefaultIfEmpty()
                where ktr.KasirTebusResepId == id
                select new { ktr, rt, rd, o, mp };

            var result = await query.ToListAsync();

            var kasirData = result.GroupBy(x => x.ktr.KasirTebusResepId).Select(group =>
            {
                var firstItem = group.First();
                return new
                {
                    firstItem.ktr?.KasirTebusResepId,
                    firstItem.ktr?.NoRegistrasi,
                    firstItem.ktr?.NoAntrian,
                    firstItem.ktr?.TanggalBayar,
                    firstItem.ktr?.StatusPembayaran,
                    firstItem.ktr?.Keterangan,
                    PaymentMethod = new
                    {
                        firstItem.mp?.MetodePembayaranId,
                        firstItem.mp?.NamaMetode
                    },
                    Resep = new
                    {
                        firstItem.rt.ResepTebusId,
                        NamaPasien = firstItem.rt.NamaPenebus,
                    },
                    DetailObat = group.Select(x => new
                    {
                        x.o.ObatId,
                        x.o.ObatName,
                        x.rd.Qty,
                        x.rd.HargaObat,
                        Subtotal = x.rd.Qty * x.rd.HargaObat
                    }).Distinct().ToList(),
                    TotalTagihan = group.Sum(x => x.rd.Qty * x.rd.HargaObat)
                };
            }).ToList();

            if (!kasirData.Any())
            {
                return NotFound(new { message = "Data billing resep tebus ini tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new { status = "success", data = kasirData.FirstOrDefault() });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KasirTebusResepViewModel vm)
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
                bool isDuplicate = _applicationDbContext.KasirTebusReseps
                                    .Any(c => c.ResepTebusId == vm.ResepTebusId);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Resep ini telah terbayar" });
                }

                // 🔹 Generate NoRegistrasi unik per hari: TR-ddMMyyyy-XX
                string tglHariIni = DateTime.UtcNow.ToString("ddMMyyyy");
                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

                int countToday = _applicationDbContext.KasirTebusReseps
                   .Count(k => k.TanggalBayar.HasValue && k.TanggalBayar.Value.Date == today.ToDateTime(TimeOnly.MinValue).Date);

                int nextNumber = countToday + 1;
                string noRegistrasi = $"TR-{tglHariIni}-{nextNumber:D2}";

                // Initialize the variable `nextAntrian` to avoid CS0818 error  
                int nextAntrian = 0;
                var resepTebus = _applicationDbContext.ResepTebuss
                    .FirstOrDefault(r => r.ResepTebusId == vm.ResepTebusId);
                if (resepTebus.IsLunas == true && resepTebus.IsLunas != null)
                {
                    var todayRt = DateTime.UtcNow.Date;

                    var lastResep = await _applicationDbContext.KasirTebusReseps
                        .Where(r => r.CreateDateTime.Date == todayRt)
                        .OrderByDescending(r => r.NoAntrian)
                        .FirstOrDefaultAsync();

                    nextAntrian = (int)((lastResep?.NoAntrian ?? 0) + 1);
                }
                else
                {
                    return BadRequest(new { message = "Resep belum lunas, tidak dapat melakukan pembayaran." });
                }

                // **Buat Data Baru**
                var data = new KasirTebusResep
                {
                    KasirTebusResepId = Guid.NewGuid(),
                    ResepTebusId = vm.ResepTebusId,
                    NoRegistrasi = noRegistrasi,
                    NoAntrian = nextAntrian,
                    PaymentMethodId = vm.PaymentMethodId,
                    NamaMetode = vm.NamaMetode,
                    StatusPembayaran = vm.StatusPembayaran,
                    Keterangan = vm.Keterangan,
                    TanggalBayar = DateTime.UtcNow,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };


                // **Simpan ke Database**
                _applicationDbContext.KasirTebusReseps.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] KasirTebusResepViewModel vm)
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
                var data = await _applicationDbContext.KasirTebusReseps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.PaymentMethodId = vm.PaymentMethodId;
                data.NamaMetode = vm.NamaMetode;
                data.StatusPembayaran = vm.StatusPembayaran;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.KasirTebusReseps.Update(data);
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
                var data = await _applicationDbContext.KasirTebusReseps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.KasirTebusReseps.Update(data);
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
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = from a in _applicationDbContext.KasirTebusReseps
                        join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false || a.IsDelete == null
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.KasirTebusResepId,
                            a.NoRegistrasi,
                            a.NoAntrian,
                            a.PaymentMethodId,
                            a.NamaMetode,
                            a.StatusPembayaran,
                            a.TanggalBayar,
                            a.Keterangan,

                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NoRegistrasi, search)
                );
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
                    "NoRegistrasi" => query.OrderByDescending(u => u.NoRegistrasi),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "NoRegistrasi" => query.OrderBy(u => u.NoRegistrasi),
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
