using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Server;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCvSharp;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Helper;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static QRCoder.PayloadGenerator;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Enum;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class UserActiveController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserActiveController> _logger;

        public UserActiveController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<UserActiveController> logger
            )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        private string GeneratePinPegawai(DateTime? tanggalLahir)
        {
            if (tanggalLahir.HasValue)
            {
                // Mengambil dua digit tanggal dan dua digit bulan dari tanggal lahir  
                string hari = tanggalLahir.Value.Day.ToString("D2");   // Format 2 digit  
                string bulan = tanggalLahir.Value.Month.ToString("D2"); // Format 2 digit  

                // Menggabungkan menjadi PIN 4 digit  
                string pinPegawai = hari + bulan;

                return pinPegawai;
            }

            return string.Empty; // Return empty string if tanggalLahir is null  
        }

        private DateTime? TryParseTanggalToUtc(string tanggal)
        {
            if (DateTime.TryParseExact(
                tanggal,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var result))
            {
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserActive(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.UserActives

                        join t in _applicationDbContext.TipeUsers
                            on a.TipeUserId equals t.TipeUserId into tipeJoin
                        from tipe in tipeJoin.DefaultIfEmpty()

                        join dept in _applicationDbContext.Departements
                            on a.DepartemenId equals dept.DepartementId into deptJoin
                        from dept in deptJoin.DefaultIfEmpty()

                        join pos in _applicationDbContext.Positions
                            on a.PositionId equals pos.PositionId into posJoin
                        from pos in posJoin.DefaultIfEmpty()

                        join creator in _applicationDbContext.UserActives
                            on a.CreateBy equals creator.UserActiveId into creatorJoin
                        from creator in creatorJoin.DefaultIfEmpty()

                        where a.IsDelete == false

                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = creator != null ? creator.FullName : "-",
                            a.UserActiveId,
                            a.UserActiveCode,
                            a.FullName,
                            a.Email,
                            a.TipeUserId,
                            NamaTipeUser = tipe != null ? tipe.NamaTipeUser : "-",
                            a.DepartemenId,
                            NamaDepartemen = dept != null ? dept.NamaDepartement : "-",
                            a.PositionId,
                            NamaPosisi = pos != null ? pos.PositionName : "-",
                            a.IdentityNumber,
                            a.PlaceOfBirth,
                            a.AgamaId,
                            a.IsPerawat,
                            a.JenisPegawai,
                            a.ProvinsiId,
                            a.KabupatenKotaId,
                            a.KecamatanId,
                            a.KelurahanId,
                            a.StatusPegawai,
                            a.Kewarganegaraan,
                            a.NoSTR,
                            a.TglAkhirKontrak,
                            a.TglAwalKontrak,
                            a.TglKeluar,
                            a.TglMasuk,
                            a.FotoName,
                            a.FotoPath,
                        }).OrderByDescending(a=>a.CreateDateTime);

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
        public IActionResult GetUserById(Guid id)
        {
            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.UserActiveId == id);
            if (user == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            // Ambil nama departemen, posisi, dan tipe user (boleh null)
            var departemen = _applicationDbContext.Departements
                .FirstOrDefault(d => d.DepartementId == user.DepartemenId);

            var posisi = _applicationDbContext.Positions
                .FirstOrDefault(p => p.PositionId == user.PositionId);

            var tipeUser = _applicationDbContext.TipeUsers
                .FirstOrDefault(t => t.TipeUserId == user.TipeUserId);

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = new
                {
                    user.UserActiveId,
                    user.UserActiveCode,
                    user.FullName,
                    user.IdentityNumber,
                    user.PlaceOfBirth,
                    user.DateOfBirth,
                    user.Gender,
                    user.Address,
                    user.Handphone,
                    user.Email,
                    user.IsActive,
                    user.DepartemenId,
                    NamaDepartemen = departemen?.NamaDepartement ?? null,
                    user.PositionId,
                    NamaPosisi = posisi?.PositionName ?? null,
                    user.TipeUserId,
                    NamaTipeUser = tipeUser?.NamaTipeUser ?? null,
                    user.ProvinsiId,
                    user.KabupatenKotaId,
                    user.KecamatanId,
                    user.KelurahanId,
                    user.Kewarganegaraan,
                    user.AgamaId,
                    user.IsPerawat,
                    user.NoSTR,
                    user.StatusPegawai,
                    user.JenisPegawai,
                    user.TglMasuk,
                    user.TglKeluar,
                    user.TglAwalKontrak,
                    user.TglAkhirKontrak,
                    user.FotoName,
                    user.FotoPath,
                    user.CreateDateTime,
                    user.CreateBy,
                    user.UpdateDateTime,
                    user.UpdateBy,
                    user.DeleteDateTime,
                    user.DeleteBy,
                    user.IsDelete
                }
            });
        }

        [HttpGet("UserActiveDoctors/{id}")]
        public async Task<IActionResult> GetUserActiveDokterById(Guid id)
        {
            try
            {
                // Ambil data UserActive berdasarkan ID
                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.UserActiveId == id && (u.IsDelete == null || u.IsDelete == false));

                if (user == null)
                {
                    return NotFound(new { message = "UserActive tidak ditemukan." });
                }

                // Ambil nama tipe user dari TipeUsers
                var tipeUser = await _applicationDbContext.TipeUsers
                    .FirstOrDefaultAsync(t => t.TipeUserId == user.TipeUserId);
                var tipeUserName = tipeUser?.NamaTipeUser ?? "Unknown";

                // Jika user adalah dokter, cari data dari tabel Dokter
                object dataDokter = null;
                if (tipeUserName.ToLower() == "dokter")
                {
                    var dokter = await _applicationDbContext.Dokters
                        .FirstOrDefaultAsync(d =>
                            d.UserActiveId == user.UserActiveId &&
                            (d.IsDelete == false));

                    if (dokter != null)
                    {
                        dataDokter = new
                        {
                            dokter.DokterId,
                            //dokter.KdDokter,
                            //dokter.NmDokter,
                            //dokter.Nik,
                            //dokter.Email,
                            //dokter.Nohp,
                            //dokter.Alamat,
                            //dokter.Spesialis,
                            //dokter.Str,
                            //dokter.Sip,
                            //dokter.TglSip,
                            //dokter.TglStr,
                            //dokter.IsActive,
                            //dokter.FotoName,
                            //dokter.FotoPath
                        };
                    }
                }

                return Ok(new
                {
                    message = "Berhasil mengambil data user active dokter",
                    data = new
                    {
                        user.UserActiveId,
                        user.UserActiveCode,
                        user.FullName,
                        user.Email,
                        user.IdentityNumber,
                        user.PlaceOfBirth,
                        user.DateOfBirth,
                        user.Gender,
                        user.Handphone,
                        user.Address,
                        user.IsActive,
                        user.TipeUserId,
                        NamaTipeUser = tipeUserName,
                        user.DepartemenId,
                        user.PositionId,
                        //FotoPath = string.IsNullOrWhiteSpace(user.FotoPath)
                        //    ? "/FotoDokter/dokter.jpg"
                        //    : user.FotoPath,
                        user.FotoName,
                        user.FotoPath,
                        dataDokter,
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPost("UserActive")]
        public async Task<IActionResult> CreateUserActive([FromForm] UserActiveViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var dateNow = DateTime.UtcNow; ;
                var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.UserActives
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.UserActiveCode)
                    .FirstOrDefault();

                string kode;
                if (lastCode == null)
                {
                    kode = "USR" + setDateNow + "0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.UserActiveCode.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kode = "USR" + setDateNow + "0001";
                    }
                    else
                    {
                        kode = "USR" + setDateNow +
                            (Convert.ToInt32(lastCode.UserActiveCode.Substring(9)) + 1).ToString("D4");
                    }
                }

                // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
                if (!DateTime.TryParseExact(vm.DateOfBirth, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                // Ambil tipe user
                var tipeUser = await _applicationDbContext.TipeUsers.FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);
                var isDokter = tipeUser?.NamaTipeUser.ToLower() == "dokter";

                // Jika dokter, generate kode dokter untuk nama file
                string kodeDokter = "";
                if (isDokter)
                {
                    var lastDr = _applicationDbContext.Dokters
                        .Where(d => d.CreateDateTime.Date == dateNow.Date)
                        .OrderByDescending(k => k.KdDokter)
                        .FirstOrDefault();

                    if (lastDr == null)
                    {
                        kodeDokter = "DKR" + setDateNow + "0001";
                    }
                    else
                    {
                        var lastCodeTrim = lastDr.KdDokter.Substring(3, 6);

                        if (lastCodeTrim != setDateNow)
                        {
                            kodeDokter = "DKR" + setDateNow + "0001";
                        }
                        else
                        {
                            kodeDokter = "DKR" + setDateNow +
                                (Convert.ToInt32(lastDr.KdDokter.Substring(9)) + 1).ToString("D4");
                        }
                    }
                }

                // validasi foto
                string fotoPath = null;
                string fotoFileName = null;
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });

                    var folder = isDokter ? "FotoDokter" : "FotoUser";
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    fotoFileName = isDokter ? $"{kodeDokter}{fileExtension}" : $"{kode}{fileExtension}";
                    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                        await vm.Foto.CopyToAsync(stream);

                    fotoPath = $"/{folder}/{fotoFileName}";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                {
                    { new StreamContent(ms) { Headers = { ContentType = new MediaTypeHeaderValue(vm.Foto.ContentType) } }, "file", fotoFileName },
                    { new StringContent(folder), "folderTarget" }
                };

                    await client.PostAsync("http://160.20.104.98:5050/upload", content);
                }
                else
                {
                    fotoPath = isDokter ? "/FotoDokter/dokter.jpg" : "/FotoUser/user.jpg";
                    fotoFileName = isDokter ? "dokter.jpg" : "user.jpg";
                }

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.UserActives
                    .Any(c => c.UserActiveCode == kode && c.Email == vm.Email);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }


                // Validate ModelState
                if (ModelState.IsValid)
                {

                    var userLogin = new ApplicationUser
                    {
                        KodeUser = kode,
                        NamaUser = vm.FullName,
                        Email = vm.Email,
                        UserName = vm.Email,
                        PhoneNumber = vm.Handphone,
                        IsActive = true
                    };
                    var id = Guid.NewGuid();
                    var user = new UserActive
                    {
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        UserActiveId = id,
                        UserActiveCode = kode,
                        FullName = vm.FullName,
                        PinPegawai = DelegasiVerifikasi.ComputeSha256Hash(GeneratePinPegawai(TryParseTanggalToUtc(vm.DateOfBirth))),
                        IdentityNumber = vm.IdentityNumber,
                        PlaceOfBirth = vm.PlaceOfBirth,
                        DateOfBirth = (DateTime)TryParseTanggalToUtc(vm.DateOfBirth),
                        Gender = vm.Gender,
                        Address = vm.Address,
                        Handphone = vm.Handphone,
                        Email = vm.Email,
                        IsActive = true,
                        DepartemenId = vm.DepartemenId,
                        PositionId = vm.PositionId,
                        TipeUserId = vm.TipeUserId,
                        ProvinsiId = vm.ProvinsiId,
                        KabupatenKotaId = vm.KabupatenKotaId,
                        KecamatanId = vm.KecamatanId,
                        KelurahanId = vm.KelurahanId,
                        Kewarganegaraan = vm.Kewarganegaraan,
                        AgamaId = vm.AgamaId,
                        IsPerawat = vm.IsPerawat,
                        NoSTR = vm.NoSTR,
                        StatusPegawai = vm.StatusPegawai,
                        JenisPegawai = vm.JenisPegawai,
                        TglMasuk = TryParseTanggalToUtc(vm.TglMasuk),
                        TglKeluar = TryParseTanggalToUtc(vm.TglKeluar),
                        TglAwalKontrak = TryParseTanggalToUtc(vm.TglAwalKontrak),
                        TglAkhirKontrak = TryParseTanggalToUtc(vm.TglAkhirKontrak),

                        FotoName = fotoFileName,
                        FotoPath = fotoPath
                    };

                    var passTglLahir = parsedDate.ToString("ddMMMyyyy");

                    using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                    var resultLogin = await _userManager.CreateAsync(userLogin, passTglLahir);

                    if (!resultLogin.Succeeded)
                    {
                        var errorMessage = string.Join(", ", resultLogin.Errors.Select(e => e.Description));
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Gagal membuat user login: {errorMessage}" });
                    }

                    _applicationDbContext.UserActives.Add(user);
                                        
                    // Jika tipe user adalah dokter → buat entri di tabel Dokter
                    if (isDokter)
                    {
                        var dokter = new Dokter
                        {
                            DokterId = Guid.NewGuid(),
                            KdDokter = kodeDokter,
                            NmDokter = vm.FullName,
                            Email = vm.Email,
                            Nohp = vm.Handphone,
                            Nik = vm.IdentityNumber,
                            Alamat = vm.Address,
                            FotoPath = fotoPath,
                            FotoName = fotoFileName,
                            UserActiveId = user.UserActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsActive = true,
                            IsDelete = false
                        };
                        _applicationDbContext.Dokters.Add(dokter);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // ROLE USER
                    var idnetuser = await _applicationDbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == vm.Email);

                    if (idnetuser != null) // ← ini yang benar
                    {
                        // Memastikan PositionId tidak kosong
                        if (vm.PositionId.HasValue)
                        {
                            var createRoleResponse = CreateRole(vm.PositionId.Value, idnetuser.Id);
                            // Jika CreateRole mengembalikan Task/IActionResult, jangan lupa pakai await
                        }
                    }
                    // END ROLE USER

                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });

            }
        }

        [HttpPut("UserActive/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UserActiveViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari data user actives
                var data = _applicationDbContext.UserActives.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // ✅ Cek duplikasi email
                var isDuplicateEmail = _applicationDbContext.UserActives
                    .Any(u => u.Email == vm.Email && u.UserActiveId != data.UserActiveId && (u.IsDelete == null || u.IsDelete == false));

                if (isDuplicateEmail)
                {
                    return Conflict(new { message = "Email sudah digunakan oleh user lain. || 409 Conflict" });
                }

                // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
                if (!DateTime.TryParseExact(vm.DateOfBirth, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                // cara tipe user dokter
                var tipeUser = await _applicationDbContext.TipeUsers.FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);
                var isDokter = tipeUser?.NamaTipeUser.ToLower() == "dokter";

                //cari data dokter
                var dataDokter = _applicationDbContext.Dokters
                    .FirstOrDefault(d => d.NmDokter == data.FullName && d.Email == data.Email);

                //update data di tabel ApplicationUser
                var userLogin = await _userManager.FindByEmailAsync(data.Email.ToString());
                if (userLogin == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }
                else
                {
                    userLogin.NamaUser = vm.FullName;
                    userLogin.Email = vm.Email;
                    userLogin.UserName = vm.Email;
                    userLogin.PhoneNumber = vm.Handphone;
                    userLogin.IsActive = true;
                }

                // Perbarui data user di tabel UserActive
                data.FullName = vm.FullName;
                data.IdentityNumber = vm.IdentityNumber;
                data.Email = vm.Email;
                data.PlaceOfBirth = vm.PlaceOfBirth;
                data.DateOfBirth = parsedDate;
                data.Gender = vm.Gender;
                data.Address = vm.Address;
                data.Handphone = vm.Handphone;
                data.Email = vm.Email;
                data.TipeUserId = vm.TipeUserId;
                data.DepartemenId = vm.DepartemenId;
                data.PositionId = vm.PositionId;
                data.IsActive = vm.IsActive;
                data.UpdateBy = UserActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;


                string fotoFileName = data.FotoName;
                string fotoPath = data.FotoPath;

                // validasi edit foto
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    var folder = isDokter ? "FotoDokter" : "FotoUser";
                    var fotoBaseName = isDokter && dataDokter != null ? dataDokter.KdDokter : data.UserActiveCode;
                    if (isDokter && dataDokter != null)
                    {
                        fotoBaseName = dataDokter.KdDokter;
                    }
                    fotoFileName = $"{fotoBaseName}{fileExtension}";
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    var fotoFilePath = Path.Combine(uploadPath, fotoFileName);
                    using var stream = new FileStream(fotoFilePath, FileMode.Create);
                    await vm.Foto.CopyToAsync(stream);

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                        { new StreamContent(ms) { Headers = { ContentType = new MediaTypeHeaderValue(vm.Foto.ContentType) } }, "file", fotoFileName },
                        { new StringContent(folder), "folderTarget" },
                        { new StringContent(data.FotoName ?? ""), "oldFileName" }
                    };

                    var response = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    fotoPath = $"/{folder}/{fotoFileName}";
                    data.FotoName = fotoFileName;
                    data.FotoPath = fotoPath;
                }


                // Update Dokter jika diperlukan
                if (isDokter && dataDokter != null)
                {
                    dataDokter.NmDokter = vm.FullName;
                    dataDokter.Email = vm.Email;
                    dataDokter.Nik = vm.IdentityNumber;
                    dataDokter.Nohp = vm.Handphone;
                    dataDokter.Alamat = vm.Address;
                    dataDokter.FotoPath = fotoPath;
                    dataDokter.FotoName = fotoFileName;
                    dataDokter.UpdateDateTime = data.UpdateDateTime;
                    dataDokter.UpdateBy = UserActiveId;
                    _applicationDbContext.Dokters.Update(dataDokter);
                }

                // Reset password
                var newPassword = parsedDate.ToString("ddMMMyyyy");
                var token = await _userManager.GeneratePasswordResetTokenAsync(userLogin);
                var resetPassResult = await _userManager.ResetPasswordAsync(userLogin, token, newPassword);

                if (!resetPassResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Gagal mengubah password. Pastikan password valid." });
                }

                // Simpan semua perubahan
                _applicationDbContext.UserActives.Update(data);
                await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // ROLE USER
                // Memastikan PositionId tidak kosong
                // ROLE USER
                var idnetuser = await _applicationDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == vm.Email);

                if (idnetuser != null) // ← ini yang benar
                {
                    // Memastikan PositionId tidak kosong
                    if (vm.PositionId.HasValue)
                    {
                        var createRoleResponse = CreateRole(vm.PositionId.Value, idnetuser.Id);
                        // Jika CreateRole mengembalikan Task/IActionResult, jangan lupa pakai await
                    }
                }
                // END ROLE USER
                return Created("", new
                {
                    message = "Update Data Berhasil || 201 Created"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost("UbahPassword")]
        public async Task<IActionResult> UbahPassword(ResetPasswordViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari user 
                var user = await _userManager.FindByEmailAsync(EmailLogin);
                if (user == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }

                // Generate token dan ubah password langsung
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

                if(!result.Succeeded)
                {
                    return BadRequest(new { message = "Gagal mengubah password.", errors = result.Errors.Select(e => e.Description) });
                }

                return Ok(new { message = "Password berhasil diubah." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }
                // cari user 
                var user = await _userManager.FindByEmailAsync(EmailLogin);
                if (user == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }
                // cari data user active
                var data = _applicationDbContext.UserActives.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                var NewPassword = data.DateOfBirth.ToString("ddMMMyyyy");

                // Generate token dan ubah password langsung
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Gagal mengubah password.", errors = result.Errors.Select(e => e.Description) });
                }
                return Ok(new { message = "Reset Password Berhasil" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("UpdateFoto/{id}")]
        public async Task<IActionResult> UpdateFoto(Guid id, [FromForm] UpdateFotoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }
            try
            {
                // **Ambil User ID dari JWT Claims**  
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari data user actives  
                var data = _applicationDbContext.UserActives.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data user tidak ditemukan." });
                }

                // cara tipe user dokter  
                var tipeUser = await _applicationDbContext.TipeUsers.FirstOrDefaultAsync(t => t.TipeUserId == data.TipeUserId);
                var isDokter = tipeUser?.NamaTipeUser.ToLower() == "dokter";
                var dataDokter = isDokter
                    ? _applicationDbContext.Dokters.FirstOrDefault(d => d.NmDokter == data.FullName && d.Email == data.Email)
                    : null;

                // variabel lokasi foto
                string fotoFileName = data.FotoName;
                string fotoPath = data.FotoPath;

                // validasi edit foto  
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    var folder = isDokter ? "FotoDokter" : "FotoUser";
                    var fotoBaseName = isDokter && dataDokter != null ? dataDokter.KdDokter : data.UserActiveCode;
                    fotoFileName = $"{fotoBaseName}{fileExtension}";
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    var fotoFilePath = Path.Combine(uploadPath, fotoFileName);
                    using var stream = new FileStream(fotoFilePath, FileMode.Create);
                    await vm.Foto.CopyToAsync(stream);

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                       { new StreamContent(ms) { Headers = { ContentType = new MediaTypeHeaderValue(vm.Foto.ContentType) } }, "file", fotoFileName },
                       { new StringContent(folder), "folderTarget" },
                       { new StringContent(data.FotoName ?? ""), "oldFileName" }
                    };

                    var response = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    fotoPath = $"/{folder}/{fotoFileName}";
                    data.FotoName = fotoFileName;
                    data.FotoPath = fotoPath;
                }

                // Update data dokter  
                if (isDokter && dataDokter != null)
                {
                    dataDokter.FotoPath = fotoPath;
                    dataDokter.FotoName = fotoFileName;
                    dataDokter.UpdateDateTime = DateTimeOffset.UtcNow;
                    dataDokter.UpdateBy = UserActiveId;
                    _applicationDbContext.Dokters.Update(dataDokter);
                }

                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;
                _applicationDbContext.UserActives.Update(data);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "Foto berhasil diperbarui." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")] 
        public async Task <IActionResult> DeleteUser(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data User**
                var data = _applicationDbContext.UserActives.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // Hapus user login dari tabel AspNetUsers (permanen)
                var userLogin = await _userManager.FindByEmailAsync(data.Email);
                if (userLogin != null)
                {
                    var result = await _userManager.DeleteAsync(userLogin);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Gagal menghapus akun login dari sistem." });
                    }
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;
                data.IsActive = false;

                _applicationDbContext.UserActives.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                // Tangani error jika ada masalah
                return StatusCode(500, $"Terjadi kesalahan saat menghapus data: {ex.Message}");
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedUserActive(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null,
        [FromQuery] EnumJenisUser? tipeUser = null)
        {
            // query
            var query = from a in _applicationDbContext.UserActives

                        join t in _applicationDbContext.TipeUsers
                            on a.TipeUserId equals t.TipeUserId into tipeJoin
                        from tipe in tipeJoin.DefaultIfEmpty()

                        join dept in _applicationDbContext.Departements
                            on a.DepartemenId equals dept.DepartementId into deptJoin
                        from dept in deptJoin.DefaultIfEmpty()

                        join pos in _applicationDbContext.Positions
                            on a.PositionId equals pos.PositionId into posJoin
                        from pos in posJoin.DefaultIfEmpty()

                        join creator in _applicationDbContext.UserActives
                            on a.CreateBy equals creator.UserActiveId into creatorJoin
                        from creator in creatorJoin.DefaultIfEmpty()

                        where a.IsDelete == false

                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = creator != null ? creator.FullName : "-",
                            a.UserActiveId,
                            a.UserActiveCode,
                            a.FullName,
                            a.Email,
                            a.TipeUserId,
                            NamaTipeUser = tipe != null ? tipe.NamaTipeUser : "-",
                            a.DepartemenId,
                            NamaDepartemen = dept != null ? dept.NamaDepartement : "-",
                            a.PositionId,
                            NamaPosisi = pos != null ? pos.PositionName : "-",
                            a.IdentityNumber,
                            a.PlaceOfBirth,
                            a.AgamaId,
                            a.IsPerawat,
                            a.JenisPegawai,
                            a.ProvinsiId,
                            a.KabupatenKotaId,
                            a.KecamatanId,
                            a.KelurahanId,
                            a.StatusPegawai,
                            a.Kewarganegaraan,
                            a.NoSTR,
                            a.TglAkhirKontrak,
                            a.TglAwalKontrak,
                            a.TglKeluar,
                            a.TglMasuk,
                            a.FotoName,
                            a.FotoPath,
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.FullName, search) ||
                    EF.Functions.ILike(u.CreateByName, search)  ||
                    EF.Functions.ILike(u.Email, search) ||
                    EF.Functions.ILike(u.NamaTipeUser, search)
                );
            }

            // === Filter Tipe User (enum -> Display(Name)) ===
            if (tipeUser.HasValue)
            {
                var memberInfo = typeof(EnumJenisUser)
                    .GetMember(tipeUser.Value.ToString())
                    .FirstOrDefault();

                var displayAttr = memberInfo?
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .Cast<DisplayAttribute>()
                    .FirstOrDefault();

                // Jika tidak ada Display, fallback ke nama enum
                string displayName = displayAttr?.Name ?? tipeUser.Value.ToString();

                // bandingkan ke kolom string di DB (terjemahan ke SQL aman)
                query = query.Where(u => u.NamaTipeUser == displayName);
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
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "UserActiveCode" => query.OrderByDescending(u => u.UserActiveCode),
                    "FullName" => query.OrderByDescending(u => u.FullName),
                    "TipeUser" => query.OrderByDescending(u => u.NamaTipeUser),

                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "UserActiveCode" => query.OrderBy(u => u.UserActiveCode),
                    "FullName" => query.OrderBy(u => u.FullName),
                    "TipeUser" => query.OrderBy(u => u.NamaTipeUser),
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

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(Guid positionId, string pendaftaranPasienBaruId)
        {
            var newRolePositions = new List<IdentityUserRole<string>>();

            var existingRoles = _applicationDbContext.UserRoles
            .Where(ur => ur.UserId == pendaftaranPasienBaruId.ToString())
            .ToList();

            // Ambil RoleId yang terkait dengan PositionId dari RolePositions
            var rolePosition = await _applicationDbContext.RolePositions
                .Where(r => r.PositionId == positionId.ToString())
                .Select(r => r.RoleId)
                .ToListAsync();

            foreach (var roleId in rolePosition)
            {
                // Cek apakah role-position sudah ada di IdentityUserRole
                var exists = await _applicationDbContext.UserRoles
                    .AnyAsync(ur => ur.RoleId == roleId && ur.UserId == pendaftaranPasienBaruId.ToString());

                if (!exists)
                {
                    // Jika belum ada, tambahkan role ke IdentityUserRole
                    newRolePositions.Add(new IdentityUserRole<string>
                    {
                        UserId = pendaftaranPasienBaruId.ToString(), // User yang terdaftar (gunakan string karena IdentityUserRole menggunakan string)
                        RoleId = roleId.ToString() // Role yang terkait dengan PositionId
                    });
                }
            }

            // Menambahkan data ke IdentityUserRole
            if (newRolePositions.Any())
            {
                _applicationDbContext.UserRoles.AddRangeAsync(newRolePositions);
                await _applicationDbContext.SaveChangesAsync();
                return Ok(new { message = "Role untuk posisi berhasil ditambahkan ke user." });
            }

            return BadRequest(new { message = "Tidak ada role baru yang perlu ditambahkan." });
        }
    }
}
