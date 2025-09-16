using System;
using System.Globalization;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class ResepDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<ResepDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResepDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepDetailController> logger,
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
            var query = (from a in _applicationDbContext.DetailReseps
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailResepId,
                             a.ResepId,
                             a.AsuransiId,
                             a.NamaAsuransi,
                             a.ObatId,
                             a.Qty,
                             a.JenisRacikan,
                             a.Signa,
                             a.SignaTambahan,
                             a.JenisObat,
                             a.HargaObat,
                             a.TotalHargaObat,
                             a.StatusCoverObat,
                             a.IsRacikan,
                             a.RacikanId,
                             a.IsIteratur,
                             a.JumlahIteratur,
                             a.TglMulaiIteratur,
                             a.JarakPenebusan,
                             a.MasaAktifIteratur,
                             a.StatusPengambilanObat,
                             a.StatusDiberikanPasien,
                             a.TakaranDosis,
                             a.IsContinued,
                             a.CaraPemakaian,
                             a.EstimasiPemberian,
                             a.TglStopPemakaian,
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
            var listdata = _applicationDbContext.DetailReseps.Find(id);
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

        [HttpPut("{id}/StatusObat")]
        public async Task<IActionResult> UpdateIsLunas(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusPengambilanObat = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
        }

        [HttpPut("{id}/IsContinuedMedicine")]
        public async Task<IActionResult> UpdateIsContinued(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Obat tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsContinued = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isContinued berhasil diperbarui." });
        }

        [HttpPut("{id}/StatusDiberikanPasien")]
        public async Task<IActionResult> UpdateStatusDiberikan(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Obat tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.StatusDiberikanPasien = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status StatusDiberikanPasien berhasil diperbarui." });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResepDetailViewModel vm)
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
                //bool isDuplicate = _applicationDbContext.Benefits
                //                    .Any(c => c.NamaBenefit == vm.NamaBenefit);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                //if (!DateTime.TryParseExact(vm.TglMulaiIteratur, "yyyy-MM-dd",
                //    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedTglMulaiIteratur))
                //{
                //    return BadRequest(new { message = "Format TglMulaiIteratur tidak valid. Gunakan format yyyy-MM-dd." });
                //}

                //parsedTglMulaiIteratur = DateTime.SpecifyKind(parsedTglMulaiIteratur, DateTimeKind.Utc);

                //if (!DateTime.TryParseExact(vm.MasaAktifIteratur, "yyyy-MM-dd",
                //    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedMasaAktif))
                //{
                //    return BadRequest(new { message = "Format TglMulaiIteratur tidak valid. Gunakan format yyyy-MM-dd." });
                //}

                //parsedMasaAktif = DateTime.SpecifyKind(parsedMasaAktif, DateTimeKind.Utc);

                // **Buat Data Baru**
                var data = new ResepDetail
                {
                    DetailResepId = Guid.NewGuid(),
                    ResepId = vm.ResepId,
                    AsuransiId = vm.AsuransiId,
                    NamaAsuransi = vm.NamaAsuransi,
                    ObatId = vm.ObatId,
                    Qty = vm.Qty,
                    TakaranDosis = vm.TakaranDosis,
                    Signa = vm.Signa,
                    SignaTambahan = vm.SignaTambahan,
                    JenisObat = vm.JenisObat,
                    HargaObat = vm.HargaObat,
                    TotalHargaObat = vm.Qty.HasValue && vm.HargaObat.HasValue ? vm.Qty.Value * vm.HargaObat.Value : 0,
                    StatusCoverObat = vm.StatusCoverObat,
                    IsRacikan = vm.IsRacikan, // Tambahkan properti IsRacikan jika diperlukan
                    //IsIteratur = vm.IsIteratur,
                    //JumlahIteratur = vm.JumlahIteratur,
                    //TglMulaiIteratur = parsedTglMulaiIteratur,
                    //JarakPenebusan = vm.JarakPenebusan,
                    //MasaAktifIteratur = parsedMasaAktif,
                    StatusPengambilanObat = false, // Default nilai StatusPengambilanObat
                    CaraPemakaian = vm.CaraPemakaian,
                    EstimasiPemberian = vm.EstimasiPemberian,
                    TglStopPemakaian = TryParseTanggalToUtc(vm.TglStopPemakaian),
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.DetailReseps.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] ResepDetailViewModel vm)
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
                var data = await _applicationDbContext.DetailReseps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //if (!DateTime.TryParseExact(vm.TglMulaiIteratur, "yyyy-MM-dd",
                //    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedTglMulaiIteratur))
                //{
                //    return BadRequest(new { message = "Format TglMulaiIteratur tidak valid. Gunakan format yyyy-MM-dd." });
                //}

                //parsedTglMulaiIteratur = DateTime.SpecifyKind(parsedTglMulaiIteratur, DateTimeKind.Utc);

                //if (!DateTime.TryParseExact(vm.MasaAktifIteratur, "yyyy-MM-dd",
                //    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedMasaAktif))
                //{
                //    return BadRequest(new { message = "Format TglMulaiIteratur tidak valid. Gunakan format yyyy-MM-dd." });
                //}
                //parsedMasaAktif = DateTime.SpecifyKind(parsedMasaAktif, DateTimeKind.Utc);


                // **Update Data**
                data.ObatId = vm.ObatId;
                data.AsuransiId = vm.AsuransiId;
                data.NamaAsuransi = vm.NamaAsuransi;
                data.ResepId = vm.ResepId;
                data.Qty = vm.Qty;
                data.Signa = vm.Signa;
                data.SignaTambahan = vm.SignaTambahan;
                data.JenisObat = vm.JenisObat;
                data.HargaObat = vm.HargaObat;
                data.TotalHargaObat = vm.Qty.HasValue && vm.HargaObat.HasValue ? vm.Qty.Value * vm.HargaObat.Value : 0;
                data.StatusCoverObat = vm.StatusCoverObat;
                data.IsRacikan = vm.IsRacikan; // Update properti IsRacikan jika diperlukan
                //data.IsIteratur = vm.IsIteratur;
                //data.JumlahIteratur = vm.JumlahIteratur;
                //data.TglMulaiIteratur = parsedTglMulaiIteratur;
                //data.JarakPenebusan = vm.JarakPenebusan;
                //data.MasaAktifIteratur = parsedMasaAktif;
                data.TakaranDosis = vm.TakaranDosis;
                data.CaraPemakaian = vm.CaraPemakaian;
                data.EstimasiPemberian = vm.EstimasiPemberian;
                data.TglStopPemakaian = TryParseTanggalToUtc(vm.TglStopPemakaian);

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.DetailReseps.Update(data);
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
                var data = await _applicationDbContext.DetailReseps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.DetailReseps.Update(data);
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
        public IActionResult PagedDetailResep(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // query
            var query = from a in _applicationDbContext.DetailReseps
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DetailResepId,
                             a.ResepId,
                             a.AsuransiId,
                             a.NamaAsuransi,
                             a.ObatId,
                             a.Qty,
                             a.JenisRacikan,
                             a.Signa,
                             a.SignaTambahan,
                             a.JenisObat,
                             a.HargaObat,
                             a.StatusCoverObat,
                             a.TotalHargaObat,
                             a.IsRacikan,
                             a.RacikanId,
                             a.IsIteratur,
                             a.JumlahIteratur,
                             a.TglMulaiIteratur,
                             a.JarakPenebusan,
                             a.MasaAktifIteratur,
                             a.StatusPengambilanObat,
                             a.TakaranDosis,
                             a.IsContinued,
                             a.CaraPemakaian,
                             a.EstimasiPemberian,
                             a.TglStopPemakaian,
                         };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaBenefit, search)
            //    );
            //}

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
            //query = sortDirection?.ToLower() == "desc"
            //    ? orderBy switch
            //    {
            //        "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
            //        "CreateByName" => query.OrderByDescending(u => u.CreateByName),
            //        "NamaBenefit" => query.OrderByDescending(u => u.NamaBenefit),
            //        _ => query.OrderByDescending(u => u.CreateDateTime)
            //    }
            //    : orderBy switch
            //    {
            //        "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
            //        "CreateByName" => query.OrderBy(u => u.CreateByName),
            //        "NamaBenefit" => query.OrderBy(u => u.NamaBenefit),
            //        _ => query.OrderBy(u => u.CreateDateTime)
            //    };

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
