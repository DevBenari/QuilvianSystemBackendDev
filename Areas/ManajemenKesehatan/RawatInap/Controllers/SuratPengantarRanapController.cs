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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class SuratPengantarRanapController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<SuratPengantarRanapController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<SuratPengantarRanapHub> _hubContext;

        public SuratPengantarRanapController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SuratPengantarRanapController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<SuratPengantarRanapHub> hubContext
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        private static string HitungUmurLengkap(DateTime? tanggalLahir)
        {
            if (!tanggalLahir.HasValue) return "-";

            var today = DateTime.Today;
            int tahun = today.Year - tanggalLahir.Value.Year;
            int bulan = today.Month - tanggalLahir.Value.Month;
            int hari = today.Day - tanggalLahir.Value.Day;

            if (hari < 0)
            {
                bulan--;
                var prevMonth = today.AddMonths(-1);
                hari += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            }

            if (bulan < 0)
            {
                tahun--;
                bulan += 12;
            }

            return $"{tahun} tahun {bulan} bulan {hari} hari";
        }

        private async Task<bool> CanCreateSuratForKunjunganAsync(Guid kunjunganId)
        {
            // 1) Ambil NoRM dari Kunjungan
            var pasien = await (from k in _applicationDbContext.Kunjungans.AsNoTracking()
                                join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                                    on k.PasienId equals p.PendaftaranPasienBaruId
                                where k.KunjunganID == kunjunganId
                                select new { p.NoRekamMedis })
                               .FirstOrDefaultAsync();

            // Kalau kunjungan/pasien/NoRM tidak ada → tidak boleh buat
            if (pasien == null || string.IsNullOrWhiteSpace(pasien.NoRekamMedis))
            {
                return false;
            }

            // 2) Cek apakah NoRM ini sudah punya Surat Pengantar Ranap aktif
            var sudahAda = await (from s in _applicationDbContext.SuratPengantarRawatInaps.AsNoTracking()
                                  join k in _applicationDbContext.Kunjungans.AsNoTracking()
                                      on s.KunjunganId equals k.KunjunganID
                                  join p in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                                      on k.PasienId equals p.PendaftaranPasienBaruId
                                  join bb in _applicationDbContext.BookingBedRanaps.AsNoTracking()
                                      on k.KunjunganID equals bb.KunjunganId 
                                  where p.NoRekamMedis == pasien.NoRekamMedis && bb.TglKeluar == null
                                  select 1)
                                 .AnyAsync();


            // Return true jika belum ada (negasi dari sudahAda), false jika sudah ada
            return !sudahAda;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = from a in _applicationDbContext.SuratPengantarRawatInaps
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()

                        join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                        join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                        join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
                        join ar in _applicationDbContext.Asuransis on k.AsuransiId equals ar.AsuransiId into asuransiGroup
                        from ar in asuransiGroup.DefaultIfEmpty()
                        join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
                        from ap in asuransiPasienGroup.DefaultIfEmpty()

                        where (a.IsDelete == false || a.IsDelete == null)
                              && (k.IsDelete == false || k.IsDelete == null)

                        orderby a.CreateDateTime descending

                        select new
                        {
                            // Data Surat Pengantar
                            a.SuratPengantarRawatInapId,
                            a.KunjunganId,
                            a.NomorSuratPengantar,
                            a.Diagnosa,
                            a.ICDId,
                            a.AlasanRanap,
                            a.RencanaTindakLanjut,
                            a.AsalUnit,
                            a.Status,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,

                            // Data Kunjungan
                            k.NoRekamMedis,
                            k.TipePasien,
                            k.TipePembayaran,
                            k.JenisKunjungan,
                            k.IsFinished,
                            //k.TglMasukRanap,
                            //k.TglKeluarRanap,

                            // Data Dokter
                            DokterId = d.DokterId,
                            DokterName = d.NmDokter,

                            // Data Poli
                            PoliklinikId = poli.PoliklinikId,
                            PoliklinikName = poli.NamaPoliklinik,

                            // Data Pasien
                            PasienId = p.PendaftaranPasienBaruId,
                            PasienName = p.NamaLengkap,
                            JenisKelamin = p.JenisKelamin,
                            Umur = HitungUmurLengkap(p.TanggalLahir),
                            p.NoPasien,

                            //data Asuransi
                            ar.NamaAsuransi,
                            ap.NoPolis
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
            var result = (
                from a in _applicationDbContext.SuratPengantarRawatInaps
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userJoin
                from u in userJoin.DefaultIfEmpty()

                join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
                join ar in _applicationDbContext.Asuransis on k.AsuransiId equals ar.AsuransiId into asuransiGroup
                from ar in asuransiGroup.DefaultIfEmpty()
                join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
                from ap in asuransiPasienGroup.DefaultIfEmpty()

                where a.SuratPengantarRawatInapId == id
                      && (a.IsDelete == false || a.IsDelete == null)
                      && (k.IsDelete == false || k.IsDelete == null)

                select new
                {
                    // Data Surat Pengantar
                    a.SuratPengantarRawatInapId,
                    a.KunjunganId,
                    a.NomorSuratPengantar,
                    a.Diagnosa,
                    a.ICDId,
                    a.AlasanRanap,
                    a.RencanaTindakLanjut,
                    a.AsalUnit,
                    a.Status,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,

                    // Data Kunjungan
                    k.NoRekamMedis,
                    k.TipePasien,
                    k.TipePembayaran,
                    k.JenisKunjungan,
                    k.IsFinished,
                    //k.TglMasukRanap,
                    //k.TglKeluarRanap,

                    // Data Dokter
                    DokterId = d.DokterId,
                    DokterName = d.NmDokter,

                    // Data Poli
                    PoliklinikId = poli.PoliklinikId,
                    PoliklinikName = poli.NamaPoliklinik,

                    // Data Pasien
                    PasienId = p.PendaftaranPasienBaruId,
                    PasienName = p.NamaLengkap,
                    JenisKelamin = p.JenisKelamin,
                    Umur = HitungUmurLengkap(p.TanggalLahir),
                    p.NoPasien,

                    // Data Asuransi
                    ar.NamaAsuransi,
                    ap.NoPolis

                }
            ).FirstOrDefault();

            if (result == null)
            {
                return NotFound(new { message = "Surat pengantar tidak ditemukan." });
            }

            return Ok(result);
        }

        //[HttpGet("DataPasien/{kunjunganId}")]
        //public async Task<IActionResult> GetDataPasienByKunjunganId(Guid kunjunganId)
        //{
        //    // Cek apakah KunjunganId valid  
        //    if (kunjunganId == Guid.Empty)
        //    {
        //        return BadRequest(new { message = "KunjunganId tidak valid." });
        //    }

        //    // Ambil data Kunjungan berdasarkan KunjunganId  
        //    var query =
        //        from k in _applicationDbContext.Kunjungans
        //        join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
        //        join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
        //        join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
        //        where k.KunjunganID == kunjunganId && (k.IsDelete == false || k.IsDelete == null)
        //        select new
        //        {
        //            k.KunjunganID,
        //            k.NoRekamMedis,
        //            k.TipePasien,
        //            k.TipePembayaran,
        //            k.JenisKunjungan,
        //            k.IsFinished,
        //            k.TglMasukRanap,
        //            k.TglKeluarRanap,
        //            DokterId = d.DokterId,
        //            DokterName = d.NmDokter,
        //            PoliklinikId = poli.PoliklinikId,
        //            PoliklinikName = poli.NamaPoliklinik,
        //            PasienId = p.PendaftaranPasienBaruId,
        //            PasienName = p.NamaLengkap,
        //            Umur = HitungUmurLengkap(p.TanggalLahir),
        //            p.JenisKelamin,
        //        };

        //    var result = await query.FirstOrDefaultAsync();

        //    if (result == null)
        //    {
        //        return NotFound(new { message = "Data pasien tidak ditemukan." });
        //    }

        //    return Ok(new
        //    {
        //        message = "Data pasien ditemukan || 200 OK",
        //        data = result
        //    });
        //}

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SuratPengantarRawatInapViewModel vm)
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
                //bool isDuplicate = _applicationDbContext.Diskons
                //                    .Any(c => c.NamaDiskon == vm.NamaDiskon);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

                // Generate Nomor Surat Pengantar Rawat Inap
                int tahunSekarang = DateTime.UtcNow.Year;
                int jumlahSuratTahunIni = await _applicationDbContext.SuratPengantarRawatInaps
                    .CountAsync(s => s.CreateDateTime.Year == tahunSekarang);

                int nomorUrut = jumlahSuratTahunIni + 1;
                string nomorSurat = $"{nomorUrut:D3}/SP-RI/MMC/{tahunSekarang}";

                // **Cek apakah KunjunganId valid dan belum ada Surat Pengantar Ranap aktif**
                var canCreate = await CanCreateSuratForKunjunganAsync(vm.KunjunganId.Value);
                if (!canCreate)
                    return StatusCode(StatusCodes.Status409Conflict,
                        new { message = "Kunjungan ini sudah dalam proses rawat inap aktif" });

                // **Buat Data Baru**
                var data = new SuratPengantarRawatInap
                {
                   SuratPengantarRawatInapId = Guid.NewGuid(),
                   KunjunganId = vm.KunjunganId,
                   Diagnosa = vm.Diagnosa,
                   ICDId = vm.ICDId,
                   AlasanRanap = vm.AlasanRanap,
                   RencanaTindakLanjut = vm.RencanaTindakLanjut,
                   AsalUnit = vm.AsalUnit,
                   NomorSuratPengantar = nomorSurat,
                   Status = FilterStatusSuratPengantarRanap.Menunggu.ToString(),
                   CreateBy = userActiveId,
                   CreateDateTime = DateTimeOffset.UtcNow,
                };

                 // **Simpan ke Database**
                 _applicationDbContext.SuratPengantarRawatInaps.Add(data);
                 int result = await _applicationDbContext.SaveChangesAsync();

                // Notifikasi signalR
                await _hubContext.Clients.All.SendAsync("Surat pengantar rawat inap ditambah", new
                {
                    action = "create",
                    suratid = data.SuratPengantarRawatInapId,
                    kunjunganId = data.KunjunganId,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] SuratPengantarRawatInapViewModel vm)
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
                var data = await _applicationDbContext.SuratPengantarRawatInaps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.Diagnosa = vm.Diagnosa;
                data.ICDId = vm.ICDId;
                data.AlasanRanap = vm.AlasanRanap;
                data.RencanaTindakLanjut = vm.RencanaTindakLanjut;
                data.AsalUnit = vm.AsalUnit;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.SuratPengantarRawatInaps.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                // Notifikasi signalR
                await _hubContext.Clients.All.SendAsync("Surat pengantar rawat inap diupdate", new
                {
                    action = "update",
                    suratid = data.SuratPengantarRawatInapId,
                    kunjunganId = data.KunjunganId,
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

        [HttpPut("{id}/Status-SuratPengantarRanap")]
        public async Task<IActionResult> UpdateIsFinished(Guid id, [FromBody] StatusSuratPengantarRanapVM request)
        {
            var data = await _applicationDbContext.SuratPengantarRawatInaps.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.Status = request.status.ToString();
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;

            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { message = "Status isFinished berhasil diperbarui." });
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
                var data = await _applicationDbContext.SuratPengantarRawatInaps.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.SuratPengantarRawatInaps.Update(data);
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
            var query = from a in _applicationDbContext.SuratPengantarRawatInaps
                        join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()

                        join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID
                        join p in _applicationDbContext.PendaftaranPasienBarus on k.PasienId equals p.PendaftaranPasienBaruId
                        join d in _applicationDbContext.Dokters on k.DokterId equals d.DokterId
                        join poli in _applicationDbContext.Polikliniks on k.PoliklinikId equals poli.PoliklinikId
                        join ar in _applicationDbContext.Asuransis on k.AsuransiId equals ar.AsuransiId into asuransiGroup
                        from ar in asuransiGroup.DefaultIfEmpty()
                        join ap in _applicationDbContext.AsuransiPasiens on p.PendaftaranPasienBaruId.ToString() equals ap.PasienId into asuransiPasienGroup
                        from ap in asuransiPasienGroup.DefaultIfEmpty()

                        where (a.IsDelete == false || a.IsDelete == null)
                              && (k.IsDelete == false || k.IsDelete == null)

                        select new
                        {
                            // Data Surat Pengantar
                            a.SuratPengantarRawatInapId,
                            a.KunjunganId,
                            a.NomorSuratPengantar,
                            a.Diagnosa,
                            a.ICDId,
                            a.AlasanRanap,
                            a.RencanaTindakLanjut,
                            a.AsalUnit,
                            a.Status,
                            a.CreateDateTime,
                            a.CreateBy,
                            CreateByName = u.FullName,

                            // Data Kunjungan
                            k.NoRekamMedis,
                            k.TipePasien,
                            k.TipePembayaran,
                            k.JenisKunjungan,
                            k.IsFinished,
                            //k.TglMasukRanap,
                            //k.TglKeluarRanap,

                            // Data Dokter
                            DokterId = d.DokterId,
                            DokterName = d.NmDokter,

                            // Data Poli
                            PoliklinikId = poli.PoliklinikId,
                            PoliklinikName = poli.NamaPoliklinik,

                            // Data Pasien
                            PasienId = p.PendaftaranPasienBaruId,
                            PasienName = p.NamaLengkap,
                            JenisKelamin = p.JenisKelamin,
                            Umur = HitungUmurLengkap(p.TanggalLahir),
                            p.NoPasien,

                            //data Asuransi
                            ar.NamaAsuransi,
                            ap.NoPolis
                        };

            //**Filter berdasarkan search(Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NomorSuratPengantar, search)
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
                    "NomorSuratPengantar" => query.OrderByDescending(u => u.NomorSuratPengantar),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "NomorSuratPengantar" => query.OrderBy(u => u.NomorSuratPengantar),
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
