using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
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

        public ResepDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResepDetailController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> GetAllDetailResep(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            var query = (from d in _applicationDbContext.DetailReseps

                             // Join ke Resep (parent)
                         join r in _applicationDbContext.Reseps
                             on d.ResepId equals r.ResepId into resepJoin
                         from r in resepJoin.DefaultIfEmpty()

                             // Join ke User (creator)
                         join u in _applicationDbContext.UserActives
                             on d.CreateBy equals u.UserActiveId into userJoin
                         from u in userJoin.DefaultIfEmpty()

                             // Join ke Obat (jika bukan racikan)
                         join o in _applicationDbContext.Obats
                             on d.ObatId equals o.ObatId into obatJoin
                         from o in obatJoin.DefaultIfEmpty()

                             // Join ke Racikan (jika racikan)
                         join ra in _applicationDbContext.Racikans
                             on d.RacikanId equals ra.RacikanId into racikanJoin
                         from ra in racikanJoin.DefaultIfEmpty()

                         where d.IsDelete == false
                         select new
                         {
                             // ===== Data Detail Resep =====
                             d.DetailResepId,
                             d.ResepId,
                             d.ObatId,
                             d.RacikanId,
                             d.IsRacikan,
                             d.Qty,
                             d.HargaObat,
                             d.TotalHargaObat,
                             d.Signa,
                             d.SignaTambahan,
                             d.TakaranDosis,
                             d.IsIteratur,
                             d.JumlahIteratur,
                             TglMulaiIteratur = d.TglMulaiIteratur.HasValue
                                                ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd")
                                                : null,
                             MasaAktifIteratur = d.MasaAktifIteratur.HasValue
                                                ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd")
                                                : null,
                             d.JarakPenebusan,
                             d.StatusCoverObat,
                             d.StatusPengambilanObat,
                             d.StatusDiberikanPasien,
                             d.CaraPemakaian,
                             d.EstimasiPemberian,
                             d.TglStopPemakaian,
                             d.CreateDateTime,
                             d.CreateBy,
                             CreateByName = u != null ? u.FullName : null,

                             // ===== Data Resep (parent) =====
                             Resep = r == null ? null : new
                             {
                                 r.ResepId,
                                 r.KunjunganId,
                                 r.NamaPasien,
                                 r.NamaDokter,
                                 r.NamaPoliklinik,
                                 r.NamaAsuransi,
                                 r.StatusPembuatanResep,
                                 r.StatusPengambilanResep,
                                 r.IsCancelled,
                                 r.IsLunas,
                                 r.CreateDateTime
                             },

                             // ===== Data Obat (jika bukan racikan) =====
                             Obat = (d.IsRacikan == false || d.IsRacikan == null) && o != null ? new
                             {
                                 o.ObatId,
                                 o.ObatName,
                                 o.Dosis,
                                 o.CaraKerja
                             } : null,

                             // ===== Data Racikan (jika racikan) =====
                             Racikan = d.IsRacikan == true && ra != null ? new
                             {
                                 ra.RacikanId,
                                 ra.NamaRacikan,
                                 ra.CreateBy,
                                 ra.CreateDateTime,

                                 DaftarRacikanDetail = (from rd in _applicationDbContext.RacikanDetails
                                                        join ob in _applicationDbContext.Obats
                                                            on rd.ObatId equals ob.ObatId into rdObatJoin
                                                        from ob in rdObatJoin.DefaultIfEmpty()
                                                        where rd.RacikanId == ra.RacikanId
                                                        select new
                                                        {
                                                            rd.DetailRacikanId,
                                                            rd.ObatId,
                                                            ObatName = ob != null ? ob.ObatName : null,
                                                            rd.QtyUsed,
                                                            rd.KomposisiDosis,
                                                            rd.CreateBy,
                                                            rd.CreateDateTime
                                                        }).ToList()
                             } : null
                         })
                         .OrderByDescending(d => d.CreateDateTime);

            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

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



        //public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        //{
        //    // Validasi agar page dan perPage minimal bernilai 1
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;

        //    // Query data
        //    var query = (from a in _applicationDbContext.DetailReseps
        //                 join u in _applicationDbContext.UserActives
        //                 on a.CreateBy equals u.UserActiveId

        //                 // join ke resep
        //                 join r in _applicationDbContext.Reseps
        //                    on a.ResepId equals r.ResepId

        //                 where a.IsDelete == false
        //                 select new
        //                 {
        //                     a.CreateDateTime,
        //                     a.CreateBy,
        //                     CreateByName = u.FullName,
        //                     a.DetailResepId,
        //                     a.ResepId,
        //                     r.KunjunganId,
        //                     a.AsuransiId,
        //                     a.NamaAsuransi,
        //                     a.ObatId,
        //                     a.Qty,
        //                     a.JenisRacikan,
        //                     a.Signa,
        //                     a.SignaTambahan,
        //                     a.JenisObat,
        //                     a.HargaObat,
        //                     a.TotalHargaObat,
        //                     a.StatusCoverObat,
        //                     a.IsRacikan,
        //                     a.RacikanId,
        //                     a.IsIteratur,
        //                     a.JumlahIteratur,
        //                     a.TglMulaiIteratur,
        //                     a.JarakPenebusan,
        //                     a.MasaAktifIteratur,
        //                     a.StatusPengambilanObat,
        //                     a.StatusDiberikanPasien,
        //                     a.TakaranDosis,
        //                     a.IsContinued,
        //                     a.CaraPemakaian,
        //                     a.EstimasiPemberian,
        //                     a.TglStopPemakaian,
        //                 }).OrderByDescending(a => a.CreateDateTime);

        //// Hitung total data sebelum paginasi
        //var totalRows = query.Count();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    // Ambil data sesuai paging
        //    var listdata = query
        //        .Skip((page - 1) * perPage)
        //        .Take(perPage)
        //        .ToList();

        //    if (!listdata.Any())
        //    {
        //        return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
        //    }

        //    // Return hasil dengan paging info
        //    return Ok(new
        //    {
        //        message = "Berhasil || 200 OK",
        //        data = listdata,
        //        pagination = new
        //        {
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalRows = totalRows,
        //            TotalPages = totalPages
        //        }
        //    });

        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdDetailResep(Guid id)
        {
            var data = (from d in _applicationDbContext.DetailReseps

                            // Join ke Resep (parent)
                        join r in _applicationDbContext.Reseps
                            on d.ResepId equals r.ResepId into resepJoin
                        from r in resepJoin.DefaultIfEmpty()

                            // Join ke User (creator)
                        join u in _applicationDbContext.UserActives
                            on d.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()

                            // Join ke Obat (jika bukan racikan)
                        join o in _applicationDbContext.Obats
                            on d.ObatId equals o.ObatId into obatJoin
                        from o in obatJoin.DefaultIfEmpty()

                            // Join ke Racikan (jika racikan)
                        join ra in _applicationDbContext.Racikans
                            on d.RacikanId equals ra.RacikanId into racikanJoin
                        from ra in racikanJoin.DefaultIfEmpty()

                        where d.IsDelete == false && d.DetailResepId == id
                        select new
                        {
                            // ===== Data Detail Resep =====
                            d.DetailResepId,
                            d.ResepId,
                            d.ObatId,
                            d.RacikanId,
                            d.IsRacikan,
                            d.Qty,
                            d.HargaObat,
                            d.TotalHargaObat,
                            d.Signa,
                            d.SignaTambahan,
                            d.TakaranDosis,
                            d.IsIteratur,
                            d.JumlahIteratur,
                            TglMulaiIteratur = d.TglMulaiIteratur.HasValue
                                                ? d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd")
                                                : null,
                            MasaAktifIteratur = d.MasaAktifIteratur.HasValue
                                                ? d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd")
                                                : null,
                            d.JarakPenebusan,
                            d.StatusCoverObat,
                            d.StatusPengambilanObat,
                            d.StatusDiberikanPasien,
                            d.CaraPemakaian,
                            d.EstimasiPemberian,
                            d.TglStopPemakaian,
                            d.CreateDateTime,
                            d.CreateBy,
                            CreateByName = u != null ? u.FullName : null,

                            // ===== Data Resep (parent) =====
                            Resep = r == null ? null : new
                            {
                                r.ResepId,
                                r.KunjunganId,
                                r.NamaPasien,
                                r.NamaDokter,
                                r.NamaPoliklinik,
                                r.NamaAsuransi,
                                r.StatusPembuatanResep,
                                r.StatusPengambilanResep,
                                r.IsCancelled,
                                r.IsLunas,
                                r.CreateDateTime
                            },

                            // ===== Data Obat (jika bukan racikan) =====
                            Obat = (d.IsRacikan == false || d.IsRacikan == null) && o != null ? new
                            {
                                o.ObatId,
                                o.ObatName,
                                o.Dosis,
                                o.CaraKerja
                            } : null,

                            // ===== Data Racikan (jika racikan) =====
                            Racikan = d.IsRacikan == true && ra != null ? new
                            {
                                ra.RacikanId,
                                ra.NamaRacikan,
                                ra.CreateBy,
                                ra.CreateDateTime,

                                DaftarRacikanDetail = (from rd in _applicationDbContext.RacikanDetails
                                                       join ob in _applicationDbContext.Obats
                                                           on rd.ObatId equals ob.ObatId into rdObatJoin
                                                       from ob in rdObatJoin.DefaultIfEmpty()
                                                       where rd.RacikanId == ra.RacikanId
                                                       select new
                                                       {
                                                           rd.DetailRacikanId,
                                                           rd.ObatId,
                                                           ObatName = ob != null ? ob.ObatName : null,
                                                           rd.QtyUsed,
                                                           rd.KomposisiDosis,
                                                           rd.CreateBy,
                                                           rd.CreateDateTime
                                                       }).ToList()
                            } : null
                        })
                        .FirstOrDefault();

            if (data == null)
            {
                return NotFound(new { message = $"DetailResep dengan ID {id} tidak ditemukan. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data
            });
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

            return Ok(new { message = "Status StatusDiberikanPasien berhasil diperbarui." });
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

                //// **Cek Duplikasi**
                //bool isDuplicate = _applicationDbContext.Benefits
                //                    .Any(c => c.NamaBenefit == vm.NamaBenefit);

                //if (isDuplicate)
                //{
                //    return Conflict(new { message = "Nama benefit ini telah tersedia" });
                //}

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
                    Qty = vm.Qty,
                    TakaranDosis = vm.TakaranDosis,
                    Signa = vm.Signa,
                    SignaTambahan = vm.SignaTambahan,
                    JenisObat = vm.JenisObat,
                    HargaObat = vm.HargaObat,
                    TotalHargaObat = vm.Qty.HasValue && vm.HargaObat.HasValue ? vm.Qty.Value * vm.HargaObat.Value : 0,
                    StatusCoverObat = vm.StatusCoverObat,
                    IsRacikan = vm.IsRacikan, // Tambahkan properti IsRacikan jika diperlukan
                    //IsIteratur = vm.IsIteratur,
                    //JumlahIteratur = vm.JumlahIteratur,
                    //TglMulaiIteratur = parsedTglMulaiIteratur,
                    //JarakPenebusan = vm.JarakPenebusan,
                    //MasaAktifIteratur = parsedMasaAktif,
                    StatusPengambilanObat = false, // Default nilai StatusPengambilanObat
                    CaraPemakaian = vm.CaraPemakaian,
                    EstimasiPemberian = vm.EstimasiPemberian,
                    TglStopPemakaian = TryParseTanggalToUtc(vm.TglStopPemakaian),
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.DetailReseps.Add(data);
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
            Guid? kunjunganId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            DateTime? startDate = null,
            DateTime? endDate = null,
            PeriodeFilter? periode = null)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query dasar
            var query = from d in _applicationDbContext.DetailReseps
                        join r in _applicationDbContext.Reseps
                            on d.ResepId equals r.ResepId into resepJoin
                        from r in resepJoin.DefaultIfEmpty()
                        join u in _applicationDbContext.UserActives
                            on d.CreateBy equals u.UserActiveId into userJoin
                        from u in userJoin.DefaultIfEmpty()
                        join o in _applicationDbContext.Obats
                            on d.ObatId equals o.ObatId into obatJoin
                        from o in obatJoin.DefaultIfEmpty()
                        join ra in _applicationDbContext.Racikans
                            on d.RacikanId equals ra.RacikanId into racikanJoin
                        from ra in racikanJoin.DefaultIfEmpty()
                        where d.IsDelete == false
                        select new { d, r, u, o, ra };

            // 🔎 Filter KunjunganId dilakukan di sini
            if (kunjunganId.HasValue)
            {
                query = query.Where(x => x.r != null && x.r.KunjunganId == kunjunganId.Value);
            }

            // 🔎 Filter tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.d.CreateDateTime >= start && x.d.CreateDateTime <= end);
            }

            // 🔎 Filter periode
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;
                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.d.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x => x.d.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek));
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.d.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            x.d.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x => x.d.CreateDateTime.Month == today.Month && x.d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(x => x.d.CreateDateTime.Month == today.AddMonths(-1).Month && x.d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.d.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.d.CreateDateTime.Year == today.Year - 1);
                        break;
                }
            }

            // Baru lakukan projection ke object hasil
            var projected = query.Select(x => new
            {
                // DetailResep
                x.d.DetailResepId,
                x.d.ResepId,
                x.d.ObatId,
                x.d.RacikanId,
                x.d.IsRacikan,
                x.d.Qty,
                x.d.HargaObat,
                x.d.TotalHargaObat,
                x.d.Signa,
                x.d.SignaTambahan,
                x.d.TakaranDosis,
                x.d.IsIteratur,
                x.d.JumlahIteratur,
                TglMulaiIteratur = x.d.TglMulaiIteratur.HasValue ? x.d.TglMulaiIteratur.Value.ToString("yyyy-MM-dd") : null,
                MasaAktifIteratur = x.d.MasaAktifIteratur.HasValue ? x.d.MasaAktifIteratur.Value.ToString("yyyy-MM-dd") : null,
                x.d.JarakPenebusan,
                x.d.StatusCoverObat,
                x.d.StatusPengambilanObat,
                x.d.StatusDiberikanPasien,
                x.d.CaraPemakaian,
                x.d.EstimasiPemberian,
                x.d.TglStopPemakaian,
                x.d.CreateDateTime,
                x.d.CreateBy,
                CreateByName = x.u != null ? x.u.FullName : null,

                // Resep
                Resep = x.r == null ? null : new
                {
                    x.r.ResepId,
                    x.r.KunjunganId,
                    x.r.NamaPasien,
                    x.r.NamaDokter,
                    x.r.NamaPoliklinik,
                    x.r.NamaAsuransi,
                    x.r.StatusPembuatanResep,
                    x.r.StatusPengambilanResep,
                    x.r.IsCancelled,
                    x.r.IsLunas,
                    x.r.CreateDateTime
                },

                // Obat
                Obat = (x.d.IsRacikan == false || x.d.IsRacikan == null) && x.o != null ? new
                {
                    x.o.ObatId,
                    x.o.ObatName,
                    x.o.Dosis,
                    x.o.CaraKerja
                } : null,

                // Racikan
                Racikan = x.d.IsRacikan == true && x.ra != null ? new
                {
                    x.ra.RacikanId,
                    x.ra.NamaRacikan,
                    x.ra.CreateBy,
                    x.ra.CreateDateTime
                } : null
            });

            // 🔎 Sorting
            projected = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => projected.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => projected.OrderByDescending(u => u.CreateByName),
                    _ => projected.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => projected.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => projected.OrderBy(u => u.CreateByName),
                    _ => projected.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
            var totalRows = projected.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = projected.Skip((page - 1) * perPage).Take(perPage).ToList();

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
