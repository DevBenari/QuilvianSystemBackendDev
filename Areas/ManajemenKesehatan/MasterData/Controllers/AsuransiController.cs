using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers;
using QuilvianSystemBackendDev.Migrations;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using ZXing.QrCode.Internal;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AsuransiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AsuransiController
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //ILogger<PendaftaranPasienBaruController> logger,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: api/Asuransi
        [HttpGet]
        public async Task<IActionResult> GetAllAsuransi()
        {
            var records = _context.Asuransis.Where(a => a.IsDelete == false).ToList();
            if (records == null || !records.Any())
            {
                return NotFound(new { message = "Tidak ada data ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }
      

        // GET: api/Asuransi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsuransiById(Guid id)
        {
            var records = await _context.Asuransis.ToListAsync();
            if (records == null)
            {
                return NotFound(new { message = $"Data dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Data ditemukan.", data = records });
        }

        // POST: api/Asuransi
        [HttpPost]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult>AddAsuransi([FromBody] AsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }
            try
            {
                //ambil user ID dari jwt claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "Anda tidak memiliki akses. || 401 Unauthorized" });
                }

                var dateNow = DateTimeOffset.Now;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _context.Asuransis
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodeAsuransi)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = $"ASR{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodeAsuransi.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = $"ASR{setDateNow}0001";
                    }
                    else
                    {
                        kode = $"ASR{setDateNow}" + (Convert.ToInt32(lastCode.KodeAsuransi.Substring(9)) + 1).ToString("D4");
                    }
                }

                // cek duplikasi
                var isDuplicate = _context.Asuransis
                    .Any(c => c.KodeAsuransi == kode);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Validasi & Simpan Foto dokumen klaim asuransi **
                //string fotoPath = null;
                //if (vm.DokumenKlaim != null && vm.DokumenKlaim.Length > 0)
                //{
                //    var maxSize = 2 * 1024 * 1024;
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                //    var fileExtension = Path.GetExtension(vm.DokumenKlaim.FileName).ToLower();

                //    if (vm.DokumenKlaim.Length > maxSize)
                //    {
                //        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                //    }

                //    if (!allowedExtensions.Contains(fileExtension))
                //    {
                //        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                //    }

                //    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPasienBaru");
                //    if (!Directory.Exists(uploadFolder))
                //    {
                //        Directory.CreateDirectory(uploadFolder);
                //    }

                //    var fotoFileName = $"{}{fileExtension}";
                //    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                //    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                //    {
                //        vm.DokumenKlaim.CopyTo(stream);
                //    }

                //    fotoPath = $"/FotoPasienBaru/{fotoFileName}";
                //}
                //else
                //{
                //    //Jika user tidak upload foto, gunakan foto default
                //    fotoPath = "/FotoPasienBaru/user.jpg";
                //}

                var data = new Asuransi
                {
                    AsuransiId = Guid.NewGuid(),
                    KodeAsuransi = kode,
                    Createdate = DateTimeOffset.Now,
                    NamaAsuransi = vm.NamaAsuransi,
                    JenisAsuransi = vm.JenisAsuransi,
                    KategoriAsuransi = vm.KategoriAsuransi,
                    StatusAsuransi = vm.StatusAsuransi,
                    TanggalMulaiKerjasama = vm.TanggalMulaiKerjasama,
                    TanggalAkhirKerjasama = vm.TanggalAkhirKerjasama,
                    RSRekanan = vm.RSRekanan,
                    MetodeKlaim = vm.MetodeKlaim,
                    WaktuKlaim = vm.WaktuKlaim,
                    BatasMaxKlaimPerTahun = vm.BatasMaxKlaimPerTahun,
                    BatasMaxKlaimPerKunjungan = vm.BatasMaxKlaimPerKunjungan,
                    DokumenKlaim = vm.DokumenKlaim,
                    Layanan = vm.Layanan,
                    PersentasiBiayaPertanggungan = vm.PersentasiBiayaPertanggungan,
                    ObatDitanggung = vm.ObatDitanggung,
                    TambahanTanggungan = vm.TambahanTanggungan,
                    BiayaTidakDitanggung = vm.BiayaTidakDitanggung,
                    MasaTunggu = vm.MasaTunggu,
                    MaxUsiaPasien = vm.MaxUsiaPasien,
                    NoRekRumahSakit = vm.NoRekRumahSakit,
                    NamaBank = vm.NamaBank,
                    NamaBankCabang = vm.NamaBankCabang,
                    TermOfPayment = vm.TermOfPayment,
                    BatasWaktuPembayaran = vm.BatasWaktuPembayaran,
                    PenaltiTerlambatBayar = vm.PenaltiTerlambatBayar,
                    NamaPerusahaanAsuransi = vm.NamaPerusahaanAsuransi,
                    AlamatPusat = vm.AlamatPusat,
                    AlamatCabang = vm.AlamatCabang,
                    NoTelepon = vm.NoTelepon,
                    EmailPusat = vm.EmailPusat,
                    NoHotlineDarurat = vm.NoHotlineDarurat,
                    NamaPerwakilan = vm.NamaPerwakilan,
                    NoTeleponPerwakilan = vm.NoTeleponPerwakilan,
                    EmailPerwakilan = vm.EmailPerwakilan,
                    JabatanPerwakilan = vm.JabatanPerwakilan,
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = UserActiveId,
                    UpdateDateTime = DateTimeOffset.Now,
                    UpdateBy = UserActiveId,
                    DeleteDateTime = DateTimeOffset.Now,
                    DeleteBy = UserActiveId,
                    IsDelete = false
                };

                Console.WriteLine(data.NamaAsuransi);
                _context.Asuransis.Add(data);
                _context.SaveChanges();
                return Created("", new
                {
                    message = "Data berhasil ditambahkan. || 201 Created",
                });
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // PUT: api/Asuransi/{id}
        [HttpPut("{id}")]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAsuransi(Guid id, [FromBody] AsuransiViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid. || 400 Bad Request" });
            }

            try
            {
                // ambil user ID dari jwt claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "Anda tidak memiliki akses. || 401 Unauthorized" });
                }

                // cari data asuransi berdasarkan ID
                var asuransi = _context.Asuransis.Find(id);
                if (asuransi == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // update data asuransi
                asuransi.NamaAsuransi = vm.NamaAsuransi ?? asuransi.NamaAsuransi;
                asuransi.JenisAsuransi = vm.JenisAsuransi ?? asuransi.JenisAsuransi;
                asuransi.KategoriAsuransi = vm.KategoriAsuransi ?? asuransi.KategoriAsuransi;
                asuransi.StatusAsuransi = vm.StatusAsuransi ?? asuransi.StatusAsuransi;
                asuransi.TanggalMulaiKerjasama = vm.TanggalMulaiKerjasama ?? asuransi.TanggalMulaiKerjasama;
                asuransi.TanggalAkhirKerjasama = vm.TanggalAkhirKerjasama ?? asuransi.TanggalAkhirKerjasama;
                asuransi.RSRekanan = vm.RSRekanan ?? asuransi.RSRekanan;
                asuransi.MetodeKlaim = vm.MetodeKlaim ?? asuransi.MetodeKlaim;
                asuransi.WaktuKlaim = vm.WaktuKlaim ?? asuransi.WaktuKlaim;
                asuransi.BatasMaxKlaimPerTahun = vm.BatasMaxKlaimPerTahun ?? asuransi.BatasMaxKlaimPerTahun;
                asuransi.BatasMaxKlaimPerKunjungan = vm.BatasMaxKlaimPerKunjungan ?? asuransi.BatasMaxKlaimPerKunjungan;
                //asuransi.DokumenKlaim = vm.DokumenKlaim ?? asuransi.DokumenKlaim;
                asuransi.Layanan = vm.Layanan ?? asuransi.Layanan;
                asuransi.PersentasiBiayaPertanggungan = vm.PersentasiBiayaPertanggungan ?? asuransi.PersentasiBiayaPertanggungan;
                asuransi.ObatDitanggung = vm.ObatDitanggung ?? asuransi.ObatDitanggung;
                asuransi.TambahanTanggungan = vm.TambahanTanggungan ?? asuransi.TambahanTanggungan;
                asuransi.BiayaTidakDitanggung = vm.BiayaTidakDitanggung ?? asuransi.BiayaTidakDitanggung;
                asuransi.MasaTunggu = vm.MasaTunggu ?? asuransi.MasaTunggu;
                asuransi.MaxUsiaPasien = vm.MaxUsiaPasien ?? asuransi.MaxUsiaPasien;
                asuransi.NoRekRumahSakit = vm.NoRekRumahSakit ?? asuransi.NoRekRumahSakit;
                asuransi.NamaBank = vm.NamaBank ?? asuransi.NamaBank;
                asuransi.NamaBankCabang = vm.NamaBankCabang ?? asuransi.NamaBankCabang;
                asuransi.TermOfPayment = vm.TermOfPayment ?? asuransi.TermOfPayment;
                asuransi.BatasWaktuPembayaran = vm.BatasWaktuPembayaran ?? asuransi.BatasWaktuPembayaran;
                asuransi.PenaltiTerlambatBayar = vm.PenaltiTerlambatBayar ?? asuransi.PenaltiTerlambatBayar;
                asuransi.NamaPerusahaanAsuransi = vm.NamaPerusahaanAsuransi ?? asuransi.NamaPerusahaanAsuransi;
                asuransi.AlamatPusat = vm.AlamatPusat ?? asuransi.AlamatPusat;
                asuransi.AlamatCabang = vm.AlamatCabang ?? asuransi.AlamatCabang;
                asuransi.NoTelepon = vm.NoTelepon ?? asuransi.NoTelepon;
                asuransi.EmailPusat = vm.EmailPusat ?? asuransi.EmailPusat;
                asuransi.NoHotlineDarurat = vm.NoHotlineDarurat ?? asuransi.NoHotlineDarurat;
                asuransi.NamaPerwakilan = vm.NamaPerwakilan ?? asuransi.NamaPerwakilan;
                asuransi.NoTeleponPerwakilan = vm.NoTeleponPerwakilan ?? asuransi.NoTeleponPerwakilan;
                asuransi.EmailPerwakilan = vm.EmailPerwakilan ?? asuransi.EmailPerwakilan;
                asuransi.JabatanPerwakilan = vm.JabatanPerwakilan ?? asuransi.JabatanPerwakilan;

                // **Update Foto Profil**
                //if (vm.Foto != null && vm.Foto.Length > 0)
                //{
                //    var maxSize = 2 * 1024 * 1024;
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                //    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                //    if (vm.Foto.Length > maxSize)
                //    {
                //        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                //    }

                //    if (!allowedExtensions.Contains(fileExtension))
                //    {
                //        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                //    }

                //    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPasienBaru");
                //    if (!Directory.Exists(uploadFolder))
                //    {
                //        Directory.CreateDirectory(uploadFolder);
                //    }

                //    var fotoFileName = $"{pasien.KodePasien}{fileExtension}";
                //    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                //    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                //    {
                //        vm.Foto.CopyTo(stream);
                //    }

                //    pasien.Foto = $"/FotoPasienBaru/{fotoFileName}";
                //}

                asuransi.UpdateDateTime = DateTimeOffset.Now;
                asuransi.UpdateBy = UserActiveId;
                _context.Asuransis.Update(asuransi);
                _context.SaveChanges();
                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                    //qrCodeUrl = $"{Request.Scheme}://{Request.Host}{pasien.QrCode}",
                    //uploadFotoUrl = $"{Request.Scheme}://{Request.Host}{pasien.Foto}"
                });
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });

            }
        }

        // DELETE: api/Asuransi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsuransi(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari data asuransi
                var asuransi = _context.Asuransis.Find(id);
                if (asuransi == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                asuransi.DeleteBy = UserActiveId;
                asuransi.DeleteDateTime = DateTimeOffset.Now;
                asuransi.IsDelete = true;

                _context.Asuransis.Update(asuransi);
                _context.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PegedPendaftaranPasienBaru(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "asc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
                DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD or YYYY-MM-DDTHH:mm:ssZ")]
                DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
                {
                    if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                    {
                        return BadRequest(new { message = "StartDate tidak boleh lebih besar dari EndDate." });
                    }

                    // Jika tidak menggunakan daterange, gunakan periode filter
                    if (!startDate.HasValue && !endDate.HasValue && periode == null)
                    {
                        return BadRequest(new { message = "Harap pilih periode atau masukkan rentang tanggal yang valid." });
                    }

                    var query = _context.Asuransis.Where(a => a.IsDelete == false).AsQueryable();

                    // 🔍 Filter berdasarkan search
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        query = query.Where(u => u.KodeAsuransi.Contains(search) ||
                                                 u.NamaAsuransi.Contains(search) ||
                                                 u.Layanan.Contains(search));
                    }

                    // 📅 Filter berdasarkan daterange
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query = query.Where(u => u.CreateDateTime.Date >= startDate.Value.Date &&
                                                 u.CreateDateTime.Date <= endDate.Value.Date);
                    }

                    // 📆 Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
                    if (periode.HasValue)
                    {
                        DateTime today = DateTime.UtcNow.Date;

                        switch (periode)
                        {
                            case PeriodeFilter.Today:
                                query = query.Where(u => u.CreateDateTime.Date == today);
                                break;
                            case PeriodeFilter.ThisWeek:
                                query = query.Where(u => u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                                                         u.CreateDateTime.Date <= today);
                                break;
                            case PeriodeFilter.LastWeek:
                                query = query.Where(u => u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                                                         u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek)));
                                break;
                            case PeriodeFilter.ThisMonth:
                                query = query.Where(u => u.CreateDateTime.Month == today.Month &&
                                                         u.CreateDateTime.Year == today.Year);
                                break;
                            case PeriodeFilter.LastMonth:
                                query = query.Where(u => u.CreateDateTime.Month == today.Month - 1 &&
                                                         u.CreateDateTime.Year == today.Year);
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

                    // Sorting Data
                    if (!string.IsNullOrEmpty(orderBy))
                    {
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(e => EF.Property<object>(e, orderBy))
                            : query.OrderBy(e => EF.Property<object>(e, orderBy));
                    }

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
