using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DetailICDController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DetailICDController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public DetailICDController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DetailICDController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDetailICD(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.DetailICDs
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            DetailICDId = a.DetailICDId,
                            SoapId = a.SoapId,
                            ICDId = a.ICDId,
                            isUtama = a.isUtama,
                        };

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
            var listdata = _applicationDbContext.DetailICDs.Find(id);
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
        public async Task<IActionResult> CreateDetailICD([FromBody] DetailICDViewModel vm)
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

                // **Ambil Tanggal Sekarang**
                var dateNow = DateTime.UtcNow;
                var setDateNow = dateNow.ToString("yyMMdd"); // Format: YYMMDD

                // **Buat Data Baru**
                var data = new DetailICD
                {
                    DetailICDId = Guid.NewGuid(),
                    SoapId = vm.SoapId,
                    ICDId = vm.ICDId,
                    isUtama = vm.isUtama,
                    CreateBy = userActiveId,
                    CreateDateTime = dateNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.DetailICDs.Add(data);
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
        public async Task<IActionResult> UpdateDetailICD(Guid id, [FromBody] DetailICDViewModel vm)
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
                var data = await _applicationDbContext.DetailICDs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.SoapId = vm.SoapId;
                data.ICDId = vm.ICDId;
                data.isUtama = vm.isUtama;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.DetailICDs.Update(data);
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
        public async Task<IActionResult> DeleteDetailIcd(Guid id)
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
                var data = await _applicationDbContext.DetailICDs.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.DetailICDs.Update(data);
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
        public async Task<IActionResult> PagedDetailICD(
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
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }


                // Query data
                var query = from a in _applicationDbContext.DetailICDs
                            join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false
                            select new
                            {
                                CreateDateTime = a.CreateDateTime,
                                CreateBy = a.CreateBy,
                                CreateByName = u.FullName,
                                DetailICDId = a.DetailICDId,
                                SoapId = a.SoapId,
                                ICDId = a.ICDId,
                                isUtama = a.isUtama,
                            };

                // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
                //if (!string.IsNullOrWhiteSpace(search))
                //{
                //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                //    query = query.Where(u =>
                //        EF.Functions.ILike(u.KodePoliklinik, search) ||
                //        EF.Functions.ILike(u.NamaPoliklinik, search)
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
                                u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                                u.CreateDateTime.Date <= today
                            );
                            break;
                        case PeriodeFilter.LastWeek:
                            query = query.Where(u =>
                                u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
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
                //        "KodePoliklinik" => query.OrderByDescending(u => u.KodePoliklinik),
                //        "NamaPoliklinik" => query.OrderByDescending(u => u.NamaPoliklinik),
                //        "LayananPoliklinik" => query.OrderByDescending(u => u.LayananPoliklinik),
                //        _ => query.OrderByDescending(u => u.CreateDateTime)
                //    }
                //    : orderBy switch
                //    {
                //        "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                //        "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                //        "KodePoliklinik" => query.OrderByDescending(u => u.KodePoliklinik),
                //        "NamaPoliklinik" => query.OrderByDescending(u => u.NamaPoliklinik),
                //        "LayananPoliklinik" => query.OrderByDescending(u => u.LayananPoliklinik),
                //        _ => query.OrderByDescending(u => u.CreateDateTime)
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }


        }


    }
}
