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
    public class BarangController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<BarangController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BarangController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BarangController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<string> GenerateKodeBarangAsync(Guid kategoriBarangId, CancellationToken ct = default)
        {
            // 1. Ambil grup kategori dari kategori barang
            var kategori = await (
                from kb in _applicationDbContext.BarangKategoris.AsNoTracking()
                join b in _applicationDbContext.Barangs.AsNoTracking()
                    on kb.KategoriBarangId equals b.KategoriBarangId
                where kb.KategoriBarangId == kategoriBarangId
                select new
                {

                    NamaKategori = kb.NamaKategoriBarang
                }
            ).FirstOrDefaultAsync(ct);

            if (kategori == null)
                throw new Exception("Kategori barang / grup kategori barang tidak ditemukan.");

            // 2. Ambil 3 huruf dari grup kategori
            var prefix = AmbilKodePrefix3Huruf(kategori.NamaKategori);

            // hasil: OBA-, ALK-, dll
            var fullPrefix = $"{prefix}-";

            // 3. Cari kode terakhir dengan prefix yang sama
            var lastKode = await _applicationDbContext.Barangs
                .AsNoTracking()
                .Where(x => x.KodeBarang != null && x.KodeBarang.StartsWith(fullPrefix))
                .OrderByDescending(x => x.KodeBarang)
                .Select(x => x.KodeBarang)
                .FirstOrDefaultAsync(ct);

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastKode))
            {
                // contoh: OBA-0007 -> ambil 0007
                var nomorPart = lastKode.Substring(fullPrefix.Length);

                if (int.TryParse(nomorPart, out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{fullPrefix}{nextNumber:D4}";
        }

        private static string AmbilKodePrefix3Huruf(string? namaGrup)
        {
            if (string.IsNullOrWhiteSpace(namaGrup))
                throw new Exception("Nama grup kategori tidak valid.");

            // ambil huruf saja
            var hurufOnly = new string(namaGrup
                .Where(char.IsLetter)
                .ToArray())
                .ToUpperInvariant();

            if (hurufOnly.Length >= 3)
                return hurufOnly.Substring(0, 3);

            // kalau kurang dari 3 huruf, pad kanan dengan X
            return hurufOnly.PadRight(3, 'X');
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var data = await (
                    from b in _applicationDbContext.Barangs.AsNoTracking()
                    where b.BarangId == id && (b.IsDelete == false || b.IsDelete == null)

                    join kb in _applicationDbContext.BarangKategoris.AsNoTracking()
                        on b.KategoriBarangId equals kb.KategoriBarangId into kbg
                    from kb in kbg.DefaultIfEmpty()

                    join br in _applicationDbContext.Brands.AsNoTracking()
                        on b.BrandId equals br.BrandId into brG
                    from br in brG.DefaultIfEmpty()

                    join kr in _applicationDbContext.KelasResikos.AsNoTracking()
                        on b.KelasResikoId equals kr.KelasResikoId into krG
                    from kr in krG.DefaultIfEmpty()

                    join u in _applicationDbContext.UserActives.AsNoTracking()
                        on b.CreateBy equals u.UserActiveId into uG
                        from u in uG.DefaultIfEmpty()
                    select new
                    {
                        u.CreateBy,
                        CreateByName = u.FullName,
                        u.CreateDateTime,
                        b.BarangId,
                        b.KodeBarang,
                        b.ItemId,
                        b.NamaBarang,
                        b.KategoriBarangId,
                        NamaKategoriBarang = kb != null ? kb.NamaKategoriBarang : null,

                        b.KelasResikoId,
                        NamaKelasResiko = kr != null ? kr.NamaKelasResiko : null,

                        b.BrandId,
                        NamaBrand = br != null ? br.NamaBrand : null,

                        b.Spesifikasi,
                        b.IsPerluResep,
                        b.StokMaximum,
                        b.StokMinimum,
                        b.Keterangan,
                    }
                ).FirstOrDefaultAsync(ct);

                if (data == null)
                    return NotFound(new { message = "Data barang tidak ditemukan." });

                return Ok(new
                {
                    message = "Data barang ditemukan.",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BarangViewModel vm, CancellationToken ct)
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

                //cek duplikasi
                bool isDuplicate = await _applicationDbContext.Barangs
                    .AnyAsync(
                    c => c.NamaBarang.ToLower().Trim()
                    == vm.NamaBarang.ToLower().Trim() 
                    && c.ItemId == vm.ItemId
                    && c.KategoriBarangId == vm.KategoriBarangId
                    && c.BrandId == vm.BrandId
                    && c.KelasResikoId == vm.KelasResikoId
                    && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Barang ini telah tersedia" });
                }

                // validasi kategori barang
                var kategoriExists = await _applicationDbContext.BarangKategoris
                    .AsNoTracking()
                    .AnyAsync(x => x.KategoriBarangId == vm.KategoriBarangId, ct);

                if (!kategoriExists)
                    return NotFound(new { message = "Kategori barang tidak ditemukan." });

                var kodeBarang = await GenerateKodeBarangAsync(vm.KategoriBarangId.Value, ct);


                // **Buat Data Baru**
                var data = new Barang
                {
                    BarangId = Guid.NewGuid(),
                    KodeBarang = kodeBarang,
                    ItemId = vm.ItemId,
                    NamaBarang = vm.NamaBarang,
                    KategoriBarangId = vm.KategoriBarangId,
                    BrandId = vm.BrandId,
                    KelasResikoId = vm.KelasResikoId,
                    Spesifikasi = vm.Spesifikasi,
                    IsPerluResep = vm.IsPerluResep,
                    StokMaximum = vm.StokMaximum,
                    StokMinimum = vm.StokMinimum,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.Barangs.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] BarangViewModel vm, CancellationToken ct)
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
                var data = await _applicationDbContext.Barangs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //cek duplikasi
                bool isDuplicate = await _applicationDbContext.Barangs
                    .AnyAsync(
                    c => c.NamaBarang.ToLower().Trim()
                    == vm.NamaBarang.ToLower().Trim()
                    && c.ItemId == vm.ItemId
                    && c.KategoriBarangId == vm.KategoriBarangId
                    && c.BrandId == vm.BrandId
                    && c.KelasResikoId == vm.KelasResikoId
                    && c.IsDelete == false
                    && c.BarangId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Barang ini telah tersedia" });
                }


                var kategori = await _applicationDbContext.BarangKategoris
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.KategoriBarangId == vm.KategoriBarangId.Value, ct);

                if (kategori == null)
                    return NotFound(new { message = "Kategori barang tidak ditemukan." });

                var kategoriBerubah = data.KategoriBarangId != vm.KategoriBarangId.Value;

                data.ItemId = vm.ItemId;
                data.NamaBarang = vm.NamaBarang;
                data.BrandId = vm.BrandId;
                data.KelasResikoId = vm.KelasResikoId;
                data.Spesifikasi = vm.Spesifikasi;
                data.IsPerluResep = vm.IsPerluResep;
                data.StokMaximum = vm.StokMaximum;
                data.StokMinimum = vm.StokMinimum;
                data.Keterangan = vm.Keterangan;

                // regenerate kode jika kategori berubah
                if (kategoriBerubah)
                {
                    data.KodeBarang = await GenerateKodeBarangAsync(vm.KategoriBarangId.Value, ct);
                }

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.Barangs.Update(data);
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
                var data = await _applicationDbContext.Barangs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Barangs.Update(data);
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
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = from b in _applicationDbContext.Barangs.AsNoTracking()
                        where (b.IsDelete == false || b.IsDelete == null)

                        join kb in _applicationDbContext.BarangKategoris.AsNoTracking()
                            on b.KategoriBarangId equals kb.KategoriBarangId into kbg
                        from kb in kbg.DefaultIfEmpty()

                        join br in _applicationDbContext.Brands.AsNoTracking()
                            on b.BrandId equals br.BrandId into brG
                        from br in brG.DefaultIfEmpty()

                        join kr in _applicationDbContext.KelasResikos.AsNoTracking()
                            on b.KelasResikoId equals kr.KelasResikoId into krG
                        from kr in krG.DefaultIfEmpty()

                        join u in _applicationDbContext.UserActives.AsNoTracking()
                            on b.CreateBy equals u.UserActiveId into uG
                        from u in uG.DefaultIfEmpty()
                        select new
                        {
                            u.CreateBy,
                            CreateByName = u.FullName,
                            u.CreateDateTime,
                            b.BarangId,
                            b.KodeBarang,
                            b.ItemId,
                            b.NamaBarang,
                            b.KategoriBarangId,
                            NamaKategoriBarang = kb != null ? kb.NamaKategoriBarang : null,

                            b.KelasResikoId,
                            NamaKelasResiko = kr != null ? kr.NamaKelasResiko : null,

                            b.BrandId,
                            NamaBrand = br != null ? br.NamaBrand : null,

                            b.Spesifikasi,
                            b.IsPerluResep,
                            b.StokMaximum,
                            b.StokMinimum,
                            b.Keterangan,
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaBarang, search)
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
