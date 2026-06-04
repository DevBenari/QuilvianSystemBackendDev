using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class PermintaanUnitController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PermintaanUnitController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<PermintaanUnitHub> _hubContext;

        public PermintaanUnitController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PermintaanUnitController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<PermintaanUnitHub> hubContext
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                    tanggal,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                var now = DateTime.Now; // atau DateTime.UtcNow jika kamu mau jam UTC
                var finalDateTime = new DateTime(
                    parsedDate.Year,
                    parsedDate.Month,
                    parsedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Local); // atau Utc jika perlu

                return finalDateTime.ToUniversalTime(); // simpan dalam UTC
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // STEP 1: Ambil data mentah (tanpa GroupBy dulu)
            var rawQuery = await (
                from a in _applicationDbContext.PermintaanUnits
                join u in _applicationDbContext.UserActives
                    on a.CreateBy equals u.UserActiveId into ua
                from u in ua.DefaultIfEmpty()

                join d in _applicationDbContext.DetailPermintaanUnits
                    on a.PermintaanUnitId equals d.PermintaanUnitId into detailJoin
                from d in detailJoin.DefaultIfEmpty()

                join ob in _applicationDbContext.Obats
                    on d.ObatId equals ob.ObatId into obatJoin
                from ob in obatJoin.DefaultIfEmpty()

                join b in _applicationDbContext.BentukObats
                    on ob.BentukObatId equals b.BentukSatuanId into bentukJoin
                from b in bentukJoin.DefaultIfEmpty()

                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    a.PermintaanUnitId,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.UnitId,
                    a.TujuanUnitId,

                    a.JenisPermintaan,
                    a.TglPembuatanPermintaan,
                    a.StatusPermintaan,
                    a.Keterangan,

                    Detail = d == null ? null : new
                    {
                        d.DetailPermintaanUnitId,
                        d.ObatId,
                        NamaObat = ob != null ? ob.ObatName : null,
                        Bentuk = b != null ? b.NamaBentukSatuan : null,
                        StokObat = ob != null ? ob.Stock : 0,
                        StockMinimal = ob != null ? ob.Minimal : 0,
                        StockMaksimal = ob != null ? ob.Maximal : 0,
                        Dosis = ob != null ? ob.TakaranDosis : 0,
                        HTE = ob != null ? ob.HTEPrice : 0,
                        d.QtyPermintaan,
                        d.SatuanItem,
                        d.KategoriItem,
                        d.Keterangan
                    }
                }
            ).AsNoTracking().ToListAsync(); // ✅ eksekusi dulu → pindah ke memory

            // STEP 2: Hitung total rows (di memory)
            var totalRows = rawQuery
                .GroupBy(x => new
                {
                    x.PermintaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    x.JenisPermintaan,
                    x.TglPembuatanPermintaan,
                    x.StatusPermintaan,
                    x.Keterangan
                })
                .Count();

            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // STEP 3: Grouping & paging di memory
            var listdata = rawQuery
                .GroupBy(x => new
                {
                    x.PermintaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    x.JenisPermintaan,
                    x.TglPembuatanPermintaan,
                    x.StatusPermintaan,
                    x.Keterangan
                })
                .Select(g => new
                {
                    g.Key.PermintaanUnitId,
                    g.Key.CreateDateTime,
                    g.Key.CreateBy,
                    g.Key.CreateByName,
                    g.Key.UnitId,
                    g.Key.JenisPermintaan,
                    g.Key.TglPembuatanPermintaan,
                    g.Key.StatusPermintaan,
                    g.Key.Keterangan,

                    DetailPermintaan = g.Where(x => x.Detail != null)
                                        .Select(x => x.Detail)
                                        .ToList()
                })
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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


        //[HttpGet]
        //public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        //{
        //    // Validasi agar page dan perPage minimal bernilai 1
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;

        //    // Query data
        //    var query = (from a in _applicationDbContext.PermintaanUnits
        //                 join u in _applicationDbContext.UserActives
        //                     on a.CreateBy equals u.UserActiveId into ua
        //                 from u in ua.DefaultIfEmpty()

        //                 where a.IsDelete == false || a.IsDelete == null
        //                 select new
        //                 {
        //                     a.CreateDateTime,
        //                     a.CreateBy,
        //                     CreateByName = u.FullName,
        //                     a.PermintaanUnitId,
        //                     a.UnitId,
        //                     a.JenisPermintaan,
        //                     a.TglPembuatanPermintaan,
        //                     a.StatusPermintaan,
        //                     a.Keterangan,

        //                     DetailPermintaan = (from d in _applicationDbContext.DetailPermintaanUnits
        //                                         // Left Join Obat
        //                                         join ob in _applicationDbContext.Obats
        //                                             on d.ObatId equals ob.ObatId into obatJoin
        //                                         from ob in obatJoin.DefaultIfEmpty()

        //                                         // Left Join bentuk obat
        //                                         join b in _applicationDbContext.BentukObats
        //                                         on ob.BentukObatId equals b.BentukObatId into bentukObat
        //                                         from b in bentukObat.DefaultIfEmpty()
        //                                         where d.PermintaanUnitId == a.PermintaanUnitId
        //                                         select new
        //                                         {
        //                                             d.DetailPermintaanUnitId,
        //                                             d.ObatId,
        //                                             NamaObat = ob != null ? ob.ObatName : null,
        //                                             Bentuk = b != null ? b.NamaBentukObat : null,
        //                                             StokObat = ob != null ? ob.Stock : 0,
        //                                             StockMinimal = ob != null ? ob.Minimal : 0,
        //                                             StockMaksimal= ob != null ? ob.Maximal : 0,
        //                                             Dosis = ob != null ? ob.TakaranDosis : 0,
        //                                             HTE = ob != null ? ob.HTEPrice : 0,
        //                                             d.QtyPermintaan,
        //                                             d.SatuanItem,
        //                                             d.KategoriItem,
        //                                             d.Keterangan
        //                                         }).ToList()
        //                 })
        //                 .OrderByDescending(a => a.CreateDateTime);

        //    // Hitung total data sebelum paginasi
        //    var totalRows = query.Count();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    // Ambil data sesuai paging
        //    var listdata = query
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .ToList();

        //    if (!listdata.Any())
        //    {
        //        return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
        //    }

        //    // Return hasil dengan paging info
        //    return Ok(new
        //    {
        //        message = "Berhasil || 200 OK",
        //        data = listdata,
        //        pagination = new
        //        {
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalRows = totalRows,
        //            TotalPages = totalPages
        //        }
        //    });
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Step 1: Ambil data mentah dengan join
            var rawData = await (
                from a in _applicationDbContext.PermintaanUnits
                join u in _applicationDbContext.UserActives
                    on a.CreateBy equals u.UserActiveId into ua
                from u in ua.DefaultIfEmpty()

                join d in _applicationDbContext.DetailPermintaanUnits
                    on a.PermintaanUnitId equals d.PermintaanUnitId into detailJoin
                from d in detailJoin.DefaultIfEmpty()

                join ob in _applicationDbContext.Obats
                    on d.ObatId equals ob.ObatId into obatJoin
                from ob in obatJoin.DefaultIfEmpty()

                join b in _applicationDbContext.BentukObats
                    on ob.BentukObatId equals b.BentukSatuanId into bentukJoin
                from b in bentukJoin.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && a.PermintaanUnitId == id

                select new
                {
                    a.PermintaanUnitId,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.UnitId,
                    a.TujuanUnitId,
                    a.JenisPermintaan,
                    a.TglPembuatanPermintaan,
                    a.StatusPermintaan,
                    a.Keterangan,

                    Detail = d == null ? null : new
                    {
                        d.DetailPermintaanUnitId,
                        d.ObatId,
                        NamaObat = ob != null ? ob.ObatName : null,
                        Bentuk = b != null ? b.NamaBentukSatuan : null,
                        StokObat = ob != null ? ob.Stock : 0,
                        StockMinimal = ob != null ? ob.Minimal : 0,
                        StockMaksimal = ob != null ? ob.Maximal : 0,
                        Dosis = ob != null ? ob.TakaranDosis : 0,
                        HTE = ob != null ? ob.HTEPrice : 0,
                        d.QtyPermintaan,
                        d.SatuanItem,
                        d.KategoriItem,
                        d.Keterangan
                    }
                }
            ).AsNoTracking().ToListAsync(); // ✅ eksekusi dulu → pindah ke memory

            if (!rawData.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            // Step 2: Grouping di memory
            var data = rawData
                .GroupBy(x => new
                {
                    x.PermintaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    x.JenisPermintaan,
                    x.TglPembuatanPermintaan,
                    x.StatusPermintaan,
                    x.Keterangan
                })
                .Select(g => new
                {
                    g.Key.PermintaanUnitId,
                    g.Key.CreateDateTime,
                    g.Key.CreateBy,
                    g.Key.CreateByName,
                    g.Key.UnitId,
                    g.Key.JenisPermintaan,
                    g.Key.TglPembuatanPermintaan,
                    g.Key.StatusPermintaan,
                    g.Key.Keterangan,

                    DetailPermintaan = g.Where(x => x.Detail != null)
                                        .Select(x => x.Detail)
                                        .ToList()
                })
                .FirstOrDefault();

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = data
            });
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermintaanUnitViewModel vm)
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

                // **Buat Data Baru untuk PermintaanUnit**
                var permintaanUnitId = Guid.NewGuid();
                var data = new PermintaanUnit
                {
                    PermintaanUnitId = permintaanUnitId,
                    UnitId = vm.UnitId,
                    TujuanUnitId = vm.TujuanUnitId,
                    JenisPermintaan = vm.JenisPermintaan,
                    TglPembuatanPermintaan = TryParseTanggalToUtc(vm.TglPembuatanPermintaan),
                    StatusPermintaan = vm.StatusPermintaan,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Buat Data Baru untuk DetailPermintaanUnit**
                var detailItems = new List<DetailPermintaanUnit>();
                if (vm.DetailPermintaanUnit != null && vm.DetailPermintaanUnit.Any())
                {
                    foreach (var detailVm in vm.DetailPermintaanUnit)
                    {
                        detailItems.Add(new DetailPermintaanUnit
                        {
                            DetailPermintaanUnitId = Guid.NewGuid(),
                            PermintaanUnitId = permintaanUnitId, // Link to the main record
                            ObatId = detailVm.ObatId,
                            QtyPermintaan = detailVm.QtyPermintaan,
                            SatuanItem = detailVm.SatuanItem,
                            KategoriItem = detailVm.KategoriItem,
                            Keterangan = detailVm.Keterangan,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                        });
                    }
                }

                // **Simpan ke Database**
                await _applicationDbContext.PermintaanUnits.AddAsync(data);
                await _applicationDbContext.DetailPermintaanUnits.AddRangeAsync(detailItems);

                int result = await _applicationDbContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("PermintaanUnitCreated", new
                {
                    Action = "create",
                    PermintaanUnitId = data.PermintaanUnitId
                });

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
        public async Task<IActionResult> Update(Guid id, [FromBody] PermintaanUnitViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cek koneksi ke database
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // Ambil User ID dari JWT Claims
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

                // Cari Data PermintaanUnit
                var data = await _applicationDbContext.PermintaanUnits.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Update Data PermintaanUnit (Parent)
                data.UnitId = vm.UnitId;
                data.TujuanUnitId = vm.TujuanUnitId;
                data.JenisPermintaan = vm.JenisPermintaan;
                data.TglPembuatanPermintaan = TryParseTanggalToUtc(vm.TglPembuatanPermintaan);
                data.StatusPermintaan = vm.StatusPermintaan;
                data.Keterangan = vm.Keterangan;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // **Strategi Baru: Hapus dan Buat Ulang Detail Items**

                // 1. Ambil semua detail yang ada di database untuk PermintaanUnit ini
                var existingDetails = await _applicationDbContext.DetailPermintaanUnits
                                                                  .Where(d => d.PermintaanUnitId == id)
                                                                  .ToListAsync();

                // 2. Hapus semua detail yang ada
                _applicationDbContext.DetailPermintaanUnits.RemoveRange(existingDetails);

                // 3. Buat dan tambahkan detail baru dari ViewModel
                if (vm.DetailPermintaanUnit != null && vm.DetailPermintaanUnit.Any())
                {
                    var newDetails = vm.DetailPermintaanUnit.Select(detailVm => new DetailPermintaanUnit
                    {
                        DetailPermintaanUnitId = Guid.NewGuid(),
                        PermintaanUnitId = id,
                        ObatId = detailVm.ObatId,
                        QtyPermintaan = detailVm.QtyPermintaan,
                        SatuanItem = detailVm.SatuanItem,
                        KategoriItem = detailVm.KategoriItem,
                        Keterangan = detailVm.Keterangan,
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    }).ToList();

                    _applicationDbContext.DetailPermintaanUnits.AddRange(newDetails);
                }

                // Simpan semua perubahan dalam satu transaksi
                int result = await _applicationDbContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("PermintaanUnitUpdate", new
                {
                    Action = "update",
                    PermintaanUnitId = data.PermintaanUnitId
                });
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

                // **Cari Data Induk**
                var data = await _applicationDbContext.PermintaanUnits
                    .FirstOrDefaultAsync(x => x.PermintaanUnitId == id);

                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Cari Detail yang terkait**
                var details = await _applicationDbContext.DetailPermintaanUnits
                    .Where(d => d.PermintaanUnitId == id)
                    .ToListAsync();

                // **Soft Delete (Tandai Data Induk)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;
                _applicationDbContext.PermintaanUnits.Update(data);

                // **Soft Delete Detail juga**
                foreach (var d in details)
                {
                    d.DeleteBy = userActiveId;
                    d.DeleteDateTime = DateTimeOffset.UtcNow;
                    d.IsDelete = true;
                }
                _applicationDbContext.DetailPermintaanUnits.UpdateRange(details);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data induk & detail berhasil dihapus (soft delete) || 200 OK" });
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
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // STEP 1: Query mentah (join semua tabel)
            var rawData = await (
                from a in _applicationDbContext.PermintaanUnits
                join u in _applicationDbContext.UserActives
                    on a.CreateBy equals u.UserActiveId into ua
                from u in ua.DefaultIfEmpty()

                join d in _applicationDbContext.DetailPermintaanUnits
                    on a.PermintaanUnitId equals d.PermintaanUnitId into detailJoin
                from d in detailJoin.DefaultIfEmpty()

                join ob in _applicationDbContext.Obats
                    on d.ObatId equals ob.ObatId into obatJoin
                from ob in obatJoin.DefaultIfEmpty()

                join b in _applicationDbContext.BentukObats
                    on ob.BentukObatId equals b.BentukSatuanId into bentukJoin
                from b in bentukJoin.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                select new
                {
                    a.PermintaanUnitId,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.UnitId,
                    a.TujuanUnitId,

                    a.JenisPermintaan,
                    a.TglPembuatanPermintaan,
                    a.StatusPermintaan,
                    a.Keterangan,

                    Detail = d == null ? null : new
                    {
                        d.DetailPermintaanUnitId,
                        d.ObatId,
                        NamaObat = ob != null ? ob.ObatName : null,
                        Bentuk = b != null ? b.NamaBentukSatuan : null,
                        StokObat = ob != null ? ob.Stock : 0,
                        StockMinimal = ob != null ? ob.Minimal : 0,
                        StockMaksimal = ob != null ? ob.Maximal : 0,
                        Dosis = ob != null ? ob.TakaranDosis : 0,
                        HTE = ob != null ? ob.HTEPrice : 0,
                        d.QtyPermintaan,
                        d.SatuanItem,
                        d.KategoriItem,
                        d.Keterangan
                    }
                }
            ).AsNoTracking().ToListAsync();

            // STEP 2: Filter di memory (karena rawData sudah diambil)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = startDate.Value.Date.ToUniversalTime();
                var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                rawData = rawData.Where(x =>
                    x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc).ToList();
            }

            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                rawData = periode switch
                {
                    PeriodeFilter.Today => rawData.Where(x => x.CreateDateTime.Date == today).ToList(),
                    PeriodeFilter.ThisWeek => rawData.Where(x =>
                        x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                        x.CreateDateTime.Date <= today).ToList(),
                    PeriodeFilter.LastWeek => rawData.Where(x =>
                        x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                        x.CreateDateTime.Date <= today.AddDays(-(int)today.DayOfWeek).AddDays(-1)).ToList(),
                    PeriodeFilter.ThisMonth => rawData.Where(x =>
                        x.CreateDateTime.Month == today.Month && x.CreateDateTime.Year == today.Year).ToList(),
                    PeriodeFilter.LastMonth => rawData.Where(x =>
                        x.CreateDateTime.Month == today.AddMonths(-1).Month &&
                        x.CreateDateTime.Year == today.AddMonths(-1).Year).ToList(),
                    PeriodeFilter.ThisYear => rawData.Where(x => x.CreateDateTime.Year == today.Year).ToList(),
                    PeriodeFilter.LastYear => rawData.Where(x => x.CreateDateTime.Year == today.Year - 1).ToList(),
                    PeriodeFilter.Last3Months => rawData.Where(x => x.CreateDateTime >= today.AddMonths(-3)).ToList(),
                    PeriodeFilter.Last6Months => rawData.Where(x => x.CreateDateTime >= today.AddMonths(-6)).ToList(),
                    _ => rawData
                };
            }

            // STEP 3: Grouping di memory
            var grouped = rawData
                .GroupBy(x => new
                {
                    x.PermintaanUnitId,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.CreateByName,
                    x.UnitId,
                    x.JenisPermintaan,
                    x.TglPembuatanPermintaan,
                    x.StatusPermintaan,
                    x.Keterangan
                })
                .Select(g => new
                {
                    g.Key.PermintaanUnitId,
                    g.Key.CreateDateTime,
                    g.Key.CreateBy,
                    g.Key.CreateByName,
                    g.Key.UnitId,
                    g.Key.JenisPermintaan,
                    g.Key.TglPembuatanPermintaan,
                    g.Key.StatusPermintaan,
                    g.Key.Keterangan,
                    DetailPermintaan = g.Where(x => x.Detail != null).Select(x => x.Detail).ToList()
                });

            // STEP 4: Sorting di memory
            grouped = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateByName" => grouped.OrderByDescending(x => x.CreateByName),
                    "CreateDateTime" => grouped.OrderByDescending(x => x.CreateDateTime),
                    _ => grouped.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateByName" => grouped.OrderBy(x => x.CreateByName),
                    "CreateDateTime" => grouped.OrderBy(x => x.CreateDateTime),
                    _ => grouped.OrderBy(x => x.CreateDateTime)
                };

            // STEP 5: Paging
            var totalRows = grouped.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = grouped
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!rows.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
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
