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
                        //int hariKe = await _applicationDbContext.InfeksiDetails
                        //    .CountAsync(x =>x.KunjunganId == vm.KunjunganId) + 1;

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

                            HariKe = d.HariKe,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] InfeksiLOViewModel vm)
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
                // 🔹 Ambil Parent Infeksi LO
                // ===========================
                var infeksi = await _applicationDbContext.InfeksiLOs
                    .FirstOrDefaultAsync(x => x.InfeksiLOId == id);

                if (infeksi == null)
                    return NotFound(new { message = "Data Infeksi LO tidak ditemukan!" });


                // ===========================
                // 🔹 UPDATE Parent
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
                // 🔹 Ambil DETAIL lama
                // ===========================
                var existingDetails = await _applicationDbContext.InfeksiDetails
                    .Where(x => x.InfeksiId == id)
                    .ToListAsync();


                // ===========================
                // 🔹 UPDATE DETAIL (TIDAK TAMBAH BARU)
                // ===========================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        // Jika DetailInfeksiId kosong → skip → tidak boleh add baru
                        if (d.InfeksiId == null || d.InfeksiId == Guid.Empty)
                            continue;

                        var existing = existingDetails
                            .FirstOrDefault(e => e.InfeksiId == id);

                        if (existing == null)
                            continue;    // detail tidak ditemukan → skip

                        // ===========================
                        // 🔹 Update vital (IsDemam)
                        // ===========================
                        var vital = await _applicationDbContext.VitalSigns
                            .Where(v => v.KunjunganId == vm.KunjunganId)
                            .OrderByDescending(v => v.CreateDateTime)
                            .FirstOrDefaultAsync();

                        decimal? suhu = vital?.Suhu;


                        // ===========================
                        // 🔹 UPDATE DETAIL EXISTING
                        // ===========================
                        existing.HariKe = d.HariKe;
                        existing.LokasiReaksi = d.LokasiReaksi;
                        existing.TglMulaiReaksi = d.TglMulaiReaksi;
                        existing.TglAkhirReaksi = d.TglAkhirReaksi;
                        existing.Nyeri = d.Nyeri;
                        existing.Merah = d.Merah;
                        existing.Bengkak = d.Bengkak;
                        existing.PUS = d.PUS;
                        existing.Menggigil = d.Menggigil;
                        existing.IsDemam = d.IsDemam ?? (suhu >= 38);
                        existing.Drainase = d.Drainase;
                        existing.Perforasi = d.Perforasi;
                        existing.Fistula = d.Fistula;
                        existing.NyeriSupraPublik = d.NyeriSupraPublik;
                        existing.NyeriSaatBerkemih = d.NyeriSaatBerkemih;
                        existing.PasangDCKe = d.PasangDCKe;
                        existing.AnyangAnyangan = d.AnyangAnyangan;
                        existing.Gatal = d.Gatal;
                        existing.Keterangan = d.Keterangan;

                        existing.UpdateBy = userActiveId;
                        existing.UpdateDateTime = DateTimeOffset.UtcNow;
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }


                // ===========================
                // 🔹 Commit transaksi
                // ===========================
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update Infeksi LO + detail berhasil (tanpa tambah detail baru)"
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

                // ================================
                // 🔹 Soft Delete Semua Detail
                // ================================
                var details = await _applicationDbContext.InfeksiDetails
                    .Where(d => d.InfeksiId == id && (d.IsDelete == false || d.IsDelete == null))
                    .ToListAsync();

                if (details.Any())
                {
                    foreach (var d in details)
                    {
                        d.DeleteBy = userActiveId;
                        d.DeleteDateTime = DateTimeOffset.UtcNow;
                        d.IsDelete = true;
                    }

                    _applicationDbContext.InfeksiDetails.UpdateRange(details);
                }
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
        public async Task<IActionResult> PagedAsync(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            DateTime? startDate = null,
            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            if (page <= 0) page = 1;
            if (perPage <= 0) perPage = 10;

            // ======================================================
            // 1) BASE QUERY
            // ======================================================
            var query = _applicationDbContext.InfeksiLOs
                .AsNoTracking()
                .Where(x => x.IsDelete == false || x.IsDelete == null);

            // filter kunjungan
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            // filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                var s = startDate.Value.Date.ToUniversalTime();
                var e = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(x => x.CreateDateTime >= s && x.CreateDateTime <= e);
            }

            // filter periode
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                DateTime start;
                DateTime end = today.AddDays(1).AddTicks(-1);

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        start = today;
                        break;
                    case PeriodeFilter.ThisWeek:
                        start = today.AddDays(-(int)today.DayOfWeek);
                        break;
                    case PeriodeFilter.LastWeek:
                        start = today.AddDays(-7 - (int)today.DayOfWeek);
                        end = today.AddDays(-(int)today.DayOfWeek).AddTicks(-1);
                        break;
                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        break;
                    case PeriodeFilter.LastMonth:
                        var last = today.AddMonths(-1);
                        start = new DateTime(last.Year, last.Month, 1);
                        end = new DateTime(last.Year, last.Month,
                                DateTime.DaysInMonth(last.Year, last.Month)).AddDays(1).AddTicks(-1);
                        break;
                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        break;
                    case PeriodeFilter.LastYear:
                        start = new DateTime(today.Year - 1, 1, 1);
                        end = new DateTime(today.Year - 1, 12, 31).AddDays(1).AddTicks(-1);
                        break;
                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        break;
                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        break;
                    default:
                        start = DateTime.MinValue;
                        break;
                }

                query = query.Where(x =>
                    x.CreateDateTime >= start.ToUniversalTime() &&
                    x.CreateDateTime <= end.ToUniversalTime());
            }

            // sort
            bool desc = sortDirection?.ToLower() == "desc";

            query = (orderBy, desc) switch
            {
                ("CreateDateTime", true) => query.OrderByDescending(x => x.CreateDateTime),
                ("CreateDateTime", false) => query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime)
                          : query.OrderBy(x => x.CreateDateTime)
            };

            // count
            var total = await query.CountAsync();

            var parents = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            if (!parents.Any())
            {
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
            }

            // ======================================================
            // 2) JOIN RUANG BEDAH BOOKING (Tanpa N+1)
            // ======================================================
            var parentIds = parents.Select(p => p.KunjunganId).ToList();

            var bookings = await _applicationDbContext.RuangBedahBookings
                .AsNoTracking()
                .Where(b => parentIds.Contains(b.KunjunganId))
                .ToListAsync();

            var bookingLookup = bookings
                .ToDictionary(b => b.KunjunganId, b => b);

            // ======================================================
            // 3) JOIN DOKTER OPERATOR 1
            // ======================================================
            var dokterIds = bookings
                .Where(b => b.DokterOperator1 != null)
                .Select(b => b.DokterOperator1.Value)
                .Distinct()
                .ToList();

            var dokterList = await _applicationDbContext.UserActives
                .AsNoTracking()
                .Where(d => dokterIds.Contains(d.UserActiveId))
                .ToListAsync();

            var dokterLookup = dokterList.ToDictionary(d => d.UserActiveId, d => d.FullName);

            // ======================================================
            // 4) JOIN DETAIL INFKSI LO
            // ======================================================
            var infeksiIds = parents.Select(p => p.InfeksiLOId).ToList();

            var details = await _applicationDbContext.InfeksiDetails
                .AsNoTracking()
                .Where(d => infeksiIds.Contains((Guid)d.InfeksiId))
                .ToListAsync();

            var detailLookup = details
                .GroupBy(d => d.InfeksiId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ======================================================
            // 5) MERGE DATA
            // ======================================================
            var final = parents.Select(p =>
            {
                bookingLookup.TryGetValue(p.KunjunganId ?? Guid.Empty, out var booking);

                var dokterOperator1 = booking != null &&
                                      booking.DokterOperator1.HasValue &&
                                      dokterLookup.ContainsKey(booking.DokterOperator1.Value)
                                      ? dokterLookup[booking.DokterOperator1.Value]
                                      : null;

                // itung jam perpanjangan
                int totalMinutes = 0;

                if (booking != null)
                {
                    // Lama operasi berdasarkan WaktuOperasi (interval)
                    if (booking.WaktuOperasi.HasValue)
                        totalMinutes += (int)booking.WaktuOperasi.Value.TotalMinutes;

                    // Tambahan jam operasi (TimeOnly)
                    if (booking.JamPerpanjangan.HasValue)
                    {
                        var jp = booking.JamPerpanjangan.Value;
                        totalMinutes += (jp.Hour * 60) + jp.Minute;
                    }
                }

                return new
                {
                    Parent = p,
                    Details = detailLookup.ContainsKey(p.InfeksiLOId)
                                ? detailLookup[p.InfeksiLOId]
                                : new List<InfeksiDetail>(),

                    Operasi = new
                    {
                        TindakanOperasi = booking?.RencanaTindakanOperasi,
                        KamarOperasi = booking?.RuangTindakan,
                        TanggalOperasi = booking?.TglOperasi,
                        DokterOperator1 = dokterOperator1,
                        LamaOperasiMenit = totalMinutes
                    }
                };
            });

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = final,
                    TotalRows = total,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(total / (double)perPage)
                }
            });
        }


    }
}
