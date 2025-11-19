using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.IGD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class InfeksiLOController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<InfeksiLOController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InfeksiLOController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<InfeksiLOController> logger,
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
            var query = (from a in _applicationDbContext.InfeksiLOs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.InfeksiLOId,
                             a.KunjunganId,
                             a.PasienId,
                             a.IsDarurat,
                             a.IsAnastesiUmum,
                             a.RondeKe,
                             a.IsTrauma,
                             a.IsProsedurMultiple,
                             a.ASAScore,
                             a.IsHbsag,
                             a.IsAntiHCV,
                             a.HasilLabHB,
                             a.HasilLabLeukosit,
                             a.TglPencatatan,
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
            var listdata = _applicationDbContext.InfeksiLOs.Find(id);
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
        public async Task<IActionResult> Create([FromBody] InfeksiLOViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ===========================
                // 🔹 Ambil User Login
                // ===========================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (emailLogin == null)
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = user.UserActiveId;

                // ===========================
                // 🔹 Insert Parent : InfeksiIO
                // ===========================
                var infeksiId = Guid.NewGuid();

                var infeksi = new InfeksiLO
                {
                    InfeksiLOId = infeksiId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    IsDarurat = vm.IsDarurat,
                    IsAnastesiUmum = vm.IsAnastesiUmum,
                    RondeKe = vm.RondeKe,
                    IsTrauma = vm.IsTrauma,
                    IsProsedurMultiple = vm.IsProsedurMultiple,
                    ASAScore = vm.ASAScore,
                    IsHbsag = vm.IsHbsag,
                    IsAntiHCV = vm.IsAntiHCV,
                    HasilLabLeukosit = vm.HasilLabLeukosit,
                    HasilLabHB = vm.HasilLabHB,
                    TglPencatatan = vm.TglPencatatan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.InfeksiLOs.Add(infeksi);
                await _applicationDbContext.SaveChangesAsync();


                // ===========================
                // 🔹 Insert Child : InfeksiDetail
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // ===========================
                        // 🔹 Hitung Hari Ke Otomatis
                        // ===========================
                        int hariKe = await _applicationDbContext.InfeksiDetails
                            .CountAsync(x =>x.KunjunganId == vm.KunjunganId) + 1;

                        // ===========================
                        // 🔹 Ambil suhu vital sign terbaru
                        // ===========================
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;

                        var detail = new InfeksiDetail
                        {
                            DetailInfeksiId = Guid.NewGuid(),
                            InfeksiId = infeksiId,
                            KunjunganId = vm.KunjunganId,
                            PasienId = vm.PasienId,

                            HariKe = hariKe,
                            LokasiReaksi = d.LokasiReaksi,
                            TglMulaiReaksi = d.TglMulaiReaksi,
                            TglAkhirReaksi = d.TglAkhirReaksi,
                            Nyeri = d.Nyeri,
                            Merah = d.Merah,
                            Bengkak = d.Bengkak,
                            PUS = d.PUS,
                            Menggigil = d.Menggigil,
                            IsDemam = d.IsDemam ?? suhu >= 38,      // 🔥 fallback dari Suhu
                            Drainase = d.Drainase,
                            Perforasi = d.Perforasi,
                            Fistula = d.Fistula,
                            NyeriSupraPublik = d.NyeriSupraPublik,
                            NyeriSaatBerkemih = d.NyeriSaatBerkemih,
                            PasangDCKe = d.PasangDCKe,
                            AnyangAnyangan = d.AnyangAnyangan,
                            Gatal = d.Gatal,
                            Keterangan = d.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.InfeksiDetails.Add(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Tambah Data Infeksi IO + Detail berhasil",
                    InfeksiId = infeksiId,
                    JumlahDetail = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid infeksiId, [FromBody] InfeksiLOViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ===========================
                // 🔹 Ambil user login
                // ===========================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (emailLogin == null)
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(x => x.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = user.UserActiveId;


                // ===========================
                // 🔹 Ambil Data Parent
                // ===========================
                var infeksi = await _applicationDbContext.InfeksiLOs
                    .FirstOrDefaultAsync(x => x.InfeksiLOId == infeksiId);

                if (infeksi == null)
                    return NotFound(new { message = "Data infeksi tidak ditemukan!" });


                // ===========================
                // 🔹 Update parent
                // ===========================
                infeksi.KunjunganId = vm.KunjunganId;
                infeksi.PasienId = vm.PasienId;
                infeksi.IsDarurat = vm.IsDarurat;
                infeksi.IsAnastesiUmum = vm.IsAnastesiUmum;
                infeksi.RondeKe = vm.RondeKe;
                infeksi.IsTrauma = vm.IsTrauma;
                infeksi.IsProsedurMultiple = vm.IsProsedurMultiple;
                infeksi.ASAScore = vm.ASAScore;
                infeksi.IsHbsag = vm.IsHbsag;
                infeksi.IsAntiHCV = vm.IsAntiHCV;
                infeksi.HasilLabLeukosit = vm.HasilLabLeukosit;
                infeksi.HasilLabHB = vm.HasilLabHB;
                infeksi.TglPencatatan = vm.TglPencatatan;
                infeksi.Keterangan = vm.Keterangan;

                infeksi.UpdateBy = userActiveId;
                infeksi.UpdateDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();


                // ===========================
                // 🔹 Hapus detail lama
                // ===========================
                var oldDetails = await _applicationDbContext.InfeksiDetails
                    .Where(x => x.InfeksiId == infeksiId)
                    .ToListAsync();

                if (oldDetails.Any())
                {
                    _applicationDbContext.InfeksiDetails.RemoveRange(oldDetails);
                    await _applicationDbContext.SaveChangesAsync();
                }


                // ===========================
                // 🔹 Insert detail baru
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {

                    foreach (var d in vm.Details)
                    {
                        // 🔹 Hitung Hari Ke otomatis
                        int hariKe = await _applicationDbContext.InfeksiDetails
                            .CountAsync(x => x.InfeksiId == infeksiId
                                             && x.KunjunganId == vm.KunjunganId) + 1;

                        // 🔹 Ambil suhu terbaru
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;

                        var detail = new InfeksiDetail
                        {
                            DetailInfeksiId = Guid.NewGuid(),
                            InfeksiId = infeksiId,
                            KunjunganId = vm.KunjunganId,
                            PasienId = vm.PasienId,

                            HariKe = hariKe,

                            LokasiReaksi = d.LokasiReaksi,
                            TglMulaiReaksi = d.TglMulaiReaksi,
                            TglAkhirReaksi = d.TglAkhirReaksi,
                            Nyeri = d.Nyeri,
                            Merah = d.Merah,
                            Bengkak = d.Bengkak,
                            PUS = d.PUS,
                            Menggigil = d.Menggigil,
                            IsDemam = d.IsDemam ?? suhu >= 38,
                            Drainase = d.Drainase,
                            Perforasi = d.Perforasi,
                            Fistula = d.Fistula,
                            NyeriSupraPublik = d.NyeriSupraPublik,
                            NyeriSaatBerkemih = d.NyeriSaatBerkemih,
                            PasangDCKe = d.PasangDCKe,
                            AnyangAnyangan = d.AnyangAnyangan,
                            Gatal = d.Gatal,
                            Keterangan = d.Keterangan,

                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        await _applicationDbContext.InfeksiDetails.AddAsync(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }


                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update data Infeksi IO + Detail berhasil",
                    InfeksiId = infeksiId,
                    JumlahDetailBaru = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
                var data = await _applicationDbContext.InfeksiLOs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.InfeksiLOs.Update(data);
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
        Guid? kunjunganId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.InfeksiLOs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null

                         // join ke booking ruang bedah
                         join b in _applicationDbContext.RuangBedahBookings
                         on a.KunjunganId equals b.KunjunganId into bGroup
                         from b in bGroup.DefaultIfEmpty()

                             // Join ke tabel Dokter untuk masing-masing operator
                         join d1 in _applicationDbContext.Dokters on b.DokterOperator1 equals d1.DokterId into d1Group
                         from d1 in d1Group.DefaultIfEmpty()

                         join d2 in _applicationDbContext.Dokters on b.DokterOperator2 equals d2.DokterId into d2Group
                         from d2 in d2Group.DefaultIfEmpty()

                         join d3 in _applicationDbContext.Dokters on b.DokterOperator3 equals d3.DokterId into d3Group
                         from d3 in d3Group.DefaultIfEmpty()

                         join d4 in _applicationDbContext.Dokters on b.DokterOperator4 equals d4.DokterId into d4Group
                         from d4 in d4Group.DefaultIfEmpty()

                         join d5 in _applicationDbContext.Dokters on b.DokterOperator5 equals d5.DokterId into d5Group
                         from d5 in d5Group.DefaultIfEmpty()
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.InfeksiLOId,
                             a.KunjunganId,
                             a.PasienId,
                             a.IsDarurat,
                             a.IsAnastesiUmum,
                             a.RondeKe,
                             a.IsTrauma,
                             a.IsProsedurMultiple,
                             a.ASAScore,
                             a.IsHbsag,
                             a.IsAntiHCV,
                             a.HasilLabHB,
                             a.HasilLabLeukosit,
                             a.TglPencatatan,
                             a.Keterangan,

                             // ttg operasi
                             TindakanOperasi = b.RencanaTindakanOperasi ?? null,
                             JenisOperasi = b.TipeOperasi ?? null,
                             WaktuOperasi = b.WaktuOperasi ?? null,
                             JamPerpanjangan = b.JamPerpanjangan ?? null,
                             TipeAnastesi = b.JenisAnastesi ?? null,
                             RuanganOP = b.RuangTindakan ?? null,
                             LamaOperasi = (b.WaktuOperasi != null)
                                ? (b.JamPerpanjangan != null
                                    ? $"{b.WaktuOperasi.Value.Add(b.JamPerpanjangan.Value.ToTimeSpan()).Hours:D2}:{b.WaktuOperasi.Value.Add(b.JamPerpanjangan.Value.ToTimeSpan()).Minutes:D2}"
                                    : $"{b.WaktuOperasi.Value.Hours:D2}:{b.WaktuOperasi.Value.Minutes:D2}")
                                : "-",
                             // nama dokter operator
                             DokterOperator1 = d1.NmDokter ?? null,
                             DokterOperator2 = d2.NmDokter ?? null,
                             DokterOperator3 = d3.NmDokter ?? null,
                             DokterOperator4 = d4.NmDokter ?? null,
                             DokterOperator5 = d5.NmDokter ?? null
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter by kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
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
