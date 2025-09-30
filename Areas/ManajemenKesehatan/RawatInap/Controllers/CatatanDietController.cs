using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
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
    public class CatatanDietController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<CatatanDietController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CatatanDietController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CatatanDietController> logger,
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
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query CatatanDiet + UserActive + Details
            var query = _applicationDbContext.CatatanDiets
                .Where(a => a.IsDelete == false || a.IsDelete == null)
                .Include(a => a.DetailIcd10) // navigation property → otomatis ambil details
                .Join(_applicationDbContext.UserActives,
                      a => a.CreateBy,
                      u => u.UserActiveId,
                      (a, u) => new
                      {
                          a.CatatanDietId,
                          a.KunjunganId,
                          a.PasienId,
                          a.Diet,
                          a.StatusDiet,
                          a.Keterangan,
                          a.TglCatatanDiet,
                          a.CreateDateTime,
                          a.CreateBy,
                          CreateByName = u.FullName,

                          // mapping details
                          DetailIcd10 = a.DetailIcd10.Select(d => new
                          {
                              d.CatatanDietDetailId,
                              d.Icd10Id
                          }).ToList()
                      });

            // Total
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Data Paged
            var listdata = await query
                .OrderByDescending(a => a.CreateDateTime)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _applicationDbContext.CatatanDiets
                .Where(a => (a.IsDelete == false || a.IsDelete == null) && a.CatatanDietId == id)
                .Include(a => a.DetailIcd10) // relasi ke detail
                .Join(_applicationDbContext.UserActives,
                      a => a.CreateBy,
                      u => u.UserActiveId,
                      (a, u) => new
                      {
                          a.CatatanDietId,
                          a.KunjunganId,
                          a.PasienId,
                          a.Diet,
                          a.StatusDiet,
                          a.Keterangan,
                          a.TglCatatanDiet,
                          a.CreateDateTime,
                          a.CreateBy,
                          CreateByName = u.FullName,

                          DetailIcd10 = a.DetailIcd10.Select(d => new
                          {
                              d.CatatanDietDetailId,
                              d.Icd10Id
                          }).ToList()
                      })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CatatanDietViewModel vm)
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

                // ✅ Ambil User ID dari JWT Claims
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

                // ✅ Buat Data CatatanDiet
                var catatanDietId = Guid.NewGuid();
                var data = new CatatanDiet
                {
                    CatatanDietId = catatanDietId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    Diet = vm.Diet,
                    StatusDiet = vm.StatusDiet,
                    Keterangan = vm.Keterangan,
                    TglCatatanDiet = TryParseTanggalToUtc(vm.TglCatatanDiet),
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.CatatanDiets.Add(data);

                // ✅ Simpan Detail ICD10 jika ada
                if (vm.DetailIcd10 != null && vm.DetailIcd10.Any())
                {
                    foreach (var detailVm in vm.DetailIcd10)
                    {
                        var detail = new CatatanDietDetail
                        {
                            CatatanDietDetailId = Guid.NewGuid(),
                            CatatanDietId = catatanDietId,
                            Icd10Id = detailVm.Icd10Id,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.CatatanDietDetails.Add(detail);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] CatatanDietViewModel vm)
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

                // ✅ Ambil User ID dari JWT Claims
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

                // ✅ Cari data CatatanDiet yang akan diupdate
                var data = await _applicationDbContext.CatatanDiets
                    .Include(cd => cd.DetailIcd10) // include biar bisa hapus detail
                    .FirstOrDefaultAsync(cd => cd.CatatanDietId == id && (cd.IsDelete == false || cd.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new { message = "Data CatatanDiet tidak ditemukan." });
                }

                // ✅ Update field utama
                data.KunjunganId = vm.KunjunganId;
                data.PasienId = vm.PasienId;
                data.Diet = vm.Diet;
                data.StatusDiet = vm.StatusDiet;
                data.Keterangan = vm.Keterangan;
                data.TglCatatanDiet = TryParseTanggalToUtc(vm.TglCatatanDiet);

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // ✅ Hapus detail lama
                if (data.DetailIcd10 != null && data.DetailIcd10.Any())
                {
                    _applicationDbContext.CatatanDietDetails.RemoveRange(data.DetailIcd10);
                }

                // ✅ Tambahkan detail baru dari VM
                if (vm.DetailIcd10 != null && vm.DetailIcd10.Any())
                {
                    foreach (var detailVm in vm.DetailIcd10)
                    {
                        var detail = new CatatanDietDetail
                        {
                            CatatanDietDetailId = Guid.NewGuid(),
                            CatatanDietId = data.CatatanDietId,
                            Icd10Id = detailVm.Icd10Id,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.CatatanDietDetails.Add(detail);
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

            // ✅ Query CatatanDiet + User + Detail
            var query = _applicationDbContext.CatatanDiets
                .Where(a => a.IsDelete == false || a.IsDelete == null)
                .Include(a => a.DetailIcd10)
                .Join(_applicationDbContext.UserActives,
                    a => a.CreateBy,
                    u => u.UserActiveId,
                    (a, u) => new
                    {
                        a.CatatanDietId,
                        a.KunjunganId,
                        a.PasienId,
                        a.Diet,
                        a.StatusDiet,
                        a.Keterangan,
                        a.TglCatatanDiet,
                        a.CreateDateTime,
                        a.CreateBy,
                        CreateByName = u.FullName,
                        DetailIcd10 = a.DetailIcd10.Select(d => new
                        {
                            d.CatatanDietDetailId,
                            d.Icd10Id
                        }).ToList()
                    });

            // ✅ Filter search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(x =>
                    x.Diet.ToLower().Contains(search) ||
                    x.StatusDiet.ToLower().Contains(search) ||
                    (x.Keterangan != null && x.Keterangan.ToLower().Contains(search))
                );
            }

            // ✅ Sorting
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "Diet" => query.OrderByDescending(x => x.Diet),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "Diet" => query.OrderBy(x => x.Diet),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };

            // ✅ Paging
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!rows.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = rows,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }


    }
}
