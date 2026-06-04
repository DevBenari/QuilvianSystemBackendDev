using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
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
    public class ResepTelaahController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResepTelaahController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResepTelaahController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<ResepTelaahController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
          _applicationDbContext = context;
          _userManager = userManager;
          _signInManager = signInManager;
          _logger = logger;
          _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // =============================
            // 1️⃣ AMBIL DATA TELA'AH + USER + RESEP
            // =============================

            var telaah = await (
                from a in _applicationDbContext.ResepTelaahs
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                join r in _applicationDbContext.Reseps on a.ResepId equals r.ResepId into rGroup
                from r in rGroup.DefaultIfEmpty()
                where (a.IsDelete == false || a.IsDelete == null) && a.TelaahResepId == id
                select new
                {
                    Telaah = a,
                    UserName = u.FullName,
                    Resep = r
                }
            ).FirstOrDefaultAsync();

            if (telaah == null)
                return NotFound(new { message = "Telaah Resep tidak ditemukan." });

            Guid resepId = telaah.Resep?.ResepId ?? Guid.Empty;

            // =============================
            // 2️⃣ AMBIL DATA OBAT (NON RACIKAN)
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
                    ObatName = o != null ? o.ObatName : null,
                    o.ObatCode,
                    KategoriObat = o.KategoriObat ?? null,
                    d.Qty,
                    d.HargaObat,
                    d.TotalHargaObat,
                    d.Signa,
                    d.SignaTambahan,
                    d.TakaranDosis,
                    d.IsIteratur,
                    d.JumlahIteratur,
                    d.CaraPemakaian,
                    d.EstimasiPemberian,
                    d.TglStopPemakaian,
                    d.StatusDiberikanPasien,
                    d.IsObatDibawaPlg
                }
            ).ToListAsync();

            // =============================
            // 3️⃣ AMBIL DATA RACIKAN
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
                    d.StatusDiberikanPasien,
                    d.ObatPagiDiambil,
                    d.ObatSiangDiambil,
                    d.ObatMalamDiambil,
                    d.IsReturn,
                    d.AlasanReturn,
                    d.QtyReturn,
                    d.DikembalikanOleh,
                    d.IsStopped,
                    ra.CreateBy,
                    ra.CreateDateTime
                }
            ).ToListAsync();

            var racikanIds = daftarRacikan.Select(r => r.RacikanId).Distinct().ToList();

            // =============================
            // 4️⃣ AMBIL DATA DETAIL RACIKAN
            // =============================

            var daftarRacikanDetail = await (
                from rd in _applicationDbContext.RacikanDetails.AsNoTracking()
                join ob in _applicationDbContext.Obats.AsNoTracking()
                    on rd.ObatId equals ob.ObatId
                where racikanIds.Contains((Guid)rd.RacikanId) &&
                      !rd.IsDelete
                select new
                {
                    rd.RacikanId,
                    rd.DetailRacikanId,
                    rd.ObatId,
                    ob.ObatName,
                    ob.ObatCode,
                    KategoriObat = ob.KategoriObat ?? null,
                    rd.QtyUsed,
                    rd.KomposisiDosis,
                    rd.CreateBy,
                    rd.CreateDateTime
                }
            ).ToListAsync();

            // =============================
            // 5️⃣ GROUP RACIKAN + DETAIL RACIKAN
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
                    x.Racikan.StatusDiberikanPasien,
                    x.Racikan.TglStopPemakaian,
                    x.Racikan.ObatPagiDiambil,
                    x.Racikan.ObatSiangDiambil,
                    x.Racikan.ObatMalamDiambil,
                    x.Racikan.IsReturn,
                    x.Racikan.AlasanReturn,
                    x.Racikan.QtyReturn,
                    x.Racikan.DikembalikanOleh,
                    x.Racikan.IsStopped,
                    x.Racikan.CreateBy,
                    x.Racikan.CreateDateTime,

                    DaftarRacikanDetail = x.DaftarRacikanDetail
                })
                .ToList();

            // =============================
            // 6️⃣ RETURN RESPONSE LENGKAP
            // =============================

            var result = new
            {
                Telaah = new
                {
                    telaah.Telaah.TelaahResepId,
                    telaah.Telaah.KunjunganId,
                    telaah.Telaah.PasienId,
                    telaah.Telaah.ResepId,

                    telaah.Telaah.IsAdministratif,
                    telaah.Telaah.IsNamaObatdanKetersediaan,
                    telaah.Telaah.IsDosisdanJumlahObat,
                    telaah.Telaah.IsAturandanCaraPenggunaan,
                    telaah.Telaah.IsTepatDosis,
                    telaah.Telaah.IsTepatWaktu,
                    telaah.Telaah.IsDuplikasi,
                    telaah.Telaah.IsPolifarmasi,
                    telaah.Telaah.IsAlergi,
                    telaah.Telaah.IsKontradiksi,
                    telaah.Telaah.IsInteraksiObat,

                    telaah.Telaah.Keterangan,
                    CreateDateTime = telaah.Telaah.CreateDateTime,
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
                },


            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResepTelaahViewModel vm)
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

                // **Buat Data Baru**
                var data = new ResepTelaah
                {
                    TelaahResepId = Guid.NewGuid(),
                    CreateDateTime = DateTimeOffset.UtcNow,
                    CreateBy = userActiveId,

                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    ResepId = vm.ResepId,
                    IsAdministratif = vm.IsAdministratif,
                    IsNamaObatdanKetersediaan = vm.IsNamaObatdanKetersediaan,
                    IsDosisdanJumlahObat = vm.IsDosisdanJumlahObat,
                    IsAturandanCaraPenggunaan = vm.IsAturandanCaraPenggunaan,
                    IsTepatDosis = vm.IsTepatDosis,
                    IsTepatWaktu = vm.IsTepatWaktu,
                    IsDuplikasi = vm.IsDuplikasi,
                    IsPolifarmasi = vm.IsPolifarmasi,
                    IsAlergi = vm.IsAlergi,
                    IsKontradiksi = vm.IsKontradiksi,
                    IsInteraksiObat = vm.IsInteraksiObat,
                    Keterangan = vm.Keterangan
                };

                // **Simpan ke Database**
                _applicationDbContext.ResepTelaahs.Add(data);
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

        [HttpPut]
        public async Task<IActionResult> Update(Guid id,[FromBody] ResepTelaahViewModel vm)
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

                // ===============================================
                // 1. CARI DATA LAMA BERDASARKAN ID
                // ===============================================
                var dataToUpdate = await _applicationDbContext.ResepTelaahs
                    .FirstOrDefaultAsync(d => d.TelaahResepId == id);

                if (dataToUpdate == null)
                {
                    return NotFound(new { message = $"Data Resep Telaah dengan ID {id} tidak ditemukan." });
                }

                // ===============================================
                // 2. UPDATE PROPERTI DENGAN DATA BARU DARI VM
                // ===============================================
                dataToUpdate.KunjunganId = vm.KunjunganId;
                dataToUpdate.PasienId = vm.PasienId;
                dataToUpdate.ResepId = vm.ResepId;
                dataToUpdate.IsAdministratif = vm.IsAdministratif;
                dataToUpdate.IsNamaObatdanKetersediaan = vm.IsNamaObatdanKetersediaan;
                dataToUpdate.IsDosisdanJumlahObat = vm.IsDosisdanJumlahObat;
                dataToUpdate.IsAturandanCaraPenggunaan = vm.IsAturandanCaraPenggunaan;
                dataToUpdate.IsTepatDosis = vm.IsTepatDosis;
                dataToUpdate.IsTepatWaktu = vm.IsTepatWaktu;
                dataToUpdate.IsDuplikasi = vm.IsDuplikasi;
                dataToUpdate.IsPolifarmasi = vm.IsPolifarmasi;
                dataToUpdate.IsAlergi = vm.IsAlergi;
                dataToUpdate.IsKontradiksi = vm.IsKontradiksi;
                dataToUpdate.IsInteraksiObat = vm.IsInteraksiObat;
                dataToUpdate.Keterangan = vm.Keterangan;

                // **Update Metadata Log**
                dataToUpdate.UpdateDateTime = DateTimeOffset.UtcNow;
                dataToUpdate.UpdateBy = userActiveId;

                // **Simpan Perubahan ke Database**
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Pembaruan Data Berhasil || 200 OK" });
                }
                else
                {
                    // Kasus ini terjadi jika data ditemukan tetapi tidak ada perubahan yang disimpan
                    return Ok(new { message = "Data berhasil ditemukan, namun tidak ada perubahan yang disimpan." });
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
                var data = await _applicationDbContext.ResepTelaahs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.ResepTelaahs.Update(data);
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
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            Guid? kunjunganId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            // ================================
            // 1️⃣ QUERY HEADER TELA’AH RESEP
            // ================================
            var query =
                from a in _applicationDbContext.ResepTelaahs
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId

                join r in _applicationDbContext.Reseps
                on a.ResepId equals r.ResepId into rGroup
                from r in rGroup.DefaultIfEmpty()
                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    a.TelaahResepId,
                    a.ResepId,
                    r.TanggalPembuatanResep,
                    r.AntrianRegistrasi,
                    r.AntrianResep,
                    a.KunjunganId,
                    a.PasienId,
                    a.IsAdministratif,
                    a.IsNamaObatdanKetersediaan,
                    a.IsAturandanCaraPenggunaan,
                    a.IsDosisdanJumlahObat,
                    a.IsTepatDosis,
                    a.IsTepatWaktu,
                    a.IsDuplikasi,
                    a.IsPolifarmasi,
                    a.IsAlergi,
                    a.IsKontradiksi,
                    a.IsInteraksiObat,
                    a.CreateDateTime,
                    CreateByName = u.FullName,
                    a.Keterangan
                };

            // ================================
            // 2️⃣ FILTER
            // ================================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId);

            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date;
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime <= end);
            }

            // Sorting
            bool desc = sortDirection?.ToLower() == "desc";
            query = orderBy switch
            {
                "CreateByName" =>
                    desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                _ =>
                    desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // ================================
            // 3️⃣ PAGINATION HEADER
            // ================================
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

            // Ambil semua ResepId dari page
            var resepIds = headerRows
                .Where(x => x.ResepId != Guid.Empty)
                .Select(x => x.ResepId)
                .Distinct()
                .ToList();

            // ================================
            // 4️⃣ AMBIL DETAIL OBAT (ALL RESEP)
            // ================================
            var allObat = await (
                from d in _applicationDbContext.DetailReseps
                join o in _applicationDbContext.Obats on d.ObatId equals o.ObatId into obatJoin
                from o in obatJoin.DefaultIfEmpty()
                where resepIds.Contains(d.ResepId)
                      && (d.IsRacikan == false || d.IsRacikan == null)
                      && !d.IsDelete
                select new
                {
                    d.ResepId,
                    d.DetailResepId,
                    d.ObatId,
                    o.ObatName,
                    o.ObatCode,
                    KategoriObat = o.KategoriObat ?? null,
                    d.Qty,
                    d.Signa,
                    d.SignaTambahan,
                    d.TakaranDosis
                }
            ).ToListAsync();

            // ================================
            // 5️⃣ AMBIL HEADER RACIKAN
            // ================================
            var allRacikan = await (
                from d in _applicationDbContext.DetailReseps
                join ra in _applicationDbContext.Racikans on d.RacikanId equals ra.RacikanId
                where resepIds.Contains(d.ResepId)
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

            // Ambil seluruh RacikanId
            var racikanIds = allRacikan.Select(x => x.RacikanId).Distinct().ToList();

            // ================================
            // 6️⃣ AMBIL KOMPOSISI RACIKAN SEKALIGUS
            // ================================
            var allRacikanDetail = await (
                from rd in _applicationDbContext.RacikanDetails
                join o in _applicationDbContext.Obats on rd.ObatId equals o.ObatId
                where racikanIds.Contains(rd.RacikanId.Value)
                select new
                {
                    rd.RacikanId,
                    rd.DetailRacikanId,
                    rd.ObatId,
                    o.ObatName,
                    o.ObatCode,
                    KategoriObat = o.KategoriObat ?? null,
                    rd.QtyUsed,
                    rd.KomposisiDosis
                }
            ).ToListAsync();

            // Mapping Racikan Detail
            var racikanDetailMap = allRacikanDetail
                .GroupBy(x => x.RacikanId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            // ================================
            // 7️⃣ BUILD FINAL RESPONSE PER ROW
            // ================================
            var finalRows = headerRows.Select(async h =>
            {
                var obatList = allObat.Where(o => o.ResepId == h.ResepId).ToList();

                var racikanDetails = racikanDetailMap
                    .Where(x => x.Key != null)
                    .SelectMany(x => x.Value)
                    .ToList();

                return new
                {
                    h.TelaahResepId,
                    h.ResepId,
                    h.TanggalPembuatanResep,
                    h.AntrianRegistrasi,
                    h.AntrianResep,
                    h.KunjunganId,
                    h.PasienId,
                    h.IsAdministratif,
                    h.IsNamaObatdanKetersediaan,
                    h.IsAturandanCaraPenggunaan,
                    h.IsDosisdanJumlahObat,
                    h.IsTepatDosis,
                    h.IsTepatWaktu,
                    h.IsDuplikasi,
                    h.IsPolifarmasi,
                    h.IsAlergi,
                    h.IsKontradiksi,
                    h.IsInteraksiObat,
                    h.CreateDateTime,
                    h.CreateByName,
                    h.Keterangan,

                    DaftarObat = obatList,
                    DaftarRacikan = racikanDetails
                };
            });
            var finalData = await Task.WhenAll(finalRows);

            // ================================
            // 8️⃣ RETURN
            // ================================
            return Ok(new
            {
                status = "success",
                data = new
                {
                    Rows = finalData,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }


    }
}
