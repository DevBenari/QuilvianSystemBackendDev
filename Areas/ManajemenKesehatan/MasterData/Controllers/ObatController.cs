using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using System.Linq;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ObatController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ObatController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObatController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllObat(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // ===============================
            // 1️⃣ Ambil query dasar
            // ===============================
            var baseQuery = _applicationDbContext.Obats
                .AsNoTracking()
                .Where(o => !o.IsDelete);

            var totalRows = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // =======================================================
            // 2️⃣ Paging — hanya ambil data yang dibutuhkan
            // =======================================================
            var obatList = await baseQuery
                .OrderByDescending(o => o.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!obatList.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            // Ambil semua ID
            var obatIds = obatList.Select(o => o.ObatId).ToList();
            var userIds = obatList.Select(o => o.CreateBy).Distinct().ToList();

            var bentukIds = obatList.Select(o => o.BentukObatId).Distinct().ToList();
            var satuanIds = obatList.Select(o => o.SatuanId).Distinct().ToList();
            var ruteIds = obatList.Select(o => o.ObatRuteId).Distinct().ToList();

            // =======================================================
            // 3️⃣ Ambil lookup secara batch
            // =======================================================

            var userLookup = await _applicationDbContext.UserActives
                .Where(u => userIds.Contains(u.UserActiveId))
                .Select(u => new { u.UserActiveId, u.FullName })
                .ToDictionaryAsync(x => x.UserActiveId, x => x.FullName);

            var bentukLookup = await _applicationDbContext.BentukObats
                .Where(b => bentukIds.Contains(b.BentukSatuanId))
                .Select(b => new { b.BentukSatuanId, b.NamaBentukSatuan })
                .ToDictionaryAsync(x => x.BentukSatuanId, x => x.NamaBentukSatuan);

            var satuanLookup = await _applicationDbContext.Satuans
                .Where(s => satuanIds.Contains(s.SatuanId))
                .Select(s => new { s.SatuanId, s.NamaSatuan })
                .ToDictionaryAsync(x => x.SatuanId, x => x.NamaSatuan);

            var ruteLookup = await _applicationDbContext.ObatRutes
                .Where(r => ruteIds.Contains(r.RuteObatId))
                .Select(r => new { r.RuteObatId, r.RuteObat })
                .ToDictionaryAsync(x => x.RuteObatId, x => x.RuteObat);

            var kandunganList = await (
                from ok in _applicationDbContext.ObatKandungans
                join k in _applicationDbContext.Kandungans on ok.KandunganId equals k.KandunganId
                where obatIds.Contains(ok.ObatId)
                select new { ok.ObatId, k.NamaKandungan }
            ).ToListAsync();

            var kandunganLookup = kandunganList
                .GroupBy(x => x.ObatId)
                .ToDictionary(g => g.Key, g => g.Select(v => v.NamaKandungan).Distinct().ToList());

            // Asuransi lookup
            var asuransiList = await (
                from oa in _applicationDbContext.ObatAsuransis
                join a in _applicationDbContext.Asuransis on oa.AsuransiId equals a.AsuransiId
                where obatIds.Contains(oa.ObatId)
                select new { oa.ObatId, a.NamaAsuransi }
            ).ToListAsync();

            var asuransiLookup = asuransiList
                .GroupBy(x => x.ObatId)
                .ToDictionary(g => g.Key, g => g.Select(v => v.NamaAsuransi).Distinct().ToList());

            // =======================================================
            // 4️⃣ Gabungkan data
            // =======================================================
            var result = obatList.Select(o => new
            {
                o.CreateDateTime,
                o.CreateBy,
                CreateByName = userLookup.ContainsKey(o.CreateBy) ? userLookup[o.CreateBy] : null,

                o.ObatId,
                o.ObatCode,
                o.ObatName,
                o.HNAPrice,
                o.HTEPrice,
                o.Stock,
                o.IsActive,
                o.Note,

                // Bentuk Obat
                o.BentukObatId,
                BentukObatName = o.BentukObatId.HasValue && bentukLookup.ContainsKey(o.BentukObatId.Value)
                        ? bentukLookup[o.BentukObatId.Value]
                        : null,

                // Satuan Obat
                o.SatuanId,
                SatuanName = o.SatuanId.HasValue && satuanLookup.ContainsKey(o.SatuanId.Value)
                    ? satuanLookup[o.SatuanId.Value]
                    : null,

                // Kandungan
                KandunganNames = kandunganLookup.ContainsKey(o.ObatId)
                        ? kandunganLookup[o.ObatId]
                        : new List<string>(),

                // Asuransi
                AsuransiNames = asuransiLookup.ContainsKey(o.ObatId)
                        ? asuransiLookup[o.ObatId]
                        : new List<string>(),

                // Fields tambahan
                o.Minimal,
                o.Maximal,
                o.Farmakologi,
                o.Peringatan,
                o.Indikasi,
                o.Kontraindikasi,
                o.CaraKerja,
                o.InteraksiObat,
                o.Dosis,
                o.TakaranDosis,
                o.JumlahSatuan,
                o.Kategori,
                o.ItemId,

                // Rute Obat
                o.ObatRuteId,
                RuteObatNama = o.ObatRuteId.HasValue && ruteLookup.ContainsKey(o.ObatRuteId.Value)
                        ? ruteLookup[o.ObatRuteId.Value]
                        : null,

                o.KategoriObat,
                o.IsControlled
            });


            // =======================================================
            // 5️⃣ Return final response
            // =======================================================
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



        [HttpGet("{id}")]
        public async Task<IActionResult> GetObatById(Guid id)
        {
            // =======================
            // 1️⃣ Ambil Obat Utama
            // =======================
            var obat = await _applicationDbContext.Obats
                .Where(o => o.ObatId == id && !o.IsDelete)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (obat == null)
                return NotFound(new { message = "Obat tidak ditemukan || 404 Not Found" });

            // =======================
            // 2️⃣ Ambil semua relasi dalam batch
            // =======================

            // Pembuat data
            var userName = await _applicationDbContext.UserActives
                .Where(u => u.UserActiveId == obat.CreateBy)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            // Bentuk obat
            var bentukObatName = await _applicationDbContext.BentukObats
                .Where(b => b.BentukSatuanId == obat.BentukObatId)
                .Select(b => b.NamaBentukSatuan)
                .FirstOrDefaultAsync();

            // Satuan obat
            var satuanName = await _applicationDbContext.Satuans
                .Where(s => s.SatuanId == obat.SatuanId)
                .Select(s => s.NamaSatuan)
                .FirstOrDefaultAsync();

            // Rute obat
            var ruteObatName = await _applicationDbContext.ObatRutes
                .Where(r => r.RuteObatId == obat.ObatRuteId)
                .Select(r => r.RuteObat)
                .FirstOrDefaultAsync();

            // Kandungan list
            var kandunganList = await (
                from ok in _applicationDbContext.ObatKandungans
                join k in _applicationDbContext.Kandungans on ok.KandunganId equals k.KandunganId
                where ok.ObatId == id
                select k.NamaKandungan
            ).Distinct().ToListAsync();

            // Asuransi list
            var asuransiList = await (
                from oa in _applicationDbContext.ObatAsuransis
                join a in _applicationDbContext.Asuransis on oa.AsuransiId equals a.AsuransiId
                where oa.ObatId == id
                select a.NamaAsuransi
            ).Distinct().ToListAsync();

            // =======================
            // 3️⃣ Gabungkan hasil
            // =======================
            var result = new
            {
                obat.CreateDateTime,
                obat.CreateBy,
                CreateByName = userName,
                obat.ObatId,
                obat.ObatCode,
                obat.ObatName,
                obat.HNAPrice,
                obat.HTEPrice,
                obat.Stock,
                obat.IsActive,
                obat.Note,

                obat.BentukObatId,
                BentukObatName = bentukObatName,

                obat.SatuanId,
                SatuanName = satuanName,

                KandunganNames = kandunganList,
                AsuransiNames = asuransiList,

                // Info tambahan
                obat.Minimal,
                obat.Maximal,
                obat.Farmakologi,
                obat.Peringatan,
                obat.Indikasi,
                obat.Kontraindikasi,
                obat.CaraKerja,
                obat.InteraksiObat,
                obat.Dosis,
                obat.TakaranDosis,
                obat.JumlahSatuan,
                obat.Kategori,
                obat.ItemId,

                obat.ObatRuteId,
                RuteObatNama = ruteObatName,

                obat.KategoriObat,
                obat.IsControlled
            };

            return Ok(new
            {
                message = "Berhasil mengambil data obat || 200 OK",
                data = result
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateObat([FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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
                var setDateNow = dateNow.ToString("yyMMdd");
                var start = dateNow.Date;
                var end = start.AddDays(1);

                var lastCode = await _applicationDbContext.Obats
                    .AsNoTracking()
                    .Where(o => o.CreateDateTime >= start && o.CreateDateTime < end)
                    .Where(o => o.ObatCode != null && o.ObatCode.StartsWith("OBT"))
                    .OrderByDescending(o => o.CreateDateTime)
                    .Select(o => o.ObatCode) // cukup ambil kodenya aja, lebih ringan
                    .FirstOrDefaultAsync();

                string kode;
                if (lastCode == null)
                {
                    kode = $"OBT{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"OBT{setDateNow}0001";
                    }
                    else
                    {
                        var lastNumber = int.Parse(lastCode.Substring(9));
                        kode = $"OBT{setDateNow}{(lastNumber + 1).ToString("D4")}";
                    }
                }

                bool isDuplicate = _applicationDbContext.Obats
                    .Any(c => c.ObatName.ToLower() == vm.ObatName.ToLower() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                var data = new Obat
                {
                    ObatId = Guid.NewGuid(),
                    CreateDateTime = dateNow.Date,
                    CreateBy = userActiveId,
                    ObatCode = kode,
                    ObatName = vm.ObatName,
                    BentukObatId = vm.BentukSatuanId,
                    HTEPrice = vm.HTEPrice,
                    HNAPrice = vm.HNAPrice,
                    Stock = vm.Stock,
                    IsActive = vm.IsActive,
                    Minimal = vm.Minimal,  // Tambahkan properti baru
                    Maximal = vm.Maximal,
                    Farmakologi = vm.Farmakologi,
                    Peringatan = vm.Peringatan,
                    Indikasi = vm.Indikasi,
                    Kontraindikasi = vm.Kontraindikasi,
                    CaraKerja = vm.CaraKerja,
                    InteraksiObat = vm.InteraksiObat,
                    Dosis = vm.Dosis,
                    TakaranDosis = vm.TakaranDosis,
                    SatuanId = vm.SatuanId,
                    JumlahSatuan = vm.JumlahSatuan,
                    Note = vm.Note,
                    Kategori = vm.Kategori,
                    ItemId = vm.ItemId,
                    ObatRuteId = vm.ObatRuteId,
                    KategoriObat = vm.KategoriObat,
                    IsControlled = vm.IsControlled,
                };

                _applicationDbContext.Obats.Add(data);
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

        [HttpPost("Alkes")]
        public async Task<IActionResult> CreateAlkes([FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                // set code
                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd");
                var start = dateNow.Date;
                var end = start.AddDays(1);

                var lastCode = await _applicationDbContext.Obats
                    .AsNoTracking()
                    .Where(o => o.CreateDateTime >= start && o.CreateDateTime < end)
                    .Where(o => o.ObatCode != null && o.ObatCode.StartsWith("ALK"))
                    .OrderByDescending(o => o.CreateDateTime)
                    .Select(o => o.ObatCode) // cukup ambil kodenya aja, lebih ringan
                    .FirstOrDefaultAsync();

                string kode;
                if (lastCode == null)
                {
                    kode = $"ALK{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"ALK{setDateNow}0001";
                    }
                    else
                    {
                        var lastNumber = int.Parse(lastCode.Substring(9));
                        kode = $"ALK{setDateNow}{(lastNumber + 1).ToString("D4")}";
                    }
                }

                bool isDuplicate = _applicationDbContext.Obats
                    .Any(c => c.ObatName.ToLower() == vm.ObatName.ToLower() && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                var data = new Obat
                {
                    ObatId = Guid.NewGuid(),
                    CreateDateTime = dateNow.Date,
                    CreateBy = userActiveId,
                    ObatCode = kode,
                    ObatName = vm.ObatName,
                    BentukObatId = vm.BentukSatuanId,
                    HTEPrice = vm.HTEPrice,
                    HNAPrice = vm.HNAPrice,
                    Stock = vm.Stock,
                    IsActive = vm.IsActive,
                    Minimal = vm.Minimal,  // Tambahkan properti baru
                    Maximal = vm.Maximal,
                    Farmakologi = vm.Farmakologi,
                    Peringatan = vm.Peringatan,
                    Indikasi = vm.Indikasi,
                    Kontraindikasi = vm.Kontraindikasi,
                    CaraKerja = vm.CaraKerja,
                    InteraksiObat = vm.InteraksiObat,
                    Dosis = vm.Dosis,
                    TakaranDosis = vm.TakaranDosis,
                    JumlahSatuan = vm.JumlahSatuan,
                    SatuanId = vm.SatuanId,
                    Note = vm.Note,
                    Kategori = vm.Kategori,
                    ItemId = vm.ItemId,
                    ObatRuteId = vm.ObatRuteId,
                    KategoriObat = vm.KategoriObat,
                    IsControlled = vm.IsControlled,
                };

                _applicationDbContext.Obats.Add(data);
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
        public async Task<IActionResult> UpdateObat(Guid id, [FromBody] ObatViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                var data = await _applicationDbContext.Obats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                bool isDuplicate = await _applicationDbContext.Obats
                    .AnyAsync(c => c.ObatName.ToLower() == vm.ObatName.ToLower() && c.ObatId != id && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                data.ObatName = vm.ObatName;
                data.BentukObatId = vm.BentukSatuanId;
                data.HTEPrice = vm.HTEPrice;
                data.Stock = vm.Stock;
                data.IsActive = vm.IsActive;
                data.Note = vm.Note;
                data.JumlahSatuan = vm.JumlahSatuan;

                data.Minimal = vm.Minimal;  // Tambahkan properti baru
                data.Maximal = vm.Maximal;
                data.Farmakologi = vm.Farmakologi;
                data.Peringatan = vm.Peringatan;
                data.Indikasi = vm.Indikasi;
                data.Kontraindikasi = vm.Kontraindikasi;
                data.CaraKerja = vm.CaraKerja;
                data.InteraksiObat = vm.InteraksiObat;
                data.TakaranDosis = vm.TakaranDosis;
                data.Kategori = vm.Kategori;
                data.ItemId = vm.ItemId;
                data.ObatRuteId = vm.ObatRuteId;
                data.KategoriObat = vm.KategoriObat;
                data.IsControlled = vm.IsControlled;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.Obats.Update(data);
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
        public async Task<IActionResult> DeleteObat(Guid id)
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

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

                var data = await _applicationDbContext.Obats.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Obats.Update(data);
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
        public async Task<IActionResult> PagedObat(
            int page = 1,
            int perPage = 10,
            string? Nama = null,
            string? kode = null,
            Guid? obatId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc")
        {
            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // 1. Menggunakan Eager Loading dengan Include untuk relasi 1-ke-1 atau 1-ke-banyak sederhana
                var query = _applicationDbContext.Obats
                    .AsNoTracking()
                    .Where(a => !a.IsDelete);

                // Filter berdasarkan ID obat
                if (obatId.HasValue && obatId != Guid.Empty)
                {
                    query = query.Where(x => x.ObatId == obatId);
                }

                //Filter Berdasarkan Kode
                if (!string.IsNullOrWhiteSpace(kode))
                {
                    var s = kode.Trim().ToLower();
                    query = query.Where(u =>
                        u.ObatCode.ToLower().Contains(s)   // Filter berdasarkan kode obat yang mengandung string 's'
                    );
                }
                //if (!string.IsNullOrWhiteSpace(search))
                //{
                //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                //    query = query.Where(u =>
                //        EF.Functions.ILike(u.ObatName, search) ||
                //        EF.Functions.ILike(u.ObatCode, search) 
                //    );
                //}
                // Lakukan join untuk mendapatkan semua data yang diperlukan
                var joinedQuery = from a in query
                                  join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into ua
                                  from u in ua.DefaultIfEmpty()
                                  join bo in _applicationDbContext.BentukObats on a.BentukObatId equals bo.BentukSatuanId into boJoin
                                  from bo in boJoin.DefaultIfEmpty()
                                  join s in _applicationDbContext.Satuans on a.SatuanId equals s.SatuanId into sJoin
                                  from s in sJoin.DefaultIfEmpty()

                                  join or in _applicationDbContext.ObatRutes.AsNoTracking()
                                  on a.ObatRuteId equals or.RuteObatId into orJoin
                                  from or in orJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      a.ObatId,
                                      a.ObatCode,
                                      a.ObatName,
                                      a.HTEPrice,
                                      a.HNAPrice,
                                      a.Stock,
                                      a.IsActive,
                                      a.Note,
                                      a.Minimal,
                                      a.Maximal,
                                      a.Farmakologi,
                                      a.Peringatan,
                                      a.Indikasi,
                                      a.Kontraindikasi,
                                      a.CaraKerja,
                                      a.InteraksiObat,
                                      a.Dosis,
                                      a.TakaranDosis,
                                      a.JumlahSatuan,
                                      a.Kategori,
                                      a.CreateDateTime,
                                      CreateByName = u.FullName,
                                      a.BentukObatId,
                                      BentukObatName = bo.NamaBentukSatuan,
                                      a.SatuanId,
                                      SatuanName = s.NamaSatuan,
                                      a.ItemId,
                                      a.ObatRuteId,
                                      NamaObatRute=or.RuteObat,
                                      a.KategoriObat,
                                      a.IsControlled
                                  };

                // 2. Ambil data Kandungan dan Asuransi secara terpisah
                var obatIdsInQuery = await joinedQuery.Select(o => o.ObatId).ToListAsync();

                var kandunganData = await (from ok in _applicationDbContext.ObatKandungans
                                           join k in _applicationDbContext.Kandungans on ok.KandunganId equals k.KandunganId
                                           where obatIdsInQuery.Contains(ok.ObatId)
                                           select new { ok.ObatId, k.NamaKandungan })
                                           .ToListAsync();

                var asuransiData = await (from oa in _applicationDbContext.ObatAsuransis
                                          join asu in _applicationDbContext.Asuransis on oa.AsuransiId equals asu.AsuransiId
                                          where obatIdsInQuery.Contains(oa.ObatId)
                                          select new { oa.ObatId, asu.NamaAsuransi })
                                          .ToListAsync();

                var groupedKandungan = kandunganData.GroupBy(k => k.ObatId).ToDictionary(g => g.Key, g => g.Select(x => x.NamaKandungan).ToList());
                var groupedAsuransi = asuransiData.GroupBy(a => a.ObatId).ToDictionary(g => g.Key, g => g.Select(x => x.NamaAsuransi).ToList());


                // 3. Filter berdasarkan search string
                if (!string.IsNullOrWhiteSpace(Nama))
                {
                    Nama = Nama.Trim().ToLower();
                    joinedQuery = joinedQuery.Where(u =>
                        EF.Functions.ILike(u.ObatName, $"%{Nama}%") 
                    );
                }

                // Lakukan penghitungan total
                int totalRows = await joinedQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                if (page > totalPages && totalPages > 0)
                {
                    return NotFound(new { message = "Page not found." });
                }


                // 4. Lakukan paging dan sorting
                var sortedQuery = orderBy?.ToLower() switch
                {
                    "obatcode" => sortDirection?.ToLower() == "desc" ? joinedQuery.OrderByDescending(u => u.ObatCode) : joinedQuery.OrderBy(u => u.ObatCode),
                    "obatname" => sortDirection?.ToLower() == "desc" ? joinedQuery.OrderByDescending(u => u.ObatName) : joinedQuery.OrderBy(u => u.ObatName),
                    _ => sortDirection?.ToLower() == "desc" ? joinedQuery.OrderByDescending(u => u.CreateDateTime) : joinedQuery.OrderBy(u => u.CreateDateTime)
                };

                var pagedRows = await sortedQuery
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();

                // 5. Gabungkan data
                var finalRows = pagedRows.Select(row => new
                {
                    row.ObatId,
                    row.ObatCode,
                    row.ObatName,
                    row.HTEPrice,
                    row.HNAPrice,
                    row.Stock,
                    row.IsActive,
                    row.Note,
                    row.Minimal,
                    row.Maximal,
                    row.Farmakologi,
                    row.Peringatan,
                    row.Indikasi,
                    row.Kontraindikasi,
                    row.CaraKerja,
                    row.InteraksiObat,
                    row.Dosis,
                    row.TakaranDosis,
                    row.JumlahSatuan,
                    row.CreateDateTime,
                    row.CreateByName,
                    row.BentukObatId,
                    row.BentukObatName,
                    row.SatuanId,
                    row.SatuanName,
                    row.ItemId,
                    row.ObatRuteId,
                    row.NamaObatRute,
                    row.KategoriObat,
                    row.IsControlled,
                    KandunganNames = groupedKandungan.ContainsKey(row.ObatId) ? groupedKandungan[row.ObatId] : new List<string>(),
                    AsuransiNames = groupedAsuransi.ContainsKey(row.ObatId) ? groupedAsuransi[row.ObatId] : new List<string>(),
                }).ToList();


                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = finalRows,
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
    }
}
