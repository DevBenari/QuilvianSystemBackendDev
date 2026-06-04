using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.DTO;
using QuilvianSystemBackendDev.Helpers;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("FrontendCorsPolicy")]
    public class DiskonDokterController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHubContext<DiskonDokterHub> _hubContext;
        private readonly ILogger<DiskonDokterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AutoLoginDTO _optAutoLogin;
        private readonly INotification _serviceNotification;
        private readonly IConfiguration _configuration;
        private readonly string _uploadUrl;

        public DiskonDokterController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DiskonDokterController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext <DiskonDokterHub> hubContext,
            IOptions<AutoLoginDTO> optAutoLogin,
            INotification serviceNotification,
            IConfiguration configuration
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _optAutoLogin = optAutoLogin.Value;
            _serviceNotification = serviceNotification;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
        }

        private async Task<(string? Path, string? FileName)> UploadFoCFileAsync(
            IFormFile? file,
            Guid diskonApprovedId,
            Guid? kunjunganId,
            Guid? pasienId,
            CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExt.Contains(ext))
                throw new Exception("File FoC hanya boleh JPG, JPEG, PNG, atau PDF.");

            if (file.Length > 10 * 1024 * 1024)
                throw new Exception("Ukuran file FoC maksimal 10MB.");

            var folderTarget = "FileFoCDiskonDokter";
            var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");

            var fileName =
                $"{diskonApprovedId}_{kunjunganId}_{pasienId}_FoC_{safeTime}{ext}";

            var filePath = $"/{folderTarget}/{fileName}";

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? GetDefaultContentType(ext)
                : file.ContentType;

            var fileContent = new StreamContent(ms);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            using var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", fileName);
            form.Add(new StringContent(folderTarget), "folderTarget");

            using var client = new HttpClient();
            var response = await client.PostAsync(_uploadUrl, form, ct);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Gagal upload file FoC ke Flask.");

            return (filePath, fileName);
        }

        private static string GetDefaultContentType(string ext)
        {
            return ext switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = from a in _applicationDbContext.DiskonDokters.AsNoTracking()
                            join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                            on a.CreateBy equals u.UserActiveId

                            join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                            on a.PasienId equals p.PendaftaranPasienBaruId into pGroup
                            from p in pGroup.DefaultIfEmpty()

                            join k in _applicationDbContext.Kunjungans.AsNoTracking()
                            on a.KunjunganId equals k.KunjunganID into kG
                            from k in kG.DefaultIfEmpty()

                            join d in _applicationDbContext.Diskons.AsNoTracking()
                            on a.DiskonId equals d.DiskonId into dG
                            from d in dG.DefaultIfEmpty()

                            join dd in _applicationDbContext.DiskonDetails.AsNoTracking()
                            on d.DiskonId equals dd.DiskonId into ddG
                            from dd in ddG.DefaultIfEmpty()

                            join dr in _applicationDbContext.Dokters.AsNoTracking()
                            on a.Approved1Id equals dr.DokterId into drG
                            from dr in drG.DefaultIfEmpty()

                            where a.IsDelete == false && a.DiskonApprovedId==id 
                            select new
                            {
                                a.CreateDateTime,
                                a.CreateBy,
                                CreateByName = u.FullName,
                                a.DiskonApprovedId,
                                a.DiskonId,
                                d.NamaDiskon,
                                DetailDiskonId = (Guid?)dd.DetailDiskonId ?? null,
                                a.PasienId,
                                NamaPasien = p.NamaLengkap ?? null,
                                NoRm = p.NoRekamMedis ?? null,
                                a.KunjunganId,
                                AsalKunjungan = k.AsalKunjungan ?? null,
                                JenisKunjungan = k.JenisKunjungan ?? null,
                                a.Approved1Id,
                                NamaDokterAproved = dr.NmDokter ?? null,
                                a.ApprovedDate1,
                                a.IsApproved1
                            };
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
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> Create([FromForm] DiskonDokterViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                if (!vm.DiskonId.HasValue || vm.DiskonId.Value == Guid.Empty)
                    return BadRequest(new { message = "DiskonId wajib diisi." });

                if (!vm.KunjunganId.HasValue || vm.KunjunganId.Value == Guid.Empty)
                    return BadRequest(new { message = "KunjunganId wajib diisi." });

                if (!vm.PasienId.HasValue || vm.PasienId.Value == Guid.Empty)
                    return BadRequest(new { message = "PasienId wajib diisi." });

                if (!vm.Approved1Id.HasValue || vm.Approved1Id.Value == Guid.Empty)
                    return BadRequest(new { message = "Approved1Id wajib diisi." });

                var isDiskonValid = await _applicationDbContext.Diskons
                    .AnyAsync(c =>
                        c.DiskonId == vm.DiskonId.Value &&
                        (c.IsDelete == false || c.IsDelete == null),
                        ct);

                if (!isDiskonValid)
                {
                    return BadRequest(new { message = "Diskon tidak ditemukan." });
                }

                var isKunjunganValid = await _applicationDbContext.Kunjungans
                    .AnyAsync(x =>
                        x.KunjunganID == vm.KunjunganId.Value &&
                        x.PasienId == vm.PasienId.Value &&
                        !x.IsDelete,
                        ct);

                if (!isKunjunganValid)
                {
                    return BadRequest(new
                    {
                        message = "Data kunjungan tidak valid atau tidak sesuai dengan pasien."
                    });
                }

                var userApproval = await _applicationDbContext.UserActives
                    .Where(c => c.UserActiveId == vm.Approved1Id.Value)
                    .Select(c => new
                    {
                        c.UserActiveId,
                        c.NoHandphone,
                        c.FullName
                    })
                    .FirstOrDefaultAsync(ct);

                if (userApproval == null)
                {
                    return BadRequest(new { message = "User approval tidak ditemukan." });
                }

                var diskonApprovedId = Guid.NewGuid();

                var uploadResult = await UploadFoCFileAsync(
                    vm.FormFile,
                    diskonApprovedId,
                    vm.KunjunganId,
                    vm.PasienId,
                    ct
                );

                var isFoCUploaded = vm.FormFile != null && vm.FormFile.Length > 0;

                var data = new DiskonDokter
                {
                    DiskonApprovedId = diskonApprovedId,
                    PasienId = vm.PasienId,
                    KunjunganId = vm.KunjunganId,
                    DiskonId = vm.DiskonId,
                    Approved1Id = vm.Approved1Id,

                    FoCFilePath = uploadResult.Path,

                    IsApproved1 = isFoCUploaded ? true : false,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                    IsDelete = false
                };

                _applicationDbContext.DiskonDokters.Add(data);

                var result = await _applicationDbContext.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                var targetUrl = "/kasir/diskon-approval/dokter/table";

                var token = AutoLoginHelper.GenerateAutoLoginToken(
                    userApproval.UserActiveId.ToString(),
                    targetUrl,
                    _optAutoLogin.SecretKey,
                    expiredMinutes: 10
                );

                var resultToken = AutoLoginHelper.ValidateTokenDebug(token, _optAutoLogin.SecretKey);

                if (!resultToken.IsValid || string.IsNullOrWhiteSpace(resultToken.UserId))
                    return Unauthorized(new { message = "Token tidak valid atau sudah kadaluarsa.", debug = resultToken.Error });

                var autoLoginUrl =
                    $"http://103.153.61.119:8084/api/Auth/AutoLogin" +
                    $"?token={Uri.EscapeDataString(token)}" +
                    $"&redirect=true" +
                    $"&setCookie=true";

                //var autoLoginUrl =
                //    $"{_optAutoLogin.BaseUrl.TrimEnd('/')}/api/Auth/AutoLogin" +
                //    $"?token={Uri.EscapeDataString(token)}" +
                //    $"&redirect=true" +
                //    $"&setCookie=true";

                WhatsAppResultDto waResult;

                if (string.IsNullOrWhiteSpace(userApproval.NoHandphone))
                {
                    waResult = new WhatsAppResultDto
                    {
                        Success = false,
                        Message = "Nomor handphone user approval kosong."
                    };
                }
                else
                {
                    var waMsg =
                        $"APPROVAL DISKON,\n\n" +
                        $"Yth. Bapak/Ibu {userApproval.FullName},\n" +
                        $"Terdapat permintaan approval diskon dokter / FoC yang menunggu tindak lanjut.\n" +
                        $"Silakan klik link berikut untuk membuka detail approval:\n\n{autoLoginUrl}";

                    waResult = await _serviceNotification.SendWhatsAppAsync(
                        userApproval.NoHandphone,
                        waMsg
                    );
                }

                return Created("", new
                {
                    message = "Tambah Data Berhasil || 201 Created",
                    data = new
                    {
                        data.DiskonApprovedId,
                        data.KunjunganId,
                        data.PasienId,
                        data.DiskonId,
                        data.Approved1Id,
                        data.IsApproved1,
                        FileFoC = new
                        {
                            Path = data.FoCFilePath
                        },
                        TargetUrl = $"{_optAutoLogin.BaseUrl.TrimEnd('/')}{targetUrl}",
                        AutoLoginUrl = autoLoginUrl,
                        WhatsAppSent = waResult.Success,
                        WhatsAppDebug = waResult
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan saat Create DiskonDokter");
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }

        [HttpPost("{id:guid}/Upload-FoC")]
        [RequestSizeLimit(20_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
        public async Task<IActionResult> UploadFoC(
            Guid id,
            [FromForm] FileFoCDiskonDokterViewModel vm,
            CancellationToken ct)

        {
            if (vm == null || vm.FormFile == null || vm.FormFile.Length == 0)
            {
                return BadRequest(new { message = "File FoC tidak valid." });
            }

            try
            {
                if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin, ct);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;

                var diskonDokter = await _applicationDbContext.DiskonDokters
                    .FirstOrDefaultAsync(x =>
                        x.DiskonApprovedId == id &&
                        (x.IsDelete == false || x.IsDelete == null),
                        ct);

                if (diskonDokter == null)
                {
                    return NotFound(new { message = "Data diskon dokter tidak ditemukan." });
                }

                var uploadResult = await UploadFoCFileAsync(
                    vm.FormFile,
                    diskonDokter.DiskonApprovedId,
                    diskonDokter.KunjunganId,
                    diskonDokter.PasienId,
                    ct
                );

                diskonDokter.FoCFilePath = uploadResult.Path;

                /*
                 * Validasi:
                 * Jika file FoC terisi / berhasil diupload,
                 * maka IsApproved1 otomatis true.
                 */
                diskonDokter.IsApproved1 = true;

                diskonDokter.UpdateDateTime = DateTimeOffset.UtcNow;
                diskonDokter.UpdateBy = userActiveId;

                var result = await _applicationDbContext.SaveChangesAsync(ct);

                if (result <= 0)
                {
                    return StatusCode(500, new { message = "File FoC gagal diperbarui." });
                }

                return Ok(new
                {
                    message = "File FoC berhasil diupload.",
                    data = new
                    {
                        diskonDokter.DiskonApprovedId,
                        diskonDokter.KunjunganId,
                        diskonDokter.PasienId,
                        diskonDokter.IsApproved1,
                        FileFoC = new
                        {
                            Path = diskonDokter.FoCFilePath,
                        }
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = $"Gagal upload file FoC: {dbEx.InnerException?.Message ?? dbEx.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan saat UploadFoC DiskonDokter");
                return StatusCode(500, new
                {
                    message = $"Terjadi kesalahan internal: {ex.Message}"
                });
            }
        }
        
        
        [HttpPut("DiskonDokter-Approval/{id}")]
        public async Task<IActionResult> DiskonDokterApproval(Guid id, [FromBody] DiskonApprovalViewModel vm)
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
                var data = await _applicationDbContext.DiskonDokters.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.Approved1Id = vm.ApprovedId;
                data.IsApproved1 = true;
                data.ApprovedDate1 = vm.ApprovedDate;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.DiskonDokters.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    await _hubContext.Clients.All.SendAsync("Diskon dokter telah disetujui", new
                    {
                        action = "update",
                        diskonAprrovedId = data.DiskonApprovedId,
                        approvalId1 = data.Approved1Id,
                    });

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
                var data = await _applicationDbContext.DiskonDokters.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.DiskonDokters.Update(data);
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
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                                    DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = from a in _applicationDbContext.DiskonDokters.AsNoTracking()
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId

                         join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                         on a.PasienId equals p.PendaftaranPasienBaruId into pGroup
                         from p in pGroup.DefaultIfEmpty()

                         join k in _applicationDbContext.Kunjungans.AsNoTracking()
                         on a.KunjunganId equals k.KunjunganID into kG
                         from k in kG.DefaultIfEmpty()

                         join d in _applicationDbContext.Diskons.AsNoTracking()
                         on a.DiskonId equals d.DiskonId into dG
                         from d in dG.DefaultIfEmpty()

                         join dd in _applicationDbContext.DiskonDetails.AsNoTracking()
                         on d.DiskonId equals dd.DiskonId into ddG
                         from dd in ddG.DefaultIfEmpty()

                         join dr in _applicationDbContext.Dokters.AsNoTracking()
                         on a.Approved1Id equals dr.UserActiveId into drG
                         from dr in drG.DefaultIfEmpty()

                         where a.IsDelete == false 
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.DiskonApprovedId,
                             a.DiskonId,
                             d.NamaDiskon,
                             DetailDiskonId = (Guid?)dd.DetailDiskonId ?? null,
                             a.PasienId,
                             NamaPasien = p.NamaLengkap ?? null,
                             NoRm =  p.NoRekamMedis ?? null,
                             a.KunjunganId,
                             AsalKunjungan = k.AsalKunjungan ?? null,
                             JenisKunjungan = k.JenisKunjungan ?? null,
                             a.Approved1Id,
                             NamaDokterAproved = dr.NmDokter ?? null,
                             a.ApprovedDate1,
                             a.IsApproved1
                         };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaDiskon, search) ||
                    EF.Functions.ILike(u.NamaPasien, search) ||
                    EF.Functions.ILike(u.NamaDokterAproved, search)
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
