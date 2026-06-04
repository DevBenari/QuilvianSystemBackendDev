using System.Security.Claims;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class LaporanBedahController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<LaporanBedahController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LaporanBedahController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LaporanBedahController> logger,
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
            var query = (from a in _applicationDbContext.LaporanBedahs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.LaporanBedahId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TindakanId,
                             a.DetailTindakan,
                             a.DokterOperatorId,
                             a.DokterAnestesiId,
                             a.DokterAsistenId,
                             a.AsistenAnestesiId,
                             a.PerawatId,
                             a.JenisAnestesi,
                             a.DiagnosaPostOp,
                             a.DiagnosaPraOp,
                             a.JaringanEksisiInsisi,
                             a.TipeUrgensi,
                             a.IsPemeriksaanPA,
                             a.TanggalOperasi,
                             a.WaktuMulaiOperasi,
                             a.WaktuSelesaiOperasi,
                             a.DurasiOperasi,
                             a.LaporanOperasi,
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
            var listdata = _applicationDbContext.LaporanBedahs.Find(id);
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
        public async Task<IActionResult> Create([FromBody] LaporanBedahViewModel vm)
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

                // **Buat Data Baru**
                var data = new LaporanBedah
                {
                    LaporanBedahId = Guid.NewGuid(), 
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TindakanId = vm.TindakanId,
                    DetailTindakan = vm.DetailTindakan,
                    DokterOperatorId = vm.DokterOperatorId,
                    DokterAnestesiId = vm.DokterAnestesiId,
                    DokterAsistenId = vm.DokterAsistenId,
                    AsistenAnestesiId = vm.AsistenAnestesiId,
                    PerawatId = vm.PerawatId,
                    JenisAnestesi = vm.JenisAnestesi,
                    DiagnosaPraOp = vm.DiagnosaPraOp,
                    DiagnosaPostOp = vm.DiagnosaPostOp,
                    JaringanEksisiInsisi = vm.JaringanEksisiInsisi,
                    TipeUrgensi = vm.TipeUrgensi,
                    IsPemeriksaanPA = vm.IsPemeriksaanPA,
                    TanggalOperasi = vm.TanggalOperasi,
                    WaktuMulaiOperasi = vm.WaktuMulaiOperasi,
                    WaktuSelesaiOperasi = vm.WaktuSelesaiOperasi,
                    DurasiOperasi = vm.DurasiOperasi,
                    LaporanOperasi = vm.LaporanOperasi,
                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                if (data.WaktuMulaiOperasi.HasValue && data.WaktuSelesaiOperasi.HasValue)
                {
                    data.DurasiOperasi = data.WaktuSelesaiOperasi - data.WaktuMulaiOperasi;
                }

                // **Simpan ke Database**
                _applicationDbContext.LaporanBedahs.Add(data);
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
        public async Task<IActionResult> Edit(Guid id, [FromBody] LaporanBedahViewModel vm)
        {
            // **1. Validasi Input**
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **2. Cek Koneksi Database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **3. Ambil User ID dari JWT Claims untuk UpdateBy**
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

                // **4. Cari Data Lama berdasarkan ID**
                var dataExisting = await _applicationDbContext.LaporanBedahs
                    .FirstOrDefaultAsync(x => x.LaporanBedahId == id);

                if (dataExisting == null)
                {
                    return NotFound(new { message = $"Data Laporan Bedah dengan ID {id} tidak ditemukan." });
                }

                // **5. Update Properti Data**
                dataExisting.KunjunganId = vm.KunjunganId;
                dataExisting.PasienId = vm.PasienId;
                dataExisting.TindakanId = vm.TindakanId;
                dataExisting.DetailTindakan = vm.DetailTindakan;
                dataExisting.DokterOperatorId = vm.DokterOperatorId;
                dataExisting.DokterAnestesiId = vm.DokterAnestesiId;
                dataExisting.DokterAsistenId = vm.DokterAsistenId;
                dataExisting.AsistenAnestesiId = vm.AsistenAnestesiId;
                dataExisting.PerawatId = vm.PerawatId;
                dataExisting.JenisAnestesi = vm.JenisAnestesi;
                dataExisting.DiagnosaPraOp = vm.DiagnosaPraOp;
                dataExisting.DiagnosaPostOp = vm.DiagnosaPostOp;
                dataExisting.JaringanEksisiInsisi = vm.JaringanEksisiInsisi;
                dataExisting.TipeUrgensi = vm.TipeUrgensi;
                dataExisting.IsPemeriksaanPA = vm.IsPemeriksaanPA;
                dataExisting.TanggalOperasi = vm.TanggalOperasi;
                dataExisting.WaktuMulaiOperasi = vm.WaktuMulaiOperasi;
                dataExisting.WaktuSelesaiOperasi = vm.WaktuSelesaiOperasi;
                dataExisting.LaporanOperasi = vm.LaporanOperasi;
                dataExisting.Keterangan = vm.Keterangan;

                // **Audit Trail Update**
                dataExisting.UpdateBy = getUserActive.UserActiveId;
                dataExisting.UpdateDateTime = DateTimeOffset.UtcNow;

                // **6. Kalkulasi Ulang Durasi**
                if (dataExisting.WaktuMulaiOperasi.HasValue && dataExisting.WaktuSelesaiOperasi.HasValue)
                {
                    dataExisting.DurasiOperasi = dataExisting.WaktuSelesaiOperasi - dataExisting.WaktuMulaiOperasi;
                }
                else
                {
                    dataExisting.DurasiOperasi = vm.DurasiOperasi; // Gunakan dari VM jika input manual
                }

                // **7. Simpan Perubahan**
                _applicationDbContext.LaporanBedahs.Update(dataExisting);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK", data = dataExisting });
                }
                else
                {
                    return StatusCode(500, new { message = "Tidak ada perubahan data yang disimpan ke database." });
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Data telah diubah oleh pengguna lain, silakan refresh." });
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
                var data = await _applicationDbContext.LaporanBedahs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.LaporanBedahs.Update(data);
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
            Guid? pasienId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.LaporanBedahs
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.LaporanBedahId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TindakanId,
                             a.DetailTindakan,
                             a.DokterOperatorId,
                             a.DokterAnestesiId,
                             a.DokterAsistenId,
                             a.AsistenAnestesiId,
                             a.PerawatId,
                             a.JenisAnestesi,
                             a.DiagnosaPostOp,
                             a.DiagnosaPraOp,
                             a.JaringanEksisiInsisi,
                             a.TipeUrgensi,
                             a.IsPemeriksaanPA,
                             a.TanggalOperasi,
                             a.WaktuMulaiOperasi,
                             a.WaktuSelesaiOperasi,
                             a.DurasiOperasi,
                             a.LaporanOperasi,
                             a.Keterangan,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }

            // filter based on pasien id
            if (pasienId.HasValue)
            {
                query = query.Where(u => u.PasienId == pasienId.Value);
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
