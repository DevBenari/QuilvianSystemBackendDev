using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
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
        private readonly IHubContext<ResepDetailHub> _hubContext;

        public ResepDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<ResepDetailHub> hubContext
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
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
            var query =
                from a in _applicationDbContext.DetailReseps
                join u in _applicationDbContext.UserActives
                    on a.CreateBy equals u.UserActiveId
                join r in _applicationDbContext.Reseps
                    on a.ResepId equals r.ResepId
                join ra in _applicationDbContext.Racikans
                    on a.RacikanId equals ra.RacikanId into racikanJoin
                from ra in racikanJoin.DefaultIfEmpty()
                join rd in _applicationDbContext.RacikanDetails
                    on ra.RacikanId equals rd.RacikanId into racikanDetailJoin
                from rd in racikanDetailJoin.DefaultIfEmpty()
                join ob in _applicationDbContext.Obats
                    on a.ObatId equals ob.ObatId into obatJoin
                from ob in obatJoin.DefaultIfEmpty()
                join obRD in _applicationDbContext.Obats
                    on rd.ObatId equals obRD.ObatId into obatRacikanJoin
                from obRD in obatRacikanJoin.DefaultIfEmpty()
                where a.IsDelete == false
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.DetailResepId,
                    a.ResepId,
                    r.KunjunganId,
                    a.AsuransiId,
                    a.NamaAsuransi,
                    ObatId = (bool)!a.IsRacikan ? a.ObatId : rd.ObatId,
                    NamaObat = (bool)!a.IsRacikan ? ob.ObatName : obRD.ObatName,
                    a.Qty,
                    a.JenisRacikan,
                    Signa = (bool)!a.IsRacikan ? a.Signa : ra.Signa,
                    SignaTambahan = (bool)!a.IsRacikan ? a.SignaTambahan : ra.SignaTambahan,
                    a.JenisObat,
                    a.HargaObat,
                    a.TotalHargaObat,
                    a.StatusCoverObat,
                    a.IsRacikan,
                    a.RacikanId,
                    NamaRacikan = ra != null ? ra.NamaRacikan : null,
                    a.IsIteratur,
                    a.JumlahIteratur,
                    a.TglMulaiIteratur,
                    a.JarakPenebusan,
                    a.MasaAktifIteratur,
                    a.StatusPengambilanObat,
                    a.StatusDiberikanPasien,
                    TakaranDosis = (bool)!a.IsRacikan ? a.TakaranDosis : obRD.TakaranDosis,
                    a.IsContinued,
                    a.CaraPemakaian,
                    a.EstimasiPemberian,
                    a.TglStopPemakaian,
                    a.IsObatDibawaPlg,
                    a.ObatPagiDiambil,
                    a.ObatSiangDiambil,
                    a.ObatMalamDiambil,
                    a.IsReturn,
                    a.AlasanReturn,
                    a.QtyReturn,
                    a.DikembalikanOleh,
                };

            var result = query.ToList();

            if (result == null || result.Count == 0)
            {
                return NotFound(new { message = "Data not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdDetailResep(Guid id)
        {
            var query =
                from a in _applicationDbContext.DetailReseps
                join u in _applicationDbContext.UserActives
                    on a.CreateBy equals u.UserActiveId
                join r in _applicationDbContext.Reseps
                    on a.ResepId equals r.ResepId
                join ra in _applicationDbContext.Racikans
                    on a.RacikanId equals ra.RacikanId into racikanJoin
                from ra in racikanJoin.DefaultIfEmpty()
                join rd in _applicationDbContext.RacikanDetails
                    on ra.RacikanId equals rd.RacikanId into racikanDetailJoin
                from rd in racikanDetailJoin.DefaultIfEmpty()
                join ob in _applicationDbContext.Obats
                    on a.ObatId equals ob.ObatId into obatJoin
                from ob in obatJoin.DefaultIfEmpty()
                join obRD in _applicationDbContext.Obats
                    on rd.ObatId equals obRD.ObatId into obatRacikanJoin
                from obRD in obatRacikanJoin.DefaultIfEmpty()
                where a.IsDelete == false && a.DetailResepId == id
                select new
                {
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
                    a.DetailResepId,
                    a.ResepId,
                    r.KunjunganId,
                    a.AsuransiId,
                    a.NamaAsuransi,
                    ObatId = (bool)!a.IsRacikan ? a.ObatId : rd.ObatId,
                    NamaObat = (bool)!a.IsRacikan ? ob.ObatName : obRD.ObatName,
                    a.Qty,
                    a.JenisRacikan,
                    Signa = (bool)!a.IsRacikan ? a.Signa : ra.Signa,
                    SignaTambahan = (bool)!a.IsRacikan ? a.SignaTambahan : ra.SignaTambahan,
                    a.JenisObat,
                    a.HargaObat,
                    a.TotalHargaObat,
                    a.StatusCoverObat,
                    a.IsRacikan,
                    a.RacikanId,
                    NamaRacikan = ra != null ? ra.NamaRacikan : null,
                    a.IsIteratur,
                    a.JumlahIteratur,
                    a.TglMulaiIteratur,
                    a.JarakPenebusan,
                    a.MasaAktifIteratur,
                    a.StatusPengambilanObat,
                    a.StatusDiberikanPasien,
                    TakaranDosis = (bool)!a.IsRacikan ? a.TakaranDosis : obRD.TakaranDosis,
                    a.IsContinued,
                    a.CaraPemakaian,
                    a.EstimasiPemberian,
                    a.TglStopPemakaian,
                    a.IsObatDibawaPlg,
                    a.ObatPagiDiambil,
                    a.ObatSiangDiambil,
                    a.ObatMalamDiambil,
                    a.IsReturn,
                    a.AlasanReturn,
                    a.QtyReturn,
                    a.DikembalikanOleh,
                };

            var result = query.ToList();

            if (result == null || result.Count == 0)
            {
                return NotFound(new { message = "DetailResepId not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = result
            });
        }

        [HttpPut("{id}/IsObatDibawaPulang")]
        public async Task<IActionResult> UpdateIsObatDibawaPulang(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsObatDibawaPlg = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ObatDibawaPulangUpdated", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });
            return Ok(new { message = "Status dibawa pulang berhasil diperbarui." });
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
            await _hubContext.Clients.All.SendAsync("StatusObatUpdated", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });
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
            await _hubContext.Clients.All.SendAsync("IsContinuedMedicine", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });
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
            await _hubContext.Clients.All.SendAsync("StatusDiberikanPasienUpdated", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });

            return Ok(new { message = "Status StatusDiberikanPasien berhasil diperbarui." });
        }

        [HttpPut("{id}/ObatPagi")]
        public async Task<IActionResult> UpdateObatPagi(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Obat tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.ObatPagiDiambil = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("Obat pagi telah diberikan", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });

            return Ok(new { message = "Obat pagi telah diberikan." });
        }

        [HttpPut("{id}/ObatSiang")]
        public async Task<IActionResult> UpdateObatSiang(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Obat tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.ObatSiangDiambil = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("Obat siang telah diberikan", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });

            return Ok(new { message = "Obat siang telah diberikan." });
        }

        [HttpPut("{id}/ObatMalam")]
        public async Task<IActionResult> UpdateObatMalam(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Obat tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.ObatMalamDiambil = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("Obat Malam telah diberikan", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });

            return Ok(new { message = "Obat malam telah diberikan." });
        }

        [HttpPut("{id}/ReturnObat")]
        public async Task<IActionResult> UpdateReturnObat(Guid id, [FromBody] EditReturnObatVM request)
        {
            try
            {
                // 🔹 Ambil data detail resep
                var data = await _applicationDbContext.DetailReseps
                    .FirstOrDefaultAsync(d => d.DetailResepId == id);

                if (data == null)
                    return NotFound(new { message = "Data resep tidak ditemukan." });

                // 🔹 Ambil user login dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userId = user.UserActiveId;

                // 🔹 Validasi jumlah return
                int qtyReturn = (int)(request.QtyReturn ?? 0);
                if (qtyReturn <= 0)
                    return BadRequest(new { message = "Jumlah return harus lebih dari 0." });

                if (qtyReturn > data.Qty)
                    return BadRequest(new { message = "Jumlah return melebihi jumlah obat yang diberikan." });

                // 🔹 Update nilai return di tabel DetailResep
                data.IsReturn = request.IsReturn;
                data.AlasanReturn = request.AlasanReturn;
                data.QtyReturn = qtyReturn;
                data.DikembalikanOleh = request.DikembalikanOleh;
                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = userId;

                // 🔹 Jika return = true, tambahkan stok kembali
                if (request.IsReturn == true)
                {
                    // === CASE 1: OBAT BIASA ===
                    if (data.IsRacikan == false && data.ObatId != null)
                    {
                        var obat = await _applicationDbContext.Obats
                            .FirstOrDefaultAsync(o => o.ObatId == data.ObatId);

                        if (obat != null)
                        {
                            obat.Stock += qtyReturn;
                            _applicationDbContext.Obats.Update(obat);
                        }
                    }

                    // === CASE 2: OBAT RACIKAN ===
                    if (data.IsRacikan == true && data.RacikanId != null)
                    {
                        // cari daftar komposisi racikan di tabel RacikanDetails
                        var racikanDetails = await _applicationDbContext.RacikanDetails
                            .Where(rd => rd.RacikanId == data.RacikanId)
                            .ToListAsync();

                        foreach (var detail in racikanDetails)
                        {
                            var obatRacikan = await _applicationDbContext.Obats
                                .FirstOrDefaultAsync(o => o.ObatId == detail.ObatId);

                            if (obatRacikan != null)
                            {
                                // stok yang dikembalikan = jumlah komposisi x qtyReturn racikan
                                int jumlahKembali = (int)(detail.QtyUsed * qtyReturn);
                                obatRacikan.Stock += jumlahKembali;

                                _applicationDbContext.Obats.Update(obatRacikan);
                            }
                        }
                    }
                }

                // 🔹 Simpan perubahan
                await _applicationDbContext.SaveChangesAsync();

                // 🔹 Kirim notifikasi realtime
                await _hubContext.Clients.All.SendAsync("ResepUpdated", new
                {
                    Action = "return",
                    DetailResepId = data.DetailResepId,
                    IsReturn = data.IsReturn,
                    QtyReturn = data.QtyReturn
                });

                return Ok(new { message = "Return obat berhasil dan stok diperbarui." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan perubahan: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }
        
        [HttpPut("{id}/IsStopped")]
        public async Task<IActionResult> UpdateStopObat(Guid id, [FromBody] StatusPengambilanObatViewModel request)
        {
            var data = await _applicationDbContext.DetailReseps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Obat tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsStopped = request.Status;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("Status stop obat telah diupdate.", new
            {
                Action = "update",
                DetailResepId = data.DetailResepId
            });

            return Ok(new { message = "Status stop obat telah diupdate." });
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

                //// **Cek Obat**
                var obat = await _applicationDbContext.Obats
                                    .FirstOrDefaultAsync(c => c.ObatId == vm.ObatId);

                var qty=0;
                var signa="";
                if ((bool)vm.IsIntervensiFarmakologi)
                {
                    qty = 1;
                    signa = "1x1";
                }
                else
                {
                    qty = (int)vm.Qty;
                    signa = vm.Signa;
                }

                // cek data resep
                var resep = await _applicationDbContext.Reseps.FirstOrDefaultAsync(c=>c.ResepId == vm.ResepId);

                int billingIndex = await _applicationDbContext.Billings
                .CountAsync(b => b.KunjunganId == resep.KunjunganId && b.JenisBilling.ToLower() == "obat");
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
                        Qty = qty,
                        TakaranDosis = obat.TakaranDosis,
                        Signa = signa,
                        SignaTambahan = vm.SignaTambahan,
                        JenisObat = vm.JenisObat,
                        HargaObat = obat.HTEPrice,
                        TotalHargaObat = vm.Qty.HasValue && vm.HargaObat.HasValue ? qty * obat.HTEPrice : 0,
                        StatusCoverObat = vm.StatusCoverObat,
                        IsRacikan = vm.IsRacikan, // Tambahkan properti IsRacikan jika diperlukan
                        IsIntervensiFarmakologi = vm.IsIntervensiFarmakologi, //inputan dari front end
                        //IsIteratur = vm.IsIteratur,
                        //JumlahIteratur = vm.JumlahIteratur,
                        //TglMulaiIteratur = parsedTglMulaiIteratur,
                        //JarakPenebusan = vm.JarakPenebusan,
                        //MasaAktifIteratur = parsedMasaAktif,
                        StatusPengambilanObat = false, // Default nilai StatusPengambilanObat
                        StatusDiberikanPasien = vm.StatusDiberikanPasien,
                        CaraPemakaian = vm.CaraPemakaian,
                        EstimasiPemberian = vm.EstimasiPemberian,
                        IsContinued = vm.IsContinued,
                        IsObatDibawaPlg = false,
                        ObatPagiDiambil = false,
                        ObatSiangDiambil = false,
                        ObatMalamDiambil = false,
                        IsReturn = false,
                        IsStopped = false,
                        TglStopPemakaian = TryParseTanggalToUtc(vm.TglStopPemakaian),
                        CreateBy = userActiveId,
                        CreateDateTime = DateTimeOffset.UtcNow,
                    };

                // **Simpan ke Database**
                _applicationDbContext.DetailReseps.Add(data);

                    var billing = new Billing
                    {
                            KunjunganId = resep.KunjunganId,
                            //DiskonId = vm.DiskonId,
                            BillingDate = DateTime.UtcNow,
                            BillingKode = $"{billingIndex:D3}",
                            ItemId = data.ObatId,
                            NamaItem = obat.ObatName,
                            HargaItem = obat.HTEPrice,
                            QtyItem = qty,
                            SubTotalItem = obat.HTEPrice * qty,
                            JenisBilling = "Obat",
                            StatusPengambilan = true,
                            CreateBy = getUserActive.UserActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                    };

                _applicationDbContext.Billings.Add(billing);

                int result = await _applicationDbContext.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ResepDetailCreated", new
                {
                    Action = "create",
                    DetailResepId = data.DetailResepId
                });
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
                await _hubContext.Clients.All.SendAsync("DetailResepUpdated", new
                {
                    Action = "update",
                    DetailResepId = data.DetailResepId
                });
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
            Guid? kunjungan = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc")
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query dasar
            var query = from a in _applicationDbContext.DetailReseps
                        join u in _applicationDbContext.UserActives
                            on a.CreateBy equals u.UserActiveId

                        join r in _applicationDbContext.Reseps
                            on a.ResepId equals r.ResepId

                        join ra in _applicationDbContext.Racikans
                            on a.RacikanId equals ra.RacikanId into racikanJoin
                        from ra in racikanJoin.DefaultIfEmpty()

                        join rd in _applicationDbContext.RacikanDetails
                            on ra.RacikanId equals rd.RacikanId into racikanDetailJoin
                        from rd in racikanDetailJoin.DefaultIfEmpty()

                        join ob in _applicationDbContext.Obats
                            on a.ObatId equals ob.ObatId into obatJoin
                            from ob in obatJoin.DefaultIfEmpty()

                        join obRD in _applicationDbContext.Obats
                            on rd.ObatId equals obRD.ObatId into obatRacikanJoin
                        from obRD in obatRacikanJoin.DefaultIfEmpty()

                        where a.IsDelete == false
                        select new
                        {
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,
                            a.DetailResepId,
                            a.ResepId,
                            r.KunjunganId,
                            a.AsuransiId,
                            a.NamaAsuransi,
                            ObatId = (bool)!a.IsRacikan ? a.ObatId : rd.ObatId,
                            NamaObat = (bool)!a.IsRacikan ? ob.ObatName : obRD.ObatName,
                            a.Qty,
                            a.JenisRacikan,
                            Signa = (bool)!a.IsRacikan ? a.Signa : ra.Signa ,
                            SignaTambahan = (bool)!a.IsRacikan ? a.SignaTambahan : ra.SignaTambahan,
                            a.JenisObat,
                            a.HargaObat,
                            a.TotalHargaObat,
                            a.StatusCoverObat,
                            a.IsRacikan,
                            a.RacikanId,
                            NamaRacikan = ra != null ? ra.NamaRacikan : null,
                            a.IsIteratur,
                            a.JumlahIteratur,
                            a.TglMulaiIteratur,
                            a.JarakPenebusan,
                            a.MasaAktifIteratur,
                            a.StatusPengambilanObat,
                            a.StatusDiberikanPasien,
                            TakaranDosis = (bool)!a.IsRacikan ? a.TakaranDosis : obRD.TakaranDosis,
                            a.IsContinued,
                            a.CaraPemakaian,
                            a.EstimasiPemberian,
                            a.TglStopPemakaian,
                            a.IsObatDibawaPlg
                        };

            // 🔎 Search sederhana
            if (!string.IsNullOrWhiteSpace(search))
            {
                string lower = search.ToLower();
                query = query.Where(x =>
                    (x.NamaAsuransi != null && x.NamaAsuransi.ToLower().Contains(lower)) ||
                    (x.CreateByName != null && x.CreateByName.ToLower().Contains(lower)) ||
                    (x.NamaRacikan != null && x.NamaRacikan.ToLower().Contains(lower))
                );
            }

            // Filter by KunjunganId
            if (kunjungan.HasValue && kunjungan != Guid.Empty)
            {
                query = query.Where(x => x.KunjunganId == kunjungan);
            }

            // 🔎 Sorting dinamis
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    "NamaAsuransi" => query.OrderByDescending(x => x.NamaAsuransi),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    "NamaAsuransi" => query.OrderBy(x => x.NamaAsuransi),
                    _ => query.OrderBy(x => x.CreateDateTime)
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
