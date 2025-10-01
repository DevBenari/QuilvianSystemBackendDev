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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.HubSignalR;
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
    public class VitalSignController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<VitalSignController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<VitalSignHub> _hubContext;

        public VitalSignController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<VitalSignController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<VitalSignHub> hubContext
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlLVitalSign(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.VitalSigns
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            VitalSignId = a.VitalSignId,
                            KunjunganId = a.KunjunganId,
                            Suhu = a.Suhu,
                            HR = a.HR,
                            RR = a.RR,
                            TekananDarahSystolic = a.TekananDarahSystolic,
                            TekananDarahDiastolic = a.TekananDarahDiastolic,
                            SaturasiOksigen = a.SaturasiOksigen,
                            Height = a.Height,
                            Weight = a.Weight,
                            BMI = a.BMI,
                            LingkarKepalaBayi = a.LingkarKepalaBayi,
                            RanapId = a.RanapId
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
            var listdata = _applicationDbContext.VitalSigns.Find(id);
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

        [HttpGet("VitalSign-ByPasien/{pasienId}")]
        public async Task<IActionResult> GetVitalSignByPasienId(Guid pasienId)
        {
            var data = (from a in _applicationDbContext.VitalSigns
                        join k in _applicationDbContext.Kunjungans
                            on a.KunjunganId equals k.KunjunganID
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false && k.PasienId == pasienId
                        orderby a.CreateDateTime descending
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            VitalSignId = a.VitalSignId,
                            KunjunganId = a.KunjunganId,
                            Suhu = a.Suhu,
                            HR = a.HR,
                            RR = a.RR,
                            TekananDarahSystolic = a.TekananDarahSystolic,
                            TekananDarahDiastolic = a.TekananDarahDiastolic,
                            SaturasiOksigen = a.SaturasiOksigen,
                            Height = a.Height,
                            Weight = a.Weight,
                            BMI = a.BMI,
                            LingkarKepalaBayi = a.LingkarKepalaBayi,
                            RanapId = a.RanapId
                        }).ToList();

            if (!data.Any())
            {
                return NotFound(new { message = "Data tidak ditemukan untuk pasien ini. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVitalSign([FromBody] VitalSignViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // 🔐 Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                Guid createBy;

                // Cek jika berasal dari delegasi
                if (vm.DelegasiId.HasValue)
                {
                    var delegasi = await _applicationDbContext.Delegasis
                        .FirstOrDefaultAsync(d =>
                            d.DelegasiId == vm.DelegasiId.Value &&
                            d.IsDelegated == true);

                    if (delegasi == null)
                    {
                        return BadRequest(new { message = "Delegasi tidak ditemukan atau sudah tidak aktif." });
                    }

                    //  Gunakan user delegasi sebagai pencatat
                    createBy = delegasi.UserDelegasiId.Value;

                    // Tandai delegasi selesai
                    //delegasi.IsDelegated = false;
                }
                else
                {
                    //  Bukan delegasi, gunakan user login
                    createBy = getUserActive.UserActiveId;
                }

                // 📝 Buat entitas VitalSign baru
                var data = new VitalSign
                {
                    VitalSignId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    Suhu = vm.Suhu,
                    HR = vm.HR,
                    RR = vm.RR,
                    TekananDarahSystolic = vm.TekananDarahSystolic,
                    TekananDarahDiastolic = vm.TekananDarahDiastolic,
                    SaturasiOksigen = vm.SaturasiOksigen,
                    Height = vm.Height,
                    Weight = vm.Weight,
                    BMI = vm.BMI,
                    LingkarKepalaBayi = vm.LingkarKepalaBayi,
                    //RanapId = vm.RanapId,
                    CreateBy = createBy,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.VitalSigns.Add(data);

                int result = await _applicationDbContext.SaveChangesAsync();

                // Notifikasi ke signalR
                await _hubContext.Clients.All.SendAsync("VitalSign ditambah", new
                {
                    action = "create",
                    VitalSignId = data.VitalSignId,
                });

                if (result > 0)
                {
                    return Created("", new 
                    { message = "Tambah Data Berhasil || 201 Created",
                        VitalSignId = data.VitalSignId
                    });
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
        public async Task<IActionResult> EditVitalSign(Guid id, [FromBody] VitalSignViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // 🔐 Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives
                    .FirstOrDefault(u => u.Email == emailLogin);

                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }

                var userActiveId = getUserActive.UserActiveId;
                Guid modifyBy;

                // 🔍 Ambil data vital sign
                var data = await _applicationDbContext.VitalSigns
                    .FirstOrDefaultAsync(v => v.VitalSignId == id);

                if (data == null)
                {
                    return NotFound(new { message = "Data vital sign tidak ditemukan." });
                }

                // 🔁 Cek apakah ini update dari delegasi
                if (vm.DelegasiId.HasValue)
                {
                    var delegasi = await _applicationDbContext.Delegasis
                        .FirstOrDefaultAsync(d =>
                            d.DelegasiId == vm.DelegasiId.Value &&
                            d.IsDelegated == true);

                    if (delegasi == null)
                    {
                        return BadRequest(new { message = "Delegasi tidak ditemukan atau sudah tidak aktif." });
                    }

                    if (delegasi.UserDelegasiId != userActiveId)
                    {
                        return Unauthorized(new { message = "Delegasi ini bukan milik Anda." });
                    }

                    modifyBy = delegasi.UserDelegasiId.Value;

                    // Tandai delegasi sudah selesai
                    delegasi.IsDelegated = false;
                }
                else
                {
                    modifyBy = userActiveId;
                }

                // 📝 Update data vital sign
                data.KunjunganId = vm.KunjunganId;
                data.Suhu = vm.Suhu;
                data.HR = vm.HR;
                data.RR = vm.RR;
                data.TekananDarahSystolic = vm.TekananDarahSystolic;
                data.TekananDarahDiastolic = vm.TekananDarahDiastolic;
                data.SaturasiOksigen = vm.SaturasiOksigen;
                data.Height = vm.Height;
                data.Weight = vm.Weight;
                data.BMI = vm.BMI;
                data.LingkarKepalaBayi = vm.LingkarKepalaBayi;
                data.UpdateBy = modifyBy;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                int result = await _applicationDbContext.SaveChangesAsync();

                // Notifikasi ke signalR
                await _hubContext.Clients.All.SendAsync("VitalSign diubah", new
                {
                    action = "update",
                    VitalSignId = data.VitalSignId,
                });

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Tidak ada perubahan yang disimpan." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal mengupdate data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> CreateVitalSign([FromBody] VitalSignViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Cek koneksi ke database**
        //        if (!_applicationDbContext.Database.CanConnect())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // **Ambil User ID dari JWT Claims**
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //        {
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });
        //        }
        //        var userActiveId = getUserActive.UserActiveId;

        //        // **Ambil Tanggal Sekarang**
        //        var dateNow = DateTime.UtcNow;
        //        var setDateNow = dateNow.ToString("yyMMdd"); // Format: YYMMDD

        //        ////// **Cek Duplikasi**
        //        //bool isDuplicate = _applicationDbContext.VitalSigns
        //        //                    .Any(c => c.KunjunganId == vm.KunjunganId);

        //        //if (isDuplicate)
        //        //{
        //        //    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
        //        //}

        //        // **Buat Data Baru**
        //        var data = new VitalSign
        //        {
        //            VitalSignId = Guid.NewGuid(),
        //            KunjunganId = vm.KunjunganId,
        //            Suhu = vm.Suhu,
        //            HR = vm.HR,
        //            RR = vm.RR,
        //            TekananDarahSystolic = vm.TekananDarahSystolic,
        //            TekananDarahDiastolic = vm.TekananDarahDiastolic,
        //            SaturasiOksigen = vm.SaturasiOksigen,
        //            Height = vm.Height,
        //            Weight = vm.Weight,
        //            BMI = vm.BMI,
        //            LingkarKepalaBayi = vm.LingkarKepalaBayi,
        //            RanapId = vm.RanapId,
        //            CreateBy = (Guid)vm.UserActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow
        //        };

        //        // **Simpan ke Database**
        //        _applicationDbContext.VitalSigns.Add(data);
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Created("", new { message = "Tambah Data Berhasil || 201 Created"});
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateVitalSign(Guid id, [FromBody] VitalSignViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Cek koneksi ke database**
        //        if (!await _applicationDbContext.Database.CanConnectAsync())
        //        {
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
        //        }

        //        // **Ambil User ID dari JWT Claims**
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var getUserActive = await _applicationDbContext.UserActives
        //            .FirstOrDefaultAsync(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //        {
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });
        //        }
        //        var userActiveId = getUserActive.UserActiveId;

        //        // **Cari Data**
        //        var data = await _applicationDbContext.VitalSigns.FindAsync(id);
        //        if (data == null)
        //        {
        //            return NotFound(new { message = "Data tidak ditemukan." });
        //        }

        //        // **Update Data**
        //        data.KunjunganId = vm.KunjunganId;
        //        data.Suhu = vm.Suhu;
        //        data.HR = vm.HR;
        //        data.RR = vm.RR;
        //        data.TekananDarahSystolic = vm.TekananDarahSystolic;
        //        data.TekananDarahDiastolic = vm.TekananDarahDiastolic;
        //        data.SaturasiOksigen = vm.SaturasiOksigen;
        //        data.Height = vm.Height;
        //        data.Weight = vm.Weight;
        //        data.BMI = vm.BMI;
        //        data.LingkarKepalaBayi = vm.LingkarKepalaBayi;
        //        //data.RanapId = vm.RanapId;

        //        data.UpdateBy = userActiveId;
        //        data.UpdateDateTime = DateTimeOffset.UtcNow;

        //        _applicationDbContext.VitalSigns.Update(data);
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //        {
        //            return Ok(new { message = "Update Data Berhasil || 200 OK" });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
        //        }
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVitalSign(Guid id)
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
                var data = await _applicationDbContext.VitalSigns.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.VitalSigns.Update(data);
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
        public async Task<IActionResult> PagedVitalSign(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")] DateTime? endDate = null,
            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
            //[FromQuery] Guid? KunjunganId = null
        )
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // Query data
                var query = from a in _applicationDbContext.VitalSigns
                            join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId
                            where a.IsDelete == false
                            select new
                            {
                                CreateDateTime = a.CreateDateTime,
                                CreateBy = a.CreateBy,
                                CreateByName = u.FullName,
                                VitalSignId = a.VitalSignId,
                                KunjunganId = a.KunjunganId,
                                Suhu = a.Suhu,
                                HR = a.HR,
                                RR = a.RR,
                                TekananDarahSystolic = a.TekananDarahSystolic,
                                TekananDarahDiastolic = a.TekananDarahDiastolic,
                                SaturasiOksigen = a.SaturasiOksigen,
                                Height = a.Height,
                                Weight = a.Weight,
                                BMI = a.BMI,
                                LingkarKepalaBayi = a.LingkarKepalaBayi,
                                RanapId = a.RanapId
                            };

                //**Filter berdasarkan search(Perbaikan agar bisa mencari 1 huruf)**
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                    query = query.Where(u =>
                        EF.Functions.ILike(u.KunjunganId.ToString(), search) 
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

                // **Sorting Data dengan cara yang lebih aman**
                var sortColumn = orderBy?.ToLower() ?? "createdatetime";
                var isDescending = sortDirection?.ToLower() == "desc";

                //query = sortColumn switch
                //{
                //    "createdatetime" => isDescending ? query.OrderByDescending(u => u.CreateDateTime) : query.OrderBy(u => u.CreateDateTime),
                //    "createbyname" => isDescending ? query.OrderByDescending(u => u.CreateByName) : query.OrderBy(u => u.CreateByName),
                //    "kodeagama" => isDescending ? query.OrderByDescending(u => u.KodeAgama) : query.OrderBy(u => u.KodeAgama),
                //    "namaagama" => isDescending ? query.OrderByDescending(u => u.NamaAgama) : query.OrderBy(u => u.NamaAgama),
                //    _ => query.OrderByDescending(u => u.CreateDateTime)
                //};

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
