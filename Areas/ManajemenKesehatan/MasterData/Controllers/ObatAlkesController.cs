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
    public class ObatAlkesController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ObatAlkesController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObatAlkesController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatAlkesController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<string> GenerateKodeObatAlkes(Guid? groupObatAlkesId)
        {
            string prefix = "OBT";

            if (groupObatAlkesId.HasValue)
            {
                var group = await _applicationDbContext.GroupObatAlkess
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.GroupObatAlkesId == groupObatAlkesId.Value
                        && x.IsDelete == false);

                if (group != null)
                {
                    var namaGroup = group.NamaGroupObatAlkes?.ToLower() ?? "";

                    if (namaGroup.Contains("alkes"))
                    {
                        prefix = "ALK";
                    }
                    else if (namaGroup.Contains("obat"))
                    {
                        prefix = "OBT";
                    }
                }
            }

            var today = DateTime.Now.ToString("yyyyMMdd");

            var kodeAwal = $"{prefix}{today}";

            var lastKode = await _applicationDbContext.ObatAlkess
                .Where(x =>
                    x.KodeObatAlkes != null
                    && x.KodeObatAlkes.StartsWith(kodeAwal))
                .OrderByDescending(x => x.KodeObatAlkes)
                .Select(x => x.KodeObatAlkes)
                .FirstOrDefaultAsync();

            int nomorUrut = 1;

            if (!string.IsNullOrWhiteSpace(lastKode))
            {
                var nomorText = lastKode.Substring(kodeAwal.Length);

                if (int.TryParse(nomorText, out int nomorTerakhir))
                {
                    nomorUrut = nomorTerakhir + 1;
                }
            }

            return $"{kodeAwal}{nomorUrut.ToString("D4")}";
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.ObatAlkess.Find(id);
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
        public async Task<IActionResult> Create([FromBody] ObatAlkesViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
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

                // Validasi nama obat/alkes
                var namaObatAlkes = vm.NamaObatAlkes?.Trim();

                if (string.IsNullOrWhiteSpace(namaObatAlkes))
                {
                    return BadRequest(new { message = "Nama Obat/Alkes wajib diisi." });
                }

                // Cek duplikasi nama obat/alkes
                bool isDuplicate = await _applicationDbContext.ObatAlkess
                    .AnyAsync(c =>
                        c.NamaObatAlkes.ToLower().Trim() == namaObatAlkes.ToLower()
                        && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Obat/Alkes ini telah tersedia." });
                }

                // Validasi etiket hanya Biru / Putih
                if (!string.IsNullOrWhiteSpace(vm.Etiket))
                {
                    var etiket = vm.Etiket.Trim().ToLower();

                    if (etiket != "biru" && etiket != "putih")
                    {
                        return BadRequest(new { message = "Etiket hanya boleh diisi Biru atau Putih." });
                    }
                }

                // Validasi stock minimal dan maksimal
                if (vm.StockMinimal.HasValue && vm.StockMaximal.HasValue)
                {
                    if (vm.StockMinimal.Value > vm.StockMaximal.Value)
                    {
                        return BadRequest(new { message = "Stock minimal tidak boleh lebih besar dari stock maksimal." });
                    }
                }

                // Validasi KomoditasId harus dari master komoditas dengan IsKomoditas = true
                //if (vm.KomoditasId.HasValue)
                //{
                //    bool komoditasValid = await _applicationDbContext.Komoditas
                //        .AnyAsync(x =>
                //            x.KomoditasId == vm.KomoditasId.Value
                //            && x.IsKomoditas == true
                //            && x.IsDelete == false);

                //    if (!komoditasValid)
                //    {
                //        return BadRequest(new { message = "Komoditas tidak valid atau bukan data komoditas." });
                //    }
                //}

                // Validasi MaterialGroupId harus dari master komoditas dengan IsMaterialGrup = true
                //if (vm.MaterialGroupId.HasValue)
                //{
                //    bool materialGroupValid = await _applicationDbContext.Komoditas
                //        .AnyAsync(x =>
                //            x.KomoditasId == vm.MaterialGroupId.Value
                //            && x.IsMaterialGrup == true
                //            && x.IsDelete == false);

                //    if (!materialGroupValid)
                //    {
                //        return BadRequest(new { message = "Material Group tidak valid atau bukan data material group." });
                //    }
                //}

                // Generate kode otomatis
                string kodeObatAlkes = await GenerateKodeObatAlkes(vm.GroupObatAlkesId);

                // Buat data baru
                var data = new ObatAlkes
                {
                    ObatAlkesId = Guid.NewGuid(),

                    KodeObatAlkes = kodeObatAlkes,
                    GroupObatAlkesId = vm.GroupObatAlkesId,
                    NamaObatAlkes = namaObatAlkes,

                    KategoriTerapeutikId = vm.KategoriTerapeutikId,
                    SubKategoriTerapeutikId = vm.SubKategoriTerapeutikId,
                    JenisObatId = vm.JenisObatId,

                    HighAlert = vm.HighAlert ?? false,

                    SatuanId = vm.SatuanId,
                    Dosis = vm.Dosis,

                    Etiket = string.IsNullOrWhiteSpace(vm.Etiket)
                        ? null
                        : vm.Etiket.Trim(),

                    KodeKFAId = vm.KodeKFAId,

                    BZA = vm.BZA,
                    POV = vm.POV,
                    POAK = vm.POAK,

                    ObatRuteId = vm.ObatRuteId,

                    KekuatanSediaan = vm.KekuatanSediaan,
                    VolumeSediaan = vm.VolumeSediaan,
                    BentukSediaan = vm.BentukSediaan,

                    KomoditasId = vm.KomoditasId,
                    MaterialGroupId = vm.MaterialGroupId,

                    StockMinimal = vm.StockMinimal,
                    StockMaximal = vm.StockMaximal,

                    BentukObatAlkesId = vm.BentukObatAlkesId,
                    GolonganObatAlkesId = vm.GolonganObatAlkesId,

                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.ObatAlkess.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        data = new
                        {
                            data.ObatAlkesId,
                            data.KodeObatAlkes,
                            data.NamaObatAlkes
                        }
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] ObatAlkesViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
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

                // Cari data lama
                var data = await _applicationDbContext.ObatAlkess
                    .FirstOrDefaultAsync(x =>
                        x.ObatAlkesId == id &&
                        (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new { message = "Data Obat/Alkes tidak ditemukan." });
                }

                // Validasi nama
                var namaObatAlkes = vm.NamaObatAlkes?.Trim();

                if (string.IsNullOrWhiteSpace(namaObatAlkes))
                {
                    return BadRequest(new { message = "Nama Obat/Alkes wajib diisi." });
                }

                // Cek duplikasi nama, kecuali data yang sedang diedit
                bool isDuplicate = await _applicationDbContext.ObatAlkess
                    .AnyAsync(c =>
                        c.ObatAlkesId != id &&
                        c.NamaObatAlkes != null &&
                        c.NamaObatAlkes.ToLower().Trim() == namaObatAlkes.ToLower() &&
                        (c.IsDelete == false || c.IsDelete == null));

                if (isDuplicate)
                {
                    return Conflict(new { message = "Obat/Alkes ini telah tersedia." });
                }

                // Validasi etiket hanya Biru / Putih
                if (!string.IsNullOrWhiteSpace(vm.Etiket))
                {
                    var etiket = vm.Etiket.Trim().ToLower();

                    if (etiket != "biru" && etiket != "putih")
                    {
                        return BadRequest(new { message = "Etiket hanya boleh diisi Biru atau Putih." });
                    }
                }

                // Validasi stock minimal dan maksimal
                if (vm.StockMinimal.HasValue && vm.StockMaximal.HasValue)
                {
                    if (vm.StockMinimal.Value > vm.StockMaximal.Value)
                    {
                        return BadRequest(new { message = "Stock minimal tidak boleh lebih besar dari stock maksimal." });
                    }
                }

                // Validasi KomoditasId harus dari master komoditas dengan IsKomoditas = true
                if (vm.KomoditasId.HasValue)
                {
                    bool komoditasValid = await _applicationDbContext.Komoditas
                        .AnyAsync(x =>
                            x.KomoditasId == vm.KomoditasId.Value &&
                            x.IsKomoditas == true &&
                            (x.IsDelete == false || x.IsDelete == null));

                    if (!komoditasValid)
                    {
                        return BadRequest(new { message = "Komoditas tidak valid atau bukan data komoditas." });
                    }
                }

                // Validasi MaterialGroupId harus dari master komoditas dengan IsMaterialGrup = true
                if (vm.MaterialGroupId.HasValue)
                {
                    bool materialGroupValid = await _applicationDbContext.Komoditas
                        .AnyAsync(x =>
                            x.KomoditasId == vm.MaterialGroupId.Value &&
                            x.IsMaterialGrup == true &&
                            (x.IsDelete == false || x.IsDelete == null));

                    if (!materialGroupValid)
                    {
                        return BadRequest(new { message = "Material Group tidak valid atau bukan data material group." });
                    }
                }

                // Update data
                // KodeObatAlkes tidak diubah karena generate otomatis saat create
                data.GroupObatAlkesId = vm.GroupObatAlkesId;
                data.NamaObatAlkes = namaObatAlkes;

                data.KategoriTerapeutikId = vm.KategoriTerapeutikId;
                data.SubKategoriTerapeutikId = vm.SubKategoriTerapeutikId;
                data.JenisObatId = vm.JenisObatId;

                data.HighAlert = vm.HighAlert ?? false;

                data.SatuanId = vm.SatuanId;
                data.Dosis = vm.Dosis;

                data.Etiket = string.IsNullOrWhiteSpace(vm.Etiket)
                    ? null
                    : vm.Etiket.Trim();

                data.KodeKFAId = vm.KodeKFAId;

                data.BZA = vm.BZA;
                data.POV = vm.POV;
                data.POAK = vm.POAK;

                data.ObatRuteId = vm.ObatRuteId;

                data.KekuatanSediaan = vm.KekuatanSediaan;
                data.VolumeSediaan = vm.VolumeSediaan;
                data.BentukSediaan = vm.BentukSediaan;

                data.KomoditasId = vm.KomoditasId;
                data.MaterialGroupId = vm.MaterialGroupId;

                data.StockMinimal = vm.StockMinimal;
                data.StockMaximal = vm.StockMaximal;

                data.BentukObatAlkesId = vm.BentukObatAlkesId;
                data.GolonganObatAlkesId = vm.GolonganObatAlkesId;

                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.ObatAlkess.Update(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Edit Data Berhasil || 200 OK",
                        data = new
                        {
                            data.ObatAlkesId,
                            data.KodeObatAlkes,
                            data.NamaObatAlkes
                        }
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal memperbarui data: {dbEx.InnerException?.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
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
                var data = await _applicationDbContext.ObatAlkess.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.ObatAlkess.Update(data);
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
            Guid? obatAlkesId = null,
            Guid? groupObatAlkesId = null,
            Guid? kategoriTerapeutikId = null,
            Guid? subKategoriTerapeutikId = null,
            Guid? jenisObatId = null,
            Guid? satuanId = null,
            Guid? kodeKFAId = null,
            Guid? obatRuteId = null,
            Guid? komoditasId = null,
            Guid? materialGroupId = null,
            Guid? bentukObatAlkesId = null,
            Guid? golonganObatAlkesId = null,
            bool? highAlert = null,
            string? etiket = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.ObatAlkess
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,

                             a.ObatAlkesId,
                             a.KodeObatAlkes,
                             a.GroupObatAlkesId,
                             a.NamaObatAlkes,

                             a.KategoriTerapeutikId,
                             a.SubKategoriTerapeutikId,
                             a.JenisObatId,

                             a.HighAlert,
                             a.SatuanId,
                             a.Dosis,
                             a.Etiket,
                             a.KodeKFAId,

                             a.BZA,
                             a.POV,
                             a.POAK,

                             a.ObatRuteId,
                             a.KekuatanSediaan,
                             a.VolumeSediaan,
                             a.BentukSediaan,

                             a.KomoditasId,
                             a.MaterialGroupId,

                             a.StockMinimal,
                             a.StockMaximal,

                             a.BentukObatAlkesId,
                             a.GolonganObatAlkesId,

                             a.Keterangan,

                             a.UpdateDateTime,
                             a.UpdateBy,
                             a.DeleteDateTime,
                             a.DeleteBy,
                             a.IsDelete
                         });

            // Filter by Id
            if (obatAlkesId.HasValue)
            {
                query = query.Where(u => u.ObatAlkesId == obatAlkesId.Value);
            }

            if (groupObatAlkesId.HasValue)
            {
                query = query.Where(u => u.GroupObatAlkesId == groupObatAlkesId.Value);
            }

            if (kategoriTerapeutikId.HasValue)
            {
                query = query.Where(u => u.KategoriTerapeutikId == kategoriTerapeutikId.Value);
            }

            if (subKategoriTerapeutikId.HasValue)
            {
                query = query.Where(u => u.SubKategoriTerapeutikId == subKategoriTerapeutikId.Value);
            }

            if (jenisObatId.HasValue)
            {
                query = query.Where(u => u.JenisObatId == jenisObatId.Value);
            }

            if (satuanId.HasValue)
            {
                query = query.Where(u => u.SatuanId == satuanId.Value);
            }

            if (kodeKFAId.HasValue)
            {
                query = query.Where(u => u.KodeKFAId == kodeKFAId.Value);
            }

            if (obatRuteId.HasValue)
            {
                query = query.Where(u => u.ObatRuteId == obatRuteId.Value);
            }

            if (komoditasId.HasValue)
            {
                query = query.Where(u => u.KomoditasId == komoditasId.Value);
            }

            if (materialGroupId.HasValue)
            {
                query = query.Where(u => u.MaterialGroupId == materialGroupId.Value);
            }

            if (bentukObatAlkesId.HasValue)
            {
                query = query.Where(u => u.BentukObatAlkesId == bentukObatAlkesId.Value);
            }

            if (golonganObatAlkesId.HasValue)
            {
                query = query.Where(u => u.GolonganObatAlkesId == golonganObatAlkesId.Value);
            }

            // Filter boolean
            if (highAlert.HasValue)
            {
                query = query.Where(u => u.HighAlert == highAlert.Value);
            }

            // Filter etiket
            if (!string.IsNullOrWhiteSpace(etiket))
            {
                etiket = $"%{etiket.ToLower()}%";

                query = query.Where(u =>
                    u.Etiket != null &&
                    EF.Functions.ILike(u.Etiket, etiket)
                );
            }

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";

                query = query.Where(u =>
                    EF.Functions.ILike(u.KodeObatAlkes, search) ||
                    EF.Functions.ILike(u.NamaObatAlkes, search));
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
