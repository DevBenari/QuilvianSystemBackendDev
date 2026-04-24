using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class LabHasilController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LabHasilController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LabHasilController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LabHasilController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        private static List<string> ToPhotoLabPaths(string? photoLabPath)
        {
            if (string.IsNullOrWhiteSpace(photoLabPath))
                return new List<string>();

            var s = photoLabPath.Trim();

            // Jika JSON array string
            if (s.StartsWith("[") && s.EndsWith("]"))
            {
                try
                {
                    return (JsonConvert.DeserializeObject<List<string>>(s) ?? new List<string>())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim())
                        .ToList();
                }
                catch
                {
                    return new List<string>();
                }
            }

            // Fallback: format CSV "/a,/b,/c"
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.LabHasils
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         join k in _applicationDbContext.Kunjungans
                         on a.KunjunganId equals k.KunjunganID into kGroup
                         from k in kGroup.DefaultIfEmpty()

                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.HasilLabId,
                             a.KunjunganId,
                             k.JenisKunjungan,
                             a.LabId,
                             a.LabBookingId,
                             a.UserActiveId,
                             a.PenanggungJawabId,
                             a.PenanggungJawabAnalisId,
                             a.TanggalPemeriksaan,
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
            var listdata = (from a in _applicationDbContext.LabHasils
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                            on a.CreateBy equals u.UserActiveId

                            join k in _applicationDbContext.Kunjungans
                            on a.KunjunganId equals k.KunjunganID into kGroup
                            from k in kGroup.DefaultIfEmpty()

                            where a.IsDelete == false || a.IsDelete == null
                            orderby a.CreateDateTime descending
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.HasilLabId,
                                a.KunjunganId,
                                k.JenisKunjungan,
                                a.LabId,
                                a.LabBookingId,
                                a.UserActiveId,
                                a.PenanggungJawabId,
                                a.PenanggungJawabAnalisId,
                                a.TanggalPemeriksaan,
                                a.Keterangan,
                            });
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
        public async Task<IActionResult> Create([FromBody] LabHasilViewModel vm)
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
                //                    .AnyAsync(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama diskon ini telah tersedia" });
                //}

                // **Buat Data Baru**
                var data = new LabHasil
                {
                    HasilLabId = Guid.NewGuid(),
                    KunjunganId =  vm.KunjunganId,
                    LabId =  vm.LabId,
                    LabBookingId = vm.LabBookingId,
                    UserActiveId = vm.UserActiveId,
                    PenanggungJawabAnalisId = vm.PenanggungJawabId,
                    PenanggungJawabId = vm.PenanggungJawabId,
                    TanggalPemeriksaan = vm.TanggalPemeriksaan,
                    Keterangan = vm.Keterangan,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.LabHasils.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] LabHasilViewModel vm)
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
                var data = await _applicationDbContext.LabHasils.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.LabId = vm.LabId;
                data.LabBookingId = vm.LabBookingId;
                data.UserActiveId = vm.UserActiveId;
                data.PenanggungJawabAnalisId = vm.PenanggungJawabId;
                data.PenanggungJawabId = vm.PenanggungJawabId;
                data.TanggalPemeriksaan = vm.TanggalPemeriksaan;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.LabHasils.Update(data);
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
                var data = await _applicationDbContext.LabHasils.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LabHasils.Update(data);
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
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            [FromQuery] string? namaLab = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =========================
            // 1) BASE QUERY (PARENT)
            // =========================
            var query = from a in _applicationDbContext.LabHasils.AsNoTracking()
                        join u0 in _applicationDbContext.UserActives.AsNoTracking()
                            on a.CreateBy equals u0.UserActiveId into uGroup
                        from u in uGroup.DefaultIfEmpty()

                        join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                            on a.KunjunganId equals k0.KunjunganID into kGroup
                        from k in kGroup.DefaultIfEmpty()

                        join l0 in _applicationDbContext.Labs.AsNoTracking()
                            on a.LabId equals l0.LabId into lGroup
                        from l in lGroup.DefaultIfEmpty()

                        where (a.IsDelete == false || a.IsDelete == null)
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,

                            a.HasilLabId,
                            a.KunjunganId,
                            JenisKunjungan = k.JenisKunjungan,

                            a.LabId,
                            NamaLab = l.NamaLab,

                            a.LabBookingId,
                            a.UserActiveId,
                            a.PenanggungJawabId,
                            a.PenanggungJawabAnalisId,
                            a.TanggalPemeriksaan,
                            a.Keterangan
                        };

            // =========================
            // FILTERS
            // =========================
            if (!string.IsNullOrWhiteSpace(namaLab))
            {
                var pattern = $"%{namaLab.ToLower()}%";
                query = query.Where(x => EF.Functions.ILike(x.NamaLab, pattern));
            }

            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range; lebih ramah index dibanding .Date)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart;
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING (aman)
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION (async)
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch, 1 query)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            // Kalau page kosong, langsung return
            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()
                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp.PemeriksaanLabId into lpgroup
                from lp in lpgroup.DefaultIfEmpty()

                join k in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k.KelasId into kgroup
                from k in kgroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId)
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = k != null ? k.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath, // string raw, diproses setelah ToList

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            // Process Photo paths in-memory + group per HasilLabId
            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.KelasId,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            // =========================
            // 3) FINAL: parent + details
            // =========================
            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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


        [HttpGet("paged-HasilLabRadiologi")]
        public async Task<IActionResult> PagedRadiologi(
            int page = 1,
            int perPage = 10,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY (PARENT) - KHUSUS RADIOLOGI
            // =========================
            var query =
                from a in _applicationDbContext.LabHasils.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on a.LabId equals l0.LabId into lGroup
                from l in lGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && l != null
                      && l.NamaLab.ToLower().Replace(" ", "") == "radiologi"
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    a.HasilLabId,
                    a.KunjunganId,
                    JenisKunjungan = k.JenisKunjungan,

                    a.LabId,
                    NamaLab = l.NamaLab,

                    a.LabBookingId,
                    a.UserActiveId,
                    a.PenanggungJawabId,
                    a.PenanggungJawabAnalisId,
                    a.TanggalPemeriksaan,
                    a.Keterangan
                };

            // =========================
            // FILTERS OPSIONAL
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k0.KelasId into kGroup
                from kk in kGroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId) // kalau Guid? lihat catatan di bawah
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = kk != null ? kk.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath,

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.NamaPemeriksaan,
                            x.KelasId,
                            x.NamaKelas,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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

        [HttpGet("paged-HasilLabMCU")]
        public async Task<IActionResult> PagedMCU(
            int page = 1,
            int perPage = 10,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY (PARENT) - KHUSUS MCU
            // =========================
            var query =
                from a in _applicationDbContext.LabHasils.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on a.LabId equals l0.LabId into lGroup
                from l in lGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && l != null
                      && l.NamaLab.ToLower().Replace(" ", "") == "mcu"
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    a.HasilLabId,
                    a.KunjunganId,
                    JenisKunjungan = k.JenisKunjungan,

                    a.LabId,
                    NamaLab = l.NamaLab,

                    a.LabBookingId,
                    a.UserActiveId,
                    a.PenanggungJawabId,
                    a.PenanggungJawabAnalisId,
                    a.TanggalPemeriksaan,
                    a.Keterangan
                };

            // =========================
            // FILTERS OPSIONAL
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k0.KelasId into kGroup
                from kk in kGroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId) // kalau Guid? lihat catatan di bawah
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = kk != null ? kk.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath,

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.NamaPemeriksaan,
                            x.KelasId,
                            x.NamaKelas,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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

        [HttpGet("paged-HasilLabGizi")]
        public async Task<IActionResult> PagedGizi(
            int page = 1,
            int perPage = 10,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY (PARENT) - KHUSUS MCU
            // =========================
            var query =
                from a in _applicationDbContext.LabHasils.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on a.LabId equals l0.LabId into lGroup
                from l in lGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && l != null
                      && l.NamaLab.ToLower().Replace(" ", "") == "gizi"
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    a.HasilLabId,
                    a.KunjunganId,
                    JenisKunjungan = k.JenisKunjungan,

                    a.LabId,
                    NamaLab = l.NamaLab,

                    a.LabBookingId,
                    a.UserActiveId,
                    a.PenanggungJawabId,
                    a.PenanggungJawabAnalisId,
                    a.TanggalPemeriksaan,
                    a.Keterangan
                };

            // =========================
            // FILTERS OPSIONAL
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k0.KelasId into kGroup
                from kk in kGroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId) // kalau Guid? lihat catatan di bawah
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = kk != null ? kk.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath,

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.NamaPemeriksaan,
                            x.KelasId,
                            x.NamaKelas,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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

        [HttpGet("paged-HasilLabRehabMedis")]
        public async Task<IActionResult> PagedRehabMedis(
            int page = 1,
            int perPage = 10,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY (PARENT) - KHUSUS MCU
            // =========================
            var query =
                from a in _applicationDbContext.LabHasils.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on a.LabId equals l0.LabId into lGroup
                from l in lGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && l != null
                      && l.NamaLab.ToLower().Replace(" ", "") == "rehabmedis"
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    a.HasilLabId,
                    a.KunjunganId,
                    JenisKunjungan = k.JenisKunjungan,

                    a.LabId,
                    NamaLab = l.NamaLab,

                    a.LabBookingId,
                    a.UserActiveId,
                    a.PenanggungJawabId,
                    a.PenanggungJawabAnalisId,
                    a.TanggalPemeriksaan,
                    a.Keterangan
                };

            // =========================
            // FILTERS OPSIONAL
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k0.KelasId into kGroup
                from kk in kGroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId) // kalau Guid? lihat catatan di bawah
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = kk != null ? kk.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath,

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.NamaPemeriksaan,
                            x.KelasId,
                            x.NamaKelas,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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


        [HttpGet("paged-HasilLabPatologiAnatomi")]
        public async Task<IActionResult> PagedPatologiAnatomi(
            int page = 1,
            int perPage = 10,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY (PARENT) - KHUSUS MCU
            // =========================
            var query =
                from a in _applicationDbContext.LabHasils.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on a.LabId equals l0.LabId into lGroup
                from l in lGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && l != null
                      && l.NamaLab.ToLower().Replace(" ", "") == "patologianatomi"
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    a.HasilLabId,
                    a.KunjunganId,
                    JenisKunjungan = k.JenisKunjungan,

                    a.LabId,
                    NamaLab = l.NamaLab,

                    a.LabBookingId,
                    a.UserActiveId,
                    a.PenanggungJawabId,
                    a.PenanggungJawabAnalisId,
                    a.TanggalPemeriksaan,
                    a.Keterangan
                };

            // =========================
            // FILTERS OPSIONAL
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k0.KelasId into kGroup
                from kk in kGroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId) // kalau Guid? lihat catatan di bawah
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = kk != null ? kk.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath,

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.NamaPemeriksaan,
                            x.KelasId,
                            x.NamaKelas,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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

        [HttpGet("paged-HasilLabMicrobiologi")]
        public async Task<IActionResult> PagedMicrobiologi(
            int page = 1,
            int perPage = 10,
            [FromQuery] EnumJenisKunjungan? JenisKunjungan = null,
            [FromQuery] Guid? kunjunganId = null,
            [FromQuery] Guid? labbookingid = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100;

            // =========================
            // 1) BASE QUERY (PARENT) - KHUSUS MCU
            // =========================
            var query =
                from a in _applicationDbContext.LabHasils.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on a.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on a.KunjunganId equals k0.KunjunganID into kGroup
                from k in kGroup.DefaultIfEmpty()

                join l0 in _applicationDbContext.Labs.AsNoTracking()
                    on a.LabId equals l0.LabId into lGroup
                from l in lGroup.DefaultIfEmpty()

                where (a.IsDelete == false || a.IsDelete == null)
                      && l != null
                      && l.NamaLab.ToLower().Replace(" ", "") == "microbiologi"
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    a.HasilLabId,
                    a.KunjunganId,
                    JenisKunjungan = k.JenisKunjungan,

                    a.LabId,
                    NamaLab = l.NamaLab,

                    a.LabBookingId,
                    a.UserActiveId,
                    a.PenanggungJawabId,
                    a.PenanggungJawabAnalisId,
                    a.TanggalPemeriksaan,
                    a.Keterangan
                };

            // =========================
            // FILTERS OPSIONAL
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            if (labbookingid.HasValue)
                query = query.Where(x => x.LabBookingId == labbookingid.Value);

            if (JenisKunjungan.HasValue)
                query = query.Where(x => x.JenisKunjungan == JenisKunjungan.Value.ToString());

            // Filter tanggal (range UTC)
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero); // exclusive
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime < endUtc);
            }

            // Filter periode (range)
            if (periode.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                DateTimeOffset start;
                DateTimeOffset end; // exclusive

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = todayStart; end = todayStart.AddDays(1); break;

                    case PeriodeFilter.ThisWeek:
                        start = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        end = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisMonthStart.AddMonths(-1);
                        end = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        end = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                        start = thisYearStart.AddYears(-1);
                        end = thisYearStart;
                        break;

                    case PeriodeFilter.Last3Months:
                        start = todayStart.AddMonths(-3);
                        end = todayStart.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = todayStart.AddMonths(-6);
                        end = todayStart.AddDays(1);
                        break;

                    default:
                        start = DateTimeOffset.MinValue;
                        end = DateTimeOffset.MaxValue;
                        break;
                }

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < end);
            }

            // =========================
            // SORTING
            // =========================
            bool desc = (sortDirection ?? "desc").ToLower() == "desc";

            query = (orderBy ?? "CreateDateTime") switch
            {
                "CreateDateTime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "CreateByName" => desc ? query.OrderByDescending(x => x.CreateByName) : query.OrderBy(x => x.CreateByName),
                "NamaLab" => desc ? query.OrderByDescending(x => x.NamaLab) : query.OrderBy(x => x.NamaLab),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================
            // PAGINATION
            // =========================
            var totalRows = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var parentRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync(ct);

            if (parentRows.Count == 0 && page > totalPages && totalRows > 0)
                return NotFound(new { message = "Page not found." });

            // =========================
            // 2) DETAILS (batch)
            // =========================
            var hasilLabIds = parentRows.Select(x => x.HasilLabId).Distinct().ToList();

            if (hasilLabIds.Count == 0)
            {
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Rows = parentRows.Select(p => new
                        {
                            p.CreateDateTime,
                            p.CreateBy,
                            p.CreateByName,
                            p.HasilLabId,
                            p.KunjunganId,
                            p.JenisKunjungan,
                            p.LabId,
                            p.NamaLab,
                            p.LabBookingId,
                            p.UserActiveId,
                            p.PenanggungJawabId,
                            p.PenanggungJawabAnalisId,
                            p.TanggalPemeriksaan,
                            p.Keterangan,
                            Details = new List<object>()
                        }).ToList(),
                        TotalRows = totalRows,
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalPages = totalPages
                    }
                });
            }

            var detailRaw = await (
                from d in _applicationDbContext.LabHasilDetails.AsNoTracking()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()

                join lp0 in _applicationDbContext.LabPemeriksaans.AsNoTracking()
                    on d.PemeriksaanLabId equals lp0.PemeriksaanLabId into lpGroup
                from lp in lpGroup.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass.AsNoTracking()
                    on d.KelasId equals k0.KelasId into kGroup
                from kk in kGroup.DefaultIfEmpty()

                where (d.IsDelete == false || d.IsDelete == null)
                      && hasilLabIds.Contains((Guid)d.HasilLabId) // kalau Guid? lihat catatan di bawah
                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u.FullName,

                    d.DetailHasilLabId,
                    d.HasilLabId,
                    d.PemeriksaanLabId,
                    NamaPemeriksaan = lp != null ? lp.NamaPemeriksaan : null,
                    d.KelasId,
                    NamaKelas = kk != null ? kk.NamaKelas : null,
                    d.TanggalSelesai,
                    d.NoPhotoLab,

                    PhotoLabPathRaw = d.PhotoLabPath,

                    d.HasilLabManual,
                    d.HasilLabAI,
                    d.JumlahFilm,
                    d.KeadaanSpecimen,
                    d.AnalisId,
                    d.IsDefinitif,
                    d.IsDuplu,
                    d.HasilMakroskopik,
                    d.HasilMikroskopik,
                    d.KesimpulanHasil,
                    d.NilaiNormal,
                    d.BloodVolume,
                    d.SputumVolume,
                    d.UrineVolume,
                    d.PusVolume,
                    d.StoolVolume,
                    d.JaringanVolume,
                    d.BodyFluidVolume,
                    d.PetugasSpecimenId,
                    d.TanggalSpecimen,
                    d.JamSpecimen,
                    d.InfoNReff,
                    d.Kondisi,
                    d.KategoriGC,
                    d.Rincian,
                    d.Anjuran,
                    d.DiagnosisPA,
                    d.Keterangan
                }
            ).ToListAsync(ct);

            var detailsLookup = detailRaw
                .Select(x =>
                {
                    var paths = ToPhotoLabPaths(x.PhotoLabPathRaw);
                    return new
                    {
                        x.HasilLabId,
                        Row = new
                        {
                            x.CreateDateTime,
                            x.CreateBy,
                            x.CreateByName,
                            x.DetailHasilLabId,
                            x.HasilLabId,
                            x.PemeriksaanLabId,
                            x.NamaPemeriksaan,
                            x.KelasId,
                            x.NamaKelas,
                            x.TanggalSelesai,
                            x.NoPhotoLab,
                            PhotoLabPath = paths,
                            JumlahFotoLab = paths.Count,
                            x.HasilLabManual,
                            x.HasilLabAI,
                            x.JumlahFilm,
                            x.KeadaanSpecimen,
                            x.AnalisId,
                            x.IsDefinitif,
                            x.IsDuplu,
                            x.HasilMakroskopik,
                            x.HasilMikroskopik,
                            x.KesimpulanHasil,
                            x.NilaiNormal,
                            x.BloodVolume,
                            x.SputumVolume,
                            x.UrineVolume,
                            x.PusVolume,
                            x.StoolVolume,
                            x.JaringanVolume,
                            x.BodyFluidVolume,
                            x.PetugasSpecimenId,
                            x.TanggalSpecimen,
                            x.JamSpecimen,
                            x.InfoNReff,
                            x.Kondisi,
                            x.KategoriGC,
                            x.Rincian,
                            x.Anjuran,
                            x.DiagnosisPA,
                            x.Keterangan
                        }
                    };
                })
                .GroupBy(x => x.HasilLabId)
                .ToDictionary(g => g.Key, g => g.Select(v => (object)v.Row).ToList());

            var rows = parentRows.Select(p => new
            {
                p.CreateDateTime,
                p.CreateBy,
                p.CreateByName,
                p.HasilLabId,
                p.KunjunganId,
                p.JenisKunjungan,
                p.LabId,
                p.NamaLab,
                p.LabBookingId,
                p.UserActiveId,
                p.PenanggungJawabId,
                p.PenanggungJawabAnalisId,
                p.TanggalPemeriksaan,
                p.Keterangan,
                Details = detailsLookup.TryGetValue(p.HasilLabId, out var det) ? det : new List<object>()
            }).ToList();

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

