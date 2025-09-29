using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class EvaluasiAwalController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<EvaluasiAwalController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EvaluasiAwalController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<EvaluasiAwalController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.EvaluasiAwals
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.EvaluasiAwalId,
                             a.KunjunganId,
                             a.PasienId,
                             a.KekuatanKemampuan,
                             a.RiwayatKesehatan,
                             a.KesehatanMental,
                             a.TersedianyaDukungan,
                             a.FinancialEvaluasiAwal,
                             a.AsuransiId,
                             a.RiwayatObatAlternatif,
                             a.RiwayatTrauma,
                             a.HarapanHasil,
                             a.AspekLegal,
                             a.DischargePlanning,
                             a.KebutuhanLain,
                             a.TglEvaluasiAwal,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.EvaluasiAwals.Find(id);
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
        public async Task<IActionResult> Create([FromBody] EvaluasiAwalViewModel vm)
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

                // Ambil user dari JWT
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

                // Buat EvaluasiAwal baru
                var evaluasiAwalId = Guid.NewGuid();
                var data = new EvaluasiAwal
                {
                    EvaluasiAwalId = evaluasiAwalId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    KekuatanKemampuan = vm.KekuatanKemampuan,
                    RiwayatKesehatan = vm.RiwayatKesehatan,
                    KesehatanMental = vm.KesehatanMental,
                    TersedianyaDukungan = vm.TersedianyaDukungan,
                    FinancialEvaluasiAwal = vm.FinancialEvaluasiAwal,
                    AsuransiId = vm.AsuransiId,
                    RiwayatObatAlternatif = vm.RiwayatObatAlternatif,
                    RiwayatTrauma = vm.RiwayatTrauma,
                    HarapanHasil = vm.HarapanHasil,
                    AspekLegal = vm.AspekLegal,
                    DischargePlanning = vm.DischargePlanning,
                    KebutuhanLain = vm.KebutuhanLain,
                    Keterangan = vm.Keterangan,
                    TglEvaluasiAwal = TryParseTanggalToUtc(vm.TglEvaluasiAwal),
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.EvaluasiAwals.Add(data);

                // Insert DetailEvaluasiAwal jika ada
                if (vm.DetailEvaluasiAwal != null && vm.DetailEvaluasiAwal.Any())
                {
                    foreach (var detailVm in vm.DetailEvaluasiAwal)
                    {
                        var detail = new EvaluasiAwalDetail
                        {
                            DetailEvaluasiAwalId = Guid.NewGuid(),
                            EvaluasiAwalId = evaluasiAwalId,
                            ChecklistItemId = detailVm.ChecklistItemId,
                            Keterangan = detailVm.Keterangan,
                            TglPenyimpanan = TryParseTanggalToUtc(detailVm.TglPenyimpanan),
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.EvaluasiAwalDetails.Add(detail);
                    }
                }

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
        public async Task<IActionResult> Update(Guid id, [FromBody] EvaluasiAwalViewModel vm)
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

                // Ambil user dari JWT
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

                // Cari data yang mau diupdate
                var data = _applicationDbContext.EvaluasiAwals.FirstOrDefault(e => e.EvaluasiAwalId == id);
                if (data == null)
                {
                    return NotFound(new { message = "Data EvaluasiAwal tidak ditemukan." });
                }

                // Update field
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.KekuatanKemampuan = vm.KekuatanKemampuan;
                data.RiwayatKesehatan = vm.RiwayatKesehatan;
                data.KesehatanMental = vm.KesehatanMental;
                data.TersedianyaDukungan = vm.TersedianyaDukungan;
                data.FinancialEvaluasiAwal = vm.FinancialEvaluasiAwal;
                data.AsuransiId = vm.AsuransiId;
                data.RiwayatObatAlternatif = vm.RiwayatObatAlternatif;
                data.RiwayatTrauma = vm.RiwayatTrauma;
                data.HarapanHasil = vm.HarapanHasil;
                data.AspekLegal = vm.AspekLegal;
                data.DischargePlanning = vm.DischargePlanning;
                data.KebutuhanLain = vm.KebutuhanLain;
                data.Keterangan = vm.Keterangan;
                data.TglEvaluasiAwal = TryParseTanggalToUtc(vm.TglEvaluasiAwal);
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // Hapus detail lama
                var oldDetails = _applicationDbContext.EvaluasiAwalDetails
                                   .Where(d => d.EvaluasiAwalId == id).ToList();
                if (oldDetails.Any())
                {
                    _applicationDbContext.EvaluasiAwalDetails.RemoveRange(oldDetails);
                }

                // Insert detail baru
                if (vm.DetailEvaluasiAwal != null && vm.DetailEvaluasiAwal.Any())
                {
                    foreach (var detailVm in vm.DetailEvaluasiAwal)
                    {
                        var detail = new EvaluasiAwalDetail
                        {
                            DetailEvaluasiAwalId = Guid.NewGuid(),
                            EvaluasiAwalId = id,
                            ChecklistItemId = detailVm.ChecklistItemId,
                            Keterangan = detailVm.Keterangan,
                            TglPenyimpanan = TryParseTanggalToUtc(detailVm.TglPenyimpanan),
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.EvaluasiAwalDetails.Add(detail);
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal update data: {dbEx.InnerException?.Message}" });
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
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // Cari header
                var data = _applicationDbContext.EvaluasiAwals.FirstOrDefault(e => e.EvaluasiAwalId == id);
                if (data == null)
                {
                    return NotFound(new { message = "Data EvaluasiAwal tidak ditemukan." });
                }

                // Soft delete header
                data.IsDelete = true;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                // Soft delete semua detail terkait
                var details = _applicationDbContext.EvaluasiAwalDetails
                                   .Where(d => d.EvaluasiAwalId == id)
                                   .ToList();

                if (details.Any())
                {
                    foreach (var d in details)
                    {
                        d.IsDelete = true;
                        d.DeleteDateTime = DateTimeOffset.UtcNow;
                    }
                }

                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Hapus Data Berhasil (Soft Delete Header + Detail) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil dihapus dari database." });
                }
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
        string? sortDirection = "desc")
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query Header + Detail
            var query = from a in _applicationDbContext.EvaluasiAwals
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId into ua
                        from u in ua.DefaultIfEmpty()

                        join d in _applicationDbContext.EvaluasiAwalDetails
                            on a.EvaluasiAwalId equals d.EvaluasiAwalId into detailGroup
                        where a.IsDelete == false || a.IsDelete == null
                        select new
                        {
                            a.EvaluasiAwalId,
                            a.KunjunganId,
                            a.PasienId,
                            a.KekuatanKemampuan,
                            a.RiwayatKesehatan,
                            a.KesehatanMental,
                            a.TersedianyaDukungan,
                            a.FinancialEvaluasiAwal,
                            a.AsuransiId,
                            a.RiwayatObatAlternatif,
                            a.RiwayatTrauma,
                            a.HarapanHasil,
                            a.AspekLegal,
                            a.DischargePlanning,
                            a.KebutuhanLain,
                            a.Keterangan,
                            a.TglEvaluasiAwal,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            Details = detailGroup.Select(d => new
                            {
                                d.DetailEvaluasiAwalId,
                                d.ChecklistItemId,
                                d.Keterangan,
                                d.TglPenyimpanan
                            })
                        };

            // Filter search sederhana
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%";
                query = query.Where(e =>
                    EF.Functions.ILike(e.KekuatanKemampuan, search) ||
                    EF.Functions.ILike(e.RiwayatKesehatan, search) ||
                    EF.Functions.ILike(e.KesehatanMental, search)
                );
            }

            // Sorting
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

            // Hitung total
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data paged
            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!rows.Any() && page > totalPages)
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
