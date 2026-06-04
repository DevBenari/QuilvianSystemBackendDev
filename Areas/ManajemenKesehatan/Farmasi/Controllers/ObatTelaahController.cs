using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class ObatTelaahController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<ObatTelaahController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ObatTelaahController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ObatTelaahController> logger,
            IWebHostEnvironment webHostEnvironment,
            ITTDService ttdServive)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _ttdService = ttdServive;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // =============================
            // 1️⃣ GET DATA TELA'AH OBAT + USER + RESEP
            // =============================

            var telaah = await (
                from t in _applicationDbContext.ObatTelaahs
                join u in _applicationDbContext.UserActives on t.CreateBy equals u.UserActiveId
                join r in _applicationDbContext.Reseps on t.ResepId equals r.ResepId into rGroup
                from r in rGroup.DefaultIfEmpty()
                where (t.IsDelete == false || t.IsDelete == null) && t.TelaahObatId == id
                select new
                {
                    Telaah = t,
                    UserName = u.FullName,
                    Resep = r
                }
            ).FirstOrDefaultAsync();

            if (telaah == null)
                return NotFound(new { message = "Data telaah obat tidak ditemukan." });

            Guid resepId = telaah.Resep?.ResepId ?? Guid.Empty;

            // =============================
            // 2️⃣ GET OBAT NON RACIKAN
            // =============================

            var daftarObat = await (
                from d in _applicationDbContext.DetailReseps.AsNoTracking()
                join o in _applicationDbContext.Obats.AsNoTracking()
                    on d.ObatId equals o.ObatId into obatJoin
                from o in obatJoin.DefaultIfEmpty()
                where d.ResepId == resepId &&
                      (d.IsRacikan == false || d.IsRacikan == null) &&
                      !d.IsDelete
                select new
                {
                    d.DetailResepId,
                    d.ObatId,
                    ObatName = o.ObatName,
                    o.ObatCode,
                    KategoriObat = o.KategoriObat,
                    d.Qty,
                    d.Signa,
                    d.SignaTambahan,
                    d.TakaranDosis,
                    d.JumlahIteratur,
                    d.IsIteratur,
                    d.CaraPemakaian,
                    d.EstimasiPemberian,
                    d.TglStopPemakaian,
                    d.StatusDiberikanPasien
                }
            ).ToListAsync();

            // =============================
            // 3️⃣ GET HEADER RACIKAN
            // =============================

            var daftarRacikan = await (
                from d in _applicationDbContext.DetailReseps.AsNoTracking()
                join ra in _applicationDbContext.Racikans.AsNoTracking()
                    on d.RacikanId equals ra.RacikanId
                where d.ResepId == resepId &&
                      d.IsRacikan == true &&
                      !d.IsDelete
                select new
                {
                    ra.RacikanId,
                    ra.NamaRacikan,
                    ra.BentukRacikanId,
                    d.Qty,
                    d.Signa,
                    d.SignaTambahan,
                    d.CaraPemakaian,
                    d.EstimasiPemberian,
                    d.TglStopPemakaian,
                    d.StatusDiberikanPasien
                }
            ).ToListAsync();

            var racikanIds = daftarRacikan.Select(r => r.RacikanId).Distinct().ToList();

            // =============================
            // 4️⃣ GET KOMPOSISI RACIKAN
            // =============================

            var daftarRacikanDetail = await (
                from rd in _applicationDbContext.RacikanDetails.AsNoTracking()
                join ob in _applicationDbContext.Obats.AsNoTracking()
                    on rd.ObatId equals ob.ObatId
                where racikanIds.Contains((Guid)rd.RacikanId) && !rd.IsDelete
                select new
                {
                    rd.RacikanId,
                    rd.DetailRacikanId,
                    rd.ObatId,
                    ob.ObatName,
                    ob.ObatCode,
                    KategoriObat = ob.KategoriObat,
                    rd.QtyUsed,
                    rd.KomposisiDosis
                }
            ).ToListAsync();

            // =============================
            // 5️⃣ MERGE RACIKAN + DETAIL
            // =============================

            var racikanWithDetail = daftarRacikan
                .GroupBy(r => r.RacikanId)
                .Select(g => new
                {
                    Racikan = g.First(),
                    DaftarRacikanDetail = daftarRacikanDetail
                        .Where(rd => rd.RacikanId == g.Key)
                        .ToList()
                })
                .Select(x => new
                {
                    x.Racikan.RacikanId,
                    x.Racikan.NamaRacikan,
                    x.Racikan.BentukRacikanId,
                    x.Racikan.Qty,
                    x.Racikan.Signa,
                    x.Racikan.SignaTambahan,
                    x.Racikan.CaraPemakaian,
                    x.Racikan.EstimasiPemberian,
                    x.Racikan.TglStopPemakaian,
                    x.Racikan.StatusDiberikanPasien,

                    DaftarRacikanDetail = x.DaftarRacikanDetail
                })
                .ToList();

            // =============================
            // 6️⃣ FINAL RESPONSE
            // =============================

            var result = new
            {
                TelaahObat = new
                {
                    telaah.Telaah.TelaahObatId,
                    telaah.Telaah.KunjunganId,
                    telaah.Telaah.PasienId,
                    telaah.Telaah.ResepId,

                    telaah.Telaah.IsTepatIdentitas,
                    telaah.Telaah.IsTepatObat,
                    telaah.Telaah.IsTepatDosis,
                    telaah.Telaah.IsTepatRute,
                    telaah.Telaah.IsTepatWaktu,

                    telaah.Telaah.PetugasCekFinalId,
                    telaah.Telaah.TTDPetugasCekFinal,
                    telaah.Telaah.Keterangan,

                    telaah.Telaah.CreateDateTime,
                    CreateBy = telaah.Telaah.CreateBy,
                    CreateByName = telaah.UserName
                },

                Resep = new
                {
                    telaah.Resep?.AntrianResep,
                    telaah.Resep?.TanggalPembuatanResep,
                    telaah.Resep?.AntrianRegistrasi,

                    DaftarObat = daftarObat,
                    DaftarRacikan = racikanWithDetail
                }
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObatTelaahViewModel vm)
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
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                //                    && c.IsDelete == false);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.PetugasCekFinalId);

                // **Buat Data Baru**
                var data = new ObatTelaah
                {
                    TelaahObatId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    ResepId = vm.ResepId,
                    IsTepatIdentitas = vm.IsTepatIdentitas,
                    IsTepatDosis = vm.IsTepatDosis,
                    IsTepatObat = vm.IsTepatObat,
                    IsTepatRute = vm.IsTepatRute,
                    IsTepatWaktu = vm.IsTepatWaktu,
                    PetugasCekFinalId = vm.PetugasCekFinalId,
                    TTDPetugasCekFinal = ttd.Path,
                    Keterangan = vm.Keterangan,
                    
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.ObatTelaahs.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] ObatTelaahViewModel vm)
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
                var data = await _applicationDbContext.ObatTelaahs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon.ToLower().Trim() == vm.NamaDiskon.ToLower().Trim()
                //                    && c.IsDelete == false && c.DiskonId != id);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // cek ttd
                var ttd = await _ttdService.CheckTTDAsync((Guid)vm.PetugasCekFinalId);

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                    data.PasienId = vm.PasienId;
                    data.ResepId = vm.ResepId;
                    data.IsTepatIdentitas = vm.IsTepatIdentitas;
                    data.IsTepatDosis = vm.IsTepatDosis;
                    data.IsTepatObat = vm.IsTepatObat;
                    data.IsTepatRute = vm.IsTepatRute;
                    data.IsTepatWaktu = vm.IsTepatWaktu;
                    data.PetugasCekFinalId = vm.PetugasCekFinalId;
                    data.TTDPetugasCekFinal = ttd.Path;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.ObatTelaahs.Update(data);
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
                var data = await _applicationDbContext.ObatTelaahs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.ObatTelaahs.Update(data);
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
        public async Task<IActionResult> PagedTelaah(
    int page = 1,
    int perPage = 10,
    Guid? kunjunganId = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string? orderBy = "CreateDateTime",
    string? sortDirection = "desc")
        {
            // ============================
            // 1️⃣ QUERY HEADER TELA’AH OBAT
            // ============================
            var query =
                from t in _applicationDbContext.ObatTelaahs
                join u in _applicationDbContext.UserActives on t.CreateBy equals u.UserActiveId
                join r in _applicationDbContext.Reseps on t.ResepId equals r.ResepId into rGroup
                from r in rGroup.DefaultIfEmpty()
                where t.IsDelete == false || t.IsDelete == null
                select new
                {
                    t.TelaahObatId,
                    t.KunjunganId,
                    t.PasienId,
                    t.ResepId,

                    t.IsTepatIdentitas,
                    t.IsTepatObat,
                    t.IsTepatDosis,
                    t.IsTepatRute,
                    t.IsTepatWaktu,

                    t.PetugasCekFinalId,
                    t.TTDPetugasCekFinal,
                    t.Keterangan,

                    // From resep
                    r.TanggalPembuatanResep,
                    r.AntrianResep,
                    r.AntrianRegistrasi,

                    t.CreateDateTime,
                    CreateByName = u.FullName
                };

            // ============================
            // 2️⃣ FILTER
            // ============================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId);

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime <= end);
            }

            // ============================
            // 3️⃣ SORTING
            // ============================
            bool desc = sortDirection?.ToLower() == "desc";

            query = orderBy switch
            {
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // ============================
            // 4️⃣ PAGINATION HEADER
            // ============================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var headerRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!headerRows.Any())
                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        Rows = new List<object>(),
                        TotalRows = 0,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = 0
                    }
                });

            // Ambil Resep ID
            var resepIds = headerRows
                .Where(x => x.ResepId != Guid.Empty)
                .Select(x => x.ResepId)
                .Distinct()
                .ToList();

            // ============================
            // 5️⃣ LOAD OBAT NON–RACIKAN
            // ============================
            var allObat = await (
                from d in _applicationDbContext.DetailReseps
                join o in _applicationDbContext.Obats on d.ObatId equals o.ObatId into oJoin
                from o in oJoin.DefaultIfEmpty()
                where resepIds.Contains((Guid)d.ResepId)
                      && (d.IsRacikan == false || d.IsRacikan == null)
                      && !d.IsDelete
                select new
                {
                    d.ResepId,
                    d.DetailResepId,
                    d.ObatId,
                    o.ObatName,
                    o.ObatCode,
                    d.Qty,
                    d.Signa,
                    d.SignaTambahan,
                    d.TakaranDosis
                }
            ).ToListAsync();

            // ============================
            // 6️⃣ LOAD RACIKAN HEADER
            // ============================
            var allRacikan = await (
                from d in _applicationDbContext.DetailReseps
                join ra in _applicationDbContext.Racikans on d.RacikanId equals ra.RacikanId
                where resepIds.Contains((Guid)d.ResepId)
                      && d.IsRacikan == true
                      && !d.IsDelete
                select new
                {
                    d.ResepId,
                    ra.RacikanId,
                    ra.NamaRacikan,
                    d.Qty,
                    d.Signa,
                    d.SignaTambahan
                }
            ).ToListAsync();

            var racikanIds = allRacikan.Select(x => x.RacikanId).Distinct().ToList();

            // ============================
            // 7️⃣ LOAD KOMPOSISI RACIKAN
            // ============================
            List<dynamic> allRacikanDetail = new();

            if (racikanIds.Any())
            {
                var temp = await (
                    from rd in _applicationDbContext.RacikanDetails
                    join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
                    where rd.RacikanId.HasValue && racikanIds.Contains(rd.RacikanId.Value)
                    select new
                    {
                        rd.RacikanId,
                        rd.DetailRacikanId,
                        rd.ObatId,
                        o.ObatName,
                        o.ObatCode,
                        rd.QtyUsed,
                        rd.KomposisiDosis
                    }
                ).ToListAsync();

                allRacikanDetail = temp.Cast<dynamic>().ToList();
            }

            var racikanDetailMap = allRacikanDetail
                .GroupBy(x => (Guid)x.RacikanId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            // ============================
            // 8️⃣ BUILD FINAL ROWS
            // ============================
            var finalRows = headerRows.Select(h =>
            {
                var obatList = allObat.Where(o => o.ResepId == h.ResepId).ToList();
                var racikanHeader = allRacikan.Where(r => r.ResepId == h.ResepId).ToList();

                var racikanFinal = racikanHeader.Select(r =>
                {
                    racikanDetailMap.TryGetValue(r.RacikanId, out var komposisi);

                    return new
                    {
                        r.RacikanId,
                        r.NamaRacikan,
                        r.Qty,
                        r.Signa,
                        r.SignaTambahan,
                        Komposisi = komposisi ?? new List<dynamic>()
                    };
                }).ToList();

                return new
                {
                    h.TelaahObatId,
                    h.KunjunganId,
                    h.PasienId,
                    h.ResepId,

                    h.IsTepatIdentitas,
                    h.IsTepatObat,
                    h.IsTepatDosis,
                    h.IsTepatRute,
                    h.IsTepatWaktu,

                    h.PetugasCekFinalId,
                    h.TTDPetugasCekFinal,
                    h.Keterangan,

                    h.TanggalPembuatanResep,
                    h.AntrianRegistrasi,
                    h.AntrianResep,

                    h.CreateDateTime,
                    h.CreateByName,

                    DaftarObat = obatList,
                    DaftarRacikan = racikanFinal
                };
            }).ToList();

            // ============================
            // 9️⃣ RESPONSE
            // ============================
            return Ok(new
            {
                status = "success",
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

    }
}
