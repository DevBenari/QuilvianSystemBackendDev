using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
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
    public class PenilaianResikoJatuhDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PenilaianResikoJatuhDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PenilaianResikoJatuhDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<PenilaianResikoJatuhDetailController> logger,
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
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // ===============================
            // 1️⃣ Query Utama: Data Detail Resiko Jatuh
            // ===============================
            var baseQuery = from a in _applicationDbContext.PenilaianResikoJatuhDetails
                            join u in _applicationDbContext.UserActives
                                on a.CreateBy equals u.UserActiveId into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            where a.IsDelete == false || a.IsDelete == null
                            orderby a.CreateDateTime descending
                            select new
                            {
                                a.DetailResikoJatuhId,
                                a.IndikatorPengkajianId,
                                a.IntervensiResikoJatuhId,
                                a.Keterangan,
                                a.CreateBy,
                                a.CreateDateTime,
                                CreateByName = u.FullName
                            };

            var totalRows = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = await baseQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .AsNoTracking()
                .ToListAsync();

            if (!listData.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            // ===============================
            // 2️⃣ Ambil semua foreign key untuk join (batching)
            // ===============================
            var indikatorPengkajianIds = listData
                .Where(x => x.IndikatorPengkajianId != Guid.Empty)
                .Select(x => x.IndikatorPengkajianId)
                .Distinct()
                .ToList();

            // ===============================
            // 3️⃣ Join ke tabel IndikatorPengkajian + Indikator + IndikatorScore
            // ===============================
            var detailJoins = await (
                from ip in _applicationDbContext.IndikatorPengkajians
                join ind in _applicationDbContext.Indikators on ip.IndikatorId equals ind.IndikatorId
                join sc in _applicationDbContext.IndikatorScores on ip.IndikatorScoreId equals sc.IndikatorScoreId
                where indikatorPengkajianIds.Contains(ip.IndikatorPengkajianId)
                select new
                {
                    ip.IndikatorPengkajianId,
                    ind.IndikatorId,
                    ind.NamaIndikator,
                    sc.IndikatorScoreId,
                    sc.NamaIndikatorScore,
                    sc.ScoreIndikator
                }
            ).ToListAsync();

            // ===============================
            // 4️⃣ Buat lookup (group by IndikatorPengkajianId)
            // ===============================
            var indikatorLookup = detailJoins.ToLookup(x => x.IndikatorPengkajianId);

            // ===============================
            // 5️⃣ Gabungkan hasil ke dalam list utama
            // ===============================
            var result = listData.Select(x => new
            {
                x.DetailResikoJatuhId,
                x.IndikatorPengkajianId,
                x.IntervensiResikoJatuhId,
                x.Keterangan,
                x.CreateBy,
                x.CreateByName,
                x.CreateDateTime,

                // 🔹 List hasil join (bisa banyak baris per indikator pengkajian)
                IndikatorDetails = indikatorLookup[x.IndikatorPengkajianId].Select(i => new
                {
                    i.IndikatorId,
                    i.NamaIndikator,
                    i.IndikatorScoreId,
                    i.NamaIndikatorScore,
                    i.ScoreIndikator
                }).ToList()
            });

            // ===============================
            // 6️⃣ Return hasil
            // ===============================
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
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // ✅ Cek koneksi ke database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ===============================
                // 1️⃣ Ambil data utama Detail Resiko Jatuh
                // ===============================
                var data = await (from a in _applicationDbContext.PenilaianResikoJatuhDetails
                                  join u in _applicationDbContext.UserActives
                                      on a.CreateBy equals u.UserActiveId into userGroup
                                  from u in userGroup.DefaultIfEmpty()
                                  where (a.IsDelete == false || a.IsDelete == null)
                                        && a.DetailResikoJatuhId == id
                                  select new
                                  {
                                      a.DetailResikoJatuhId,
                                      a.IndikatorPengkajianId,
                                      a.IntervensiResikoJatuhId,
                                      a.Keterangan,
                                      a.CreateBy,
                                      CreateByName = u.FullName,
                                      a.CreateDateTime
                                  })
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new { message = $"Data Detail Resiko Jatuh dengan ID {id} tidak ditemukan. || 404 Not Found" });

                // ===============================
                // 2️⃣ Ambil data join IndikatorPengkajian + Indikator + IndikatorScore
                // ===============================
                var indikatorDetails = await (
                    from ip in _applicationDbContext.IndikatorPengkajians
                    join ind in _applicationDbContext.Indikators on ip.IndikatorId equals ind.IndikatorId
                    join sc in _applicationDbContext.IndikatorScores on ip.IndikatorScoreId equals sc.IndikatorScoreId
                    where ip.IndikatorPengkajianId == data.IndikatorPengkajianId
                    select new
                    {
                        ip.IndikatorPengkajianId,
                        ind.IndikatorId,
                        ind.NamaIndikator,
                        sc.IndikatorScoreId,
                        sc.NamaIndikatorScore,
                        sc.ScoreIndikator
                    }
                ).AsNoTracking().ToListAsync();

                // ===============================
                // 3️⃣ Bentuk response akhir
                // ===============================
                var result = new
                {
                    data.DetailResikoJatuhId,
                    data.IndikatorPengkajianId,
                    data.IntervensiResikoJatuhId,
                    data.Keterangan,
                    data.CreateBy,
                    data.CreateByName,
                    data.CreateDateTime,

                    // 🔹 List detail join (bisa lebih dari satu baris)
                    IndikatorDetails = indikatorDetails.Any() ? indikatorDetails : null
                };

                // ===============================
                // 4️⃣ Return hasil
                // ===============================
                return Ok(new
                {
                    message = "Berhasil mengambil data Detail Penilaian Resiko Jatuh || 200 OK",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PenilaianResikoJatuhDetailVM vm)
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

                ////// **Cek Duplikasi**
                //bool isDuplicate = await _applicationDbContext.Diskons
                //                    .AnyAsync(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new PenilaianResikoJatuhDetail
                {
                    DetailResikoJatuhId = Guid.NewGuid(),
                    IndikatorPengkajianId = vm.IndikatorPengkajianId,
                    IntervensiResikoJatuhId = vm.IntervensiResikoJatuhId,
                    IsIntervensiChecklist = vm.IsIntervensiChecklist,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };
                // **Simpan ke Database**
                _applicationDbContext.PenilaianResikoJatuhDetails.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] PenilaianResikoJatuhDetailVM vm)
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

                // **Cari Data Lama**
                var data = await _applicationDbContext.PenilaianResikoJatuhDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data permintaan darah tidak ditemukan." });
                }

                // **Update Data**
                data.IntervensiResikoJatuhId = vm.IntervensiResikoJatuhId;
                data.IsIntervensiChecklist = vm.IsIntervensiChecklist;
                data.IndikatorPengkajianId = vm.IndikatorPengkajianId;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.PenilaianResikoJatuhDetails.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
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
                var data = await _applicationDbContext.PenilaianResikoJatuhDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.PenilaianResikoJatuhDetails.Update(data);
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
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ===============================
                // 1️⃣ Base Query (tabel utama)
                // ===============================
                var query = from a in _applicationDbContext.PenilaianResikoJatuhDetails
                            join u in _applicationDbContext.UserActives
                                on a.CreateBy equals u.UserActiveId into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            where a.IsDelete == false || a.IsDelete == null
                            select new
                            {
                                a.DetailResikoJatuhId,
                                a.IndikatorPengkajianId,
                                a.IntervensiResikoJatuhId,
                                a.Keterangan,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.CreateDateTime
                            };

                // ===============================
                // 2️⃣ Filter opsional
                // ===============================

                // 🔍 Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string searchLower = $"%{search.ToLower()}%";
                    query = query.Where(q =>
                        EF.Functions.ILike(q.Keterangan.ToLower(), searchLower) ||
                        EF.Functions.ILike(q.CreateByName.ToLower(), searchLower));
                }

                // 📅 Filter tanggal
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date.ToUniversalTime();
                    var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                    query = query.Where(q => q.CreateDateTime >= startUtc && q.CreateDateTime <= endUtc);
                }

                // ===============================
                // 3️⃣ Sorting
                // ===============================
                query = sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "CreateByName" => query.OrderByDescending(q => q.CreateByName),
                        "Keterangan" => query.OrderByDescending(q => q.Keterangan),
                        _ => query.OrderByDescending(q => q.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "CreateByName" => query.OrderBy(q => q.CreateByName),
                        "Keterangan" => query.OrderBy(q => q.Keterangan),
                        _ => query.OrderBy(q => q.CreateDateTime)
                    };

                // ===============================
                // 4️⃣ Paging
                // ===============================
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var listData = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .AsNoTracking()
                    .ToListAsync();

                if (!listData.Any())
                    return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

                // ===============================
                // 5️⃣ Ambil semua ID untuk batch join
                // ===============================
                var indikatorPengkajianIds = listData
                    .Where(x => x.IndikatorPengkajianId != Guid.Empty)
                    .Select(x => x.IndikatorPengkajianId)
                    .Distinct()
                    .ToList();

                // ===============================
                // 6️⃣ Join ke tabel Indikator, Score, dan Pengkajian
                // ===============================
                var indikatorData = await (
                    from ip in _applicationDbContext.IndikatorPengkajians
                    join ind in _applicationDbContext.Indikators on ip.IndikatorId equals ind.IndikatorId
                    join sc in _applicationDbContext.IndikatorScores on ip.IndikatorScoreId equals sc.IndikatorScoreId
                    where indikatorPengkajianIds.Contains(ip.IndikatorPengkajianId)
                    select new
                    {
                        ip.IndikatorPengkajianId,
                        ind.IndikatorId,
                        ind.NamaIndikator,
                        sc.IndikatorScoreId,
                        sc.NamaIndikatorScore,
                        sc.ScoreIndikator
                    }
                ).AsNoTracking().ToListAsync();

                // ===============================
                // 7️⃣ Group hasil join jadi lookup
                // ===============================
                var indikatorLookup = indikatorData.ToLookup(x => x.IndikatorPengkajianId);

                // ===============================
                // 8️⃣ Gabungkan hasil utama + list join
                // ===============================
                var result = listData.Select(x => new
                {
                    x.DetailResikoJatuhId,
                    x.IndikatorPengkajianId,
                    x.IntervensiResikoJatuhId,
                    x.Keterangan,
                    x.CreateBy,
                    x.CreateByName,
                    x.CreateDateTime,
                    IndikatorDetails = indikatorLookup[x.IndikatorPengkajianId].Select(i => new
                    {
                        i.IndikatorId,
                        i.NamaIndikator,
                        i.IndikatorScoreId,
                        i.NamaIndikatorScore,
                        i.ScoreIndikator
                    }).ToList()
                });

                // ===============================
                // 9️⃣ Return hasil
                // ===============================
                return Ok(new
                {
                    message = "Berhasil || 200 OK",
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    },
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


    }
}
