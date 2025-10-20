using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KajianPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KajianPasienController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KajianPasienController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<KajianPasienController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =========================
            // 1️⃣ Ambil data utama Kajian Pasien
            // =========================
            var baseQuery = from a in _applicationDbContext.KajianPasiens
                            join u in _applicationDbContext.UserActives
                                on a.CreateBy equals u.UserActiveId into userGroup
                            from u in userGroup.DefaultIfEmpty()

                            join k in _applicationDbContext.Kunjungans
                                on a.KunjunganId equals k.KunjunganID into kunjunganGroup
                            from k in kunjunganGroup.DefaultIfEmpty()

                            where a.IsDelete == false || a.IsDelete == null
                            orderby a.CreateDateTime descending
                            select new
                            {
                                a.KajianPasienId,
                                a.KunjunganId,
                                a.VitalSignId,
                                a.DokterId,
                                a.UserActiveId,
                                a.KeadaanUmum,
                                a.KeadaanKulit,
                                a.KeadaanKepalaLeher,
                                a.KeadaanDada,
                                a.KeadaanJantung,
                                a.KeadaanParuParu,
                                a.KeadaanAbdomen,
                                a.KeadaanGenitalia,
                                a.KeadaanAnggotaGerak,
                                a.KeadaanLainnya,
                                a.StatusLokalis,
                                a.PemeriksaanPenunjang,
                                a.DiagnosaSaatIni,
                                a.DiagnosaBanding,
                                a.DaftarMasalah,
                                a.Program,
                                a.Terapi,
                                a.Edukasi,
                                a.EdukasiKepada,
                                a.Keterangan,
                                a.TglKajian,
                                a.CreateBy,
                                a.CreateDateTime,
                                a.KajianUtamaPengkajian,
                                a.CurrentMedicationId,
                                CreateByName = u.FullName,
                                k.NoRekamMedis
                            };

            var totalRows = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var listData = await baseQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .AsNoTracking()
                .ToListAsync();

            if (!listData.Any())
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });

            // =========================
            // 2️⃣ Ambil data PainAssessment & SuratPengantar dalam batch (hindari N+1)
            // =========================
            var kunjunganIds = listData.Select(x => x.KunjunganId).Distinct().ToList();

            var painAssessments = await _applicationDbContext.PainAssessments
                .Where(p => kunjunganIds.Contains(p.KunjunganId))
                .Select(p => new
                {
                    p.PainAssessmentId,
                    p.KunjunganId,
                    p.InheritedDisease,
                    p.CreateDateTime
                })
                .OrderByDescending(p => p.CreateDateTime)
                .ToListAsync();

            var suratPengantar = await _applicationDbContext.SuratPengantarRawatInaps
                .Where(s => kunjunganIds.Contains(s.KunjunganId))
                .Select(s => new
                {
                    s.KunjunganId,
                    s.AsalUnit
                })
                .ToListAsync();

            // =========================
            // 3️⃣ Buat lookup untuk relasi
            // =========================
            var painLookup = painAssessments.ToLookup(p => p.KunjunganId);
            var suratLookup = suratPengantar.ToLookup(s => s.KunjunganId);

            // =========================
            // 4️⃣ Gabungkan hasil (tanpa duplikasi, 1 KajianPasien = 1 baris)
            // =========================
            var result = listData.Select(x => new
            {
                x.CreateDateTime,
                x.CreateBy,
                x.CreateByName,
                x.KajianPasienId,
                x.KunjunganId,
                x.VitalSignId,
                x.NoRekamMedis,
                x.DokterId,
                x.UserActiveId,
                x.KeadaanUmum,
                x.KeadaanKulit,
                x.KeadaanKepalaLeher,
                x.KeadaanDada,
                x.KeadaanJantung,
                x.KeadaanParuParu,
                x.KeadaanAbdomen,
                x.KeadaanGenitalia,
                x.KeadaanAnggotaGerak,
                x.KeadaanLainnya,
                x.StatusLokalis,
                x.PemeriksaanPenunjang,
                x.DiagnosaSaatIni,
                x.DiagnosaBanding,
                x.DaftarMasalah,
                x.Program,
                x.Terapi,
                x.Edukasi,
                x.EdukasiKepada,
                x.Keterangan,
                x.TglKajian,
                x.KajianUtamaPengkajian,
                x.CurrentMedicationId,
                // 🔹 List Pain Assessment (semua record terkait)
                PainAssessments = painLookup[x.KunjunganId].ToList(),
                // 🔹 Info surat pengantar (ambil 1 saja karena jarang multiple)
                AsalUnit = suratLookup[x.KunjunganId].FirstOrDefault()?.AsalUnit
            });

            // =========================
            // 5️⃣ Return hasil
            // =========================
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = result,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        //{
        //    // Validasi agar page dan perPage minimal bernilai 1
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;


        //    var query = (from a in _applicationDbContext.KajianPasiens
        //                 join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
        //                 from u in userGroup.DefaultIfEmpty()

        //                 join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID into kunjunganGroup
        //                 from k in kunjunganGroup.DefaultIfEmpty()

        //                 join pa in _applicationDbContext.PainAssessments on a.KunjunganId equals pa.KunjunganId into painGroup
        //                 from pa in painGroup.DefaultIfEmpty()

        //                 join sp in _applicationDbContext.SuratPengantarRawatInaps on a.KunjunganId equals sp.KunjunganId into suratGroup
        //                 from sp in suratGroup.DefaultIfEmpty()

        //                 where a.IsDelete == false || a.IsDelete == null
        //                 select new
        //                 {
        //                     a.CreateDateTime,
        //                     a.CreateBy,
        //                     CreateByName = u.FullName,
        //                     a.KajianPasienId,
        //                     a.KunjunganId,
        //                     a.VitalSignId,
        //                     k.NoRekamMedis,
        //                     a.DokterId,
        //                     a.UserActiveId,
        //                     a.KeadaanUmum,
        //                     a.KeadaanKulit,
        //                     a.KeadaanKepalaLeher,
        //                     a.KeadaanDada,
        //                     a.KeadaanJantung,
        //                     a.KeadaanParuParu,
        //                     a.KeadaanAbdomen,
        //                     a.KeadaanGenitalia,
        //                     a.KeadaanAnggotaGerak,
        //                     a.KeadaanLainnya,
        //                     a.StatusLokalis,
        //                     a.PemeriksaanPenunjang,
        //                     a.DiagnosaSaatIni,
        //                     a.DiagnosaBanding,
        //                     a.DaftarMasalah,
        //                     a.Program,
        //                     a.Terapi,
        //                     a.Edukasi,
        //                     a.EdukasiKepada,
        //                     a.Keterangan,
        //                     a.TglKajian,
        //                     a.KajianUtamaPengkajian,
        //                     a.CurrentMedicationId,

        //                     // info pain assessment
        //                     pa.InheritedDisease,

        //                     // info surat pengantar
        //                     sp.AsalUnit,

        //                 }).OrderByDescending(a => a.CreateDateTime);

        //    // Hitung total data sebelum paginasi
        //    var totalRows = query.Count();
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
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.KajianPasiens.Find(id);
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
        public async Task<IActionResult> Create([FromBody] KajianPasienViewModel vm)
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

                // **Buat Data Baru**
                var data = new KajianPasien
                {
                    KajianPasienId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    VitalSignId = vm.VitalSignId,
                    DokterId = vm.DokterId,
                    UserActiveId = userActiveId,
                    KeadaanUmum = vm.KeadaanUmum,
                    KeadaanKulit = vm.KeadaanKulit,
                    KeadaanKepalaLeher = vm.KeadaanKepalaLeher,
                    KeadaanDada = vm.KeadaanDada,
                    KeadaanJantung = vm.KeadaanJantung,
                    KeadaanParuParu = vm.KeadaanParuParu,
                    KeadaanAbdomen = vm.KeadaanAbdomen,
                    KeadaanGenitalia = vm.KeadaanGenitalia,
                    KeadaanAnggotaGerak = vm.KeadaanAnggotaGerak,
                    KeadaanLainnya = vm.KeadaanLainnya,
                    StatusLokalis = vm.StatusLokalis,
                    PemeriksaanPenunjang = vm.PemeriksaanPenunjang,
                    DiagnosaSaatIni = vm.DiagnosaSaatIni,
                    DiagnosaBanding = vm.DiagnosaBanding,
                    DaftarMasalah = vm.DaftarMasalah,
                    Program = vm.Program,
                    Terapi = vm.Terapi,
                    Edukasi = true,
                    EdukasiKepada = vm.EdukasiKepada,
                    Keterangan = vm.Keterangan,
                    TglKajian = DateTime.UtcNow, // Atau gunakan TglKajian dari VM jika ada
                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                // **Simpan ke Database**
                _applicationDbContext.KajianPasiens.Add(data);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] KajianPasienViewModel vm)
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
                var data = await _applicationDbContext.KajianPasiens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.KunjunganId = vm.KunjunganId;
                data.VitalSignId = vm.VitalSignId;
                data.DokterId = vm.DokterId;
                data.UserActiveId = userActiveId;
                data.KeadaanUmum = vm.KeadaanUmum;
                data.KeadaanKulit = vm.KeadaanKulit;
                data.KeadaanKepalaLeher = vm.KeadaanKepalaLeher;
                data.KeadaanDada = vm.KeadaanDada;
                data.KeadaanJantung = vm.KeadaanJantung;
                data.KeadaanParuParu = vm.KeadaanParuParu;
                data.KeadaanAbdomen = vm.KeadaanAbdomen;
                data.KeadaanGenitalia = vm.KeadaanGenitalia;
                data.KeadaanAnggotaGerak = vm.KeadaanAnggotaGerak;
                data.KeadaanLainnya = vm.KeadaanLainnya;
                data.StatusLokalis = vm.StatusLokalis;
                data.PemeriksaanPenunjang = vm.PemeriksaanPenunjang;
                data.DiagnosaSaatIni = vm.DiagnosaSaatIni;
                data.DiagnosaBanding = vm.DiagnosaBanding;
                data.DaftarMasalah = vm.DaftarMasalah;
                data.Program = vm.Program;
                data.Terapi = vm.Terapi;
                data.Edukasi = true; // Asumsikan edukasi selalu true saat update
                data.EdukasiKepada = vm.EdukasiKepada;
                data.Keterangan = vm.Keterangan;
                data.TglKajian = DateTime.UtcNow; // Atau gunakan TglKajian dari VM jika ada



                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.KajianPasiens.Update(data);
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
                var data = await _applicationDbContext.KajianPasiens.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.KajianPasiens.Update(data);
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
            var query = (from a in _applicationDbContext.KajianPasiens
                         join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into userGroup
                         from u in userGroup.DefaultIfEmpty()

                         join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID into kunjunganGroup
                         from k in kunjunganGroup.DefaultIfEmpty()

                         join pa in _applicationDbContext.PainAssessments on a.KunjunganId equals pa.KunjunganId into painGroup
                         from pa in painGroup.DefaultIfEmpty()

                         join sp in _applicationDbContext.SuratPengantarRawatInaps on a.KunjunganId equals sp.KunjunganId into suratGroup
                         from sp in suratGroup.DefaultIfEmpty()

                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.KajianPasienId,
                             a.KunjunganId,
                             a.VitalSignId,
                             k.NoRekamMedis,
                             a.DokterId,
                             a.UserActiveId,
                             a.KeadaanUmum,
                             a.KeadaanKulit,
                             a.KeadaanKepalaLeher,
                             a.KeadaanDada,
                             a.KeadaanJantung,
                             a.KeadaanParuParu,
                             a.KeadaanAbdomen,
                             a.KeadaanGenitalia,
                             a.KeadaanAnggotaGerak,
                             a.KeadaanLainnya,
                             a.StatusLokalis,
                             a.PemeriksaanPenunjang,
                             a.DiagnosaSaatIni,
                             a.DiagnosaBanding,
                             a.DaftarMasalah,
                             a.Program,
                             a.Terapi,
                             a.Edukasi,
                             a.EdukasiKepada,
                             a.Keterangan,
                             a.TglKajian,
                             a.KajianUtamaPengkajian,
                             a.CurrentMedicationId,

                             // info pain assessment
                             pa.InheritedDisease,

                             // info surat pengantar
                             sp.AsalUnit,

                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
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
