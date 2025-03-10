using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class AgamaController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AgamaController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AgamaController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<AgamaController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAgama(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.Agamas
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            AgamaId = a.AgamaId,
                            KodeAgama = a.KodeAgama,
                            NamaAgama = a.NamaAgama,
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
        public async Task<IActionResult> GetAgamaById(Guid id)
        {
            var listdata = _applicationDbContext.Agamas.Find(id);
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
        public async Task<IActionResult> CreateAgama([FromBody] AgamaViewModel vm)
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


                // **Ambil Kode Terakhir**
                var lastCode = _applicationDbContext.Agamas
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.CreateDateTime)

                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"AGM{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeAgama.Substring(3, 6);
                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"AGM{setDateNow}0001";
                    }
                    else
                    {
                        // Mengambil nomor urut terakhir dan menambah 1
                        var lastNumber = int.Parse(lastCode.KodeAgama.Substring(9));
                        kode = $"AGM{setDateNow}{(lastNumber + 1).ToString("D4")}";
                    }
                }

                // **Cek Duplikasi**
                bool isDuplicate = _applicationDbContext.Agamas
                    .Any(c => c.KodeAgama == kode || c.NamaAgama.ToLower() == vm.NamaAgama.ToLower());

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Buat Data Baru**
                var data = new Agama
                {
                    AgamaId = Guid.NewGuid(),
                    CreateDateTime = dateNow,
                    CreateBy = userActiveId,
                    KodeAgama = kode,
                    NamaAgama = vm.NamaAgama
                };

                // **Simpan ke Database**
                _applicationDbContext.Agamas.Add(data);
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
        public async Task<IActionResult> UpdateAgama(Guid id, [FromBody] AgamaViewModel vm)
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
                var data = await _applicationDbContext.Agamas.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Cek Duplikasi NamaAgama**
                bool isDuplicate = await _applicationDbContext.Agamas
                    .AnyAsync(c => c.NamaAgama.ToLower() == vm.NamaAgama.ToLower() && c.AgamaId != id);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Update Data**
                data.NamaAgama = vm.NamaAgama;
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTime.UtcNow;

                _applicationDbContext.Agamas.Update(data);
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
        public async Task<IActionResult> DeleteAgama(Guid id)
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
                var data = await _applicationDbContext.Agamas.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Agamas.Update(data);
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
        public async Task<IActionResult> PagedAgama(
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

                // **Query Dasar**
                var query = from a in _applicationDbContext.Agamas
                            join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false
                            select new
                            {
                                CreateDateTime = a.CreateDateTime,
                                CreateBy = a.CreateBy,
                                CreateByName = u.FullName,
                                AgamaId = a.AgamaId,
                                KodeAgama = a.KodeAgama,
                                NamaAgama = a.NamaAgama,
                            };

                // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                    query = query.Where(u =>
                        EF.Functions.ILike(u.KodeAgama, search) ||
                        EF.Functions.ILike(u.NamaAgama, search)
                    );
                }

                // **Filter berdasarkan tanggal**
                if (startDate.HasValue && endDate.HasValue)
                {
                    DateTime startUtc = startDate.Value.Date;
                    DateTime endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1);

                    query = query.Where(u =>
                        u.CreateDateTime >= startUtc &&
                        u.CreateDateTime <= endUtc);
                }

                // **Sorting Data dengan cara yang lebih aman**
                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                query = sortColumn switch
                {
                    "createdatetime" => isDescending ? query.OrderByDescending(u => u.CreateDateTime) : query.OrderBy(u => u.CreateDateTime),
                    "createbyname" => isDescending ? query.OrderByDescending(u => u.CreateByName) : query.OrderBy(u => u.CreateByName),
                    "kodeagama" => isDescending ? query.OrderByDescending(u => u.KodeAgama) : query.OrderBy(u => u.KodeAgama),
                    "namaagama" => isDescending ? query.OrderByDescending(u => u.NamaAgama) : query.OrderBy(u => u.NamaAgama),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

                // **Pagination**
                int totalRows = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
                var rows = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

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
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
    }
}

