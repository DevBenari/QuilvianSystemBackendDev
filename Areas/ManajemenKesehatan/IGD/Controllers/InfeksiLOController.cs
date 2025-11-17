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

            try
            {
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user dari JWT token
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Auto increment Ronde berdasarkan pasien
                var lastRonde = await _applicationDbContext.InfeksiLOs
                    .Where(x => x.PasienId == vm.PasienId)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => x.RondeKe)
                    .FirstOrDefaultAsync();

                string rondeBaru = "1";
                if (int.TryParse(lastRonde, out int lastNum))
                    rondeBaru = (lastNum + 1).ToString();

                // ✅ Mapping ke entity
                var data = new InfeksiLO
                {
                    InfeksiLOId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    IsDarurat = vm.IsDarurat,
                    IsAnastesiUmum = vm.IsAnastesiUmum,
                    RondeKe = rondeBaru,
                    IsTrauma = vm.IsTrauma,
                    IsProsedurMultiple = vm.IsProsedurMultiple,
                    ASAScore = vm.ASAScore,
                    IsHbsag = vm.IsHbsag,
                    IsAntiHCV = vm.IsAntiHCV,
                    HasilLabLeukosit = vm.HasilLabLeukosit,
                    HasilLabHB = vm.HasilLabHB,
                    TglPencatatan = vm.TglPencatatan ?? DateTime.UtcNow,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.InfeksiLOs.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new
                    {
                        message = "Tambah Data Infeksi Luka Operasi Berhasil || 201 Created",
                        data = new
                        {
                            data.InfeksiLOId,
                            data.KunjunganId,
                            data.PasienId,
                            data.RondeKe,
                            data.IsDarurat,
                            data.IsAnastesiUmum,
                            data.IsTrauma,
                            data.ASAScore
                        }
                    });
                }

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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
        public async Task<IActionResult> Update(Guid id, [FromBody] InfeksiLOViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user dari JWT token
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cek apakah data ada di database
                var existingData = await _applicationDbContext.InfeksiLOs
                    .FirstOrDefaultAsync(x => x.InfeksiLOId == id && (x.IsDelete == false || x.IsDelete == null));

                if (existingData == null)
                    return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });

                // ==================================================
                // 🔹 Update field yang boleh diubah
                // ==================================================
                existingData.KunjunganId = vm.KunjunganId ?? existingData.KunjunganId;
                existingData.PasienId = vm.PasienId ?? existingData.PasienId;
                existingData.IsDarurat = vm.IsDarurat ?? existingData.IsDarurat;
                existingData.IsAnastesiUmum = vm.IsAnastesiUmum ?? existingData.IsAnastesiUmum;
                existingData.IsTrauma = vm.IsTrauma ?? existingData.IsTrauma;
                existingData.IsProsedurMultiple = vm.IsProsedurMultiple ?? existingData.IsProsedurMultiple;
                existingData.ASAScore = vm.ASAScore ?? existingData.ASAScore;
                existingData.IsHbsag = vm.IsHbsag ?? existingData.IsHbsag;
                existingData.IsAntiHCV = vm.IsAntiHCV ?? existingData.IsAntiHCV;
                existingData.HasilLabLeukosit = vm.HasilLabLeukosit ?? existingData.HasilLabLeukosit;
                existingData.HasilLabHB = vm.HasilLabHB ?? existingData.HasilLabHB;
                existingData.TglPencatatan = vm.TglPencatatan ?? existingData.TglPencatatan;
                existingData.Keterangan = vm.Keterangan ?? existingData.Keterangan;

                // 🔹 Metadata update
                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                // ==================================================
                // ✅ Simpan ke database
                // ==================================================
                _applicationDbContext.InfeksiLOs.Update(existingData);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new
                    {
                        message = "Update Data Infeksi Luka Operasi Berhasil || 200 OK",
                        data = new
                        {
                            existingData.InfeksiLOId,
                            existingData.KunjunganId,
                            existingData.PasienId,
                            existingData.RondeKe,
                            existingData.IsDarurat,
                            existingData.IsAnastesiUmum,
                            existingData.ASAScore,
                            existingData.IsHbsag,
                            existingData.IsAntiHCV,
                            existingData.HasilLabHB,
                            existingData.HasilLabLeukosit
                        }
                    });
                }

                return StatusCode(500, new { message = "Tidak ada perubahan data atau gagal menyimpan ke database." });
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
