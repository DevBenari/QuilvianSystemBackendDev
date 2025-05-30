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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using SkiaSharp;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using static QRCoder.PayloadGenerator;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
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
                            d.Email == user.Email &&
                            d.NmDokter == user.FullName &&
                            (d.IsDelete == null || d.IsDelete == false));

                    if (dokter != null)
                    {
                        dataDokter = new
                        {
                            dokter.DokterId,
                            dokter.KdDokter,
                            dokter.NmDokter,
                            dokter.Nik,
                            dokter.Email,
                            dokter.Nohp,
                            dokter.Alamat,
                            dokter.Spesialis,
                            dokter.Str,
                            dokter.Sip,
                            dokter.TglSip,
                            dokter.TglStr,
                            dokter.IsActive,
                            dokter.FotoName,
                            dokter.FotoPath
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
                        DokterInfo = dataDokter
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }



        //[HttpPost("UserActiveDoctors")]
        //public async Task<IActionResult> CreateUserActiveDokter([FromForm] UserActiveViewModel vm)
        //{
        //    if (vm == null)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Ambil User ID dari JWT Claims**
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var dateNow = DateTime.UtcNow; ;
        //        var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

        //        // Ambil data terakhir untuk hari ini (tanpa ToString di query)
        //       var lastCode = _applicationDbContext.UserActives
        //            .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //            .OrderByDescending(k => k.UserActiveCode)
        //            .FirstOrDefault();

        //        string kode;
        //        if (lastCode == null)
        //        {
        //            kode = "USR" + setDateNow + "0001";
        //        }
        //        else
        //        {
        //            var lastCodeTrim = lastCode.UserActiveCode.Substring(3, 6);

        //            if (lastCodeTrim != setDateNow)
        //            {
        //                kode = "USR" + setDateNow + "0001";
        //            }
        //            else
        //            {
        //                kode = "USR" + setDateNow +
        //                    (Convert.ToInt32(lastCode.UserActiveCode.Substring(9)) + 1).ToString("D4");
        //            }
        //        }

        //        // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
        //        if (!DateTime.TryParseExact(vm.DateOfBirth, "yyyy-MM-dd",
        //            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        //        {
        //            return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
        //        }
        //        parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        //        // Cek Duplikasi
        //        var isDuplicate = _applicationDbContext.UserActives
        //            .Any(c => c.UserActiveCode == kode  && c.Email == vm.Email);

        //        if (isDuplicate)
        //        {
        //            return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
        //        }

        //        // Ambil nama tipe user dari tabel MstTipeUser berdasarkan TipeUserId
        //        var tipeUser = await _applicationDbContext.TipeUsers
        //            .FirstOrDefaultAsync(t => t.TipeUserId == vm.TipeUserId);

        //        var tipeUserName = tipeUser?.NamaTipeUser ?? "Unknown";

        //        // Cek apakah tipe user adalah dokter
        //        if (tipeUserName.ToLower() == "dokter")
        //        {
        //            //buat kode dokter
        //            // Ambil data terakhir untuk hari ini (tanpa ToString di query)
        //            var lastCodedr = _applicationDbContext.Dokters
        //                .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //                .OrderByDescending(k => k.KdDokter)
        //                .FirstOrDefault();

        //            string kodeDokter;
        //            if (lastCodedr == null)
        //            {
        //                kodeDokter = "DKR" + setDateNow + "0001";
        //            }
        //            else
        //            {
        //                var lastCodeTrim = lastCodedr.KdDokter.Substring(3, 6);

        //                if (lastCodeTrim != setDateNow)
        //                {
        //                    kodeDokter = "DKR" + setDateNow + "0001";
        //                }
        //                else
        //                {
        //                    kodeDokter = "DKR" + setDateNow +
        //                        (Convert.ToInt32(lastCodedr.KdDokter.Substring(9)) + 1).ToString("D4");
        //                }
        //            }

        //            // validasi foto dokter
        //            string fotoPath = null;
        //            string fotoFileName = null;
        //            if (vm.Foto != null && vm.Foto.Length > 0)
        //            {
        //                var maxSize = 2 * 1024 * 1024;
        //                var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        //                var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

        //                if (vm.Foto.Length > maxSize)
        //                {
        //                    return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
        //                }

        //                if (!allowedExtensions.Contains(fileExtension))
        //                {
        //                    return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
        //                }

        //                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoDokter");
        //                if (!Directory.Exists(uploadFolder))
        //                {
        //                    Directory.CreateDirectory(uploadFolder);
        //                }

        //                fotoFileName = $"{kodeDokter}{fileExtension}";
        //                var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

        //                using (var stream = new FileStream(fotoFilePath, FileMode.Create))
        //                {
        //                    vm.Foto.CopyTo(stream);
        //                }

        //                fotoPath = $"/FotoDokter/{fotoFileName}";

        //                // 📤 **Kirim foto ke server Python Flask**
        //                using var client = new HttpClient();
        //                using var ms = new MemoryStream();
        //                await vm.Foto.CopyToAsync(ms);
        //                ms.Position = 0;

        //                var content = new MultipartFormDataContent {
        //                // File utama
        //                { new StreamContent(ms) {
        //                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
        //                }, "file", fotoFileName },

        //                // Nama folder tujuan di server Flask
        //                { new StringContent("FotoDokter"), "folderTarget" }
        //                };

        //                // Ganti IP di bawah dengan alamat Python Flask server Anda
        //                var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
        //            }
        //            else
        //            {
        //                //Jika user tidak upload foto, gunakan foto default
        //                fotoPath = "/FotoDokter/dokter.jpg";
        //                fotoFileName = "dokter.jpg";
        //            }
        //            var userLogin = new ApplicationUser
        //            {
        //                KodeUser = kode,
        //                NamaUser = vm.FullName,
        //                Email = vm.Email,
        //                UserName = vm.Email,
        //                PhoneNumber = vm.Handphone,
        //                IsActive = true
        //            };

        //            var user = new UserActive
        //            {
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                CreateBy = UserActiveId,
        //                UserActiveId = Guid.NewGuid(),
        //                UserActiveCode = kode,
        //                FullName = vm.FullName,
        //                IdentityNumber = vm.IdentityNumber,
        //                PlaceOfBirth = vm.PlaceOfBirth,
        //                DateOfBirth = parsedDate,
        //                Gender = vm.Gender,
        //                Address = vm.Address,
        //                Handphone = vm.Handphone,
        //                Email = vm.Email,
        //                IsActive = true,
        //                DepartemenId = vm.DepartemenId,
        //                PositionId = vm.PositionId,
        //                TipeUserId = vm.TipeUserId,
        //                FotoName = fotoFileName,
        //                FotoPath = fotoPath,
        //            };

        //            var dataDokter = new Dokter
        //            {
        //                DokterId = Guid.NewGuid(),
        //                Alamat = vm.Address,
        //                NmDokter = vm.FullName,
        //                KdDokter = kodeDokter,
        //                Nohp = vm.Handphone,
        //                Email = vm.Email,
        //                Nik = vm.IdentityNumber,
        //                CreateBy = UserActiveId,
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                FotoName = fotoFileName,
        //                FotoPath = fotoPath,
        //                IsActive = true
        //            };

        //            _applicationDbContext.Dokters.Add(dataDokter);
        //            await _applicationDbContext.SaveChangesAsync();

        //            // **Buat password berdasarkan tanggal lahir**
        //            var passTglLahir = parsedDate.ToString("ddMMMyyyy");

        //            var resultLogin = await _userManager.CreateAsync(userLogin, passTglLahir);

        //            if (resultLogin.Succeeded)
        //            {
        //                _applicationDbContext.UserActives.Add(user);
        //                _applicationDbContext.SaveChanges();
        //                return Created("", new
        //                {
        //                    message = "Tambah Data Berhasil || 201 Created"
        //                });
        //            }
        //            else
        //            {
        //                return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
        //            }

        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Tipe User tidak valid || 400 Bad Request" });
        //        }
        //    }

            
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

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

                    var user = new UserActive
                    {
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        UserActiveId = Guid.NewGuid(),
                        UserActiveCode = kode,
                        FullName = vm.FullName,
                        IdentityNumber = vm.IdentityNumber,
                        PlaceOfBirth = vm.PlaceOfBirth,
                        DateOfBirth = parsedDate,
                        Gender = vm.Gender,
                        Address = vm.Address,
                        Handphone = vm.Handphone,
                        Email = vm.Email,
                        IsActive = true,
                        DepartemenId = vm.DepartemenId,
                        PositionId = vm.PositionId,
                        TipeUserId = vm.TipeUserId,
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

        //[HttpPut("UserActiveDoctors/{id}")]
        //public async Task<IActionResult> UpdateUserDoctors(Guid id, [FromForm] UserActiveViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        //Ambil User ID dari JWT Claims
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        // **Cari Data Pasien**
        //        var data = _applicationDbContext.UserActives.Find(id);
        //        if (data == null)
        //        {
        //            return NotFound(new { message = "Data tidak ditemukan." });
        //        }

        //        // Cek duplikasi nama dokter
        //        var dataDokter = _applicationDbContext.Dokters
        //            .FirstOrDefault(d => d.NmDokter == data.FullName && d.Email == data.Email);

        //        var isDuplicateNamaDokter = _applicationDbContext.Dokters
        //            .Any(d => d.NmDokter == vm.FullName && d.DokterId != dataDokter.DokterId && (d.IsDelete == null || d.IsDelete == false));

        //        if (isDuplicateNamaDokter)
        //        {
        //            return Conflict(new { message = "Nama dokter sudah digunakan oleh dokter lain. || 409 Conflict" });
        //        }

        //        // Cek duplikasi email
        //        var isDuplicateEmail = _applicationDbContext.UserActives
        //            .Any(u => u.Email == vm.Email && u.UserActiveId != data.UserActiveId && (u.IsDelete == null || u.IsDelete == false));

        //        if (isDuplicateEmail)
        //        {
        //            return Conflict(new { message = "Email sudah digunakan oleh user lain. || 409 Conflict" });
        //        }

        //        // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
        //        if (!DateTime.TryParseExact(vm.DateOfBirth, "yyyy-MM-dd",
        //            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        //        {
        //            return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
        //        }
        //        parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        //        //update data di tabel ApplicationUser
        //        var userLogin = await _userManager.FindByEmailAsync(data.Email.ToString());
        //        if (userLogin == null)
        //        {
        //            return NotFound(new { message = "User tidak ditemukan." });
        //        }
        //        else
        //        {
        //            userLogin.NamaUser = vm.FullName;
        //            userLogin.Email = vm.Email;
        //            userLogin.UserName = vm.Email;
        //            userLogin.PhoneNumber = vm.Handphone;
        //            userLogin.IsActive = true;
        //        }

        //        // Perbarui data user di tabel UserActive
        //        data.FullName = vm.FullName;
        //        data.IdentityNumber = vm.IdentityNumber;
        //        data.PlaceOfBirth = vm.PlaceOfBirth;
        //        data.DateOfBirth = parsedDate;
        //        data.Gender = vm.Gender;
        //        data.Address = vm.Address;
        //        data.Handphone = vm.Handphone;
        //        data.Email = vm.Email;
        //        data.IsActive = vm.IsActive;

        //        data.UpdateBy = UserActiveId;
        //        data.UpdateDateTime = DateTimeOffset.UtcNow;

        //        // perbarui data user dokter di tabel dokter
        //        dataDokter.NmDokter = vm.FullName;
        //        dataDokter.Email = vm.Email;
        //        dataDokter.Nik = vm.IdentityNumber;
        //        dataDokter.Alamat = vm.Address;
        //        dataDokter.Nohp = vm.Handphone;

        //        // Reset password lama (jika ada)
        //        var newPassword = parsedDate.ToString("ddMMMyyyy");
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(userLogin);
        //        var resetPassResult = await _userManager.ResetPasswordAsync(userLogin, token, newPassword);

        //        // validasi edit foto
        //        if (vm.Foto != null && vm.Foto.Length > 0)
        //        {
        //            var maxSize = 2 * 1024 * 1024; // Maksimum 2MB
        //            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        //            var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

        //            if (vm.Foto.Length > maxSize)
        //            {
        //                return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
        //            }

        //            if (!allowedExtensions.Contains(fileExtension))
        //            {
        //                return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
        //            }

        //            var fotoFileName = $"{dataDokter.KdDokter}{fileExtension}";
        //            var oldFileName = dataDokter.FotoName ?? "";

        //            using var client = new HttpClient();
        //            using var ms = new MemoryStream();
        //            await vm.Foto.CopyToAsync(ms);
        //            ms.Position = 0;

        //            var content = new MultipartFormDataContent
        //            {
        //                {
        //                    new StreamContent(ms)
        //                    {
        //                        Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
        //                    }, "file", fotoFileName
        //                },
        //                { new StringContent("FotoDokter"), "folderTarget" },
        //                { new StringContent(oldFileName), "oldFileName" }
        //            };

        //            var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
        //            if (!flaskResponse.IsSuccessStatusCode)
        //            {
        //                return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
        //            }

        //            dataDokter.FotoName = fotoFileName;
        //            dataDokter.FotoPath = $"/FotoDokter/{fotoFileName}";


        //            data.FotoName = fotoFileName;
        //            data.FotoPath = dataDokter.FotoPath;
        //        }

        //        dataDokter.UpdateDateTime = data.UpdateDateTime;
        //        dataDokter.UpdateBy = data.UpdateBy;

        //        _applicationDbContext.Dokters.Update(dataDokter);
        //        _applicationDbContext.SaveChanges();

        //        await _applicationDbContext.SaveChangesAsync();

        //        _applicationDbContext.UserActives.Update(data);
        //        await _applicationDbContext.SaveChangesAsync();


        //        if (!resetPassResult.Succeeded)
        //        {
        //            return BadRequest(new { message = "Gagal mengubah password. Pastikan password valid." });
        //        }
        //        else
        //        {
        //            _applicationDbContext.UserActives.Update(data);
        //            _applicationDbContext.SaveChanges();

        //            return Created("", new
        //            {
        //                message = "Update Data Berhasil || 201 Created"
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Tangani error jika terjadi masalah
        //        return StatusCode(500, $"Terjadi kesalahan saat memperbarui data: {ex.Message}");
        //    }
        //}


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
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
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
                            a.IsActive,
                            a.FotoName,
                            a.FotoPath,
                            a.IdentityNumber,
                            a.PlaceOfBirth,
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.FullName, search) ||
                    EF.Functions.ILike(u.CreateByName, search)  ||
                    EF.Functions.ILike(u.Email, search) 
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

                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "UserActiveCode" => query.OrderBy(u => u.UserActiveCode),
                    "FullName" => query.OrderBy(u => u.FullName),
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
