using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Areas.HRD.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class TransferPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITTDService _ttdService;
        private readonly ILogger<TransferPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TransferPasienController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TransferPasienController> logger,
            IWebHostEnvironment webHostEnvironment,
            ITTDService ttdService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _ttdService = ttdService;
        }



        [HttpGet]
        public async Task<IActionResult> GetAllTransferPasien(int page = 1, int perPage = 10)
        {
            try
            {
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ✅ Query utama
                var query = from t in _applicationDbContext.TransferPasiens
                            join u in _applicationDbContext.UserActives
                                on t.CreateBy equals u.UserActiveId into userGroup
                            from u in userGroup.DefaultIfEmpty()

                            join b in _applicationDbContext.Beds
                            on t.BedId equals b.BedId into bGroup
                            from b in bGroup.DefaultIfEmpty()

                            join d1 in _applicationDbContext.UserActives
                                on t.DokterId1 equals d1.UserActiveId into dokter1Group
                            from d1 in dokter1Group.DefaultIfEmpty()

                            join d2 in _applicationDbContext.UserActives
                                on t.DokterId2 equals d2.UserActiveId into dokter2Group
                            from d2 in dokter2Group.DefaultIfEmpty()

                            join d3 in _applicationDbContext.UserActives
                                on t.DokterId3 equals d3.UserActiveId into dokter3Group
                            from d3 in dokter3Group.DefaultIfEmpty()

                            where t.IsDelete == false || t.IsDelete == null
                            orderby t.CreateDateTime descending
                            select new
                            {
                                t.TransferPasienId,
                                t.KunjunganId,
                                t.BedId,
                                b.NomorBed,
                                b.Deskripsi,
                                t.DiagnosaUtama,
                                t.DiagnosaSekunder,
                                DokterUtama = d1 != null ? d1.FullName : null,
                                DokterPendamping = d2 != null ? d2.FullName : null,
                                DokterTambahan = d3 != null ? d3.FullName : null,
                                t.IndikasiRanap,
                                t.IsAlergic,
                                t.AlergicOf,
                                t.AlasanPindahPasien,
                                t.TglPindah,
                                t.PengawasanHarianId,
                                t.ObservasiCairanId,
                                t.IndikatorPengkajianId,
                                t.PemberianObatId,
                                t.TotalScoreAldrete,
                                t.TotalScoreSteward,
                                t.IsICU,
                                t.BarangDiserahkan,
                                t.IntervensiPerawat,
                                t.PlanningTindakan,

                                // 🔹 File Path TTD
                                t.PetugasMenyerahkanId,
                                t.TTDMenyerahkanPath,

                                t.PetugasMengetahuiId,
                                t.TTDMengetahuiPath,

                                t.PetugasPenerimaId,
                                t.TTDPenerimaPath,

                                t.Keterangan,
                                t.CreateDateTime,
                                CreateByName = u != null ? u.FullName : null
                            };

                // ✅ Hitung total data sebelum paginasi
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                // ✅ Ambil data sesuai halaman
                var listData = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .AsNoTracking()
                    .ToListAsync();

                // ✅ Response tetap OK meski kosong
                return Ok(new
                {
                    message = listData.Any() ? "Berhasil || 200 OK" : "Tidak ada data Transfer Pasien || 200 OK",
                    data = listData.Any() ? listData : null,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    }
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // ✅ Cek koneksi database
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Query utama TransferPasien
                var data = await (from t in _applicationDbContext.TransferPasiens
                                  join u in _applicationDbContext.UserActives
                                      on t.CreateBy equals u.UserActiveId into userGroup
                                  from u in userGroup.DefaultIfEmpty()

                                  join b in _applicationDbContext.Beds
                                    on t.BedId equals b.BedId into bGroup
                                  from b in bGroup.DefaultIfEmpty()

                                      // join Dokter 1, 2, 3 (opsional)
                                  join d1 in _applicationDbContext.UserActives
                                      on t.DokterId1 equals d1.UserActiveId into dokter1Group
                                  from d1 in dokter1Group.DefaultIfEmpty()

                                  join d2 in _applicationDbContext.UserActives
                                      on t.DokterId2 equals d2.UserActiveId into dokter2Group
                                  from d2 in dokter2Group.DefaultIfEmpty()

                                  join d3 in _applicationDbContext.UserActives
                                      on t.DokterId3 equals d3.UserActiveId into dokter3Group
                                  from d3 in dokter3Group.DefaultIfEmpty()

                                  where (t.IsDelete == false || t.IsDelete == null)
                                        && t.TransferPasienId == id
                                  select new
                                  {
                                      t.TransferPasienId,
                                      t.KunjunganId,
                                      t.BedId,
                                      b.NomorBed,
                                      b.Deskripsi,
                                      t.DiagnosaUtama,
                                      t.DiagnosaSekunder,
                                      DokterUtama = d1 != null ? d1.FullName : null,
                                      DokterPendamping = d2 != null ? d2.FullName : null,
                                      DokterTambahan = d3 != null ? d3.FullName : null,
                                      t.IndikasiRanap,
                                      t.IsAlergic,
                                      t.AlergicOf,
                                      t.AlasanPindahPasien,
                                      t.TglPindah,
                                      t.PengawasanHarianId,
                                      t.ObservasiCairanId,
                                      t.IndikatorPengkajianId,
                                      t.PemberianObatId,
                                      t.TotalScoreAldrete,
                                      t.TotalScoreSteward,
                                      t.IsICU,
                                      t.BarangDiserahkan,
                                      t.IntervensiPerawat,
                                      t.PlanningTindakan,

                                      // 🔹 File Path TTD
                                      t.PetugasMenyerahkanId,
                                      t.TTDMenyerahkanPath,

                                      t.PetugasMengetahuiId,
                                      t.TTDMengetahuiPath,

                                      t.PetugasPenerimaId,
                                      t.TTDPenerimaPath,

                                      t.Keterangan,
                                      t.CreateDateTime,
                                      CreateByName = u.FullName
                                  }).AsNoTracking()
                                  .FirstOrDefaultAsync();

                // ✅ Jika tidak ditemukan
                if (data == null)
                    return NotFound(new { message = $"Data Transfer Pasien dengan ID {id} tidak ditemukan. || 404 Not Found" });

                // ✅ Return hasil
                return Ok(new
                {
                    message = "Berhasil mengambil data Transfer Pasien || 200 OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransferPasienViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            try
            {
                // ✅ Cek koneksi DB
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                // ✅ Ambil user dari JWT
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // cek ttd user
                var ttdMengetahui = await _ttdService.CheckTTDAsync((Guid)vm.PetugasMengetahuiId);
                var ttdMenyerahkan = await _ttdService.CheckTTDAsync((Guid)vm.PetugasMenyerahkanId);
                var ttdPenerima = await _ttdService.CheckTTDAsync((Guid)vm.PetugasPenerimaId);

                //// **Cek Duplikasi**
                //var today = DateTime.UtcNow.Date;
                //bool isDuplicate = await _applicationDbContext.TransferPasiens
                //                    .AnyAsync(c => c.KunjunganId == vm.KunjunganId && c.CreateDateTime == today
                //                    && c.IsDelete == false);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Kunjungan ini telah melakukan proses transfer pasien" });
                //}

                // ==================================================
                // ✅ Simpan ke tabel TransferPasien
                // ==================================================
                var data = new TransferPasien
                {
                    TransferPasienId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    BedId = vm.BedId,
                    DiagnosaUtama = vm.DiagnosaUtama,
                    DiagnosaSekunder = vm.DiagnosaSekunder,
                    DokterId1 = vm.DokterId1,
                    DokterId2 = vm.DokterId2,
                    DokterId3 = vm.DokterId3,
                    IndikasiRanap = vm.IndikasiRanap,
                    IsAlergic = vm.IsAlergic ?? false,
                    AlergicOf = vm.AlergicOf,
                    AlasanPindahPasien = vm.AlasanPindahPasien,
                    TglPindah = vm.TglPindah,
                    PengawasanHarianId = vm.PengawasanHarianId,
                    ObservasiCairanId = vm.ObservasiCairanId,
                    IndikatorPengkajianId = vm.IndikatorPengkajianId,
                    PemberianObatId = vm.PemberianObatId,
                    TotalScoreAldrete = vm.TotalScoreAldrete,
                    TotalScoreSteward = vm.TotalScoreSteward,
                    IsICU = vm.IsICU ?? false,
                    BarangDiserahkan = vm.BarangDiserahkan,
                    IntervensiPerawat = vm.IntervensiPerawat,
                    PlanningTindakan = vm.PlanningTindakan,

                    PetugasMenyerahkanId = vm.PetugasMenyerahkanId,
                    TTDMenyerahkanPath = ttdMenyerahkan.Path,

                    PetugasMengetahuiId = vm.PetugasMengetahuiId,
                    TTDMengetahuiPath = ttdMengetahui.Path,

                    PetugasPenerimaId = vm.PetugasMengetahuiId,
                    TTDPenerimaPath = ttdPenerima.Path,

                    Keterangan = vm.Keterangan,
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.TransferPasiens.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                    return Created("", new
                    {
                        message = "Tambah Data Transfer Pasien Berhasil || 201 Created",
                        TTDIdMenyerahkan = ttdMenyerahkan.TTDId,
                        TTDIdMengetahui = ttdMengetahui.TTDId,
                        TTDIdPenerima = ttdPenerima.TTDId,
                    });

                return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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


        //[HttpPost]
        //[RequestSizeLimit(10_000_000)] // 10 MB
        //[RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        //public async Task<IActionResult> Create([FromForm] TransferPasienViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //        return BadRequest(new { message = "Data tidak valid." });

        //    try
        //    {
        //        // ✅ Cek koneksi ke database
        //        if (!_applicationDbContext.Database.CanConnect())
        //            return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //        // ✅ Ambil user aktif dari JWT
        //        var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(emailLogin))
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });

        //        var getUserActive = await _applicationDbContext.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
        //        if (getUserActive == null)
        //            return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        //        var userActiveId = getUserActive.UserActiveId;

        //        // ==================================================
        //        // 🔹 FUNGSI HELPER UPLOAD FILE KE SERVER FLASK
        //        // ==================================================
        //        async Task<(string? fileUrl, Guid? ttdId)> UploadTTDAsync(IFormFile? file, string prefix, string folderTarget)
        //        {
        //            if (file == null || file.Length == 0) return (null, null);

        //            var maxSize = 1 * 1024 * 1024; // 1MB
        //            var allowedExtensions = new[] { ".jpg", ".jpeg" };
        //            var ext = Path.GetExtension(file.FileName).ToLower();

        //            if (file.Length > maxSize)
        //                throw new Exception($"Ukuran file {prefix} terlalu besar! Maksimal 1MB.");

        //            if (!allowedExtensions.Contains(ext))
        //                throw new Exception($"Format file {prefix} tidak valid! Gunakan JPG atau JPEG.");

        //            var safeTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        //            var fileName = $"{getUserActive.FullName}_{safeTime}_{prefix}{ext}";

        //            using var client = new HttpClient();
        //            using var ms = new MemoryStream();
        //            await file.CopyToAsync(ms);
        //            ms.Position = 0;

        //            using var content = new MultipartFormDataContent
        //    {
        //        { new StreamContent(ms) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType) } }, "file", fileName },
        //        { new StringContent(folderTarget), "folderTarget" }
        //    };

        //            var response = await client.PostAsync(_uploadUrl, content);
        //            if (!response.IsSuccessStatusCode)
        //                throw new Exception($"Gagal upload file {prefix} ke server Flask.");

        //            var body = await response.Content.ReadAsStringAsync();
        //            dynamic json = JsonConvert.DeserializeObject(body);
        //            string fileUrl = json.fileUrl;

        //            // Simpan ke MasterTTD
        //            var newTTD = new MasterTTD
        //            {
        //                TTDId = Guid.NewGuid(),
        //                UserActiveId = userActiveId,
        //                TTDPath = fileUrl,
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                CreateBy = userActiveId
        //            };

        //            _applicationDbContext.MasterTTDs.Add(newTTD);
        //            await _applicationDbContext.SaveChangesAsync();

        //            return (fileUrl, newTTD.TTDId);
        //        }

        //        // ==================================================
        //        // ✅ UPLOAD 3 FILE TTD
        //        // ==================================================
        //        var (menyerahkanPath, menyerahkanId) = await UploadTTDAsync(vm.TTDMenyerahkan, "TTDMenyerahkan", "TTDUser");
        //        var (mengetahuiPath, mengetahuiId) = await UploadTTDAsync(vm.TTDMengetahui, "TTDMengetahui", "TTDUser");
        //        var (penerimaPath, penerimaId) = await UploadTTDAsync(vm.TTDPenerima, "TTDPenerima", "TTDUser");

        //        // ==================================================
        //        // ✅ BUAT DATA TRANSFER PASIEN
        //        // ==================================================
        //        var data = new TransferPasien
        //        {
        //            TransferPasienId = Guid.NewGuid(),
        //            KunjunganId = vm.KunjunganId,
        //            KamarId = vm.KamarId,
        //            DiagnosaUtama = vm.DiagnosaUtama,
        //            DiagnosaSekunder = vm.DiagnosaSekunder,
        //            DokterId1 = vm.DokterId1,
        //            DokterId2 = vm.DokterId2,
        //            DokterId3 = vm.DokterId3,
        //            IndikasiRanap = vm.IndikasiRanap,
        //            IsAlergic = vm.IsAlergic ?? false,
        //            AlergicOf = vm.AlergicOf,
        //            AlasanPindahPasien = vm.AlasanPindahPasien,
        //            TglPindah = vm.TglPindah,
        //            PengawasanHarianId = vm.PengawasanHarianId,
        //            ObservasiCairanId = vm.ObservasiCairanId,
        //            IndikatorPengkajianId = vm.IndikatorPengkajianId,
        //            PemberianObatId = vm.PemberianObatId,
        //            TotalScoreAldrete = vm.TotalScoreAldrete,
        //            TotalScoreSteward = vm.TotalScoreSteward,
        //            IsICU = vm.IsICU ?? false,
        //            BarangDiserahkan = vm.BarangDiserahkan,
        //            IntervensiPerawat = vm.IntervensiPerawat,
        //            PlanningTindakan = vm.PlanningTindakan,

        //            TTDMenyerahkanId = menyerahkanId,
        //            TTDMenyerahkanPath = menyerahkanPath,
        //            TTDMengetahuiId = mengetahuiId,
        //            TTDMengetahuiPath = mengetahuiPath,
        //            TTDPenerimaId = penerimaId,
        //            TTDPenerimaPath = penerimaPath,

        //            Keterangan = vm.Keterangan,
        //            CreateBy = userActiveId,
        //            CreateDateTime = DateTimeOffset.UtcNow
        //        };

        //        _applicationDbContext.TransferPasiens.Add(data);
        //        int result = await _applicationDbContext.SaveChangesAsync();

        //        if (result > 0)
        //            return Created("", new { message = "Tambah Data Transfer Pasien Berhasil || 201 Created" });

        //        return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
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
                var data = await _applicationDbContext.TransferPasiens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.TransferPasiens.Update(data);
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
        public async Task<IActionResult> GetPagedTransferPasien(
        int page = 1,
        int perPage = 10,
        string? search = null,
        Guid? kunjunganId = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        DateTime? startDate = null,
        DateTime? endDate = null)
        {
            try
            {
                if (!_applicationDbContext.Database.CanConnect())
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

                if (page < 1) page = 1;
                if (perPage < 1) perPage = 10;

                // ==============================
                // 1️⃣ Base Query
                // ==============================
                var query = from t in _applicationDbContext.TransferPasiens
                            join u in _applicationDbContext.UserActives
                                on t.CreateBy equals u.UserActiveId into userGroup
                            from u in userGroup.DefaultIfEmpty()

                            join b in _applicationDbContext.Beds
                                on t.BedId equals b.BedId into bGroup
                            from b in bGroup.DefaultIfEmpty()

                            join d1 in _applicationDbContext.UserActives
                                on t.DokterId1 equals d1.UserActiveId into dokter1Group
                            from d1 in dokter1Group.DefaultIfEmpty()

                            join d2 in _applicationDbContext.UserActives
                                on t.DokterId2 equals d2.UserActiveId into dokter2Group
                            from d2 in dokter2Group.DefaultIfEmpty()

                            join d3 in _applicationDbContext.UserActives
                                on t.DokterId3 equals d3.UserActiveId into dokter3Group
                            from d3 in dokter3Group.DefaultIfEmpty()

                            where t.IsDelete == false || t.IsDelete == null
                            select new
                            {
                                t.TransferPasienId,
                                t.KunjunganId,
                                t.BedId,
                                b.NomorBed,
                                b.Deskripsi,
                                t.DiagnosaUtama,
                                t.DiagnosaSekunder,
                                DokterUtama = d1 != null ? d1.FullName : null,
                                DokterPendamping = d2 != null ? d2.FullName : null,
                                DokterTambahan = d3 != null ? d3.FullName : null,
                                t.IndikasiRanap,
                                t.IsAlergic,
                                t.AlergicOf,
                                t.AlasanPindahPasien,
                                t.TglPindah,
                                t.PengawasanHarianId,
                                t.ObservasiCairanId,
                                t.IndikatorPengkajianId,
                                t.PemberianObatId,
                                t.TotalScoreAldrete,
                                t.TotalScoreSteward,
                                t.IsICU,
                                t.BarangDiserahkan,
                                t.IntervensiPerawat,
                                t.PlanningTindakan,

                                t.PetugasMenyerahkanId,
                                t.TTDMenyerahkanPath,

                                t.PetugasMengetahuiId,
                                t.TTDMengetahuiPath,

                                t.PetugasPenerimaId,
                                t.TTDPenerimaPath,

                                t.Keterangan,
                                t.CreateDateTime,
                                CreateByName = u != null ? u.FullName : null
                            };

                // ==============================
                // 2️⃣ Filter
                // ==============================

                // filter by kunjungan id
                if (kunjunganId.HasValue)
                {
                    query=query.Where(u=>u.KunjunganId == kunjunganId.Value);
                }

                // 🔹 Search — di Diagnosa, Alasan, atau Nama Dokter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string searchLower = $"%{search.ToLower()}%";
                    query = query.Where(x =>
                        EF.Functions.ILike(x.DiagnosaUtama!, searchLower) ||
                        EF.Functions.ILike(x.DiagnosaSekunder!, searchLower) ||
                        EF.Functions.ILike(x.AlasanPindahPasien!, searchLower) ||
                        EF.Functions.ILike(x.DokterUtama!, searchLower) ||
                        EF.Functions.ILike(x.DokterPendamping!, searchLower) ||
                        EF.Functions.ILike(x.DokterTambahan!, searchLower) ||
                        EF.Functions.ILike(x.CreateByName!, searchLower));
                }

                // 🔹 Filter tanggal pindah pasien
                if (startDate.HasValue && endDate.HasValue)
                {
                    var startUtc = startDate.Value.Date;
                    var endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.TglPindah >= startUtc && x.TglPindah <= endUtc);
                }

                // ==============================
                // 3️⃣ Sorting
                // ==============================
                query = sortDirection?.ToLower() == "desc"
                    ? orderBy switch
                    {
                        "DiagnosaUtama" => query.OrderByDescending(x => x.DiagnosaUtama),
                        "DokterUtama" => query.OrderByDescending(x => x.DokterUtama),
                        "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                        _ => query.OrderByDescending(x => x.CreateDateTime)
                    }
                    : orderBy switch
                    {
                        "DiagnosaUtama" => query.OrderBy(x => x.DiagnosaUtama),
                        "DokterUtama" => query.OrderBy(x => x.DokterUtama),
                        "CreateByName" => query.OrderBy(x => x.CreateByName),
                        _ => query.OrderBy(x => x.CreateDateTime)
                    };

                // ==============================
                // 4️⃣ Paging
                // ==============================
                var totalRows = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

                var listData = await query
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .AsNoTracking()
                    .ToListAsync();

                // ==============================
                // 5️⃣ Return hasil
                // ==============================
                return Ok(new
                {
                    message = listData.Any() ? "Berhasil || 200 OK" : "Tidak ada data Transfer Pasien || 200 OK",
                    data = listData.Any() ? listData : null,
                    pagination = new
                    {
                        CurrentPage = page,
                        PerPage = perPage,
                        TotalRows = totalRows,
                        TotalPages = totalPages
                    }
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

    }
}
