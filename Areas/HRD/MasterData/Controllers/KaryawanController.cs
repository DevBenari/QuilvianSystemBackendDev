using System.Data;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KaryawanController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<KaryawanController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadUrl;


        public KaryawanController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<KaryawanController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = (from k in _applicationDbContext.Karyawans.AsNoTracking()
                            join u in _applicationDbContext.UserActives.AsNoTracking()
                              on k.UserActiveId equals u.UserActiveId
                            join fn in _applicationDbContext.UserActives.AsNoTracking()
                            on k.CreateBy equals fn.UserActiveId
                            where k.IsDelete == false || k.IsDelete == null
                            select new
                            {
                                k.KaryawanId,
                                k.UserActiveId,
                                NamaKaryawan = u.FullName, // Mengambil nama dari tabel UserActives
                                k.DepartementId,
                                k.InstalasiUnitId,
                                k.JabatanId,
                                k.NoIdentitas,
                                k.KodeKaryawan,
                                k.NoKaryawan,
                                k.NoRekening,
                                k.BankId,
                                k.TanggalKontrak,
                                k.TanggalAwalKerja,
                                k.TanggalAkhirKerja,
                                k.NoHandphone,
                                k.Email,
                                k.Alamat,
                                k.FotoName,
                                k.FotoPath,
                                CreateBy = fn.FullName,
                                k.CreateDateTime
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

        [HttpGet("by-NoKaryawan/{noKaryawan}")]
        public async Task<IActionResult> GetByNoKaryawan(string noKaryawan)
        {
            if (string.IsNullOrWhiteSpace(noKaryawan))
            {
                return BadRequest(new { message = "NoKaryawan wajib diisi." });
            }

            try
            {
                var data = await (
                    from k in _applicationDbContext.Karyawans.AsNoTracking()
                    where (k.IsDelete == false || k.IsDelete == null)
                          && k.NoKaryawan == noKaryawan

                    join u0 in _applicationDbContext.UserActives.AsNoTracking()
                        on k.UserActiveId equals u0.UserActiveId into ug
                    from u in ug.DefaultIfEmpty()

                    join fn0 in _applicationDbContext.UserActives.AsNoTracking()
                        on k.CreateBy equals fn0.UserActiveId into fng
                    from fn in fng.DefaultIfEmpty()

                    select new
                    {
                        k.KaryawanId,
                        k.UserActiveId,
                        NamaKaryawan = u != null ? u.FullName : null,
                        k.DepartementId,
                        k.InstalasiUnitId,
                        k.JabatanId,
                        k.NoIdentitas,
                        k.KodeKaryawan,
                        k.NoKaryawan,
                        k.NoRekening,
                        k.BankId,
                        k.TanggalKontrak,
                        k.TanggalAwalKerja,
                        k.TanggalAkhirKerja,
                        k.NoHandphone,
                        k.Email,
                        k.Alamat,
                        k.FotoName,
                        k.FotoPath,
                        CreateBy = fn != null ? fn.FullName : null,
                        k.CreateDateTime
                    }
                ).FirstOrDefaultAsync();

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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KaryawanViewModel vm)
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
                bool isDuplicate = await _applicationDbContext.Karyawans
                                    .AnyAsync(c => c.NoIdentitas.ToLower().Trim() == vm.NoIdentitas.ToLower().Trim()
                                    && c.UserActiveId == vm.UserActiveId
                                    && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Data karyawan ini telah tersedia" });
                }

                string noKaryawan = "";
                var dateNow = DateTime.UtcNow; ;
                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

                var lastKaryawan = _applicationDbContext.Karyawans
                    .Where(k => k.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.NoKaryawan)
                    .FirstOrDefault();

                if (lastKaryawan == null)
                {
                    noKaryawan = "KRY" + setDateNow + "0001";
                }
                else
                {
                    var lastCodeTrim = lastKaryawan.NoKaryawan.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        noKaryawan = "KRY" + setDateNow + "0001";
                    }
                    else
                    {
                        noKaryawan = "KRY" + setDateNow +
                            (Convert.ToInt32(lastKaryawan.NoKaryawan.Substring(9)) + 1).ToString("D4");
                    }
                }

                // **Buat Data Baru**
                var data = new Karyawan
                {
                    KaryawanId = Guid.NewGuid(),
                    UserActiveId = vm.UserActiveId,
                    DepartementId = vm.DepartementId ?? Guid.Empty,
                    InstalasiUnitId = vm.InstalasiUnitId ?? Guid.Empty,
                    JabatanId = vm.JabatanId ?? Guid.Empty,
                    NoKaryawan = noKaryawan,
                    NoIdentitas = vm.NoIdentitas,
                    KodeKaryawan = vm.KodeKaryawan,
                    NoRekening = vm.NoRekening,
                    BankId = vm.BankId,
                    TanggalKontrak = vm.TanggalKontrak,
                    TanggalAwalKerja = vm.TanggalAwalKerja,
                    TanggalAkhirKerja = vm.TanggalAkhirKerja,
                    NoHandphone = vm.NoHandphone,
                    Email = vm.Email,
                    Alamat = vm.Alamat,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.Karyawans.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] KaryawanViewModel vm)
        {
            // 1. Validasi awal input
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // 2. Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // 3. Ambil User ID dari JWT Claims untuk audit trail (UpdateBy)
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

                // 4. Cari data lama di database
                var data = await _applicationDbContext.Karyawans
                    .FirstOrDefaultAsync(x => x.KaryawanId == id && (x.IsDelete == false || x.IsDelete == null));

                if (data == null)
                {
                    return NotFound(new { message = "Data karyawan tidak ditemukan." });
                }

                // 5. Cek Duplikasi (NoIdentitas tidak boleh sama dengan milik orang lain)
                bool isDuplicate = await _applicationDbContext.Karyawans
                    .AnyAsync(c => c.NoIdentitas.ToLower().Trim() == vm.NoIdentitas.ToLower().Trim()
                                && c.KaryawanId != id // Pastikan bukan mengecek dirinya sendiri
                                && c.UserActiveId == vm.UserActiveId
                                && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "No Identitas ini sudah digunakan oleh karyawan lain." });
                }

                // 6. Mapping perubahan data dari ViewModel ke Entity yang sudah ada
                data.UserActiveId = vm.UserActiveId;
                data.DepartementId = vm.DepartementId ?? Guid.Empty;
                data.InstalasiUnitId = vm.InstalasiUnitId ?? Guid.Empty;
                data.JabatanId = vm.JabatanId ?? Guid.Empty;
                data.NoIdentitas = vm.NoIdentitas;
                data.KodeKaryawan = vm.KodeKaryawan;
                data.NoRekening = vm.NoRekening;
                data.BankId = vm.BankId;
                data.TanggalKontrak = vm.TanggalKontrak;
                data.TanggalAwalKerja = vm.TanggalAwalKerja;
                data.TanggalAkhirKerja = vm.TanggalAkhirKerja;
                data.NoHandphone = vm.NoHandphone;
                data.Email = vm.Email;
                data.Alamat = vm.Alamat;

                // Metadata untuk audit trail
                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                // 7. Simpan perubahan
                _applicationDbContext.Karyawans.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil", data = data });
                }
                else
                {
                    return StatusCode(500, new { message = "Gagal memperbarui data ke database." });
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

        [HttpPut("UploadFotoKaryawan/{id}")]
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> UploadFotoKaryawan(Guid id, [FromForm] UploadFotoKaryawanViewModel vm)
        {
            if (vm == null || vm.FotoKaryawan == null || vm.FotoKaryawan.Length == 0)
            {
                return BadRequest(new { message = "File foto karyawan tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // ✅ Ambil user aktif
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // ✅ Cari PraOperasi berdasarkan ID
                var data = await _applicationDbContext.Karyawans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data karyawan tidak ditemukan." });
                }

                var fileName="";

                // ✅ Proses upload file TTD
                async Task<string?> UploadToFlaskAsync(IFormFile? file, string prefix)
                {
                    if (file == null || file.Length == 0)
                        return null;

                    var allowedExt = new[] { ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExt.Contains(ext))
                        throw new Exception($"{prefix} harus JPG atau JPEG.");

                    if (file.Length > 5 * 1024 * 1024)
                        throw new Exception($"{prefix} maksimal 5MB.");

                    var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
                    fileName = $"{data.NoKaryawan}_{safeTime}{ext}";

                    // 👉 Sesuaikan nama folder dengan kebutuhan kamu
                    var folderTarget = "FotoKaryawan";
                    var filePath = $"/{folderTarget}/{fileName}";

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "image/jpeg"
                        : file.ContentType;

                    var fileContent = new StreamContent(ms);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    using var form = new MultipartFormDataContent();
                    form.Add(fileContent, "file", fileName);
                    form.Add(new StringContent(folderTarget), "folderTarget");

                    using var client = new HttpClient();
                    var response = await client.PostAsync(_uploadUrl, form);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gagal upload {prefix} ke Flask.");

                    // ⚠ Di sini kita pakai pola yang sama seperti UpdatePenandaan:
                    //     tidak baca JSON dari Flask, tapi pakai path lokal yang sudah dibentuk
                    return filePath;
                }


                // Upload file → folder TTDUser
                var path = await UploadToFlaskAsync(vm.FotoKaryawan, "FotoKaryawan");

                // ✅ Update PraOperasi
                data.FotoPath = path;
                data.FotoName = fileName;

                _applicationDbContext.Karyawans.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Ok(new { message = "Foto Karyawan berhasil diupload", path, karyawanId = data.KaryawanId });

                return StatusCode(500, new { message = "TTD gagal diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
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
                var data = await _applicationDbContext.Karyawans.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.Karyawans.Update(data);
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
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                            DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from k in _applicationDbContext.Karyawans.AsNoTracking()
                              join u in _applicationDbContext.UserActives.AsNoTracking()
                                on k.UserActiveId equals u.UserActiveId
                              join fn in _applicationDbContext.UserActives.AsNoTracking()
                              on k.CreateBy equals fn.UserActiveId
                              where k.IsDelete == false || k.IsDelete == null
                              select new
                              {
                                  k.KaryawanId,
                                  k.UserActiveId,
                                  NamaKaryawan = u.FullName, // Mengambil nama dari tabel UserActives
                                  k.DepartementId,
                                  k.InstalasiUnitId,
                                  k.JabatanId,
                                  k.NoIdentitas,
                                  k.KodeKaryawan,
                                  k.NoRekening,
                                  k.NoKaryawan,
                                  k.BankId,
                                  k.TanggalKontrak,
                                  k.TanggalAwalKerja,
                                  k.TanggalAkhirKerja,
                                  k.NoHandphone,
                                  k.Email,
                                  k.Alamat,
                                  k.FotoName,
                                  k.FotoPath,
                                  CreateBy = fn.FullName,
                                  k.CreateDateTime
                              });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaKaryawan, search)
                );
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
                    "NamaKaryawan" => query.OrderByDescending(u => u.NamaKaryawan),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "NamaKaryawan" => query.OrderBy(u => u.NamaKaryawan),
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
