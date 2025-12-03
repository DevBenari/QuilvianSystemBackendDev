using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Gizi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class GiziAssessmentController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<GiziAssessmentController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GiziAssessmentController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<GiziAssessmentController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "AssessmentId tidak valid." });

            try
            {
                // ============================
                // 1️⃣ Ambil Parent (Assessment Gizi)
                // ============================
                var parent =
                    await (from a in _applicationDbContext.GiziAssessments
                           join p in _applicationDbContext.PendaftaranPasienBarus
                               on a.PasienId equals p.PendaftaranPasienBaruId into pasienJoin
                           from pasien in pasienJoin.DefaultIfEmpty()

                           join k in _applicationDbContext.Kunjungans
                               on a.KunjunganId equals k.KunjunganID into kunjunganJoin
                           from kunjungan in kunjunganJoin.DefaultIfEmpty()

                           where a.AssessmentId == id && (a.IsDelete == false || a.IsDelete == null)
                           select new
                           {
                               a.AssessmentId,
                               a.KunjunganId,
                               KunjunganNo = kunjungan.Antrian,
                               a.PasienId,
                               pasien.NamaLengkap,
                               pasien.NoRekamMedis,

                               // Assessment
                               a.Anthropometri,
                               a.Biokimia,
                               a.Klinis,
                               a.RiwayatGizi,
                               a.RiwayatPersonal,
                               a.DiagnosisGizi,
                               a.IntervensiGizi,
                               a.JenisDiet,
                               a.BentukMakanan,
                               a.Frekuensi,
                               a.RutePemberian,

                               a.Energi,
                               a.Protein,
                               a.Karbohidrat,
                               a.Lemak,

                               a.EdukasiAwal,
                               a.Keterangan,
                               a.TglPencatatan,

                               a.CreateBy,
                               a.CreateDateTime,
                               a.UpdateBy,
                               a.UpdateDateTime
                           })
                           .FirstOrDefaultAsync();

                if (parent == null)
                {
                    return NotFound(new { message = "Data Assessment Gizi tidak ditemukan." });
                }


                // ============================
                // 2️⃣ Ambil Child (Evaluasi Gizi)
                // ============================
                var evaluasiList =
                    await (from e in _applicationDbContext.GiziEvaluasis
                           where e.AssessmentGiziId == id && (e.IsDelete == false || e.IsDelete == null)
                           orderby e.TglEvaluasi descending
                           select new
                           {
                               e.EvaluasiGiziId,
                               e.AssessmentGiziId,
                               e.TglEvaluasi,
                               e.MakananPokok,
                               e.LHTinggiLemak,
                               e.LHRendahLemak,
                               e.LaukNabati,
                               e.Sayur,
                               e.Buah,
                               e.SusuDiabetes,
                               e.SusuBiasa,
                               e.JumlahKalori,
                               e.IdentifikasiMasalah,
                               e.TindakLanjut,
                               e.CatatanPerawat,
                               e.CreateBy,
                               e.CreateDateTime,
                               e.UpdateBy,
                               e.UpdateDateTime
                           })
                           .ToListAsync();


                // ============================
                // 3️⃣ Gabungkan Parent + Child
                // ============================
                return Ok(new
                {
                    status = "success",
                    message = "Data retrieved successfully",
                    data = new
                    {
                        Assessment = parent,
                        Evaluasi = evaluasiList
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateFull([FromBody] GiziAssessmentViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // =============================
                // AMBIL USER LOGIN
                // =============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });


                // =============================
                // 1️⃣ INSERT ASSESSMENT GIZI (PARENT)
                // =============================
                var assessmentId = Guid.NewGuid();

                var assessment = new GiziAssessment
                {
                    AssessmentId = assessmentId,
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    Anthropometri = vm.Anthropometri,
                    Biokimia = vm.Biokimia,
                    Klinis = vm.Klinis,
                    RiwayatGizi = vm.RiwayatGizi,
                    RiwayatPersonal = vm.RiwayatPersonal,
                    DiagnosisGizi = vm.DiagnosisGizi,
                    IntervensiGizi = vm.IntervensiGizi,
                    JenisDiet = vm.JenisDiet,
                    BentukMakanan = vm.BentukMakanan,
                    Frekuensi = vm.Frekuensi,
                    RutePemberian = vm.RutePemberian,
                    Energi = vm.Energi,
                    Protein = vm.Protein,
                    Karbohidrat = vm.Karbohidrat,
                    Lemak = vm.Lemak,
                    EdukasiAwal = vm.EdukasiAwal,
                    Keterangan = vm.Keterangan,
                    TglPencatatan = vm.TglPencatatan,
                    CreateBy = user.UserActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };

                _applicationDbContext.GiziAssessments.Add(assessment);
                await _applicationDbContext.SaveChangesAsync();


                // =============================
                // 2️⃣ INSERT EVALUASI GIZI (DETAIL)
                // =============================
                if (vm.EvaluasiGizi != null && vm.EvaluasiGizi.Any())
                {
                    foreach (var ev in vm.EvaluasiGizi)
                    {
                        var detail = new GiziEvaluasi
                        {
                            EvaluasiGiziId = Guid.NewGuid(),
                            AssessmentGiziId = assessmentId,
                            TglEvaluasi = ev.TglEvaluasi,
                            MakananPokok = ev.MakananPokok,
                            LHTinggiLemak = ev.LHTinggiLemak,
                            LHRendahLemak = ev.LHRendahLemak,
                            LaukNabati = ev.LaukNabati,
                            Sayur = ev.Sayur,
                            Buah = ev.Buah,
                            SusuDiabetes = ev.SusuDiabetes,
                            SusuBiasa = ev.SusuBiasa,
                            JumlahKalori = ev.JumlahKalori,
                            IdentifikasiMasalah = ev.IdentifikasiMasalah,
                            TindakLanjut = ev.TindakLanjut,
                            CatatanPerawat = ev.CatatanPerawat,
                            CreateBy = user.UserActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.GiziEvaluasis.Add(detail);
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }


                // =============================
                // COMMIT TRANSACTION
                // =============================
                await trx.CommitAsync();

                return Created("", new
                {
                    message = "Assessment & Evaluasi Gizi berhasil disimpan!",
                    assessmentId = assessmentId
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
                [FromRoute] Guid id,
                [FromBody] GiziAssessmentViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var trx = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // =============================
                // AMBIL USER LOGIN
                // =============================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var user = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (user == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });


                // =============================
                // 1️⃣ AMBIL DATA ASSESSMENT (PARENT)
                // =============================
                var assessment = await _applicationDbContext.GiziAssessments
                    .FirstOrDefaultAsync(a => a.AssessmentId == id);

                if (assessment == null)
                    return NotFound(new { message = "Assessment Gizi tidak ditemukan." });


                // =============================
                // UPDATE PARENT
                // =============================
                assessment.KunjunganId = vm.KunjunganId;
                assessment.PasienId = vm.PasienId;
                assessment.Anthropometri = vm.Anthropometri;
                assessment.Biokimia = vm.Biokimia;
                assessment.Klinis = vm.Klinis;
                assessment.RiwayatGizi = vm.RiwayatGizi;
                assessment.RiwayatPersonal = vm.RiwayatPersonal;
                assessment.DiagnosisGizi = vm.DiagnosisGizi;
                assessment.IntervensiGizi = vm.IntervensiGizi;
                assessment.JenisDiet = vm.JenisDiet;
                assessment.BentukMakanan = vm.BentukMakanan;
                assessment.Frekuensi = vm.Frekuensi;
                assessment.RutePemberian = vm.RutePemberian;
                assessment.Energi = vm.Energi;
                assessment.Protein = vm.Protein;
                assessment.Karbohidrat = vm.Karbohidrat;
                assessment.Lemak = vm.Lemak;
                assessment.EdukasiAwal = vm.EdukasiAwal;
                assessment.Keterangan = vm.Keterangan;
                assessment.TglPencatatan = vm.TglPencatatan;
                assessment.UpdateBy = user.UserActiveId;
                assessment.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.GiziAssessments.Update(assessment);
                await _applicationDbContext.SaveChangesAsync();


                // =============================
                // 2️⃣ PROSES DETAIL EVALUASI GIZI
                // =============================
                if (vm.EvaluasiGizi != null)
                {
                    foreach (var item in vm.EvaluasiGizi)
                    {
                        // CASE A: Update data lama
                        if (item.AssessmentGiziId != null)
                        {
                            var detail = await _applicationDbContext.GiziEvaluasis
                                .FirstOrDefaultAsync(e =>
                                    e.AssessmentGiziId == id &&
                                    e.EvaluasiGiziId == item.AssessmentGiziId);

                            if (detail != null)
                            {
                                detail.TglEvaluasi = item.TglEvaluasi;
                                detail.MakananPokok = item.MakananPokok;
                                detail.LHTinggiLemak = item.LHTinggiLemak;
                                detail.LHRendahLemak = item.LHRendahLemak;
                                detail.LaukNabati = item.LaukNabati;
                                detail.Sayur = item.Sayur;
                                detail.Buah = item.Buah;
                                detail.SusuDiabetes = item.SusuDiabetes;
                                detail.SusuBiasa = item.SusuBiasa;
                                detail.JumlahKalori = item.JumlahKalori;
                                detail.IdentifikasiMasalah = item.IdentifikasiMasalah;
                                detail.TindakLanjut = item.TindakLanjut;
                                detail.CatatanPerawat = item.CatatanPerawat;
                                detail.UpdateBy = user.UserActiveId;
                                detail.UpdateDateTime = DateTimeOffset.UtcNow;

                                _applicationDbContext.GiziEvaluasis.Update(detail);
                            }
                        }
                        else
                        {
                            // CASE B: Insert baru
                            var newDetail = new GiziEvaluasi
                            {
                                EvaluasiGiziId = Guid.NewGuid(),
                                AssessmentGiziId = id,
                                TglEvaluasi = item.TglEvaluasi,
                                MakananPokok = item.MakananPokok,
                                LHTinggiLemak = item.LHTinggiLemak,
                                LHRendahLemak = item.LHRendahLemak,
                                LaukNabati = item.LaukNabati,
                                Sayur = item.Sayur,
                                Buah = item.Buah,
                                SusuDiabetes = item.SusuDiabetes,
                                SusuBiasa = item.SusuBiasa,
                                JumlahKalori = item.JumlahKalori,
                                IdentifikasiMasalah = item.IdentifikasiMasalah,
                                TindakLanjut = item.TindakLanjut,
                                CatatanPerawat = item.CatatanPerawat,
                                CreateBy = user.UserActiveId,
                                CreateDateTime = DateTimeOffset.UtcNow
                            };

                            _applicationDbContext.GiziEvaluasis.Add(newDetail);
                        }
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }


                // =============================
                // COMMIT TRANSACTION
                // =============================
                await trx.CommitAsync();


                return Ok(new
                {
                    message = "Assessment & Evaluasi Gizi berhasil diperbarui.",
                    assessmentId = id
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Kesalahan internal: {ex.Message}" });
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
                var data = await _applicationDbContext.GiziAssessments.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTime.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.GiziAssessments.Update(data);
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
        Guid? kunjunganId = null,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // ============================
            // 1️⃣ BASE QUERY (Assessment)
            // ============================
            var query =
                from a in _applicationDbContext.GiziAssessments
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId
                join p in _applicationDbContext.PendaftaranPasienBarus on a.PasienId equals p.PendaftaranPasienBaruId into pasienJoin
                from pasien in pasienJoin.DefaultIfEmpty()
                join k in _applicationDbContext.Kunjungans on a.KunjunganId equals k.KunjunganID into kunjunganJoin
                from kunjungan in kunjunganJoin.DefaultIfEmpty()
                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    // Identity
                    a.AssessmentId,
                    a.KunjunganId,
                    KunjunganNo = kunjungan.Antrian,
                    a.PasienId,
                    pasien.NamaLengkap,
                    pasien.NoRekamMedis,

                    // Assessment fields
                    a.Anthropometri,
                    a.Biokimia,
                    a.Klinis,
                    a.RiwayatGizi,
                    a.RiwayatPersonal,
                    a.DiagnosisGizi,
                    a.IntervensiGizi,
                    a.JenisDiet,
                    a.BentukMakanan,
                    a.Frekuensi,
                    a.RutePemberian,

                    a.Energi,
                    a.Protein,
                    a.Karbohidrat,
                    a.Lemak,

                    a.EdukasiAwal,
                    a.Keterangan,
                    a.TglPencatatan,

                    a.CreateBy,
                    a.CreateDateTime,
                    a.UpdateBy,
                    a.UpdateDateTime,

                    CreateByName = u.FullName,

                    // For search
                    SearchField =
                        ((pasien.NamaLengkap ?? "") + " " +
                         (pasien.NoRekamMedis ?? "") + " " +
                         (a.DiagnosisGizi ?? "") + " " +
                         (a.JenisDiet ?? "")).ToLower(),
                };


            // ============================
            // 2️⃣ FILTER KUNJUNGAN ID
            // ============================
            if (kunjunganId.HasValue)
            {
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);
            }


            // ============================
            // 3️⃣ SEARCH
            // ============================
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(x => x.SearchField.Contains(s));
            }


            // ============================
            // 4️⃣ FILTER TANGGAL
            // ============================
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date.ToUniversalTime();
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime <= end);
            }


            // ============================
            // 5️⃣ FILTER PERIODE
            // ============================
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == today);
                        break;

                    case PeriodeFilter.ThisWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek)
                            && x.CreateDateTime.Date <= today);
                        break;

                    case PeriodeFilter.LastWeek:
                        query = query.Where(x =>
                            x.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek)
                            && x.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek));
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        query = query.Where(x =>
                            x.CreateDateTime.Month == today.Month - 1 &&
                            x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == today.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }


            // ============================
            // 6️⃣ SORTING
            // ============================
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "NamaLengkap" => query.OrderByDescending(x => x.NamaLengkap),
                    "KunjunganNo" => query.OrderByDescending(x => x.KunjunganNo),
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "NamaLengkap" => query.OrderBy(x => x.NamaLengkap),
                    "KunjunganNo" => query.OrderBy(x => x.KunjunganNo),
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };


            // ============================
            // 7️⃣ PAGINATION
            // ============================
            int totalRows = await query.CountAsync();

            var pagedRows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            var assessmentIds = pagedRows.Select(r => r.AssessmentId).ToList();


            // ============================
            // 8️⃣ JOIN Evaluasi Gizi (tanpa N+1)
            // ============================
            var evaluasi =
                await (from e in _applicationDbContext.GiziEvaluasis
                       where assessmentIds.Contains((Guid)e.AssessmentGiziId)
                       select new
                       {
                           e.EvaluasiGiziId,
                           e.AssessmentGiziId,
                           e.TglEvaluasi,
                           e.MakananPokok,
                           e.LHTinggiLemak,
                           e.LHRendahLemak,
                           e.LaukNabati,
                           e.Sayur,
                           e.Buah,
                           e.SusuDiabetes,
                           e.SusuBiasa,
                           e.JumlahKalori,
                           e.IdentifikasiMasalah,
                           e.TindakLanjut,
                           e.CatatanPerawat
                       }).ToListAsync();


            // ============================
            // 9️⃣ MERGE PARENT + CHILD
            // ============================
            var result = pagedRows.Select(parent => new
            {
                Assessment = parent,
                Evaluasi = evaluasi.Where(x => x.AssessmentGiziId == parent.AssessmentId)
            });


            // ============================
            // 🔟 RETURN
            // ============================
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = result,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = (int)Math.Ceiling(totalRows / (double)perPage)
                }
            });
        }



    }
}
